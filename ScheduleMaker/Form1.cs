using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Dynasty;
using NCAA14Utility;
using NCAA14Utility.Scheduling;
using System.Windows.Forms.VisualStyles;

namespace ScheduleMaker
{
    /// <summary>
    /// Edits a CFB 27 dynasty's schedule in the preseason (PreseasonSchedule): one grid of every game, sorted by week
    /// and home team. Edits go straight into the save; the grid only re-sorts on Ctrl+H.
    /// </summary>
    public partial class Form1 : Form
    {
        private const int LastRegularSeasonWeek = 14;
        private const int NoTeamIndex = 255;
        private const string ContinuationFile = "continuationfile.txt";
        private const string RankingsFile = "rankings.csv";
        private const string ScheduleModelFile =@"D:\repos\CFB-Tools\Release\schedulemodel.txt";

        private readonly DataGridView grid = new DataGridView();
        private readonly ToolStripStatusLabel status = new ToolStripStatusLabel();

        private FranchiseFile? dynasty;
        private PreseasonSchedule? schedule;
        private string? savePath;
        private bool isDirty;
        private string saveFile;
        private bool scheduleLoaded=false;

        // grid columns
        private DataGridViewTextBoxColumn weekColumn = null!;
        private DataGridViewComboBoxColumn dayColumn = null!;
        private DataGridViewComboBoxColumn timeColumn = null!;
        private DataGridViewComboBoxColumn awayColumn = null!;
        private DataGridViewComboBoxColumn homeColumn = null!;
        private DataGridViewComboBoxColumn stadiumColumn = null!;
        private DataGridViewCheckBoxColumn kickoffColumn = null!;
        private DataGridViewTextBoxColumn conferenceColumn = null!;

        private bool populating;

        public Form1()
        {
            InitializeComponent();
            Text = "CFB 27 Schedule Maker";
            ClientSize = new Size(1200, 800);
            // the exe's icon (ApplicationIcon) for the title bar too
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            BuildGrid();
            BuildMenu();

            var statusStrip = new StatusStrip();
            statusStrip.Items.Add(status);
            Controls.Add(statusStrip);
            status.Text = "Ctrl+O to open a preseason dynasty save.";

            FormClosing += OnFormClosing;
        }

        #region read / save

