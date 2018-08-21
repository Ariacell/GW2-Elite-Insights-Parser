﻿using LuckParser.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public abstract class AbstractMasterPlayer : AbstractPlayer
    {
        // Boons
        private readonly List<BoonDistribution> _boonDistribution = new List<BoonDistribution>();
        private readonly List<Dictionary<long, long>> _boonPresence = new List<Dictionary<long, long>>();
        private readonly List<Dictionary<long, long>> _condiPresence = new List<Dictionary<long, long>>();
        private readonly Dictionary<long, BoonsGraphModel> _boonPoints = new Dictionary<long, BoonsGraphModel>();
        private readonly Dictionary<long, Dictionary<int, string[]>> _boonExtra = new Dictionary<long, Dictionary<int, string[]>>();
        // dps graphs
        private readonly Dictionary<int, List<Point>> _dpsGraphs = new Dictionary<int, List<Point>>();
        // Minions
        private readonly Dictionary<string, Minions> _minions = new Dictionary<string, Minions>();
        // Replay
        protected CombatReplay Replay;

        protected AbstractMasterPlayer(AgentItem agent) : base(agent)
        {

        }

        public Dictionary<string, Minions> GetMinions(ParsedLog log)
        {
            if (_minions.Count == 0)
            {
                SetMinions(log);
            }
            return _minions;
        }
        public void AddDPSGraph(int id, List<Point> graph)
        {
            _dpsGraphs[id] = graph;
        }
        public List<Point> GetDPSGraph(int id)
        {
            if (_dpsGraphs.ContainsKey(id))
            {
                return _dpsGraphs[id];
            }
            return new List<Point>();
        }
        public BoonDistribution GetBoonDistribution(ParsedLog log, List<PhaseData> phases, List<Boon> toTrack, int phaseIndex)
        {
            if (_boonDistribution.Count == 0)
            {
                SetBoonDistribution(log, phases, toTrack);
            }
            return _boonDistribution[phaseIndex];
        }
        public Dictionary<long, BoonsGraphModel> GetBoonGraphs(ParsedLog log, List<PhaseData> phases, List<Boon> toTrack)
        {
            if (_boonDistribution.Count == 0)
            {
                SetBoonDistribution(log, phases, toTrack);
            }
            return _boonPoints;
        }
        public Dictionary<long, long> GetBoonPresence(ParsedLog log, List<PhaseData> phases, List<Boon> toTrack, int phaseIndex)
        {
            if (_boonDistribution.Count == 0)
            {
                SetBoonDistribution(log, phases, toTrack);
            }
            return _boonPresence[phaseIndex];
        }

        public Dictionary<long, Dictionary<int, string[]>> GetExtraBoonData(ParsedLog log, List<PhaseData> phases, List<Boon> toTrack)
        {
            if (_boonDistribution.Count == 0)
            {
                SetBoonDistribution(log, phases, toTrack);
            }
            return _boonExtra;
        }

        public Dictionary<long, long> GetCondiPresence(ParsedLog log, List<PhaseData> phases, List<Boon> toTrack, int phaseIndex)
        {
            if (_boonDistribution.Count == 0)
            {
                SetBoonDistribution(log, phases, toTrack);
            }
            return _condiPresence[phaseIndex];
        }
        public void InitCombatReplay(ParsedLog log, int pollingRate, bool trim, bool forceInterpolate)
        {
            if (log.GetMovementData().Count == 0)
            {
                // no movement data, old arc version
                return;
            }
            if (Replay == null)
            {
                Replay = new CombatReplay();
                SetMovements(log);
                Replay.PollingRate(pollingRate, log.GetBossData().GetAwareDuration(), forceInterpolate);
                SetCombatReplayIcon(log);
                if (trim)
                {
                    CombatItem test = log.GetCombatList().FirstOrDefault(x => x.GetSrcAgent() == Agent.GetAgent() && (x.IsStateChange().IsDead() || x.IsStateChange().IsDespawn()));
                    if (test != null)
                    {
                        Replay.Trim(Agent.GetFirstAware() - log.GetBossData().GetFirstAware(), test.GetTime() - log.GetBossData().GetFirstAware());
                    }
                    else
                    {
                        Replay.Trim(Agent.GetFirstAware() - log.GetBossData().GetFirstAware(), Agent.GetLastAware() - log.GetBossData().GetFirstAware());
                    }
                }
                SetAdditionalCombatReplayData(log, pollingRate);
            }
        }
        public CombatReplay GetCombatReplay()
        {
            return Replay;
        }

        public long GetDeath(ParsedLog log, long start, long end)
        {
            long offset = log.GetBossData().GetFirstAware();
            CombatItem dead = log.GetCombatList().LastOrDefault(x => x.GetSrcInstid() == Agent.GetInstid() && x.IsStateChange().IsDead() && x.GetTime() >= start + offset && x.GetTime() <= end + offset);
            if (dead != null && dead.GetTime() > 0)
            {
                return dead.GetTime();
            }
            return 0;
        }

        // private getters
        private BoonMap GetBoonMap(ParsedLog log, List<Boon> toTrack)
        {
            BoonMap boonMap = new BoonMap
            {
                toTrack
            };
            // Fill in Boon Map
            long timeStart = log.GetBossData().GetFirstAware();
            List<long> tableIds = Boon.GetBoonList().Select(x => x.GetID()).ToList();
            tableIds.AddRange(Boon.GetOffensiveTableList().Select(x => x.GetID()));
            tableIds.AddRange(Boon.GetDefensiveTableList().Select(x => x.GetID()));
            tableIds.AddRange(Boon.GetCondiBoonList().Select(x => x.GetID()));
            foreach (CombatItem c in log.GetBoonData())
            {
                if (!boonMap.ContainsKey(c.GetSkillID()))
                {
                    continue;
                }
                long time = c.GetTime() - timeStart;
                ushort dst = c.IsBuffremove() == ParseEnum.BuffRemove.None ? c.GetDstInstid() : c.GetSrcInstid();
                if (Agent.GetInstid() == dst)
                {
                    // don't add buff initial table boons and buffs in non golem mode, for others overstack is irrevelant
                    if (c.IsStateChange() == ParseEnum.StateChange.BuffInitial && (log.IsBenchmarkMode() || !tableIds.Contains(c.GetSkillID())))
                    {
                        List<BoonLog> loglist = boonMap[c.GetSkillID()];
                        loglist.Add(new BoonLog(0, 0, long.MaxValue, 0));
                    }
                    else if (time >= 0 && time < log.GetBossData().GetAwareDuration())
                    {
                        if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                        {
                            ushort src = c.GetSrcMasterInstid() > 0 ? c.GetSrcMasterInstid() : c.GetSrcInstid();
                            List<BoonLog> loglist = boonMap[c.GetSkillID()];

                            if (loglist.Count == 0 && c.GetOverstackValue() > 0)
                            {
                                loglist.Add(new BoonLog(0, 0, time, 0));
                            }
                            loglist.Add(new BoonLog(time, src, c.GetValue(), 0));
                        }
                        else if (Boon.RemovePermission(c.GetSkillID(), c.IsBuffremove(), c.GetIFF()) && time < log.GetBossData().GetAwareDuration() - 50)
                        {
                            if (c.IsBuffremove() == ParseEnum.BuffRemove.All)//All
                            {
                                List<BoonLog> loglist = boonMap[c.GetSkillID()];
                                if (loglist.Count == 0)
                                {
                                    loglist.Add(new BoonLog(0, 0, time, 0));
                                }
                                else
                                {
                                    for (int cnt = loglist.Count - 1; cnt >= 0; cnt--)
                                    {
                                        BoonLog curBL = loglist[cnt];
                                        if (curBL.GetOverstack() == 0 && curBL.GetTime() + curBL.GetValue() > time)
                                        {
                                            long subtract = (curBL.GetTime() + curBL.GetValue()) - time;
                                            curBL.AddValue(-subtract);
                                            // add removed as overstack
                                            curBL.AddOverstack((uint)subtract);
                                        }
                                    }
                                }
                            }
                            else if (c.IsBuffremove() == ParseEnum.BuffRemove.Single)//Single
                            {
                                List<BoonLog> loglist = boonMap[c.GetSkillID()];
                                if (loglist.Count == 0)
                                {
                                    loglist.Add(new BoonLog(0, 0, time, 0));
                                }
                                else
                                {
                                    int cnt = loglist.Count - 1;
                                    BoonLog curBL = loglist[cnt];
                                    if (curBL.GetOverstack() == 0 && curBL.GetTime() + curBL.GetValue() > time)
                                    {
                                        long subtract = (curBL.GetTime() + curBL.GetValue()) - time;
                                        curBL.AddValue(-subtract);
                                        // add removed as overstack
                                        curBL.AddOverstack((uint)subtract);
                                    }
                                }
                            }
                            else if (c.IsBuffremove() == ParseEnum.BuffRemove.Manual)//Manuel
                            {
                                List<BoonLog> loglist = boonMap[c.GetSkillID()];
                                if (loglist.Count == 0)
                                {
                                    loglist.Add(new BoonLog(0, 0, time, 0));
                                }
                                else
                                {
                                    for (int cnt = loglist.Count - 1; cnt >= 0; cnt--)
                                    {
                                        BoonLog curBL = loglist[cnt];
                                        long ctime = curBL.GetTime() + curBL.GetValue();
                                        if (curBL.GetOverstack() == 0 && ctime > time)
                                        {
                                            long subtract = (curBL.GetTime() + curBL.GetValue()) - time;
                                            curBL.AddValue(-subtract);
                                            // add removed as overstack
                                            curBL.AddOverstack((uint)subtract);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return boonMap;
        }
        private void SetMovements(ParsedLog log)
        {
            foreach (CombatItem c in log.GetMovementData())
            {
                if (c.GetSrcInstid() != Agent.GetInstid())
                {
                    continue;
                }
                long time = c.GetTime() - log.GetBossData().GetFirstAware();
                byte[] xy = BitConverter.GetBytes(c.GetDstAgent());
                float x = BitConverter.ToSingle(xy, 0);
                float y = BitConverter.ToSingle(xy, 4);
                if (c.IsStateChange() == ParseEnum.StateChange.Position)
                {
                    Replay.AddPosition(new Point3D(x, y, c.GetValue(), time));
                }
                else
                {
                    Replay.AddVelocity(new Point3D(x, y, c.GetValue(), time));
                }
            }
        }
        protected abstract void SetAdditionalCombatReplayData(ParsedLog log, int pollingRate);
        protected abstract void SetCombatReplayIcon(ParsedLog log);

        private void GenerateExtraBoonData(ParsedLog log, long boonid, Point[] accurateUptime, List<PhaseData> phases)
        {

            switch (boonid)
            {
                // Frost Spirit
                case 50421:
                    _boonExtra[boonid] = new Dictionary<int, string[]>();
                    for (int i = 0; i < phases.Count; i++)
                    {
                        List<DamageLog> dmLogs = GetJustPlayerDamageLogs(0, log, phases[i].GetStart(), phases[i].GetEnd());
                        int totalDamage = Math.Max(dmLogs.Sum(x => x.GetDamage()), 1);
                        int totalBossDamage = Math.Max(dmLogs.Where(x => x.GetDstInstidt() == log.GetBossData().GetInstid()).Sum(x => x.GetDamage()), 1);
                        List<DamageLog> effect = dmLogs.Where(x => accurateUptime[(int)x.GetTime()].Y > 0 && x.IsCondi() == 0).ToList();
                        List<DamageLog> effectBoss = effect.Where(x => x.GetDstInstidt() == log.GetBossData().GetInstid()).ToList();
                        int damage = (int)(effect.Sum(x => x.GetDamage()) / 21.0);
                        int bossDamage = (int)(effectBoss.Sum(x => x.GetDamage()) / 21.0);
                        double gain = Math.Round(100.0 * ((double)totalDamage / (totalDamage - damage) - 1.0), 2);
                        double gainBoss = Math.Round(100.0 * ((double)totalBossDamage / (totalBossDamage - bossDamage) - 1.0), 2);
                        string gainText = effect.Count + " out of " + dmLogs.Count(x => x.IsCondi() == 0) + " hits <br> Pure Frost Spirit Damage: "
                                + damage + "<br> Effective Damage Increase: " + gain + "%";
                        string gainBossText = effectBoss.Count + " out of " + dmLogs.Count(x => x.GetDstInstidt() == log.GetBossData().GetInstid() && x.IsCondi() == 0) + " hits <br> Pure Frost Spirit Damage: "
                                + bossDamage + "<br> Effective Damage Increase: " + gainBoss + "%";
                        _boonExtra[boonid][i] = new [] { gainText, gainBossText };
                    }
                    break;
                // Kalla Elite
                case 45026:
                    _boonExtra[boonid] = new Dictionary<int, string[]>();
                    for (int i = 0; i < phases.Count; i++)
                    {
                        List<DamageLog> dmLogs = GetJustPlayerDamageLogs(0, log, phases[i].GetStart(), phases[i].GetEnd());
                        int totalDamage = Math.Max(dmLogs.Sum(x => x.GetDamage()), 1);
                        int totalBossDamage = Math.Max(dmLogs.Where(x => x.GetDstInstidt() == log.GetBossData().GetInstid()).Sum(x => x.GetDamage()), 1);
                        int effectCount = dmLogs.Count(x => accurateUptime[(int)x.GetTime()].Y > 0 && x.IsCondi() == 0);
                        int effectBossCount = dmLogs.Count(x => accurateUptime[(int)x.GetTime()].Y > 0 && x.IsCondi() == 0 && x.GetDstInstidt() == log.GetBossData().GetInstid());
                        int damage = (int)(effectCount * (325 + 3000 * 0.04));
                        int bossDamage = (int)(effectBossCount * (325 + 3000 * 0.04));
                        double gain = Math.Round(100.0 * ((double)(totalDamage + damage) / totalDamage - 1.0), 2);
                        double gainBoss = Math.Round(100.0 * ((double)(totalBossDamage + bossDamage) / totalBossDamage - 1.0), 2);
                        string gainText = effectCount + " out of " + dmLogs.Count(x => x.IsCondi() == 0) + " hits <br> Estimated Soulcleave Damage: "
                                + damage + "<br> Estimated Damage Increase: " + gain + "%";
                        string gainBossText = effectBossCount + " out of " + dmLogs.Count(x => x.GetDstInstidt() == log.GetBossData().GetInstid() && x.IsCondi() == 0) + " hits <br> Estimated Soulcleave Damage: "
                                + bossDamage + "<br> Estimated Damage Increase: " + gainBoss + "%";
                        _boonExtra[boonid][i] = new [] { gainText, gainBossText };
                    }
                    break;
                // GoE
                case 31803:
                    _boonExtra[boonid] = new Dictionary<int, string[]>();
                    for (int i = 0; i < phases.Count; i++)
                    {
                        List<DamageLog> dmLogs = GetJustPlayerDamageLogs(0, log, phases[i].GetStart(), phases[i].GetEnd());
                        int totalDamage = Math.Max(dmLogs.Sum(x => x.GetDamage()), 1);
                        int totalBossDamage = Math.Max(dmLogs.Where(x => x.GetDstInstidt() == log.GetBossData().GetInstid()).Sum(x => x.GetDamage()), 1);
                        List<DamageLog> effect = dmLogs.Where(x => accurateUptime[(int)x.GetTime()].Y > 0 && x.IsCondi() == 0).ToList();
                        List<DamageLog> effectBoss = effect.Where(x => x.GetDstInstidt() == log.GetBossData().GetInstid()).ToList();
                        int damage = (int)(effect.Sum(x => x.GetDamage()) / 11.0);
                        int bossDamage = (int)(effectBoss.Sum(x => x.GetDamage()) / 11.0);
                        double gain = Math.Round(100.0 * ((double)totalDamage / (totalDamage - damage) - 1.0), 2);
                        double gainBoss = Math.Round(100.0 * ((double)totalBossDamage / (totalBossDamage - bossDamage) - 1.0), 2);
                        string gainText = effect.Count + " out of " + dmLogs.Count(x => x.IsCondi() == 0) + " hits <br> Pure GoE Damage: "
                                + damage + "<br> Effective Damage Increase: " + gain + "%";
                        string gainBossText = effectBoss.Count + " out of " + dmLogs.Count(x => x.GetDstInstidt() == log.GetBossData().GetInstid() && x.IsCondi() == 0) + " hits <br> Pure GoE Damage: "
                                + bossDamage + "<br> Effective Damage Increase: " + gainBoss + "%";
                        _boonExtra[boonid][i] = new [] { gainText, gainBossText };
                    }
                    break;
            }
        }

        private void SetBoonDistribution(ParsedLog log, List<PhaseData> phases, List<Boon> toTrack)
        {
            BoonMap toUse = GetBoonMap(log, toTrack);
            long dur = log.GetBossData().GetAwareDuration();
            int fightDuration = (int)(dur) / 1000;
            // Init boon/condi presence points
            BoonsGraphModel boonPresencePoints = new BoonsGraphModel("Number of Boons");
            BoonsGraphModel condiPresencePoints = new BoonsGraphModel("Number of Conditions");
            HashSet<long> extraDataID = new HashSet<long>
            {
                50421,
                45026,
                31803
            };
            for (int i = 0; i <= fightDuration; i++)
            {
                boonPresencePoints.GetBoonChart().Add(new Point(i, 0));
                condiPresencePoints.GetBoonChart().Add(new Point(i, 0));
            }
            for (int i = 0; i < phases.Count; i++)
            {
                _boonDistribution.Add(new BoonDistribution());
                _boonPresence.Add(new Dictionary<long, long>());
                _condiPresence.Add(new Dictionary<long, long>());
            }

            var toFill = new Point[dur + 1];
            var toFillPresence = new Point[dur + 1];

            long death = GetDeath(log, 0, dur) - log.GetBossData().GetFirstAware();

            foreach (Boon boon in toTrack)
            {
                long boonid = boon.GetID();
                if (toUse.TryGetValue(boonid, out var logs) && logs.Count != 0)
                {
                    if (_boonDistribution[0].ContainsKey(boonid))
                    {
                        continue;
                    }
                    bool requireExtraData = extraDataID.Contains(boonid);
                    var simulator = boon.CreateSimulator(log);
                    simulator.Simulate(logs, dur);
                    if (death > 0 && GetCastLogs(log, death + 5000, fightDuration).Count == 0)
                    {
                        simulator.Trim(death);
                    }
                    else
                    {
                        simulator.Trim(dur);
                    }
                    var simulation = simulator.GetSimulationResult();
                    var updateBoonPresence = Boon.GetBoonList().Any(x => x.GetID() == boonid);
                    var updateCondiPresence = boonid != 873 && Boon.GetCondiBoonList().Any(x => x.GetID() == boonid);
                    foreach (var simul in simulation)
                    {
                        for (int i = 0; i < phases.Count; i++)
                        {
                            var phase = phases[i];
                            if (!_boonDistribution[i].TryGetValue(boonid, out var distrib))
                            {
                                distrib = new Dictionary<ushort, OverAndValue>();
                                _boonDistribution[i].Add(boonid, distrib);
                            }
                            if (updateBoonPresence)
                                Add(_boonPresence[i], boonid, simul.GetItemDuration(phase.GetStart(), phase.GetEnd()));
                            if (updateCondiPresence)
                                Add(_condiPresence[i], boonid, simul.GetItemDuration(phase.GetStart(), phase.GetEnd()));
                            foreach (ushort src in simul.GetSrc())
                            {
                                if (distrib.TryGetValue(src, out var toModify))
                                {
                                    toModify.Value += simul.GetDuration(src, phase.GetStart(), phase.GetEnd());
                                    toModify.Overstack += simul.GetOverstack(src, phase.GetStart(), phase.GetEnd());
                                    distrib[src] = toModify;
                                }
                                else
                                {
                                    distrib.Add(src, new OverAndValue(
                                        simul.GetDuration(src, phase.GetStart(), phase.GetEnd()),
                                        simul.GetOverstack(src, phase.GetStart(), phase.GetEnd())));
                                }
                            }
                        }
                    }
                    // Graphs
                    // full precision
                    for (int i = 0; i <= dur; i++)
                    {
                        toFill[i] = new Point(i, 0);
                        if (updateBoonPresence || updateCondiPresence)
                        {
                            toFillPresence[i] = new Point(i, 0);
                        }
                    }
                    foreach (var simul in simulation)
                    {
                        int start = (int)simul.GetStart();
                        int end = (int)simul.GetEnd();

                        bool present = simul.GetItemDuration() > 0;
                        for (int i = start; i <= end; i++)
                        {
                            toFill[i] = new Point(i, simul.GetStack(i));
                            if (updateBoonPresence || updateCondiPresence)
                            {
                                toFillPresence[i] = new Point(i, present ? 1 : 0);
                            }
                        }
                    }
                    if (requireExtraData)
                    {
                        GenerateExtraBoonData(log, boonid, toFill, phases);
                    }
                    // reduce precision to seconds
                    var reducedPrecision = new List<Point>(capacity: fightDuration + 1);
                    var boonPresence = boonPresencePoints.GetBoonChart();
                    var condiPresence = condiPresencePoints.GetBoonChart();
                    if (Replay != null && (updateCondiPresence || updateBoonPresence || Boon.GetDefensiveTableList().Any(x => x.GetID() == boonid) || Boon.GetOffensiveTableList().Any(x => x.GetID() == boonid)))
                    {
                        foreach (int time in Replay.GetTimes())
                        {
                            Replay.AddBoon(boonid, toFill[time].Y);
                        }

                    }
                    for (int i = 0; i <= fightDuration; i++)
                    {
                        reducedPrecision.Add(new Point(i, toFill[1000 * i].Y));
                        if (updateBoonPresence)
                        {
                            boonPresence[i] = new Point(i, boonPresence[i].Y + toFillPresence[1000 * i].Y);
                        }
                        if (updateCondiPresence)
                        {
                            condiPresence[i] = new Point(i, condiPresence[i].Y + toFillPresence[1000 * i].Y);
                        }
                    }
                    _boonPoints[boonid] = new BoonsGraphModel(boon.GetName(), reducedPrecision);
                }
            }
            _boonPoints[-2] = boonPresencePoints;
            _boonPoints[-3] = condiPresencePoints;
        }
        private void SetMinions(ParsedLog log)
        {
            List<AgentItem> combatMinion = log.GetAgentData().GetNPCAgentList().Where(x => x.GetMasterAgent() == Agent.GetAgent()).ToList();
            Dictionary<string, Minions> auxMinions = new Dictionary<string, Minions>();
            foreach (AgentItem agent in combatMinion)
            {
                string id = agent.GetName();
                if (!auxMinions.ContainsKey(id))
                {
                    auxMinions[id] = new Minions(id.GetHashCode());
                }
                auxMinions[id].Add(new Minion(agent));
            }
            foreach (KeyValuePair<string, Minions> pair in auxMinions)
            {
                if (pair.Value.GetDamageLogs(0, log, 0, log.GetBossData().GetAwareDuration()).Count > 0)
                {
                    _minions[pair.Key] = pair.Value;
                }
            }
        }

        protected override void SetDamageLogs(ParsedLog log)
        {
            long timeStart = log.GetBossData().GetFirstAware();
            foreach (CombatItem c in log.GetDamageData())
            {
                if (Agent.GetInstid() == c.GetSrcInstid() && c.GetTime() > log.GetBossData().GetFirstAware() && c.GetTime() < log.GetBossData().GetLastAware())//selecting player or minion as caster
                {
                    long time = c.GetTime() - timeStart;
                    AddDamageLog(time, c);
                }
            }
            Dictionary<string, Minions> minionsList = GetMinions(log);
            foreach (Minions mins in minionsList.Values)
            {
                DamageLogs.AddRange(mins.GetDamageLogs(0, log, 0, log.GetBossData().GetAwareDuration()));
            }
            DamageLogs.Sort((x, y) => x.GetTime() < y.GetTime() ? -1 : 1);
        }
        protected override void SetCastLogs(ParsedLog log)
        {
            long timeStart = log.GetBossData().GetFirstAware();
            CastLog curCastLog = null;
            foreach (CombatItem c in log.GetCastData())
            {
                if (!(c.GetTime() > log.GetBossData().GetFirstAware() && c.GetTime() < log.GetBossData().GetLastAware()))
                {
                    continue;
                }
                ParseEnum.StateChange state = c.IsStateChange();
                if (state == ParseEnum.StateChange.Normal)
                {
                    if (Agent.GetInstid() == c.GetSrcInstid())//selecting player as caster
                    {
                        if (c.IsActivation().IsCasting())
                        {
                            long time = c.GetTime() - timeStart;
                            curCastLog = new CastLog(time, c.GetSkillID(), c.GetValue(), c.IsActivation());
                            CastLogs.Add(curCastLog);
                        }
                        else
                        {
                            if (curCastLog != null)
                            {
                                if (curCastLog.GetID() == c.GetSkillID())
                                {
                                    curCastLog.SetEndStatus(c.GetValue(), c.IsActivation());
                                    curCastLog = null;
                                }
                            }
                        }

                    }
                }
                else if (state == ParseEnum.StateChange.WeaponSwap)
                {//Weapon swap
                    if (Agent.GetInstid() == c.GetSrcInstid())//selecting player as caster
                    {
                        if ((int)c.GetDstAgent() == 4 || (int)c.GetDstAgent() == 5)
                        {
                            long time = c.GetTime() - timeStart;
                            CastLog swapLog = new CastLog(time, -2, (int)c.GetDstAgent(), c.IsActivation());
                            CastLogs.Add(swapLog);
                        }
                    }
                }
            }
        }
        private static void Add<T>(Dictionary<T, long> dictionary, T key, long value)
        {
            if (dictionary.TryGetValue(key, out var existing))
            {
                dictionary[key] = existing + value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

    }
}
