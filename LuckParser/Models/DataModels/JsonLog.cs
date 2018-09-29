﻿using System;
using System.Collections.Generic;
using LuckParser.Models.ParseModels;

namespace LuckParser.Models.DataModels
{
    class JsonLog
    {

        public class MechanicDesc
        {
            public string PlotlySymbol;
            public string PlotlyColor;
            public string Description;
            public string PlotlyName;
        }

        public class JsonExtraLog
        {
            public Dictionary<long, string> BuffIcons = null;
            public Dictionary<long, string> SkillIcons = null;
            public string FightIcon = null;
            public Dictionary<string, string> GeneralIcons = null;
            public Dictionary<string, MechanicDesc> MechanicData = null;
        }

        public string EliteInsightsVersion;
        public int TriggerID;
        public string FightName;
        public string ArcVersion;
        public string RecordedBy;
        public string TimeStart;
        public string TimeEnd;
        public string Duration;
        public int Success;
        public List<JsonBoss> Boss;
        public List<JsonPlayer> Players;
        public List<JsonPhase> Phases;
        public List<JsonMechanic> Mechanics;
        public string[] UploadLinks;
        public Dictionary<long, string> SkillNames;
        public Dictionary<long, string> BuffNames;
        public JsonExtraLog ED = null;
    }
}