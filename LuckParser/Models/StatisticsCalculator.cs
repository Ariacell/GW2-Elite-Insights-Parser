using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LuckParser.Models.ParseModels;
using LuckParser.Parser;
using LuckParser.Setting;
using static LuckParser.Models.Statistics;

namespace LuckParser.Models
{
    /// <summary>
    /// Calculates statistical information from a log
    /// </summary>
    class StatisticsCalculator
    {
        public class Switches
        {
            public bool CalculateDPS = false;
            public bool CalculateStats = false;
            public bool CalculateDefense = false;
            public bool CalculateSupport = false;
            public bool CalculateBoons = false;
            public bool CalculateConditions = false;
            public bool CalculateCombatReplay = false;
            public bool CalculateMechanics = false;
        }

        private Statistics _statistics;

        private ParsedLog _log;
        private List<PhaseData> _phases;

        public StatisticsCalculator()
        {
        }

        /// <summary>
        /// Calculate a statistic from a log
        /// </summary>
        /// <param name="log"></param>
        /// <param name="switches"></param>
        /// <returns></returns>
        public Statistics CalculateStatistics(ParsedLog log, Switches switches)
        {
            if (switches.CalculateBoons) CalculateBoons();
            if (switches.CalculateDefense) CalculateDefenses();
            if (switches.CalculateSupport) CalculateSupport();

            if (switches.CalculateConditions) CalculateConditions();
            //

            return _statistics;
        }
        private void CalculateDefenses()
        {
            CombatData combatData = _log.CombatData;
            foreach (Player player in _log.PlayerList)
            {
                FinalDefenses[] phaseDefense = new FinalDefenses[_phases.Count];
                for (int phaseIndex = 0; phaseIndex < _phases.Count; phaseIndex++)
                {
                    FinalDefenses final = new FinalDefenses();

                    PhaseData phase = _phases[phaseIndex];
                    long start = _log.FightData.ToLogSpace(phase.Start);
                    long end = _log.FightData.ToLogSpace(phase.End);

                    List<DamageLog> damageLogs = player.GetDamageTakenLogs(null, _log, phase.Start, phase.End);
                    //List<DamageLog> healingLogs = player.getHealingReceivedLogs(log, phase.getStart(), phase.getEnd());

                    final.DamageTaken = damageLogs.Sum(x => (long)x.Damage);
                    //final.allHealReceived = healingLogs.Sum(x => x.getDamage());
                    final.BlockedCount = damageLogs.Count(x => x.Result == ParseEnum.Result.Block);
                    final.InvulnedCount = 0;
                    final.DamageInvulned = 0;
                    final.EvadedCount = damageLogs.Count(x => x.Result == ParseEnum.Result.Evade);
                    final.DodgeCount = player.GetCastLogs(_log, 0, _log.FightData.FightDuration).Count(x => x.SkillId == SkillItem.DodgeId);
                    final.DamageBarrier = damageLogs.Sum(x => x.ShieldDamage);
                    final.InterruptedCount = damageLogs.Count(x => x.Result == ParseEnum.Result.Interrupt);
                    foreach (DamageLog log in damageLogs.Where(x => x.Result == ParseEnum.Result.Absorb))
                    {
                        final.InvulnedCount++;
                        final.DamageInvulned += log.Damage;
                    }
                    List<CombatItem> deads = combatData.GetStatesData(player.InstID, ParseEnum.StateChange.ChangeDead, start, end);
                    List<CombatItem> downs = combatData.GetStatesData(player.InstID, ParseEnum.StateChange.ChangeDown, start, end);
                    List<CombatItem> dcs = combatData.GetStatesData(player.InstID, ParseEnum.StateChange.Despawn, start, end);
                    final.DownCount = downs.Count - combatData.GetBoonData(5620).Where(x => x.SrcInstid == player.InstID && x.Time >= start && x.Time <= end && x.IsBuffRemove == ParseEnum.BuffRemove.All).Count();
                    final.DeadCount = deads.Count;
                    final.DcCount = dcs.Count;

                    phaseDefense[phaseIndex] = final;
                }
                List<(long start, long end)> dead = new List<(long start, long end)>();
                List<(long start, long end)> down = new List<(long start, long end)>();
                List<(long start, long end)> dc = new List<(long start, long end)>();
                combatData.GetAgentStatus(player.FirstAware, player.LastAware, player.InstID, dead, down, dc);

                for (int phaseIndex = 0; phaseIndex < _phases.Count; phaseIndex++)
                {
                    FinalDefenses defenses = phaseDefense[phaseIndex];
                    PhaseData phase = _phases[phaseIndex];
                    long start = phase.Start;
                    long end = phase.End;
                    defenses.DownDuration = (int)down.Where(x => x.end >= start && x.start <= end).Sum(x => Math.Min(end, x.end) - Math.Max(x.start, start));
                    defenses.DeadDuration = (int)dead.Where(x => x.end >= start && x.start <= end).Sum(x => Math.Min(end, x.end) - Math.Max(x.start, start));
                    defenses.DcDuration = (int)dc.Where(x => x.end >= start && x.start <= end).Sum(x => Math.Min(end, x.end) - Math.Max(x.start, start));
                }

                _statistics.Defenses[player] = phaseDefense;
            }
        }

