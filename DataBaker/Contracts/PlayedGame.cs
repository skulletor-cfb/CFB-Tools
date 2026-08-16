using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public interface IPlayedGame
    {
        int Year { get; set; }

        bool IsWinningTeam(int teamId);

        int Week { get; set; }

        string Score { get; set; }
    }

    public class PlayedGame : IPlayedGame
    {
        //    Week,Game,Location,OppId,Opponent,Result,Score,TeamId,TeamName,BowlId
        public int Week { get; set; }
        public int Game { get; set; }
        public string Location { get; set; }
        public int OppId { get; set; }
        public string Opponent { get; set; }
        public string Score { get; set; }
        public int? BowlId { get; set; }
        public string Team { get; set; }
        public int TeamId { get; set; }
        public int Year { get; set; }
        public bool WonGame { get; set; }
        public bool IsKickoff
        {
            get
            {
                var bowlId = this.BowlId.HasValue ? this.BowlId.Value : -1;
                if (Season.ClassicGames.Contains(bowlId))
                {
                    return true;
                }

                return this.Week <= 2 && Season.kickoffGames.Contains(bowlId);
            }
        }

        public bool IsWinningTeam(int teamId) { return WonGame; }

        public static bool MatchBowlId(PlayedGame s, int bowlId)
        {
            return s != null && bowlId == s.BowlId;
        }

        public static bool MatchTeamForPostSeasion(PlayedGame s, int teamId, Season season, Func<PlayedGame, bool> filter)
        {
            if (s == null)
                return false;


            if (s.TeamId == teamId && filter(s))
            {
                s.Year = season.Year;
                return true;
            }

            return false;
        }

        public static PlayedGame Generate(string[] parts, int year = 0)
        {
            if (parts[0] == string.Empty)
                return null;

            var pg = new PlayedGame
            {
                Week = parts[0].ToInt(),
                Game = parts[1].ToInt(),
                Location = parts[2],
                OppId = parts[3].ToInt(),
                Opponent = parts[4],
                WonGame = string.Equals(parts[5], "Win", StringComparison.OrdinalIgnoreCase),
                Score = parts[6],
                TeamId = parts[7].ToInt(),
                Team = parts[8],
                BowlId = parts[9].ToNullableInt(),
                Year = year
            };

            // special case godaddy.com bowl
            if (pg.BowlId.HasValue == false && pg.Week > 16)
            {
                pg.BowlId = 0;
            }

            return pg;
        }
    }
}