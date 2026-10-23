using System.Collections.Generic;

namespace EA_DB_Editor
{
    public interface IDataEngine
    {
        /// <summary>
        /// Whether or not to skip a historic record
        /// </summary>
        bool SkipFirstHistoricRecord { get; }
        /// <summary>
        /// the number of teams in dynasty
        /// </summary>
        int TeamCount { get; }

        // map of team ids to names
        Dictionary<int, string> TeamNames { get; }

        /// <summary>
        /// returns an annual list of bowl champions
        /// </summary>
        /// <param name="bowlChampions"></param>
        /// <returns>returns the current year</returns>
        void ReadBowlChampions(Dictionary<string, BowlChampion> bowlChampions);

        /// <summary>
        /// checks if the current season is over
        /// </summary>
        /// <returns>return true if it is over</returns>
        bool IsSeasonOver();

        /// <summary>
        /// Reads metadata about bowls
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Bowl> CreateBowlTable();

        /// <summary>
        /// creates the team schedule
        /// </summary>
        /// <returns></returns>
        Dictionary<int, TeamSchedule> CreateTeamSchedule(bool isPreseason);

        /// <summary>
        /// returns the list of all americans
        /// </summary>
        /// <returns></returns>
        List<AllAmerican> CreateAllAmericans();

        /// <summary>
        /// Read the database for players
        /// </summary>
        /// <param name="Rosters"></param>
        /// <param name="Players"></param>
        void CreatePlayers(Dictionary<int, List<Player>> Rosters, Dictionary<int, Player> Players);

        /// <summary>
        /// get the stats
        /// </summary>
        void ReadStats();

        /// <summary>
        /// reads the conference metadata
        /// </summary>
        /// <returns></returns>
        Dictionary<int, Conference> ReadConferenceMetadata();

        /// <summary>
        /// reads team metadata
        /// </summary>
        /// <param name="isPreseason"></param>
        /// <returns></returns>
        Dictionary<int, Team> ReadTeams(bool isPreseason);

        /// <summary>
        /// Reads coach data
        /// </summary>
        /// <returns></returns>
        /// <param name="isPreseason">building the preseason magazine</param>
        Dictionary<string, Coach> ReadCoaches(bool isPreseason = false);

        /// <summary>
        /// reads the historic records for a team
        /// </summary>
        /// <returns></returns>
        Dictionary<int, Dictionary<int, TeamSeasonRecord>> CreateTeamHistoricRecords();

        /// <summary>
        /// reads the game schedule
        /// </summary>
        /// <param name="isPreseason"></param>
        /// <returns></returns>
        Dictionary<string, ScheduledGame> ReadSchedule(bool isPreseason);

        /// <summary>
        /// read records for a schools
        /// </summary>
        /// <param name="recreateUsingRecordsFile"></param>
        /// <returns></returns>
        Dictionary<int, List<Record>> ReadSchoolRecords(bool recreateUsingRecordsFile);

        /// <summary>
        /// read stats for games
        /// </summary>
        /// <param name="games"></param>
        void ReadGameStats(Dictionary<string, ScheduledGame> games);

        /// <summary>
        /// returns the list of conference champs
        /// </summary>
        /// <returns></returns>
        List<ConferenceChampion> ReadConferenceChamps();

        /// <summary>
        /// read stadiums data
        /// </summary>
        /// <returns></returns>
        Dictionary<int, Stadium> ReadStadiums();

        /// <summary>
        /// read the recruit classes
        /// </summary>
        /// <returns></returns>
        Dictionary<int, RecruitClassRanking> ReadRecruitClasses();

        /// <summary>
        /// read recruits from the dynasty
        /// </summary>
        /// <returns></returns>
        Dictionary<int, Recruit> ReadRecruits();

        /// <summary>
        /// Read awards from db
        /// </summary>
        /// <returns></returns>
        Dictionary<int, List<Award>> ReadAwards();

        /// <summary>
        /// calculates the number of free roster spots
        /// </summary>
        /// <param name="ranking"></param>
        /// <returns></returns>
        int CalculateRosterSpots(RecruitClassRanking ranking);

        /// <summary>
        /// Reads the ncaa record book
        /// </summary>
        /// <returns></returns>
        List<Record> ReadNcaaRecords();

        /// <summary>
        /// read stats for a team
        /// </summary>
        /// <returns></returns>
        Dictionary<int, TeamStat> ReadTeamStats();

        /// <summary>
        /// writes bowl/kickoff game records
        /// </summary>
        void CommitTeamRecords();

        /// <summary>
        /// reads the draft history table
        /// </summary>
        /// <returns></returns>
        Dictionary<int, DraftClass[]> ReadDraftHistory();

        /// <summary>
        /// reads media coverage about teams
        /// </summary>
        /// <returns></returns>
        Dictionary<int, MediaCoverage[]> ReadMediaCoverage();

        /// <summary>
        /// reads team depth charts
        /// </summary>
        /// <returns></returns>
        Dictionary<int, Dictionary<int, DepthChartPosition[]>> ReadDepthCharts();

        /// <summary>
        /// read coaches on hot seat
        /// </summary>
        /// <returns></returns>
        Dictionary<int, int> FindCoachesOnHotSeat();

        /// <summary>
        /// reads the name of a stadium
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        string ReadStadiumName(int siteId);
    }
}
