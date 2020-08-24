﻿using GW2EIEvtcParser.ParsedData;
using System.Collections.Generic;
using System.Linq;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    internal class SpellbreakerHelper : WarriorHelper
    {
        /////////////////////
        internal static readonly List<InstantCastFinder> SpellbreakerInstantCastFinders = new List<InstantCastFinder>()
        {
            new BuffGainCastFinder(43745, 40616, InstantCastFinder.DefaultICD), // Sight beyond Sight
            new DamageCastFinder(45534, 45534, InstantCastFinder.DefaultICD), // Loss Aversion

        };

        internal static readonly List<Buff> SpellbreakerBuffs = new List<Buff>
        {
                new Buff("Sight beyond Sight",40616, ParserHelper.Source.Spellbreaker, BuffNature.GraphOnlyBuff,"https://wiki.guildwars2.com/images/d/d7/Sight_beyond_Sight.png"),
                new Buff("Full Counter",43949, ParserHelper.Source.Spellbreaker, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/f/fb/Full_Counter.png"),
                new Buff("Attacker's Insight",41963, ParserHelper.Source.Spellbreaker, BuffStackType.Stacking, 5, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/8/89/Attacker%27s_Insight.png"),
        };


    }
}