        private void CalculateSupport()
        {
            foreach (Player player in _log.PlayerList)
            {
                FinalSupport[] phaseSupport = new FinalSupport[_phases.Count];
                for (int phaseIndex = 0; phaseIndex < _phases.Count; phaseIndex++)
                {
                    FinalSupport final = new FinalSupport();

                    PhaseData phase = _phases[phaseIndex];

                    int[] resArray = player.GetReses(_log, phase.Start, phase.End);
                    int[] cleanseArray = player.GetCleanses(_log, phaseIndex);
                    //List<DamageLog> healingLogs = player.getHealingLogs(log, phase.getStart(), phase.getEnd());
                    //final.allHeal = healingLogs.Sum(x => x.getDamage());
                    final.Resurrects = resArray[0];
                    final.ResurrectTime = resArray[1] / 1000.0;
                    final.CondiCleanse = cleanseArray[0];
                    final.CondiCleanseTime = cleanseArray[1] / 1000.0;

                    phaseSupport[phaseIndex] = final;
                }
                _statistics.Support[player] = phaseSupport;
            }
        }

        private Dictionary<long, FinalBuffs>[] GetBoonsForPlayers(List<Player> playerList, Player player)
        {
            Dictionary<long, FinalBuffs>[] uptimesByPhase =
                new Dictionary<long, FinalBuffs>[_phases.Count];

            for (int phaseIndex = 0; phaseIndex < _phases.Count; phaseIndex++)
            {
                PhaseData phase = _phases[phaseIndex];
                long fightDuration = phase.End - phase.Start;

                Dictionary<Player, BoonDistribution> boonDistributions = new Dictionary<Player, BoonDistribution>();
                foreach (Player p in playerList)
                {
                    boonDistributions[p] = p.GetBoonDistribution(_log, phaseIndex);
                }

                HashSet<Boon> boonsToTrack = new HashSet<Boon>(boonDistributions.SelectMany(x => x.Value).Select(x => Boon.BoonsByIds[x.Key]));

                Dictionary<long, FinalBuffs> final =
                    new Dictionary<long, FinalBuffs>();

                foreach (Boon boon in boonsToTrack)
                {
                    long totalGeneration = 0;
                    long totalOverstack = 0;
                    long totalWasted = 0;
                    long totalUnknownExtension = 0;
                    long totalExtension = 0;
                    long totalExtended = 0;
                    bool hasGeneration = false;
                    foreach (BoonDistribution boons in boonDistributions.Values)
                    {
                        if (boons.ContainsKey(boon.ID))
                        {
                            hasGeneration = hasGeneration || boons.HasSrc(boon.ID, player.AgentItem);
                            totalGeneration += boons.GetGeneration(boon.ID, player.AgentItem);
                            totalOverstack += boons.GetOverstack(boon.ID, player.AgentItem);
                            totalWasted += boons.GetWaste(boon.ID, player.AgentItem);
                            totalUnknownExtension += boons.GetUnknownExtension(boon.ID, player.AgentItem);
                            totalExtension += boons.GetExtension(boon.ID, player.AgentItem);
                            totalExtended += boons.GetExtended(boon.ID, player.AgentItem);
                        }
                    }

                    if (hasGeneration)
                    {
                        FinalBuffs uptime = new FinalBuffs();
                        final[boon.ID] = uptime;
                        if (boon.Type == Boon.BoonType.Duration)
                        {
                            uptime.Generation = Math.Round(100.0 * totalGeneration / fightDuration / playerList.Count, 2);
                            uptime.Overstack = Math.Round(100.0 * (totalOverstack + totalGeneration) / fightDuration / playerList.Count, 2);
                            uptime.Wasted = Math.Round(100.0 * (totalWasted) / fightDuration / playerList.Count, 2);
                            uptime.UnknownExtended = Math.Round(100.0 * (totalUnknownExtension) / fightDuration / playerList.Count, 2);
                            uptime.ByExtension = Math.Round(100.0 * (totalExtension) / fightDuration / playerList.Count, 2);
                            uptime.Extended = Math.Round(100.0 * (totalExtended) / fightDuration / playerList.Count, 2);
                        }
                        else if (boon.Type == Boon.BoonType.Intensity)
                        {
                            uptime.Generation = Math.Round((double)totalGeneration / fightDuration / playerList.Count, 2);
                            uptime.Overstack = Math.Round((double)(totalOverstack + totalGeneration) / fightDuration / playerList.Count, 2);
                            uptime.Wasted = Math.Round((double)(totalWasted) / fightDuration / playerList.Count, 2);
                            uptime.UnknownExtended = Math.Round((double)(totalUnknownExtension) / fightDuration / playerList.Count, 2);
                            uptime.ByExtension = Math.Round((double)(totalExtension) / fightDuration / playerList.Count, 2);
                            uptime.Extended = Math.Round((double)(totalExtended) / fightDuration / playerList.Count, 2);
                        }
                    }
                }

                uptimesByPhase[phaseIndex] = final;
            }

            return uptimesByPhase;
        }

