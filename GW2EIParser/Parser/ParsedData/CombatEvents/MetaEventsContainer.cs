﻿using System.Collections.Generic;

namespace GW2EIParser.Parser.ParsedData.CombatEvents
{
    public class MetaEventsContainer
    {
        public List<BuildEvent> BuildEvents { get; } = new List<BuildEvent>();
        public List<LanguageEvent> LanguageEvents { get; } = new List<LanguageEvent>();
        public List<LogEndEvent> LogEndEvents { get; } = new List<LogEndEvent>();
        public List<LogStartEvent> LogStartEvents { get; } = new List<LogStartEvent>();
        public List<MapIDEvent> MapIDEvents { get; } = new List<MapIDEvent>();
        public List<ShardEvent> ShardEvents { get; } = new List<ShardEvent>();
        public List<PointOfViewEvent> PointOfViewEvents { get; } = new List<PointOfViewEvent>();
        public Dictionary<AgentItem, List<GuildEvent>> GuildEvents { get; } = new Dictionary<AgentItem, List<GuildEvent>>();
        public Dictionary<long, BuffInfoEvent> BuffInfoEvents { get; } = new Dictionary<long, BuffInfoEvent>();
        public Dictionary<ParseEnum.BuffCategory, List<BuffInfoEvent>> BuffInfoEventsByCategory { get; } = new Dictionary<ParseEnum.BuffCategory, List<BuffInfoEvent>>();
    }
}
