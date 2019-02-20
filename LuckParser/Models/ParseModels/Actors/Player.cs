﻿using LuckParser.Controllers;
using LuckParser.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public class Player : AbstractMasterActor
    {
        public class Consumable
        {
            public Boon Buff { get; }
            public long Time { get; }
            public int Duration { get; }
            public int Stack { get; set; }

            public Consumable(Boon item, long time, int duration)
            {
                Buff = item;
                Time = time;
                Duration = duration;
                Stack = 1;
            }
        }

        public class DeathRecap
        {
            public class DeathRecapDamageItem
            {
                public long ID;
                public bool IndirectDamage;
                public string Src;
                public int Damage;
                public int Time;
            }

            public int DeathTime;
            public List<DeathRecapDamageItem> ToDown;
            public List<DeathRecapDamageItem> ToKill;
        }
        // Fields
        public readonly string Account;
        public readonly int Group;
       
        private readonly List<Consumable> _consumeList = new List<Consumable>();
        private List<DeathRecap> _deathRecaps = new List<DeathRecap>();
        // statistics
        private Dictionary<Target, List<Statistics.FinalDPS>> _dpsTarget;
        private Dictionary<Target, List<Statistics.FinalStats>> _statsTarget;
        private List<Statistics.FinalStatsAll> _statsAll;
        private List<Statistics.FinalDefenses> _defenses;
        private List<Statistics.FinalSupport> _support;
        //weaponslist
        private string[] _weaponsArray;

        // Constructors
        public Player(AgentItem agent, bool noSquad) : base(agent)
        {
            string[] name = agent.Name.Split('\0');
            Account = name[1];
            Group = noSquad ? 1 : int.Parse(name[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        
        // Public methods
        public int[] GetCleanses(ParsedLog log, int phaseIndex) {
            int[] cleanse = { 0, 0 };
            foreach (Player p in log.PlayerList)
            {
                foreach(List<long> list in p.GetCondiCleanse(log,phaseIndex, AgentItem).Values)
                {
                    cleanse[0] += list.Count;
                    cleanse[1] += (int)list.Sum();
                }
            }
            return cleanse;
        }
        public int[] GetReses(ParsedLog log, long start, long end)
        {
            List<CastLog> cls = GetCastLogs(log, start, end);
            int[] reses = { 0, 0 };
            foreach (CastLog cl in cls) {
                if (cl.SkillId == SkillItem.ResurrectId)
                {
                    reses[0]++;
                    reses[1] += cl.ActualDuration;
                }
            }
            return reses;
        }

        public Statistics.FinalDPS GetDPSTarget(ParsedLog log, int phaseIndex, Target target)
        {
            if (_dpsTarget == null)
            {
                _dpsTarget = new Dictionary<Target, List<Statistics.FinalDPS>>();
                foreach (Target tar in log.FightData.Logic.Targets)
                {
                    _dpsTarget[tar] = new List<Statistics.FinalDPS>();
                    foreach (PhaseData phase in log.FightData.GetPhases(log))
                    {
                        _dpsTarget[tar].Add(GetFinalDPS(log, phase, tar));
                    }
                }
            }
            if (target == null)
            {
                return GetDPSAll(log, phaseIndex);
            }
            return _dpsTarget[target][phaseIndex];
        }

        public List<Statistics.FinalDPS> GetDPSTarget(ParsedLog log, Target target)
        {
            if (_dpsTarget == null)
            {
                _dpsTarget = new Dictionary<Target, List<Statistics.FinalDPS>>();
                foreach (Target tar in log.FightData.Logic.Targets)
                {
                    _dpsTarget[tar] = new List<Statistics.FinalDPS>();
                    foreach (PhaseData phase in log.FightData.GetPhases(log))
                    {
                        _dpsTarget[tar].Add(GetFinalDPS(log, phase, tar));
                    }
                }
            }
            if (target == null)
            {
                return GetDPSAll(log);
            }
            return _dpsTarget[target];
        }

        public Statistics.FinalStatsAll GetStatsAll(ParsedLog log, int phaseIndex)
        {
            if (_statsAll == null)
            {
                SetStats(log);
            }
            return _statsAll[phaseIndex];
        }

        public Statistics.FinalStats GetStatsTarget(ParsedLog log, int phaseIndex, Target target)
        {
            if (_statsTarget == null)
            {
                SetStats(log);
            }
            return _statsTarget[target][phaseIndex];
        }

        public List<Statistics.FinalStatsAll> GetStatsAll(ParsedLog log)
        {
            if (_statsAll == null)
            {
                SetStats(log);
            }
            return _statsAll;
        }

        public List<Statistics.FinalStats> GetStatsTarget(ParsedLog log, Target target)
        {
            if (_statsTarget == null)
            {
                SetStats(log);
            }
            return _statsTarget[target];
        }

        private void FillFinalStats(ParsedLog log, List<DamageLog> dls, Statistics.FinalStats final, Dictionary<Target, Statistics.FinalStats> targetsFinal)
        {
            HashSet<long> nonCritable = new HashSet<long>
                    {
                        9292,
                        5492,
                        13014,
                        30770,
                        52370
                    };
            // (x - 1) / x
            double fiveGain = 0.05 / 1.05;
            double tenGain = 0.1 / 1.1;
            foreach (DamageLog dl in dls)
            {
                if (!dl.IsIndirectDamage)
                {
                    foreach (var pair in targetsFinal)
                    {
                        Target target = pair.Key;
                        if (dl.DstInstId == target.InstID && dl.Time <= log.FightData.ToFightSpace(target.LastAware) && dl.Time >= log.FightData.ToFightSpace(target.FirstAware))
                        {
                            Statistics.FinalStats targetFinal = pair.Value;
                            if (dl.Result == ParseEnum.Result.Crit)
                            {
                                targetFinal.CriticalRate++;
                                targetFinal.CriticalDmg += dl.Damage;
                            }

                            if (dl.IsNinety)
                            {
                                targetFinal.ScholarRate++;
                                targetFinal.ScholarDmg += (int)Math.Round(fiveGain * dl.Damage);
                            }

                            if (dl.IsFifty)
                            {
                                targetFinal.EagleRate++;
                                targetFinal.EagleDmg += (int)Math.Round(tenGain * dl.Damage);
                            }

                            if (dl.IsMoving)
                            {
                                targetFinal.MovingRate++;
                                targetFinal.MovingDamage += (int)Math.Round(fiveGain * dl.Damage);
                            }

                            if (dl.IsFlanking)
                            {
                                targetFinal.FlankingDmg += (int)Math.Round(tenGain * dl.Damage);
                                targetFinal.FlankingRate++;
                            }

                            if (dl.Result == ParseEnum.Result.Glance)
                            {
                                targetFinal.GlanceRate++;
                            }

                            if (dl.Result == ParseEnum.Result.Blind)
                            {
                                targetFinal.Missed++;
                            }
                            if (dl.Result == ParseEnum.Result.Interrupt)
                            {
                                targetFinal.Interrupts++;
                            }

                            if (dl.Result == ParseEnum.Result.Absorb)
                            {
                                targetFinal.Invulned++;
                            }
                            targetFinal.DirectDamageCount++;
                            targetFinal.DirectDamage += dl.Damage;
                            if (!nonCritable.Contains(dl.SkillId))
                            {
                                targetFinal.CritableDirectDamageCount++;
                            }
                        }
                    }
                    if (dl.Result == ParseEnum.Result.Crit)
                    {
                        final.CriticalRate++;
                        final.CriticalDmg += dl.Damage;
                    }

                    if (dl.IsNinety)
                    {
                        final.ScholarRate++;
                        final.ScholarDmg += (int)Math.Round(fiveGain * dl.Damage);
                    }

                    if (dl.IsFifty)
                    {
                        final.EagleRate++;
                        final.EagleDmg += (int)Math.Round(tenGain * dl.Damage);
                    }

                    if (dl.IsMoving)
                    {
                        final.MovingRate++;
                        final.MovingDamage += (int)Math.Round(fiveGain * dl.Damage);
                    }

                    if (dl.IsFlanking)
                    {
                        final.FlankingDmg += (int)Math.Round(tenGain * dl.Damage);
                        final.FlankingRate++;
                    }

                    if (dl.Result == ParseEnum.Result.Glance)
                    {
                        final.GlanceRate++;
                    }

                    if (dl.Result == ParseEnum.Result.Blind)
                    {
                        final.Missed++;
                    }
                    if (dl.Result == ParseEnum.Result.Interrupt)
                    {
                        final.Interrupts++;
                    }

                    if (dl.Result == ParseEnum.Result.Absorb)
                    {
                        final.Invulned++;
                    }
                    final.DirectDamageCount++;
                    final.DirectDamage += dl.Damage;
                    if (!nonCritable.Contains(dl.SkillId))
                    {
                        final.CritableDirectDamageCount++;
                    }
                }
            }
        }

        private void SetStats(ParsedLog log)
        {
            int phaseIndex = -1;
            _statsAll = new List<Statistics.FinalStatsAll>();
            _statsTarget = new Dictionary<Target, List<Statistics.FinalStats>>();
            foreach (PhaseData phase in log.FightData.GetPhases(log))
            {
                phaseIndex++;
                Dictionary<Target, Statistics.FinalStats> targetDict = new Dictionary<Target, Statistics.FinalStats>();
                foreach (Target target in log.FightData.Logic.Targets)
                {
                    if (!_statsTarget.ContainsKey(target))
                    {
                        _statsTarget[target] = new List<Statistics.FinalStats>();
                    }
                    _statsTarget[target].Add(new Statistics.FinalStats());
                    targetDict[target] = _statsTarget[target].Last();
                }
                Statistics.FinalStatsAll final = new Statistics.FinalStatsAll();
                FillFinalStats(log, GetJustPlayerDamageLogs(null, log, phase.Start, phase.End), final, targetDict);
                _statsAll.Add(final);
                // If conjured sword, stop
                if (Account == ":Conjured Sword")
                {
                    return;
                }
                foreach (CastLog cl in GetCastLogs(log, phase.Start, phase.End))
                {
                    if (cl.EndActivation == ParseEnum.Activation.CancelCancel)
                    {
                        final.Wasted++;
                        final.TimeWasted += cl.ActualDuration;
                    }
                    if (cl.EndActivation == ParseEnum.Activation.CancelFire)
                    {
                        if (cl.ActualDuration < cl.ExpectedDuration)
                        {
                            final.Saved++;
                            final.TimeSaved += cl.ExpectedDuration - cl.ActualDuration;
                        }
                    }
                    if (cl.SkillId == SkillItem.WeaponSwapId)
                    {
                        final.SwapCount++;
                    }
                }
                final.TimeSaved = Math.Round(final.TimeSaved / 1000.0, 3);
                final.TimeWasted = Math.Round(final.TimeWasted / 1000.0, 3);

                double avgBoons = 0;
                foreach (long duration in GetBoonPresence(log, phaseIndex).Values)
                {
                    avgBoons += duration;
                }
                final.AvgBoons = avgBoons / phase.GetDuration();

                double avgCondis = 0;
                foreach (long duration in GetCondiPresence(log, phaseIndex).Values)
                {
                    avgCondis += duration;
                }
                final.AvgConditions = avgCondis / phase.GetDuration();

                if (Properties.Settings.Default.ParseCombatReplay && log.FightData.Logic.CanCombatReplay)
                {
                    List<Point3D> positions = CombatReplay.Positions.Where(x => x.Time >= phase.Start && x.Time <= phase.End).ToList();
                    int offset = CombatReplay.Positions.Count(x => x.Time < phase.Start);
                    if (positions.Count > 1)
                    {
                        List<float> distances = new List<float>();
                        for (int time = 0; time < positions.Count; time++)
                        {

                            float deltaX = positions[time].X - log.Statistics.StackCenterPositions[time + offset].X;
                            float deltaY = positions[time].Y - log.Statistics.StackCenterPositions[time + offset].Y;
                            //float deltaZ = positions[time].Z - StackCenterPositions[time].Z;


                            distances.Add((float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
                        }
                        final.StackDist = distances.Sum() / distances.Count;
                    }
                    else
                    {
                        final.StackDist = -1;
                    }
                }
            }
        }

        public Statistics.FinalDefenses GetDefenses(ParsedLog log, int phaseIndex)
        {
            if (_defenses == null)
            {
                SetDefenses(log);
            }
            return _defenses[phaseIndex];
        }

        public List<Statistics.FinalDefenses> GetDefenses(ParsedLog log)
        {
            if (_defenses == null)
            {
                SetDefenses(log);
            }
            return _defenses;
        }

        private void SetDefenses(ParsedLog log)
        {
            List<(long start, long end)> dead = new List<(long start, long end)>();
            List<(long start, long end)> down = new List<(long start, long end)>();
            List<(long start, long end)> dc = new List<(long start, long end)>();
            log.CombatData.GetAgentStatus(FirstAware, LastAware, InstID, dead, down, dc);
            _defenses = new List<Statistics.FinalDefenses>();
            foreach (PhaseData phase in log.FightData.GetPhases(log))
            {
                Statistics.FinalDefenses final = new Statistics.FinalDefenses();
                _defenses.Add(final);
                long start = log.FightData.ToLogSpace(phase.Start);
                long end = log.FightData.ToLogSpace(phase.End);
                List<DamageLog> damageLogs = GetDamageTakenLogs(null, log, phase.Start, phase.End);
                //List<DamageLog> healingLogs = player.getHealingReceivedLogs(log, phase.getStart(), phase.getEnd());

                final.DamageTaken = damageLogs.Sum(x => (long)x.Damage);
                //final.allHealReceived = healingLogs.Sum(x => x.getDamage());
                final.BlockedCount = damageLogs.Count(x => x.Result == ParseEnum.Result.Block);
                final.InvulnedCount = 0;
                final.DamageInvulned = 0;
                final.EvadedCount = damageLogs.Count(x => x.Result == ParseEnum.Result.Evade);
                final.DodgeCount = GetCastLogs(log, 0, log.FightData.FightDuration).Count(x => x.SkillId == SkillItem.DodgeId);
                final.DamageBarrier = damageLogs.Sum(x => x.ShieldDamage);
                final.InterruptedCount = damageLogs.Count(x => x.Result == ParseEnum.Result.Interrupt);
                foreach (DamageLog dl in damageLogs.Where(x => x.Result == ParseEnum.Result.Absorb))
                {
                    final.InvulnedCount++;
                    final.DamageInvulned += dl.Damage;
                }
                List<CombatItem> deads = log.CombatData.GetStatesData(InstID, ParseEnum.StateChange.ChangeDead, start, end);
                List<CombatItem> downs = log.CombatData.GetStatesData(InstID, ParseEnum.StateChange.ChangeDown, start, end);
                List<CombatItem> dcs = log.CombatData.GetStatesData(InstID, ParseEnum.StateChange.Despawn, start, end);
                final.DownCount = downs.Count - log.CombatData.GetBoonData(5620).Where(x => x.SrcInstid == InstID && x.Time >= start && x.Time <= end && x.IsBuffRemove == ParseEnum.BuffRemove.All).Count();
                final.DeadCount = deads.Count;
                final.DcCount = dcs.Count;

                //
                start = phase.Start;
                end = phase.End;
                final.DownDuration = (int)down.Where(x => x.end >= start && x.start <= end).Sum(x => Math.Min(end, x.end) - Math.Max(x.start, start));
                final.DeadDuration = (int)dead.Where(x => x.end >= start && x.start <= end).Sum(x => Math.Min(end, x.end) - Math.Max(x.start, start));
                final.DcDuration = (int)dc.Where(x => x.end >= start && x.start <= end).Sum(x => Math.Min(end, x.end) - Math.Max(x.start, start));
            }
        }

        public Statistics.FinalSupport GetSupport(ParsedLog log, int phaseIndex)
        {
            if (_support == null)
            {
                SetSupport(log);
            }
            return _support[phaseIndex];
        }

        public List<Statistics.FinalSupport> GetSupport(ParsedLog log)
        {
            if (_support == null)
            {
                SetSupport(log);
            }
            return _support;
        }

        private void SetSupport(ParsedLog log)
        {
            _support = new List<Statistics.FinalSupport>();
            List<PhaseData> phases = log.FightData.GetPhases(log);
            for (int phaseIndex = 0; phaseIndex < phases.Count; phaseIndex++)
            {
                Statistics.FinalSupport final = new Statistics.FinalSupport();
                _support.Add(final);
                PhaseData phase = phases[phaseIndex];

                int[] resArray = GetReses(log, phase.Start, phase.End);
                int[] cleanseArray = GetCleanses(log, phaseIndex);
                //List<DamageLog> healingLogs = player.getHealingLogs(log, phase.getStart(), phase.getEnd());
                //final.allHeal = healingLogs.Sum(x => x.getDamage());
                final.Resurrects = resArray[0];
                final.ResurrectTime = resArray[1] / 1000.0;
                final.CondiCleanse = cleanseArray[0];
                final.CondiCleanseTime = cleanseArray[1] / 1000.0;
            }
        }

        public List<DeathRecap> GetDeathRecaps(ParsedLog log)
        {
            if(_deathRecaps == null)
            {
                return null;
            }
            if (_deathRecaps.Count == 0)
            {
                SetDeathRecaps(log);
            }
            return _deathRecaps;
        }

        public string[] GetWeaponsArray(ParsedLog log)
        {
            if (_weaponsArray == null)
            {
                EstimateWeapons( log);
            }
            return _weaponsArray;
        }

        public List<Consumable> GetConsumablesList(ParsedLog log, long start, long end)
        {
            if (_consumeList.Count == 0)
            {
                SetConsumablesList(log);
            }
            return _consumeList.Where(x => x.Time >= start && x.Time <= end).ToList() ;
        }
        
        // Private Methods

        private void SetDeathRecaps(ParsedLog log)
        {
            List<DeathRecap> res = _deathRecaps;
            List<CombatItem> deads = log.CombatData.GetStatesData(InstID, ParseEnum.StateChange.ChangeDead, log.FightData.FightStart, log.FightData.FightEnd);
            List<CombatItem> downs = log.CombatData.GetStatesData(InstID, ParseEnum.StateChange.ChangeDown, log.FightData.FightStart, log.FightData.FightEnd);
            long lastTime = log.FightData.FightStart;
            List<DamageLog> damageLogs = GetDamageTakenLogs(null, log, 0, log.FightData.FightDuration);
            foreach (CombatItem dead in deads)
            {
                DeathRecap recap = new DeathRecap()
                {
                    DeathTime = (int)(log.FightData.ToFightSpace(dead.Time))
                };
                CombatItem downed = downs.LastOrDefault(x => x.Time <= dead.Time && x.Time >= lastTime);
                if (downed != null)
                {
                    List<DamageLog> damageToDown = damageLogs.Where(x => x.Time < log.FightData.ToFightSpace(downed.Time) && x.Damage > 0 && x.Time > log.FightData.ToFightSpace(lastTime)).ToList();
                    recap.ToDown = damageToDown.Count > 0 ? new List<DeathRecap.DeathRecapDamageItem>() : null;
                    int damage = 0;
                    for (int i = damageToDown.Count - 1; i >= 0; i--)
                    {
                        DamageLog dl = damageToDown[i];
                        AgentItem ag = log.AgentData.GetAgentByInstID(dl.SrcInstId, log.FightData.ToLogSpace(dl.Time));
                        DeathRecap.DeathRecapDamageItem item = new DeathRecap.DeathRecapDamageItem()
                        {
                            Time = (int)dl.Time,
                            IndirectDamage = dl.IsIndirectDamage,
                            ID = dl.SkillId,
                            Damage = dl.Damage,
                            Src = ag != null ? ag.Name.Replace("\u0000", "").Split(':')[0] : ""
                        };
                        damage += dl.Damage;
                        recap.ToDown.Add(item);
                        if (damage > 20000)
                        {
                            break;
                        }
                    }
                    List<DamageLog> damageToKill = damageLogs.Where(x => x.Time > log.FightData.ToFightSpace(downed.Time) && x.Time < log.FightData.ToFightSpace(dead.Time) && x.Damage > 0 && x.Time > log.FightData.ToFightSpace(lastTime)).ToList();
                    recap.ToKill = damageToKill.Count > 0 ? new List<DeathRecap.DeathRecapDamageItem>() : null;
                    for (int i = damageToKill.Count - 1; i >= 0; i--)
                    {
                        DamageLog dl = damageToKill[i];
                        AgentItem ag = log.AgentData.GetAgentByInstID(dl.SrcInstId, log.FightData.ToLogSpace(dl.Time));
                        DeathRecap.DeathRecapDamageItem item = new DeathRecap.DeathRecapDamageItem()
                        {
                            Time = (int)dl.Time,
                            IndirectDamage = dl.IsIndirectDamage,
                            ID = dl.SkillId,
                            Damage = dl.Damage,
                            Src = ag != null ? ag.Name.Replace("\u0000", "").Split(':')[0] : ""
                        };
                        recap.ToKill.Add(item);
                    }
                }
                else
                {
                    recap.ToDown = null;
                    List<DamageLog> damageToKill = damageLogs.Where(x => x.Time < log.FightData.ToFightSpace(dead.Time) && x.Damage > 0 && x.Time > log.FightData.ToFightSpace(lastTime)).ToList();
                    recap.ToKill = damageToKill.Count > 0 ? new List<DeathRecap.DeathRecapDamageItem>() : null;
                    int damage = 0;
                    for (int i = damageToKill.Count - 1; i >= 0; i--)
                    {
                        DamageLog dl = damageToKill[i];
                        AgentItem ag = log.AgentData.GetAgentByInstID(dl.SrcInstId, log.FightData.ToLogSpace(dl.Time));
                        DeathRecap.DeathRecapDamageItem item = new DeathRecap.DeathRecapDamageItem()
                        {
                            Time = (int)dl.Time,
                            IndirectDamage = dl.IsIndirectDamage,
                            ID = dl.SkillId,
                            Damage = dl.Damage,
                            Src = ag != null ? ag.Name.Replace("\u0000", "").Split(':')[0] : ""
                        };
                        damage += dl.Damage;
                        recap.ToKill.Add(item);
                        if (damage > 20000)
                        {
                            break;
                        }
                    }
                }
                lastTime = dead.Time;
                res.Add(recap);
            }
            if (_deathRecaps.Count == 0)
            {
                _deathRecaps = null;
            }
        }

        private void EstimateWeapons(ParsedLog log)
        {
            if (Prof == "Sword")
            {
                _weaponsArray = new string[]
                {
                    "Sword",
                    "2Hand",
                    null,
                    null
                };
                return;
            }
            string[] weapons = new string[4];//first 2 for first set next 2 for second set
            SkillData skillList = log.SkillData;
            List<CastLog> casting = GetCastLogs(log, 0, log.FightData.FightDuration);      
            int swapped = 0;//4 for first set and 5 for next
            long swappedTime = 0;
            List<CastLog> swaps = casting.Where(x => x.SkillId == SkillItem.WeaponSwapId).Take(2).ToList();
            // If the player never swapped, assume they are on their first set
            if (swaps.Count == 0)
            {
                swapped = 4;
            }
            // if the player swapped, check on which set they started
            else
            {
                swapped = swaps.First().ExpectedDuration == 4 ? 5 : 4;
            }
            foreach (CastLog cl in casting)
            {
                if (cl.ActualDuration == 0 && cl.SkillId != SkillItem.WeaponSwapId)
                {
                    continue;
                }
                SkillItem skill = skillList.Get(cl.SkillId);
                if (!skill.EstimateWeapons(weapons, swapped, cl.Time > swappedTime) && cl.SkillId == SkillItem.WeaponSwapId)
                {
                    //wepswap  
                    swapped = cl.ExpectedDuration;
                    swappedTime = cl.Time;
                }
            }
            _weaponsArray = weapons;
        }    
        
        private void SetConsumablesList(ParsedLog log)
        {
            List<Boon> consumableList = Boon.GetConsumableList();
            long fightDuration = log.FightData.FightDuration;
            foreach (Boon consumable in consumableList)
            {
                foreach (CombatItem c in log.CombatData.GetBoonData(consumable.ID))
                {
                    if (c.IsBuffRemove != ParseEnum.BuffRemove.None || (c.IsBuff != 18 && c.IsBuff != 1) || AgentItem.InstID != c.DstInstid)
                    {
                        continue;
                    }
                    long time = 0;
                    if (c.IsBuff != 18)
                    {
                        time = log.FightData.ToFightSpace(c.Time);
                    }
                    if (time <= fightDuration)
                    {
                        Consumable existing = _consumeList.Find(x => x.Time == time && x.Buff.ID == consumable.ID);
                        if (existing != null)
                        {
                            existing.Stack++;
                        } else
                        {
                            _consumeList.Add(new Consumable(consumable, time, c.Value));
                        }
                    }
                }
            }
            _consumeList.Sort((x, y) => x.Time.CompareTo(y.Time));

        }

        protected override void SetAdditionalCombatReplayData(ParsedLog log)
        {
            CombatReplay.Icon = GeneralHelper.GetProfIcon(Prof);
            // Down and deads
            List<(long, long)> dead = CombatReplay.Deads;
            List<(long, long)> down = CombatReplay.Downs;
            List<(long, long)> dc = CombatReplay.DCs;
            log.CombatData.GetAgentStatus(FirstAware, LastAware, InstID, dead, down, dc);
            // Fight related stuff
            log.FightData.Logic.ComputeAdditionalPlayerData(this, log);
            List<Point3D> facings = CombatReplay.Rotations;
            if (facings.Any())
            {
                CombatReplay.Actors.Add(new FacingActor(((int)CombatReplay.TimeOffsets.start, (int)CombatReplay.TimeOffsets.end), new AgentConnector(this), facings));
            }
        }

        //
        private class PlayerSerializable : AbstractMasterActorSerializable
        {
            public int Group { get; set; }
            public long[] Dead { get; set; }
            public long[] Down { get; set; }
        }

        public override AbstractMasterActorSerializable GetCombatReplayJSON(CombatReplayMap map)
        {
            PlayerSerializable aux = new PlayerSerializable
            {
                Group = Group,
                Img = CombatReplay.Icon,
                Type = "Player",
                ID = GetCombatReplayID(),
                Positions = new double[2 * CombatReplay.Positions.Count],
                Dead = new long[2 * CombatReplay.Deads.Count],
                Down = new long[2 * CombatReplay.Downs.Count]
            };
            int i = 0;
            foreach (Point3D pos in CombatReplay.Positions)
            {
                (double x, double y) = map.GetMapCoord(pos.X, pos.Y);
                aux.Positions[i++] = x;
                aux.Positions[i++] = y;
            }
            i = 0;
            foreach ((long start, long end) in CombatReplay.Deads)
            {
                aux.Dead[i++] = start;
                aux.Dead[i++] = end;
            }
            i = 0;
            foreach ((long start, long end) in CombatReplay.Downs)
            {
                aux.Down[i++] = start;
                aux.Down[i++] = end;
            }

            return aux;
        }


        /*protected override void setHealingLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            foreach (CombatItem c in log.getHealingData())
            {
                if (agent.InstID == c.getSrcInstid() && c.getTime() > log.getBossData().getFirstAware() && c.getTime() < log.getBossData().getLastAware())//selecting player or minion as caster
                {
                    long time = c.getTime() - time_start;
                    addHealingLog(time, c);
                }
            }
            Dictionary<string, Minions> min_list = getMinions(log);
            foreach (Minions mins in min_list.Values)
            {
                healing_logs.AddRange(mins.getHealingLogs(log, 0, log.getBossData().getAwareDuration()));
            }
            healing_logs.Sort((x, y) => x.getTime() < y.getTime() ? -1 : 1);
        }

        protected override void setHealingReceivedLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            foreach (CombatItem c in log.getHealingReceivedData())
            {
                if (agent.InstID == c.getDstInstid() && c.getTime() > log.getBossData().getFirstAware() && c.getTime() < log.getBossData().getLastAware())
                {//selecting player as target
                    long time = c.getTime() - time_start;
                    addHealingReceivedLog(time, c);
                }
            }
        }*/
    }
}