        private void CalculateBoons()
        {
            foreach (Player player in _log.PlayerList)
            {
                // Boons applied to self
                Dictionary<long, FinalBuffs>[] selfUptimesByPhase = new Dictionary<long, FinalBuffs>[_phases.Count];
                for (int phaseIndex = 0; phaseIndex < _phases.Count; phaseIndex++)
                {
                    Dictionary<long, FinalBuffs> final = new Dictionary<long, FinalBuffs>();

                    PhaseData phase = _phases[phaseIndex];

                    BoonDistribution selfBoons = player.GetBoonDistribution(_log, phaseIndex);
                    Dictionary<long, long> boonPresence = player.GetBoonPresence(_log, phaseIndex);
                    Dictionary<long, long> condiPresence = player.GetCondiPresence(_log, phaseIndex);

                    long fightDuration = phase.End - phase.Start;
                    foreach (Boon boon in player.TrackedBoons)
                    {
                        if (selfBoons.ContainsKey(boon.ID))
                        {
                            FinalBuffs uptime = new FinalBuffs
                            {
                                Uptime = 0,
                                Generation = 0,
                                Overstack = 0,
                                Wasted = 0,
                                UnknownExtended = 0,
                                ByExtension = 0,
                                Extended = 0
                            };
                            final[boon.ID] = uptime;
                            long generation = selfBoons.GetGeneration(boon.ID, player.AgentItem);
                            if (boon.Type == Boon.BoonType.Duration)
                            {
                                uptime.Uptime = Math.Round(100.0 * selfBoons.GetUptime(boon.ID) / fightDuration, 2);
                                uptime.Generation = Math.Round(100.0 * generation / fightDuration, 2);
                                uptime.Overstack = Math.Round(100.0 * (selfBoons.GetOverstack(boon.ID, player.AgentItem) + generation) / fightDuration, 2);
                                uptime.Wasted = Math.Round(100.0 * selfBoons.GetWaste(boon.ID, player.AgentItem) / fightDuration, 2);
                                uptime.UnknownExtended = Math.Round(100.0 * selfBoons.GetUnknownExtension(boon.ID, player.AgentItem) / fightDuration, 2);
                                uptime.ByExtension = Math.Round(100.0 * selfBoons.GetExtension(boon.ID, player.AgentItem) / fightDuration, 2);
                                uptime.Extended = Math.Round(100.0 * selfBoons.GetExtended(boon.ID, player.AgentItem) / fightDuration, 2);
                            }
                            else if (boon.Type == Boon.BoonType.Intensity)
                            {
                                uptime.Uptime = Math.Round((double)selfBoons.GetUptime(boon.ID) / fightDuration, 2);
                                uptime.Generation = Math.Round((double)generation / fightDuration, 2);
                                uptime.Overstack = Math.Round((double)(selfBoons.GetOverstack(boon.ID, player.AgentItem) + generation) / fightDuration, 2);
                                uptime.Wasted = Math.Round((double)selfBoons.GetWaste(boon.ID, player.AgentItem) / fightDuration, 2);
                                uptime.UnknownExtended = Math.Round((double)selfBoons.GetUnknownExtension(boon.ID, player.AgentItem) / fightDuration, 2);
                                uptime.ByExtension = Math.Round((double)selfBoons.GetExtension(boon.ID, player.AgentItem) / fightDuration, 2);
                                uptime.Extended = Math.Round((double)selfBoons.GetExtended(boon.ID, player.AgentItem) / fightDuration, 2);
                                if (boonPresence.TryGetValue(boon.ID, out long presenceValueBoon))
                                {
                                    uptime.Presence = Math.Round(100.0 * presenceValueBoon / fightDuration, 2);
                                }
                                else if (condiPresence.TryGetValue(boon.ID, out long presenceValueCondi))
                                {
                                    uptime.Presence = Math.Round(100.0 * presenceValueCondi / fightDuration, 2);
                                }
                            }
                        }
                    }

                    selfUptimesByPhase[phaseIndex] = final;
                }
                _statistics.SelfBuffs[player] = selfUptimesByPhase;

                // Boons applied to player's group
                var otherPlayersInGroup = _log.PlayerList
                    .Where(p => p.Group == player.Group && player.InstID != p.InstID)
                    .ToList();
                _statistics.GroupBuffs[player] = GetBoonsForPlayers(otherPlayersInGroup, player);

                // Boons applied to other groups
                var offGroupPlayers = _log.PlayerList.Where(p => p.Group != player.Group).ToList();
                _statistics.OffGroupBuffs[player] = GetBoonsForPlayers(offGroupPlayers, player);

                // Boons applied to squad
                var otherPlayers = _log.PlayerList.Where(p => p.InstID != player.InstID).ToList();
                _statistics.SquadBuffs[player] = GetBoonsForPlayers(otherPlayers, player);
            }
        }

