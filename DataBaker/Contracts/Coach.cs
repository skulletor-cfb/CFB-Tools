using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DataBaker.Contracts
{
    [JsonObject]
    public class Coach
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Age")]
        public int Age { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AllAmericans")]
        public int AllAmericans { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AlmaMaterId")]
        public int AlmaMaterId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AlmaMaterName")]
        public string AlmaMaterName { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BowlWins")]
        public int BowlWins { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CareerConferenceChampionships")]
        public int CareerConferenceChampionships { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CareerLoss")]
        public int CareerLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CareerNationalChampionships")]
        public int CareerNationalChampionships { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CareerRecord")]
        public string CareerRecord { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CareerWin")]
        public int CareerWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachBowlLoss")]
        public int CoachBowlLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachBowlWin")]
        public int CoachBowlWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachOfYearAwards")]
        public int CoachOfYearAwards { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "ConferenceChampionships")]
        public int ConferenceChampionships { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DefPlaybookId")]
        public int DefPlaybookId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HeismanWinners")]
        public int HeismanWinners { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Id")]
        public int Id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "LastName")]
        public string LastName { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "LongestWinStreak")]
        public int LongestWinStreak { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "NationalChampionships")]
        public int NationalChampionships { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "OffPlaybookId")]
        public int OffPlaybookId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Position")]
        public int Position { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RivalLoss")]
        public int RivalLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RivalWin")]
        public int RivalWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamLoss")]
        public int TeamLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRecord")]
        public string TeamRecord { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamWin")]
        public int TeamWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Top25Classes")]
        public int Top25Classes { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Top25Loss")]
        public int Top25Loss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Top25Win")]
        public int Top25Win { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "WinningSeasons")]
        public int WinningSeasons { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "YearsAsHeadCoach")]
        public int YearsAsHeadCoach { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "YearsWithTeam")]
        public int YearsWithTeam { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "WinPct")]
        public int WinPct
        {
            get
            {
                if ((CareerWin + CareerLoss) == 0) return 0;
                return (CareerWin * 1000) / (CareerWin + CareerLoss);
            }

            set { }
        }

        public int TeamId { get; set; }

        public int Year { get; set; }

        public static Coach Generate(int year, Coach coach)
        {
            coach.Year = year;
            return coach;
        }

        public string Job
        {
            get
            {
                switch (this.Position)
                {
                    case 0:
                        return "HC";
                    case 1:
                        return "OC";
                    case 2:
                        return "DC";
                }

                return string.Empty;
            }
        }
    }

    public class CoachKey
    {
        public CoachKey() { }
        public CoachKey(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode() ^ this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CoachKey;
            return other != null && other.Id == this.Id && string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
