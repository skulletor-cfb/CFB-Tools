using MaddenFranchise;

namespace BowlEditor
{
    /// <summary>
    /// Edits a CFB 27 dynasty's postseason, in two steps. A week-16 save opens the CFP poll (<see cref="CfpPoll"/>): the game
    /// builds the playoff from it when advancing to bowl week. A bowl-week save (17 or 18) opens the bowl matchups
    /// (<see cref="BowlMatchups"/>): one grid row per game, away and home picked from the FBS teams, CFP games shown for
    /// reference. The save is only written by Save as.
    /// </summary>
    public sealed class MainForm : Form
    {
        private const int PollWeek = 16;

        private static readonly Color ChangedColor = Color.FromArgb(255, 248, 205);
        private static readonly Color ConflictColor = Color.FromArgb(255, 214, 214);
        private static readonly Color PlayoffColor = Color.FromArgb(222, 240, 226);

        private readonly DataGridView grid = new DataGridView();
        private readonly DataGridView pollGrid = new DataGridView();
        private readonly ToolStripStatusLabel status = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };

        private FranchiseFile? dynasty;
        private BowlMatchups? matchups;
        private CfpPoll? poll;
        private string? savePath;
        private bool populating;

        private DataGridViewComboBoxColumn awayColumn = null!;
        private DataGridViewComboBoxColumn homeColumn = null!;
        private DataGridViewTextBoxColumn rankColumn = null!;

        public MainForm()
        {
            Text = "CFB 27 Bowl Editor";
            ClientSize = new Size(1000, 760);
            // the exe's icon (ApplicationIcon) for the title bar too
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            BuildGrid();
            BuildPollGrid();
            BuildMenu();

            var statusStrip = new StatusStrip();
            statusStrip.Items.Add(status);
            Controls.Add(statusStrip);
            status.Text = "Ctrl+O to open a dynasty save: week 16 to edit the CFP poll, bowl week 1 (week 17) to edit bowl matchups.";

            FormClosing += OnFormClosing;
        }

        private bool IsDirty => poll?.IsChanged == true || matchups?.Games.Any(g => g.IsChanged) == true;

        #region open / save

