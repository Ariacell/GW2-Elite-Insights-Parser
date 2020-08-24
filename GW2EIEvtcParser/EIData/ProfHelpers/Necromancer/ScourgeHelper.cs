﻿using GW2EIEvtcParser.ParsedData;
using System.Collections.Generic;
using System.Linq;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    internal class ScourgeHelper : NecromancerHelper
    {

        internal static readonly List<InstantCastFinder> ScourgeInstantCastFinders = new List<InstantCastFinder>()
        {
            // Trail of Anguish
        };

        internal static readonly List<Buff> ScourgeBuffs = new List<Buff>
        {    
                new Buff("Sadistic Searing",43626, ParserHelper.Source.Scourge, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/dd/Sadistic_Searing.png"),
        };
    }
}