        /// <summary>Ctrl+O. Picks a dynasty save (DYNASTY-xxx, no extension) and shows its preseason schedule.</summary>
        private void OpenSave()
        {
            if (isDirty &&
                MessageBox.Show(this, "You have unsaved schedule changes. Open another save anyway?", "Open",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            // Documents\EA SPORTS College Football 27\saves (MyDocuments follows a OneDrive redirect)
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

            this.saveFile = dialog.FileName;
            dynasty = FranchiseFile.Load(this.saveFile); 
            savePath = this.saveFile;

        }

        private void LoadSchedule()
        { 
            // only run this once
            if (scheduleLoaded)
            {
                return; 
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                LoadSchedule(this.dynasty, this.saveFile);
                scheduleLoaded = true;
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

        /// <summary>Ctrl+R. Writes the schedule as it stands (schedulemodel.txt, the ScheduleDisplay format) to the current directory.</summary>
        private void ReadSave()
        {
            LoadSchedule();
            if (dynasty == null || schedule == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            try
            {
                grid.EndEdit();
                var path = Path.Combine(Environment.CurrentDirectory, ScheduleModelFile);
                ScheduleModelExporter.Create(dynasty, schedule).WriteJsonFile(path);
                status.Text = $"Wrote {path}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ctrl+Z. Initializes the season the game generated: tops the FCS games up to 110 with FCS-vs-FCS placeholders, then
        /// sets every conference's games to our rotation for the year in the save's name (DYNASTY-Y588P is 2588). Nothing
        /// is written until Save as.
        /// </summary>
        private void InitializeSeason()
        {
            LoadSchedule();
            if (dynasty == null || schedule == null || savePath == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            try
            {
                int year = SeasonInitializer.YearFromFileName(savePath);
                Cursor = Cursors.WaitCursor;
                grid.EndEdit();
                var report = SeasonInitializer.Initialize(dynasty, schedule, year);
                isDirty = true;
                RefreshGrid();

                var problems = schedule.Validate();
                MessageBox.Show(this, $"{year} season\n\n" + string.Join("\n", report) +
                    (problems.Count > 0 ? "\n\nValidate:\n" + string.Join("\n", problems.Take(20)) : string.Empty),
                    "Initialize season", MessageBoxButtons.OK, problems.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Initialize season", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Ctrl+G. Smooths G5 schedules (see <see cref="G5Smoother"/>): spreads out extra FCS games, then extra P5 games
        /// of G5 teams, by trading opponents within a week. Nothing is written until Save as.
        /// </summary>
        private void SmoothG5Schedules()
        {
            LoadSchedule();
            if (dynasty == null || schedule == null || savePath == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                grid.EndEdit();
                var report = new G5Smoother(dynasty, schedule).Smooth();
                isDirty = true;
                RefreshGrid();

                var problems = schedule.Validate();
                MessageBox.Show(this, string.Join("\n", report.Take(60)) + (report.Count > 60 ? $"\n...and {report.Count - 60} more" : string.Empty) +
                    (problems.Count > 0 ? "\n\nValidate:\n" + string.Join("\n", problems.Take(20)) : string.Empty),
                    "Smooth G5 schedules", MessageBoxButtons.OK, problems.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Smooth G5 schedules", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Ctrl+I. Replaces the whole schedule with an NCAA 14 SCHD dump (CSV); the teams new in CFB 27 get a round robin
        /// and an FCS game. Nothing is written until Save as.
        /// </summary>
        private void ImportNcaa14Schedule()
        {
            if (schedule == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Import NCAA 14 schedule (SCHD.csv)",
                Filter = "CSV files|*.csv|All files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                grid.EndEdit();
                var report = Ncaa14ScheduleImporter.Import(schedule, dialog.FileName);
                schedule.AcceptChanges();
                isDirty = true;
                RefreshGrid();
                MessageBox.Show(this, string.Join("\n\n", report.Take(2)) +
                    (report.Count > 2 ? "\n\nProblems:\n" + string.Join("\n", report.Skip(2).Take(30)) : "\n\nEvery FBS team has 12 games, one per week."),
                    "Import", MessageBoxButtons.OK, report.Count > 2 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Ctrl+P. Ports the NCAA 14 rosters (a PLAY dump, CSV) onto the open save; hometowns come from cities.csv and
        /// announcer surname audio from cfb27_surname_audio.csv (optional), both looked for next to PLAY.csv and one folder up.
        /// Nothing is written until Save as.
        /// </summary>
        private void ImportNcaa14Rosters()
        {
            if (dynasty == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Import NCAA 14 rosters (PLAY.csv)",
                Filter = "CSV files|*.csv|All files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var folder = Path.GetDirectoryName(dialog.FileName)!;
            string? FindNearby(string name) => new[] { folder, Path.GetDirectoryName(folder) }
                .Where(d => d != null)
                .Select(d => Path.Combine(d!, name))
                .FirstOrDefault(File.Exists);
            var cities = FindNearby("cities.csv");
            var surnameAudio = FindNearby("cfb27_surname_audio.csv");
            if (cities == null)
            {
                using var citiesDialog = new OpenFileDialog { Title = "NCAA 14 hometowns (cities.csv)", Filter = "CSV files|*.csv|All files|*.*", CheckFileExists = true };
                if (citiesDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                cities = citiesDialog.FileName;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                var report = Ncaa14RosterImporter.Import(dynasty, dialog.FileName, cities, surnameAudio);
                isDirty = true;
                UpdateStatus();
                MessageBox.Show(this, string.Join("\n\n", report.Take(1)) +
                    (report.Count > 1 ? "\n\nSkipped:\n" + string.Join("\n", report.Skip(1).Take(30)) : string.Empty),
                    "Import rosters", MessageBoxButtons.OK, report.Count > 1 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Ctrl+B. Writes rankings.csv (CFP rank, team, record, conference title) to the current directory. CFP rankings only
        /// exist in season saves, which Ctrl+O can't open, so without an open save this asks for one.
        /// </summary>
        private void ReadRankings()
        {
            var file = dynasty;
            if (file == null)
            {
                using var dialog = new OpenFileDialog
                {
                    Title = "Read rankings from a dynasty save",
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
                    file = FranchiseFile.Load(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Read rankings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }

            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, RankingsFile);
                int teams = RankingsExporter.Write(file, path);
                status.Text = teams == 0 ? "This save has no CFP rankings yet (the CFP poll hasn't started)." : $"Wrote {path} ({teams} teams).";
                Clipboard.SetText(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Read rankings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Ctrl+T. Ports NCAA 14 team metadata (a TEAM dump, CSV) onto the open save: polls, recruiting pitch grades and
        /// prestige, and zeroes the teams' history totals. Nothing is written until Save as.
        /// </summary>
        private void ImportNcaa14Teams()
        {
            if (dynasty == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Import NCAA 14 teams (TEAM.csv)",
                Filter = "CSV files|*.csv|All files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                var report = Ncaa14TeamImporter.Import(dynasty, dialog.FileName);
                isDirty = true;
                UpdateStatus();
                MessageBox.Show(this, report[0] + (report.Count > 1 ? "\n\nSkipped:\n" + string.Join("\n", report.Skip(1).Take(30)) : string.Empty),
                    "Import teams", MessageBoxButtons.OK, report.Count > 1 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Ctrl+K. Ports the NCAA 14 coaching staffs (a COCH dump, CSV) onto the open save and writes continuationfile.txt
        /// (NCAA 14 coach id → CFB 27 coach row) to the current directory. The save itself isn't written until Save as.
        /// </summary>
        private void ImportNcaa14Coaches()
        {
            if (dynasty == null)
            {
                status.Text = "Ctrl+O to open a save first.";
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Import NCAA 14 coaches (COCH.csv)",
                Filter = "CSV files|*.csv|All files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                var (report, mapping) = Ncaa14CoachImporter.Import(dynasty, dialog.FileName);
                var continuation = Path.Combine(Environment.CurrentDirectory, ContinuationFile);
                Ncaa14CoachImporter.WriteContinuationFile(mapping, dialog.FileName, continuation);
                isDirty = true;
                UpdateStatus();
                MessageBox.Show(this, report[0] + $"\n\nWrote {continuation} ({mapping.Count} coaches)." +
                    (report.Count > 1 ? "\n\nSkipped:\n" + string.Join("\n", report.Skip(1).Take(30)) : string.Empty),
                    "Import coaches", MessageBoxButtons.OK, report.Count > 1 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Shows a save's preseason schedule. Throws (from PreseasonSchedule.Load) if the save is past the preseason,
        /// since the game moves the schedule into SeasonGame at week 0.
        /// </summary>
        public void LoadSchedule(FranchiseFile file, string? path)
        {
            var loaded = PreseasonSchedule.Load(file);

            schedule = loaded;
            ScheduleHelpers.TeamAndConferences = CFBConference.ReadTeamConferences(file);
            isDirty = false;

            SetColumnChoices();
            RefreshGrid();
        }

        private void SaveAs()
        {
            if (dynasty == null)
            {
                return;
            }

            grid.EndEdit();
            if (schedule != null)
            {
                var problems = schedule!.Validate();
                if (problems.Count > 0 &&
                    MessageBox.Show(this, "The schedule has problems:\n\n" + string.Join("\n", problems.Take(20)) + "\n\nSave anyway?",
                        "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
            }

            if (this.newFile == null)
            {
                using var dialog = new SaveFileDialog
                {
                    Title = "Save schedule as",
                    InitialDirectory = savePath == null ? null : Path.GetDirectoryName(savePath),
                    FileName = savePath == null ? "DYNASTY-EDIT" : Path.GetFileName(savePath) + "-EDIT",
                    Filter = "Dynasty saves|DYNASTY-*|All files|*.*",
                    OverwritePrompt = true,
                };

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                newFile = dialog.FileName;
            }

            try
            {
                var result = dynasty.Save(newFile);
                isDirty = false;
                savePath = newFile;
                status.Text = $"Saved {result.Destination} ({result.CompressedSize:N0} of {result.Budget:N0} bytes).";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string newFile = null;

        #endregion

        #region layout

        private void BuildMenu()
        {
            var menu = new MenuStrip();

            var file = new ToolStripMenuItem("&File");
            file.DropDownItems.Add(new ToolStripMenuItem("&Open save...", null, (s, e) => OpenSave(), Keys.Control | Keys.O));
            file.DropDownItems.Add(new ToolStripMenuItem("&Write schedule model", null, (s, e) => ReadSave(), Keys.Control | Keys.R));
            file.DropDownItems.Add(new ToolStripMenuItem("&Import NCAA 14 schedule...", null, (s, e) => ImportNcaa14Schedule(), Keys.Control | Keys.I));
            file.DropDownItems.Add(new ToolStripMenuItem("Import NCAA 14 &rosters...", null, (s, e) => ImportNcaa14Rosters(), Keys.Control | Keys.P));
            file.DropDownItems.Add(new ToolStripMenuItem("Import NCAA 14 &coaches...", null, (s, e) => ImportNcaa14Coaches(), Keys.Control | Keys.K));
            file.DropDownItems.Add(new ToolStripMenuItem("Import NCAA 14 &teams...", null, (s, e) => ImportNcaa14Teams(), Keys.Control | Keys.T));
            file.DropDownItems.Add(new ToolStripMenuItem("Save &as...", null, (s, e) => SaveAs(), Keys.Control | Keys.S));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (s, e) => Close()));

            var games = new ToolStripMenuItem("&Games");
            games.DropDownItems.Add(new ToolStripMenuItem("&Add game", null, (s, e) => AddGame(), Keys.Control | Keys.N));
            games.DropDownItems.Add(new ToolStripMenuItem("&Flip home and away", null, (s, e) => FlipSelected(), Keys.Control | Keys.F));
            games.DropDownItems.Add(new ToolStripSeparator());
            games.DropDownItems.Add(new ToolStripMenuItem("Re&fresh grid", null, (s, e) => { grid.EndEdit(); RefreshGrid(); }, Keys.Control | Keys.H));
            games.DropDownItems.Add(new ToolStripMenuItem("&Validate", null, (s, e) => ShowValidation()));

            var functions = new ToolStripMenuItem("F&unctions");
            functions.DropDownItems.Add(new ToolStripMenuItem("Read &Rankings", null, (s, e) => ReadRankings(), Keys.Control | Keys.B));
            functions.DropDownItems.Add(new ToolStripMenuItem("&Initialize season schedule", null, (s, e) => InitializeSeason(), Keys.Control | Keys.Z));
            functions.DropDownItems.Add(new ToolStripMenuItem("Smooth &G5 schedules", null, (s, e) => SmoothG5Schedules(), Keys.Control | Keys.G));

            menu.Items.Add(file);
            menu.Items.Add(games);
            menu.Items.Add(functions);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void BuildGrid()
        {
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoGenerateColumns = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            grid.RowHeadersWidth = 30;

            weekColumn = new DataGridViewTextBoxColumn { HeaderText = "Week", Width = 60, ValueType = typeof(int), MaxInputLength = 2 };
            dayColumn = ComboColumn("Day", 100);
            timeColumn = ComboColumn("Time", 160);
            awayColumn = ComboColumn("Away", 180);
            homeColumn = ComboColumn("Home", 180);
            stadiumColumn = ComboColumn("Stadium", 300);
            kickoffColumn = new DataGridViewCheckBoxColumn { HeaderText = "Kickoff", Width = 60 };
            conferenceColumn = new DataGridViewTextBoxColumn { HeaderText = "Conf", Width = 120, ReadOnly = true };

            grid.Columns.AddRange(homeColumn, awayColumn, weekColumn, stadiumColumn, kickoffColumn, dayColumn, timeColumn, conferenceColumn);

            // checkbox edits take effect right away rather than when the cell loses focus; a dropdown pick takes effect on
            // Enter or a click (typing only moves through the list), a typed week on Enter or leaving the cell
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewCheckBoxCell)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.CellValidating += OnCellValidating;
            grid.CellEndEdit += (s, e) => grid.Rows[e.RowIndex].ErrorText = string.Empty;
            ComboTypeAhead.Attach(grid);
            grid.CellValueChanged += OnCellValueChanged;
            grid.DataError += (s, e) => e.ThrowException = false;

            Controls.Add(grid);
        }

        private static DataGridViewComboBoxColumn ComboColumn(string header, int width) =>
            new DataGridViewComboBoxColumn
            {
                HeaderText = header,
                Width = width,
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing,
            };

        private sealed class Choice<T>
        {
            public Choice(T value, string name)
            {
                Value = value;
                Name = name;
            }

            public T Value { get; }
            public string Name { get; }
        }

        /// <summary>Teams, days, time slots and stadiums for the combo columns.</summary>
        private void SetColumnChoices()
        {
            var s = schedule!;

            dayColumn.DataSource = s.DayOptions.ToList();
            timeColumn.DataSource = s.TimeSlotOptions.ToList();

            var teams = s.TeamTable.Records
                .Where(r => !r.IsEmpty && !string.IsNullOrEmpty(r["DisplayName"] as string))
                .Select(r => new Choice<int>(r.Index, (string)r["DisplayName"]!))
                .OrderBy(t => t.Name)
                .ToList();
            foreach (var column in new[] { awayColumn, homeColumn })
            {
                column.ValueMember = nameof(Choice<int>.Value);
                column.DisplayMember = nameof(Choice<int>.Name);
                column.DataSource = teams.ToList();
            }

            stadiumColumn.ValueMember = nameof(Choice<long>.Value);
            stadiumColumn.DisplayMember = nameof(Choice<long>.Name);
            stadiumColumn.DataSource = StadiumChoices();
        }

        /// <summary>
        /// "(Home stadium)" first, then the neutral sites marked with "*" (the known ones plus any other non-home stadium
        /// the schedule already uses), then every team's home stadium. Each group is alphabetical.
        /// </summary>
        private List<Choice<long>> StadiumChoices()
        {
            var homes = new Dictionary<long, string>();
            foreach (var team in schedule!.TeamTable.Records.Where(r => !r.IsEmpty))
            {
                if (team["Stadium"] is not string bits || Convert.ToUInt32(bits, 2) == 0)
                {
                    continue;
                }

                var stadium = CFBStadium.FindHome(team["AssetName"] as string, team["ShortName"] as string);
                homes.TryAdd(Convert.ToUInt32(bits, 2), stadium?.Name ?? $"{team["DisplayName"]} home stadium");
            }

            var neutrals = new Dictionary<long, string>();
            // the generic and high-school stadiums aren't game venues (Oval Stadium crashed a game)
            foreach (var neutral in CFBStadium.NeutralSites.Where(n => !homes.ContainsKey(n.Key) && !CFBStadium.NonGameSites.Contains(n.Key)))
            {
                neutrals.TryAdd(neutral.Key, neutral.Value);
            }

            foreach (var game in schedule.Games.Where(g => g.StadiumId != 0 && !homes.ContainsKey(g.StadiumId)))
            {
                neutrals.TryAdd(game.StadiumId, $"Stadium 0x{game.StadiumId:X8}");
            }

            var choices = new List<Choice<long>> { new Choice<long>(0, "(Home stadium)") };
            choices.AddRange(neutrals.OrderBy(n => n.Value).Select(n => new Choice<long>(n.Key, "* " + n.Value)));
            choices.AddRange(homes.OrderBy(h => h.Value).Select(h => new Choice<long>(h.Key, h.Value)));
            return choices;
        }

        #endregion

        #region grid

        /// <summary>Ctrl+H. Rebuilds the grid sorted by week, then home team, and goes back to the top.</summary>
        private void RefreshGrid()
        {
            if (schedule == null)
            {
                return;
            }

            populating = true;
            grid.SuspendLayout();
            grid.Rows.Clear();
            foreach (var game in schedule.Games
                .OrderBy(g => g.Week)
                .ThenBy(g => schedule.TeamName(g.HomeTeamRow) ?? string.Empty))
            {
                AddRow(game);
            }
            grid.ResumeLayout();
            populating = false;

            HighlightConflicts();
            if (grid.Rows.Count > 0)
            {
                grid.FirstDisplayedScrollingRowIndex = 0;
                // the read-only column, so landing there doesn't open a cell for editing
                grid.CurrentCell = grid.Rows[0].Cells[conferenceColumn.Index];
            }

            UpdateStatus();
        }

        private void AddRow(ScheduledGame game)
        {
            int index = grid.Rows.Add();
            var row = grid.Rows[index];
            row.Tag = game;
            FillRow(row);
        }

        private void FillRow(DataGridViewRow row)
        {
            var game = (ScheduledGame)row.Tag!;
            bool wasPopulating = populating;
            populating = true;

            row.Cells[weekColumn.Index].Value = game.Week;
            row.Cells[dayColumn.Index].Value = game.DayOfWeek;
            row.Cells[timeColumn.Index].Value = game.TimeSlot;
            row.Cells[awayColumn.Index].Value = game.AwayTeamRow;
            row.Cells[homeColumn.Index].Value = game.HomeTeamRow;
            row.Cells[stadiumColumn.Index].Value = (long)game.StadiumId;
            row.Cells[kickoffColumn.Index].Value = game.IsKickoffGame;
            row.Cells[conferenceColumn.Index].Value = schedule!.IsConferenceGame(game) ? schedule.ConferenceOf(game.HomeTeamRow) : string.Empty;

            populating = wasPopulating;
        }

        /// <summary>A typed week must be a whole week of the regular season; anything else stays in the cell with an error.</summary>
        private void OnCellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != weekColumn.Index || populating)
            {
                return;
            }

            if (!int.TryParse(Convert.ToString(e.FormattedValue)?.Trim(), out var week) || week < 0 || week > LastRegularSeasonWeek)
            {
                grid.Rows[e.RowIndex].ErrorText = $"Week must be 0-{LastRegularSeasonWeek}.";
                e.Cancel = true;
            }
        }

        private void OnCellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (populating || e.RowIndex < 0 || grid.Rows[e.RowIndex].Tag is not ScheduledGame game)
            {
                return;
            }

            var value = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (value == null)
            {
                return;
            }

            string? swapNote = null;
            if (e.ColumnIndex == weekColumn.Index) game.Week = Convert.ToInt32(value);
            else if (e.ColumnIndex == dayColumn.Index) game.DayOfWeek = (string)value;
            else if (e.ColumnIndex == timeColumn.Index) game.TimeSlot = (string)value;
            else if (e.ColumnIndex == awayColumn.Index) game.SetAwayTeam(Convert.ToInt32(value));
            else if (e.ColumnIndex == homeColumn.Index) game.SetHomeTeam(Convert.ToInt32(value));
//            else if (e.ColumnIndex == awayColumn.Index) swapNote = SwapInTeam(game, game.AwayTeamRow, Convert.ToInt32(value));
  //          else if (e.ColumnIndex == homeColumn.Index) swapNote = SwapInTeam(game, game.HomeTeamRow, Convert.ToInt32(value));
            else if (e.ColumnIndex == stadiumColumn.Index) game.StadiumId = (uint)Convert.ToInt64(value);
            else if (e.ColumnIndex == kickoffColumn.Index) game.IsKickoffGame = (bool)value;
            else return;

            isDirty = true;

            // the conference label depends on the teams; the row keeps its place until Ctrl+H
            FillRow(grid.Rows[e.RowIndex]);
            HighlightConflicts();
            UpdateStatus();
            if (swapNote != null)
            {
                status.Text = swapNote;
            }
        }

        /// <summary>
        /// Puts <paramref name="newTeam"/> in <paramref name="game"/> in place of <paramref name="oldTeam"/>, and the old team
        /// takes the new team's place in the game it had that week (same side), so both games stay in their week. Nothing
        /// else changes when the new team has a bye, or is a generic FCS team (they play several games a week).
        /// Returns a note for the status bar when another game changed.
        /// </summary>
        private string? SwapInTeam(ScheduledGame game, int oldTeam, int newTeam)
        {
            var other = IsGenericFcsTeam(newTeam) || newTeam == oldTeam ? null
                : schedule!.Games.FirstOrDefault(g => g != game && g.Week == game.Week && (g.HomeTeamRow == newTeam || g.AwayTeamRow == newTeam));

            if (game.HomeTeamRow == oldTeam) game.SetHomeTeam(newTeam);
            else game.SetAwayTeam(newTeam);

            if (other == null)
            {
                return null;
            }

            if (other.HomeTeamRow == newTeam) other.SetHomeTeam(oldTeam);
            else other.SetAwayTeam(oldTeam);

            var otherRow = grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag == other);
            if (otherRow != null)
            {
                FillRow(otherRow);
            }

            return $"{schedule.TeamName(oldTeam)} takes {schedule.TeamName(newTeam)}'s place in week {game.Week}: " +
                $"{schedule.TeamName(other.AwayTeamRow)} at {schedule.TeamName(other.HomeTeamRow)}.";
        }

        /// <summary>
        /// Rows where a team plays twice in one week are shown in red. The game's generic FCS opponents (TeamIndex 255)
        /// legitimately play several games a week, so they don't count.
        /// </summary>
        private void HighlightConflicts()
        {
            var games = grid.Rows.Cast<DataGridViewRow>().Select(r => (Row: r, Game: (ScheduledGame)r.Tag!)).ToList();
            var doubleBooked = games
                .SelectMany(x => new[] { (Team: x.Game.HomeTeamRow, x.Game.Week), (Team: x.Game.AwayTeamRow, x.Game.Week) })
                .Where(x => x.Team >= 0 && !IsGenericFcsTeam(x.Team))
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var (row, game) in games)
            {
                bool conflict = doubleBooked.Contains((game.HomeTeamRow, game.Week)) || doubleBooked.Contains((game.AwayTeamRow, game.Week));
                row.DefaultCellStyle.BackColor = conflict ? Color.MistyRose : Color.Empty;
            }
        }

        private bool IsGenericFcsTeam(int teamRow) =>
            Convert.ToInt32(schedule!.TeamTable.Records[teamRow]["TeamIndex"]) == NoTeamIndex;

        private void SelectGame(ScheduledGame game)
        {
            var row = grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => ReferenceEquals(r.Tag, game));
            if (row != null)
            {
                grid.CurrentCell = row.Cells[weekColumn.Index];
            }
        }

        private void UpdateStatus()
        {
            if (schedule == null)
            {
                return;
            }

            int conflicts = grid.Rows.Cast<DataGridViewRow>().Count(r => r.DefaultCellStyle.BackColor == Color.MistyRose);
            status.Text = $"{(savePath == null ? "(no file)" : Path.GetFileName(savePath))}: {schedule.Games.Count} games" +
                (conflicts > 0 ? $", {conflicts} in conflict" : string.Empty) +
                (isDirty ? " - unsaved changes" : string.Empty) +
                "   |   Ctrl+H refresh, Ctrl+N add, Ctrl+F flip, Ctrl+S save as";
        }

        #endregion

        #region commands

        /// <summary>Adds a game at the bottom of the grid (it sorts into place on Ctrl+H), starting from the selected game's week and teams.</summary>
        private void AddGame()
        {
            if (schedule == null)
            {
                return;
            }

            grid.EndEdit();
            var template = grid.CurrentRow?.Tag as ScheduledGame;
            int home = template?.HomeTeamRow ?? schedule.Games[0].HomeTeamRow;
            int away = template?.AwayTeamRow ?? schedule.Games[0].AwayTeamRow;
            int week = template?.Week ?? 0;

            try
            {
                var game = schedule.AddGame(home, away, week);
                isDirty = true;
                AddRow(game);
                SelectGame(game);
                HighlightConflicts();
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Add game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FlipSelected()
        {
            grid.EndEdit();
            foreach (var row in grid.SelectedRows.Cast<DataGridViewRow>())
            {
                ((ScheduledGame)row.Tag!).FlipHomeAndAway();
                FillRow(row);
                isDirty = true;
            }

            HighlightConflicts();
            UpdateStatus();
        }

        private void ShowValidation()
        {
            if (schedule == null)
            {
                return;
            }

            grid.EndEdit();
            var problems = schedule.Validate();
            MessageBox.Show(this,
                problems.Count == 0 ? "No problems found." : string.Join("\n", problems.Take(40)),
                "Validate", MessageBoxButtons.OK, problems.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        #endregion

        private void OnFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (isDirty &&
                MessageBox.Show(this, "You have unsaved schedule changes. Close anyway?", "Schedule Maker",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}