        private void CalculateConditions()
        {
            foreach (Target target in _log.FightData.Logic.Targets)
            {
                Dictionary<long, FinalTargetBuffs>[] stats = new Dictionary<long, FinalTargetBuffs>[_phases.Count];
                for (int phaseIndex = 0; phaseIndex < _phases.Count; phaseIndex++)
                {
                    BoonDistribution boonDistribution = target.GetBoonDistribution(_log, phaseIndex);
                    Dictionary<long, FinalTargetBuffs> rates = new Dictionary<long, FinalTargetBuffs>();
                    Dictionary<long, long> boonPresence = target.GetBoonPresence(_log, phaseIndex);
                    Dictionary<long, long> condiPresence = target.GetCondiPresence(_log, phaseIndex);

                    PhaseData phase = _phases[phaseIndex];
                    long fightDuration = phase.GetDuration();

                    foreach (Boon boon in target.TrackedBoons)
                    {
                        if (boonDistribution.ContainsKey(boon.ID))
                        {
                            FinalTargetBuffs buff = new FinalTargetBuffs(_log.PlayerList);
                            rates[boon.ID] = buff;
                            if (boon.Type == Boon.BoonType.Duration)
                            {
                                buff.Uptime = Math.Round(100.0 * boonDistribution.GetUptime(boon.ID) / fightDuration, 2);
                                foreach (Player p in _log.PlayerList)
                                {
                                    long gen = boonDistribution.GetGeneration(boon.ID, p.AgentItem);
                                    buff.Generated[p] = Math.Round(100.0 * gen / fightDuration, 2);
                                    buff.Overstacked[p] = Math.Round(100.0 * (boonDistribution.GetOverstack(boon.ID, p.AgentItem) + gen) / fightDuration, 2);
                                    buff.Wasted[p] = Math.Round(100.0 * boonDistribution.GetWaste(boon.ID, p.AgentItem) / fightDuration, 2);
                                    buff.UnknownExtension[p] = Math.Round(100.0 * boonDistribution.GetUnknownExtension(boon.ID, p.AgentItem) / fightDuration, 2);
                                    buff.Extension[p] = Math.Round(100.0 * boonDistribution.GetExtension(boon.ID, p.AgentItem) / fightDuration, 2);
                                    buff.Extended[p] = Math.Round(100.0 * boonDistribution.GetExtended(boon.ID, p.AgentItem) / fightDuration, 2);
                                }
                            }
                            else if (boon.Type == Boon.BoonType.Intensity)
                            {
                                buff.Uptime = Math.Round((double)boonDistribution.GetUptime(boon.ID) / fightDuration, 2);
                                foreach (Player p in _log.PlayerList)
                                {
                                    long gen = boonDistribution.GetGeneration(boon.ID, p.AgentItem);
                                    buff.Generated[p] = Math.Round((double)gen / fightDuration, 2);
                                    buff.Overstacked[p] = Math.Round((double)(boonDistribution.GetOverstack(boon.ID, p.AgentItem) + gen) / fightDuration, 2);
                                    buff.Wasted[p] = Math.Round((double)boonDistribution.GetWaste(boon.ID, p.AgentItem) / fightDuration, 2);
                                    buff.UnknownExtension[p] = Math.Round((double)boonDistribution.GetUnknownExtension(boon.ID, p.AgentItem) / fightDuration, 2);
                                    buff.Extension[p] = Math.Round((double)boonDistribution.GetExtension(boon.ID, p.AgentItem) / fightDuration, 2);
                                    buff.Extended[p] = Math.Round((double)boonDistribution.GetExtended(boon.ID, p.AgentItem) / fightDuration, 2);
                                }
                                if (boonPresence.TryGetValue(boon.ID, out long presenceValueBoon))
                                {
                                    buff.Presence = Math.Round(100.0 * presenceValueBoon / fightDuration, 2);
                                }
                                else if (condiPresence.TryGetValue(boon.ID, out long presenceValueCondi))
                                {
                                    buff.Presence = Math.Round(100.0 * presenceValueCondi / fightDuration, 2);
                                }
                            }
                        }
                    }
                    stats[phaseIndex] = rates;
                }
                _statistics.TargetBuffs[target] = stats;
            }
        }
    }
}