using NCAA14Utility;

namespace TVSlots
{
    public partial class Form1 : Form
    {
        private const string file = @"D:\repos\CFB-Tools\Release\schedulemodel.txt";
        private static readonly Color AlternateWeekColor = Color.FromArgb(198, 231, 245);
        private DateTime lastEdit = DateTime.MinValue;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.AllowUserToAddRows = false;
            // widen each column to fit its longest game text (and header)
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ThreadPool.QueueUserWorkItem(Loop, null);
        }

        private void Loop(object state)
        {
            while (true)
            {
                Thread.Sleep(2000);

                if (!File.Exists(file))
                    continue;

                var lastWrite = File.GetLastWriteTimeUtc(file);

                if (lastWrite > this.lastEdit)
                {
                    this.lastEdit = lastWrite;

                    this.Invoke(new Action(() =>
                    {
                        var scheduleModel = file.ReadJsonFile<ScheduleModel>();
                        FillGrid(scheduleModel);
                    }));
                }
            }
        }

        private void FillGrid(ScheduleModel scheduleModel)
        {
            // column order matches the designer: Week, SEC, ACC, Big 10, Pac 12, Big 12
            var conferences = new[]
            {
                GamesPerConference(scheduleModel, 11),
                GamesPerConference(scheduleModel, 0),
                GamesPerConference(scheduleModel, 1),
                GamesPerConference(scheduleModel, 10),
                GamesPerConference(scheduleModel, 2),
            };

            var maxWeek = scheduleModel.TeamSchedules
                .SelectMany(ts => ts.ScheduleNodes)
                .Select(n => n.Week)
                .DefaultIfEmpty(0)
                .Max();

            dataGridView1.SuspendLayout();
            dataGridView1.Rows.Clear();

            for (int week = 1; week <= maxWeek; week++)
            {
                var weekGames = conferences
                    .Select(c => c.TryGetValue(week, out var games) ? games : new List<string>())
                    .ToArray();

                // one row per game for the busiest conference that week, at least one row per week
                var rowCount = Math.Max(1, weekGames.Max(g => g.Count));
                var backColor = week % 2 == 1 ? AlternateWeekColor : Color.White;

                for (int r = 0; r < rowCount; r++)
                {
                    var cells = new List<string> { r == 0 ? week.ToString() : string.Empty };
                    cells.AddRange(weekGames.Select(g => r < g.Count ? g[r] : string.Empty));

                    var idx = dataGridView1.Rows.Add(cells.ToArray());
                    dataGridView1.Rows[idx].DefaultCellStyle.BackColor = backColor;
                }
            }

            dataGridView1.ResumeLayout();
        }

        private Dictionary<int, List<string>> GamesPerConference(ScheduleModel schedule, int confId)
        {
            var teamsById = schedule.TeamSchedules.ToDictionary(ts => ts.TeamId);

            return schedule.TeamSchedules
                .Where(ts => ts.ConferenceId == confId)
                .SelectMany(ts => ts.ScheduleNodes
                    .Where(g => g.IsHomeGame)
                    .Where(g => g.IsP5Game)
                    .Where(g => g.Day == 5)
                    .Select(g => new { Home = ts, Game = g }))
                .GroupBy(x => x.Game.Week)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp
                        .OrderBy(x => x.Game.Day)
                        .ThenBy(x => x.Home.TeamName)
                        .Select(x => $"{ShortName(teamsById, x.Game.OpponentId, x.Game.Opponent)} at {ShortName(x.Home)}")
                        .ToList());
        }

        private static string ShortName(TeamSeasonSchedule team)
        {
            return string.IsNullOrEmpty(team.TeamAbbreviation) ? team.TeamName : team.TeamAbbreviation;
        }

        private static string ShortName(Dictionary<int, TeamSeasonSchedule> teamsById, int teamId, string fallback)
        {
            return teamsById.TryGetValue(teamId, out var team) ? ShortName(team) : fallback;
        }
    }
}
