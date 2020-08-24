﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    internal class GuardianHelper : ProfHelper
    {
        internal static readonly List<InstantCastFinder> GuardianInstantCastFinders = new List<InstantCastFinder>()
        {
            new BuffGainCastFinder(9082, 9123, InstantCastFinder.DefaultICD), // Shield of Wrath
            new BuffGainCastFinder(9104, 9103, 0), // Zealot's Flame
            //new BuffLossCastFinder(9115,9114,InstantCastFinder.DefaultICD), // Virtue of Justice
            //new BuffLossCastFinder(9120,9119,InstantCastFinder.DefaultICD), // Virtue of Resolve
            //new BuffLossCastFinder(9118,9113,InstantCastFinder.DefaultICD), // Virtue of Courage
            new DamageCastFinder(9247,9247, InstantCastFinder.DefaultICD), // Judge's Intervention
            new DamageCastFinder(21795, 21795, InstantCastFinder.DefaultICD), // Glacial Heart
            new DamageCastFinder(22499, 22499, InstantCastFinder.DefaultICD), // Shattered Aegis
        };

        internal static readonly List<Buff> GuardianBuffs = new List<Buff>
        {        
                //skills
                new Buff("Zealot's Flame", 9103, ParserHelper.Source.Guardian, BuffStackType.Queue, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/7/7a/Zealot%27s_Flame.png"),
                new Buff("Purging Flames",21672, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/2/28/Purging_Flames.png"),
                new Buff("Litany of Wrath",21665, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/4/4a/Litany_of_Wrath.png"),
                new Buff("Renewed Focus",9255, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/1/10/Renewed_Focus.png"),
                new Buff("Shield of Wrath",9123, ParserHelper.Source.Guardian, BuffStackType.Stacking, 3, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/bc/Shield_of_Wrath.png"),
                new Buff("Binding Blade (Self)",9225, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/de/Binding_Blade.png"),
                new Buff("Binding Blade",9148, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/de/Binding_Blade.png"),
                //signets
                new Buff("Signet of Resolve",9220, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/7/75/Signet_of_Resolve.png"),
                new Buff("Signet of Resolve (Shared)", 46554, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/7/75/Signet_of_Resolve.png"),
                new Buff("Bane Signet",9092, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/5/56/Bane_Signet.png"),
                new Buff("Bane Signet (PI)",9240, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/5/56/Bane_Signet.png"),
                new Buff("Signet of Judgment",9156, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/f/fe/Signet_of_Judgment.png"),
                new Buff("Signet of Judgment (PI)",9239, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/f/fe/Signet_of_Judgment.png"),
                new Buff("Signet of Mercy",9162, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/3/37/Signet_of_Mercy.png"),
                new Buff("Signet of Mercy (PI)",9238, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/3/37/Signet_of_Mercy.png"),
                new Buff("Signet of Wrath",9100, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/1/18/Signet_of_Wrath.png"),
                new Buff("Signet of Wrath (PI)",9237, ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/1/18/Signet_of_Wrath.png"),
                new Buff("Signet of Courage",29633, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/5/5d/Signet_of_Courage.png"),
                new Buff("Signet of Courage (Shared)",43487 , ParserHelper.Source.Guardian, BuffStackType.Stacking, 25, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/5/5d/Signet_of_Courage.png"),
                //virtues
                new Buff("Virtue of Justice", 9114, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/1/10/Virtue_of_Justice.png"),
                new Buff("Virtue of Courage", 9113, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/a/a9/Virtue_of_Courage.png"),
                new Buff("Virtue of Resolve", 9119, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/b2/Virtue_of_Resolve.png"),
                //traits
                new Buff("Strength in Numbers",13796, ParserHelper.Source.Guardian, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/7/7b/Strength_in_Numbers.png"),
                new Buff("Invigorated Bulwark",30207, ParserHelper.Source.Guardian, BuffStackType.Stacking, 5, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/0/00/Invigorated_Bulwark.png"),
                new Buff("Battle Presence", 17046, ParserHelper.Source.Guardian, BuffStackType.Queue, 2, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/2/27/Battle_Presence.png"),
                new Buff("Symbolic Avenger",56890, ParserHelper.Source.Guardian, BuffStackType.Stacking, 5, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/e/e5/Symbolic_Avenger.png", 97950, ulong.MaxValue),
                new Buff("Inspiring Virtue",59592, ParserHelper.Source.Guardian, BuffStackType.Queue, 99, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/8/8f/Virtuous_Solace.png", 102321, 102389),
                new Buff("Inspiring Virtue",59592, ParserHelper.Source.Guardian, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/8/8f/Virtuous_Solace.png", 102389, ulong.MaxValue),
        };

    }
}
