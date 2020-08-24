﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    internal class NecromancerHelper : ProfHelper
    {
        internal static readonly List<InstantCastFinder> NecromancerInstantCastFinders = new List<InstantCastFinder>()
        {
            new BuffGainCastFinder(10574, 790, InstantCastFinder.DefaultICD), // Death shroud
            new BuffLossCastFinder(10585, 790, InstantCastFinder.DefaultICD), // Death shroud
            new DamageCastFinder(29560, 29560, InstantCastFinder.DefaultICD), // Spiteful Spirit
            new DamageCastFinder(13907, 13907, InstantCastFinder.DefaultICD), // Lesser Enfeeble
            new DamageCastFinder(13906, 13906, InstantCastFinder.DefaultICD), // Lesser Spinal Shivers
            new BuffGainCastFinder(10583, 10582, InstantCastFinder.DefaultICD), // Spectral Armor
            new BuffGainCastFinder(10685, 15083, InstantCastFinder.DefaultICD, 0, 94051), // Spectral Walk
            new BuffGainCastFinder(10685, 53476, InstantCastFinder.DefaultICD, 94051, ulong.MaxValue), // Spectral Walk
            //new BuffGainCastFinder(10635,???, 80647, ulong.MaxValue), // Lich's Gaze
        };

        internal static readonly List<Buff> NecromancerBuffs = new List<Buff>
        {     
                //forms
                new Buff("Lich Form",10631, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/a/ab/Lich_Form.png"),
                new Buff("Death Shroud", 790, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/f/f5/Death_Shroud.png"),
                //signets
                new Buff("Signet of Vampirism (Passive)",21761, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/7/73/Signet_of_Vampirism.png"),
                new Buff("Signet of Vampirism (Active)",21766, ParserHelper.Source.Necromancer, BuffStackType.Stacking, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/7/73/Signet_of_Vampirism.png"),
                new Buff("Signet of Vampirism (Shroud)",43885, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/7/73/Signet_of_Vampirism.png"),
                new Buff("Plague Signet (Passive)",10630, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/c/c5/Plague_Signet.png"),
                new Buff("Plague Signet (Shroud)",44164, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/c/c5/Plague_Signet.png"),
                new Buff("Signet of Spite (Passive)",10621, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/df/Signet_of_Spite.png"),
                new Buff("Signet of Spite (Shroud)",43772, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/df/Signet_of_Spite.png"),
                new Buff("Signet of the Locust (Passive)",10614, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/a/a3/Signet_of_the_Locust.png"),
                new Buff("Signet of the Locust (Shroud)",40283, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/a/a3/Signet_of_the_Locust.png"),
                new Buff("Signet of Undeath (Passive)",10610, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/9/9c/Signet_of_Undeath.png"),
                new Buff("Signet of Undeath (Shroud)",40583, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/9/9c/Signet_of_Undeath.png"),
                //skills
                new Buff("Spectral Walk",15083, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/3/33/Spectral_Walk.png", 0, 94051),
                new Buff("Spectral Walk",53476, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/3/33/Spectral_Walk.png", 94051, ulong.MaxValue),
                new Buff("Spectral Armor",10582, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/d1/Spectral_Armor.png"),
                new Buff("Infusing Terror", 30129, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/db/Infusing_Terror.png"),
                new Buff("Locust Swarm", 10567, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/7/77/Locust_Swarm.png"),
                //new Boon("Sand Cascade", 43759, BoonSource.Necromancer, BoonType.Duration, 1, BoonNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/1/1e/Sand_Cascade.png"),
                //traits
                new Buff("Corrupter's Defense",30845, ParserHelper.Source.Necromancer, BuffStackType.Stacking, 10, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/1/11/Corrupter%27s_Fervor.png", 0, 99526),
                new Buff("Death's Carapace",30845, ParserHelper.Source.Necromancer, BuffStackType.Stacking, 30, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/5/58/Death%27s_Carapace.png", 99526, ulong.MaxValue),
                new Buff("Flesh of the Master",13810, ParserHelper.Source.Necromancer, BuffStackType.Stacking, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/e/e9/Flesh_of_the_Master.png", 0, 99526),
                new Buff("Vampiric Aura", 30285, ParserHelper.Source.Necromancer, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/d/da/Vampiric_Presence.png"),
                new Buff("Vampiric Strikes", 30398, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/da/Vampiric_Presence.png"),
                new Buff("Last Rites",29726, ParserHelper.Source.Necromancer, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/1/1a/Last_Rites_%28effect%29.png"),
                new Buff("Soul Barbs",53489, ParserHelper.Source.Necromancer, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/bd/Soul_Barbs.png"),
        };

        private static readonly HashSet<long> _shroudTransform = new HashSet<long>
        {
            10574,10585,30792, 30961,
        };

        public static bool IsShroudTransform(long id)
        {
            return _shroudTransform.Contains(id);
        }
    }
}
