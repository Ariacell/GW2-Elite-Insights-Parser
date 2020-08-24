﻿using GW2EIEvtcParser.ParsedData;
using System.Collections.Generic;
using System.Linq;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData
{
    internal class WarriorHelper : ProfHelper
    {

        internal static readonly List<InstantCastFinder> WarriorInstantCastFinders = new List<InstantCastFinder>()
        {
            new DamageCastFinder(14268, 14268, InstantCastFinder.DefaultICD, 84794, ulong.MaxValue), // Reckless Impact
            new BuffGainCastFinder(45534, 45534, InstantCastFinder.DefaultICD), // Berserker Stance
        };

        private static HashSet<AgentItem> GetBannerAgents(Dictionary<long, List<AbstractBuffEvent>> buffData, long id, HashSet<AgentItem> playerAgents)
        {
            if (buffData.TryGetValue(id, out List<AbstractBuffEvent> list))
            {
                return new HashSet<AgentItem>(list.Where(x => x is BuffApplyEvent && x.By.Type == AgentItem.AgentType.Gadget && x.By.Master == null && playerAgents.Contains(x.To.GetFinalMaster())).Select(x => x.By));
            }
            return new HashSet<AgentItem>();
        }

        internal static readonly List<Buff> WarriorBuffs = new List<Buff>
        {
            //skills
                new Buff("Riposte",14434, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff,"https://wiki.guildwars2.com/images/d/de/Riposte.png"),
                //signets
                new Buff("Healing Signet",786, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff,"https://wiki.guildwars2.com/images/8/85/Healing_Signet.png"),
                new Buff("Dolyak Signet",14458, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/6/60/Dolyak_Signet.png"),
                new Buff("Signet of Fury",14459, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/c/c1/Signet_of_Fury.png"),
                new Buff("Signet of Might",14444, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/4/40/Signet_of_Might.png"),
                new Buff("Signet of Stamina",14478, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/6/6b/Signet_of_Stamina.png"),
                new Buff("Signet of Rage",14496, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/bc/Signet_of_Rage.png"),
                //banners
                new Buff("Banner of Strength", 14417, ParserHelper.Source.Warrior, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/thumb/e/e1/Banner_of_Strength.png/33px-Banner_of_Strength.png"),
                new Buff("Banner of Discipline", 14449, ParserHelper.Source.Warrior, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/thumb/5/5f/Banner_of_Discipline.png/33px-Banner_of_Discipline.png"),
                new Buff("Banner of Tactics",14450, ParserHelper.Source.Warrior, BuffNature.SupportBuffTable, "https://wiki.guildwars2.com/images/thumb/3/3f/Banner_of_Tactics.png/33px-Banner_of_Tactics.png"),
                new Buff("Banner of Defense",14543, ParserHelper.Source.Warrior, BuffNature.DefensiveBuffTable, "https://wiki.guildwars2.com/images/thumb/f/f1/Banner_of_Defense.png/33px-Banner_of_Defense.png"),
                //stances
                new Buff("Shield Stance",756, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff,"https://wiki.guildwars2.com/images/d/de/Shield_Stance.png"),
                new Buff("Berserker's Stance",14453, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff,"https://wiki.guildwars2.com/images/8/8a/Berserker_Stance.png"),
                new Buff("Enduring Pain",787, ParserHelper.Source.Warrior, BuffStackType.Queue, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/2/24/Endure_Pain.png"),
                new Buff("Balanced Stance",34778, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/2/27/Balanced_Stance.png"),
                new Buff("Defiant Stance",21816, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/d/db/Defiant_Stance.png"),
                new Buff("Rampage",14484, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/e/e4/Rampage.png"),
                //traits
                new Buff("Soldier's Focus", 58102, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/2/29/Soldier%27s_Focus.png", 99526, ulong.MaxValue),
                new Buff("Brave Stride", 43063, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/b/b8/Death_from_Above.png"),
                new Buff("Empower Allies", 14222, ParserHelper.Source.Warrior, BuffNature.OffensiveBuffTable, "https://wiki.guildwars2.com/images/thumb/4/4c/Empower_Allies.png/20px-Empower_Allies.png"),
                new Buff("Peak Performance",46853, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/9/98/Peak_Performance.png"),
                new Buff("Furious Surge", 30204, ParserHelper.Source.Warrior, BuffStackType.Stacking, 25, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/6/65/Furious.png"),
                //new Boon("Health Gain per Adrenaline bar Spent",-1, BoonSource.Warrior, BoonType.Intensity, 3, BoonEnum.GraphOnlyBuff,RemoveType.Normal),
                new Buff("Rousing Resilience",24383, ParserHelper.Source.Warrior, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/c/ca/Rousing_Resilience.png"),
                new Buff("Berserker's Power",42539, ParserHelper.Source.Warrior, BuffStackType.Stacking, 3, BuffNature.GraphOnlyBuff, "https://wiki.guildwars2.com/images/6/6f/Berserker%27s_Power.png"),
        };


        /*private static HashSet<AgentItem> FindBattleStandards(Dictionary<long, List<AbstractBuffEvent>> buffData, HashSet<AgentItem> playerAgents)
        {
            if (buffData.TryGetValue(725, out List<AbstractBuffEvent> list))
            {
                var battleBannerCandidates = new HashSet<AgentItem>(list.Where(x => x is BuffApplyEvent && x.By.Type == AgentItem.AgentType.Gadget && (playerAgents.Contains(x.To) || playerAgents.Contains(x.To.Master))).Select(x => x.By));
                if (battleBannerCandidates.Count > 0)
                {
                    if (buffData.TryGetValue(740, out list))
                    {
                        battleBannerCandidates.IntersectWith(new HashSet<AgentItem>(list.Where(x => x is BuffApplyEvent && x.By.Type == AgentItem.AgentType.Gadget && (playerAgents.Contains(x.To) || playerAgents.Contains(x.To.Master))).Select(x => x.By)));
                        if (battleBannerCandidates.Count > 0)
                        {
                            if (buffData.TryGetValue(719, out list))
                            {
                                battleBannerCandidates.IntersectWith(new HashSet<AgentItem>(list.Where(x => x is BuffApplyEvent && x.By.Type == AgentItem.AgentType.Gadget && (playerAgents.Contains(x.To) || playerAgents.Contains(x.To.Master))).Select(x => x.By)));
                                return battleBannerCandidates;
                            }
                        }
                    }
                }
            }
            return new HashSet<AgentItem>();
        }*/

        public static void AttachMasterToWarriorBanners(List<Player> players, Dictionary<long, List<AbstractBuffEvent>> buffData, Dictionary<long, List<AbstractCastEvent>> castData)
        {
            var playerAgents = new HashSet<AgentItem>(players.Select(x => x.AgentItem));
            HashSet<AgentItem> strBanners = GetBannerAgents(buffData, 14417, playerAgents),
                defBanners = GetBannerAgents(buffData, 14543, playerAgents),
                disBanners = GetBannerAgents(buffData, 14449, playerAgents),
                tacBanners = GetBannerAgents(buffData, 14450, playerAgents);
                //battleBanner = FindBattleStandards(buffData, playerAgents);
            var warriors = players.Where(x => x.Prof == "Warrior" || x.Prof == "Spellbreaker" || x.Prof == "Berserker").ToList();
            // if only one warrior, could only be that one
            if (warriors.Count == 1)
            {
                Player warrior = warriors[0];
                SetGadgetMaster(strBanners, warrior.AgentItem);
                SetGadgetMaster(disBanners, warrior.AgentItem);
                SetGadgetMaster(tacBanners, warrior.AgentItem);
                SetGadgetMaster(defBanners, warrior.AgentItem);
                //SetBannerMaster(battleBanner, warrior.AgentItem);
            }
            else if (warriors.Count > 1)
            {
                // land and under water cast ids
                AttachMasterToGadgetByCastData(castData, strBanners, new List<long> { 14405, 14572 }, 1000);
                AttachMasterToGadgetByCastData(castData, defBanners, new List<long> { 14528, 14570 }, 1000);
                AttachMasterToGadgetByCastData(castData, disBanners, new List<long> { 14407, 14571 }, 1000);
                AttachMasterToGadgetByCastData(castData, tacBanners, new List<long> { 14408, 14573 }, 1000);
                //AttachMasterToBanner(castData, battleBanner, 14419, 14569);
            }
        }

    }
}
