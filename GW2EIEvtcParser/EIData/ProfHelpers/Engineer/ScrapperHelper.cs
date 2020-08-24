﻿using GW2EIEvtcParser.ParsedData;
using System.Collections.Generic;
using System.Linq;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;
using static GW2EIEvtcParser.EIData.DamageModifier;

namespace GW2EIEvtcParser.EIData
{
    internal class ScrapperHelper
    {
        internal static readonly List<InstantCastFinder> InstantCastFinder = new List<InstantCastFinder>()
        {
        };

        internal static readonly List<DamageModifier> DamageMods = new List<DamageModifier>
        {
        };

        internal static readonly List<Buff> Buffs = new List<Buff>
        {
                new Buff("Watchful Eye",31229, ParserHelper.Source.Scrapper, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/2/29/Bulwark_Gyro.png"),
                new Buff("Watchful Eye PvP",46910, ParserHelper.Source.Scrapper, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/2/29/Bulwark_Gyro.png"),

        };
    }
}
