﻿using GW2EIEvtcParser.ParsedData;
using System.Collections.Generic;
using System.Linq;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    internal class SoulbeastHelper : RangerHelper
    {
        internal static readonly List<InstantCastFinder> SoulbeastInstantCastFinders = new List<InstantCastFinder>()
        {
            // Stout
            new BuffGainCastFinder(42944,40272,InstantCastFinder.DefaultICD), // Beastmode
            new BuffLossCastFinder(43014,40272,InstantCastFinder.DefaultICD), // Leave Beastmode
            // Deadly
            new BuffGainCastFinder(42944,44932,InstantCastFinder.DefaultICD), // Beastmode
            new BuffLossCastFinder(43014,44932,InstantCastFinder.DefaultICD), // Leave Beastmode
            // Versatile
            new BuffGainCastFinder(42944,44693,InstantCastFinder.DefaultICD), // Beastmode
            new BuffLossCastFinder(43014,44693,InstantCastFinder.DefaultICD), // Leave Beastmode
            // Ferocious
            new BuffGainCastFinder(42944,41720,InstantCastFinder.DefaultICD), // Beastmode
            new BuffLossCastFinder(43014,41720,InstantCastFinder.DefaultICD), // Leave Beastmode
            // Supportive
            new BuffGainCastFinder(42944,40069,InstantCastFinder.DefaultICD), // Beastmode
            new BuffLossCastFinder(43014,40069,InstantCastFinder.DefaultICD), // Leave Beastmode
            // 
            new BuffGiveCastFinder(45789,41815,InstantCastFinder.DefaultICD), // Dolyak Stance
            new BuffGiveCastFinder(45970,45038,InstantCastFinder.DefaultICD), // Moa Stance
            new BuffGiveCastFinder(40498,44651,InstantCastFinder.DefaultICD), // Vulture Stance
        };

        internal static readonly List<Buff> SoulbeastBuffs = new List<Buff>
        {
                new Buff("Dolyak Stance",41815, ParserHelper.Source.Soulbeast, BuffStackType.Queue, 25, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/7/71/Dolyak_Stance.png"),
                new Buff("Griffon Stance",46280, ParserHelper.Source.Soulbeast, BuffStackType.Queue, 25, BuffNature.SupportBuffTable, "https://wiki.guildwars2.com/images/9/98/Griffon_Stance.png"),
                new Buff("Moa Stance",45038, ParserHelper.Source.Soulbeast, BuffStackType.Queue, 25, BuffNature.SupportBuffTable, "https://wiki.guildwars2.com/images/6/66/Moa_Stance.png"),
                new Buff("Vulture Stance",44651, ParserHelper.Source.Soulbeast, BuffStackType.Queue, 25, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/8/8f/Vulture_Stance.png"),
                new Buff("Bear Stance",40045, ParserHelper.Source.Soulbeast, BuffStackType.Queue, 25, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/f/f0/Bear_Stance.png"),
                new Buff("One Wolf Pack",44139, ParserHelper.Source.Soulbeast, BuffStackType.Queue, 25, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/3/3b/One_Wolf_Pack.png"),
                new Buff("Deadly",44932, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/9/94/Deadly_%28Archetype%29.png"),
                new Buff("Ferocious",41720, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/e/e9/Ferocious_%28Archetype%29.png"),
                new Buff("Supportive",40069, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/3/32/Supportive_%28Archetype%29.png"),
                new Buff("Versatile",44693, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/bb/Versatile_%28Archetype%29.png"),
                new Buff("Stout",40272, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/8/80/Stout_%28Archetype%29.png"),
                new Buff("Unstoppable Union",44439, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/b2/Unstoppable_Union.png"),
                new Buff("Twice as Vicious",45600, ParserHelper.Source.Soulbeast, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/0/00/Twice_as_Vicious.png"),
        };

    }
}