        private void OpenSave()
        {
            if (IsDirty && !ConfirmDiscard("Open another save"))
            {
                return;
            }

            var savesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EA SPORTS College Football 27", "saves");
            using var dialog = new OpenFileDialog
            {
                Title = "Open dynasty save",
                InitialDirectory = savePath != null ? Path.GetDirectoryName(savePath) : Directory.Exists(savesFolder) ? savesFolder : null,
                Filter = "Dynasty saves|DYNASTY-*|All files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                var file = FranchiseFile.Load(dialog.FileName);
                var seasonInfo = file.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
                bool pollWeek = Convert.ToInt32(seasonInfo["CurrentWeek"]) == PollWeek;

                dynasty = file;
                savePath = dialog.FileName;
                poll = pollWeek ? new CfpPoll(file) : null;
                matchups = pollWeek ? null : new BowlMatchups(file);
                Text = $"CFB 27 Bowl Editor - {Path.GetFileName(savePath)}";
                grid.Visible = !pollWeek;
                pollGrid.Visible = pollWeek;

                if (poll != null)
                {
                    FillPoll();
                    return;
                }

                FillGrid();
                if (matchups!.NotEditableReason != null)
                {
                    MessageBox.Show(this, matchups.NotEditableReason, "Bowls can't be edited", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (matchups.Games.Count == 0)
                {
                    MessageBox.Show(this, "This save has no bowl matchups yet. The game sets them after the conference championships.",
                        "No bowls", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SaveAs()
        {
            if (dynasty == null)
            {
                return;
            }

            grid.EndEdit();
            pollGrid.EndEdit();
            if (matchups != null)
            {
                var problems = matchups.Problems();
                if (problems.Count > 0)
                {
                    MessageBox.Show(this, "Fix these first:\n\n" + string.Join("\n", problems), "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var leftOut = matchups.TeamsLeftOut();
                if (leftOut.Count > 0 &&
                    MessageBox.Show(this, "These teams no longer have a postseason game:\n\n" + string.Join("\n", leftOut) + "\n\nSave anyway?",
                        "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            using var dialog = new SaveFileDialog
            {
                Title = poll != null ? "Save CFP poll as" : "Save bowls as",
                InitialDirectory = savePath == null ? null : Path.GetDirectoryName(savePath),
                FileName = (savePath == null ? "DYNASTY" : Path.GetFileName(savePath)) + (poll != null ? "-POLL" : "-BOWLS"),
                Filter = "Dynasty saves|DYNASTY-*|All files|*.*",
                OverwritePrompt = true,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                int changed = poll != null ? poll.Apply() : matchups!.Apply();
                var result = dynasty.Save(dialog.FileName);
                savePath = dialog.FileName;
                Text = $"CFB 27 Bowl Editor - {Path.GetFileName(savePath)}";
                if (poll != null)
                {
                    FillPoll();
                }
                else
                {
                    FillGrid();
                }

                status.Text = poll != null
                    ? $"Saved the CFP poll ({changed} team(s) moved) to {result.Destination}. Advance to bowl week to build the playoff from it."
                    : $"Saved {changed} changed game(s) to {result.Destination}. Changed games are unplayed again; advancing simulates them." +
                      (matchups!.ClosedUserRequest ? "  Your team left this week's game, so its play-your-game request was closed." : string.Empty) +
                      (matchups.WrittenResults.Count > 0 ? $"  Final score written (no box score): {string.Join("; ", matchups.WrittenResults)}." : string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ConfirmDiscard(string action) =>
            MessageBox.Show(this, $"You have unsaved changes. {action} anyway?", "Bowl Editor",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

        private void OnFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (IsDirty && !ConfirmDiscard("Close"))
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region layout

        private void BuildMenu()
        {
            var menu = new MenuStrip();
            var file = new ToolStripMenuItem("&File");
            file.DropDownItems.Add(new ToolStripMenuItem("&Open save...", null, (s, e) => OpenSave(), Keys.Control | Keys.O));
            file.DropDownItems.Add(new ToolStripMenuItem("Save &as...", null, (s, e) => SaveAs(), Keys.Control | Keys.S));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (s, e) => Close()));

            var pollMenu = new ToolStripMenuItem("&Poll");
            pollMenu.DropDownItems.Add(new ToolStripMenuItem("Move &up", null, (s, e) => NudgeSelected(-1), Keys.Control | Keys.Up));
            pollMenu.DropDownItems.Add(new ToolStripMenuItem("Move &down", null, (s, e) => NudgeSelected(1), Keys.Control | Keys.Down));

            menu.Items.Add(file);
            menu.Items.Add(pollMenu);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private static void SetupGrid(DataGridView view)
        {
            view.Dock = DockStyle.Fill;
            view.AllowUserToAddRows = false;
            view.AllowUserToDeleteRows = false;
            view.AllowUserToResizeRows = false;
            view.RowHeadersVisible = false;
            view.SelectionMode = DataGridViewSelectionMode.CellSelect;
            view.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            view.DataError += (s, e) => e.ThrowException = false;
        }

        private void BuildGrid()
        {
            SetupGrid(grid);
            grid.EditMode = DataGridViewEditMode.EditOnEnter;

            awayColumn = TeamColumn("Away");
            homeColumn = TeamColumn("Home");
            grid.Columns.Add(TextColumn("Game", 190));
            grid.Columns.Add(homeColumn);
            grid.Columns.Add(awayColumn);
            grid.Columns.Add(TextColumn("Week", 50));
            grid.Columns.Add(TextColumn("Date", 80));
            grid.Columns.Add(TextColumn("Seeds", 80));

            // commit a team pick right away, so the row colors and status update as you choose
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.CellValueChanged += OnCellValueChanged;

            Controls.Add(grid);
        }

        private void BuildPollGrid()
        {
            SetupGrid(pollGrid);
            pollGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            pollGrid.Visible = false;

            rankColumn = new DataGridViewTextBoxColumn { HeaderText = "Rank", FillWeight = 40, SortMode = DataGridViewColumnSortMode.NotSortable };
            pollGrid.Columns.Add(rankColumn);
            pollGrid.Columns.Add(TextColumn("Team", 160));
            pollGrid.Columns.Add(TextColumn("Record", 60));
            pollGrid.Columns.Add(TextColumn("AP", 40));
            pollGrid.Columns.Add(TextColumn("Was", 40));
            pollGrid.Columns.Add(TextColumn("Playoff", 80));
            pollGrid.CellValueChanged += OnRankChanged;

            Controls.Add(pollGrid);
        }

        private static DataGridViewTextBoxColumn TextColumn(string header, int weight) =>
            new DataGridViewTextBoxColumn { HeaderText = header, ReadOnly = true, FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable };

        private static DataGridViewComboBoxColumn TeamColumn(string header) =>
            new DataGridViewComboBoxColumn
            {
                HeaderText = header,
                FillWeight = 200,
                ValueMember = nameof(TeamChoice.Row),
                DisplayMember = nameof(TeamChoice.Label),
                ValueType = typeof(int),
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat,
                SortMode = DataGridViewColumnSortMode.NotSortable,
            };

        #endregion

        #region bowls

        private void FillGrid()
        {
            populating = true;
            try
            {
                grid.Rows.Clear();
                if (matchups == null)
                {
                    return;
                }

                awayColumn.DataSource = matchups.BowlTeams;
                homeColumn.DataSource = matchups.BowlTeams;
                bool readOnly = matchups.NotEditableReason != null;

                foreach (var game in matchups.Games)
                {
                    int index = grid.Rows.Add(game.Bowl, (object?)game.Home ?? DBNull.Value, (object?)game.Away ?? DBNull.Value, $"{game.Week}", game.Date, game.Seeds);
                    var row = grid.Rows[index];
                    row.Tag = game;
                    row.ReadOnly = readOnly;
                    if (game.Locked)
                    {
                        // the CFP field comes from the poll; a quarterfinal's visitor is the first-round winner
                        row.Cells[awayColumn.Index] = new DataGridViewTextBoxCell { Value = game.AwayNote ?? matchups.TeamName(game.Away) };
                        row.Cells[homeColumn.Index] = new DataGridViewTextBoxCell { Value = matchups.TeamName(game.Home) };
                        row.ReadOnly = true;
                        row.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                    }
                }
            }
            finally
            {
                populating = false;
            }

            Recolor();
        }

        private void OnCellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (populating || e.RowIndex < 0 || grid.Rows[e.RowIndex].Tag is not BowlSlot game || game.Locked)
            {
                return;
            }

            if (e.ColumnIndex != homeColumn.Index && e.ColumnIndex != awayColumn.Index)
            {
                return;
            }

            // a team taken from another game swaps with the one it replaces; show that game's new team too
            var value = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as int?;
            var swapped = matchups!.Assign(game, e.ColumnIndex == homeColumn.Index, value);
            if (swapped != null)
            {
                ShowTeams(swapped);
            }

            Recolor();
        }

        /// <summary>Puts a game's current teams back into its row (after a swap changed them).</summary>
        private void ShowTeams(BowlSlot game)
        {
            var row = grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag == game);
            if (row == null)
            {
                return;
            }

            populating = true;
            try
            {
                row.Cells[awayColumn.Index].Value = (object?)game.Away ?? DBNull.Value;
                row.Cells[homeColumn.Index].Value = (object?)game.Home ?? DBNull.Value;
            }
            finally
            {
                populating = false;
            }
        }

        /// <summary>Yellow: changed. Red: a team in two games (or playing itself). The status line says what's wrong.</summary>
        private void Recolor()
        {
            if (matchups == null)
            {
                return;
            }

            var problems = matchups.Problems();
            var counts = matchups.Games.SelectMany(g => new[] { g.Home, g.Away }).Where(r => r != null).GroupBy(r => r!.Value).ToDictionary(g => g.Key, g => g.Count());
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Tag is not BowlSlot game)
                {
                    continue;
                }

                bool conflict = new[] { game.Home, game.Away }.Any(r => r != null && counts[r.Value] > 1);
                row.DefaultCellStyle.BackColor = conflict ? ConflictColor : game.IsChanged ? ChangedColor : SystemColors.Window;
            }

            int changed = matchups.Games.Count(g => g.IsChanged);
            var leftOut = matchups.TeamsLeftOut();
            status.Text = matchups.NotEditableReason != null ? "Read only: " + matchups.NotEditableReason
                : problems.Count > 0 ? string.Join("  ", problems)
                : $"{matchups.Games.Count} games, {changed} changed (they go back to Unplayed for the game to simulate)." +
                  (leftOut.Count > 0 ? $"  No game now: {string.Join(", ", leftOut)}." : string.Empty);
        }

        #endregion

        #region poll

        private void FillPoll(PollEntry? select = null)
        {
            if (poll == null)
            {
                return;
            }

            populating = true;
            try
            {
                pollGrid.Rows.Clear();
                foreach (var entry in poll.Entries)
                {
                    int rank = poll.RankOf(entry);
                    int index = pollGrid.Rows.Add($"{rank}", entry.Name, entry.Record, entry.MediaRank >= 1 && entry.MediaRank <= 25 ? $"{entry.MediaRank}" : string.Empty,
                        $"{entry.OriginalRank}", rank <= CfpPoll.PlayoffTeams ? $"#{rank} seed" : string.Empty);
                    var row = pollGrid.Rows[index];
                    row.Tag = entry;
                    row.DefaultCellStyle.BackColor = entry.OriginalRank != rank ? ChangedColor : rank <= CfpPoll.PlayoffTeams ? PlayoffColor : SystemColors.Window;
                    if (entry == select)
                    {
                        pollGrid.CurrentCell = row.Cells[rankColumn.Index];
                    }
                }
            }
            finally
            {
                populating = false;
            }

            int moved = poll.Entries.Count(e => e.OriginalRank != poll.RankOf(e));
            status.Text = $"CFP poll: type a rank (or Ctrl+Up / Ctrl+Down) to move a team; the top {CfpPoll.PlayoffTeams} make the playoff when you advance. " +
                $"{moved} team(s) moved.";
        }

        private void OnRankChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (populating || poll == null || e.RowIndex < 0 || e.ColumnIndex != rankColumn.Index || pollGrid.Rows[e.RowIndex].Tag is not PollEntry entry)
            {
                return;
            }

            var text = pollGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
            if (int.TryParse(text, out int rank))
            {
                poll.Move(entry, rank);
            }

            // rebuild after the edit has finished
            BeginInvoke(() => FillPoll(entry));
        }

        private void NudgeSelected(int by)
        {
            if (poll == null || pollGrid.CurrentRow?.Tag is not PollEntry entry)
            {
                return;
            }

            poll.Move(entry, poll.RankOf(entry) + by);
            FillPoll(entry);
        }

        #endregion
    }
}
