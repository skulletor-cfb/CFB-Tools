using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CFB27.Data.Model
{

    public class CFBStory : BaseRecord
    {
        public string EventContext { get; set; }
        public string Character1 { get; set; }
        public string Character2 { get; set; }
        public string Character3 { get; set; }
        public string Character4 { get; set; }
        public string Team { get; set; }
        public int TagHash { get; set; }
        public string Tag { get; set; }
        public int HeaderHash { get; set; }
        public string Header { get; set; }
        public string FullStory { get; set; }
        public string HubPanelType3 { get; set; }
        public string HubPanelData5 { get; set; }
        public int Identity { get; set; }
        public string CurrentStage { get; set; }
        public string HubPanelData2 { get; set; }
        public string HubPanelData3 { get; set; }
        public string HubPanelData4 { get; set; }
        public bool IsTopStory { get; set; }
        public int CurrentWeek { get; set; }
        public int Priority { get; set; }
        public int ShowWeek { get; set; }
        public string HubPanelData1 { get; set; }
        public bool IsInfoSentience { get; set; }
        public bool IsNew { get; set; }
        public int SeasonYear { get; set; }
        public string HubPanelType5 { get; set; }
        public string HubPanelType4 { get; set; }
        public string HubPanelType1 { get; set; }
        public string HubPanelType2 { get; set; }
        public bool IsBreaking { get; set; }
        public string Category { get; set; }
        public int SeasonWeek { get; set; }

        [JsonIgnore]
        public int TeamId => this.Team.ToRowId();
    }
}