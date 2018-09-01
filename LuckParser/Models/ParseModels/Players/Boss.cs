﻿using LuckParser.Models.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public class Boss : AbstractMasterPlayer
    {
        // Constructors
        public Boss(AgentItem agent) : base(agent)
        {
        }

        private List<PhaseData> _phases = new List<PhaseData>();
        private readonly List<long> _phaseData = new List<long>();
        private CombatReplayMap _map;
        private readonly List<Mob> _thrashMobs = new List<Mob>();

        public List<PhaseData> GetPhases(ParsedLog log, bool getAllPhases)
        {

            if (_phases.Count == 0)
            {
                long fightDuration = log.GetBossData().GetAwareDuration();
                if (!getAllPhases)
                {
                    _phases.Add(new PhaseData(0, fightDuration));
                    _phases[0].SetName("Full Fight");
                    return _phases;
                }
                GetCastLogs(log, 0, fightDuration);
                _phases = log.GetBossData().GetBossBehavior().GetPhases(this, log, CastLogs);
            }
            return _phases;
        }

        public void AddPhaseData(long data)
        {
            _phaseData.Add(data);
        }

        public List<long> GetPhaseData()
        {
            return _phaseData;
        }

        public CombatReplayMap GetCombatMap(ParsedLog log)
        {
            if (_map == null)
            {
                _map = log.GetBossData().GetBossBehavior().GetCombatMap();
            }
            return _map;
        }

        public List<Mob> GetThrashMobs()
        {
            return _thrashMobs;
        }

        // Private Methods

        protected override void SetDamagetakenLogs(ParsedLog log)
        {
            // nothing to do
            /*long time_start = log.getBossData().getFirstAware();
            foreach (CombatItem c in log.getDamageTakenData())
            {
                if (agent.getInstid() == c.getDstInstid() && c.getTime() > log.getBossData().getFirstAware() && c.getTime() < log.getBossData().getLastAware())
                {//selecting player as target
                    long time = c.getTime() - time_start;
                    foreach (AgentItem item in log.getAgentData().getAllAgentsList())
                    {//selecting all
                        addDamageTakenLog(time, item.getInstid(), c);
                    }
                }
            }*/
        }

        protected override void SetAdditionalCombatReplayData(ParsedLog log, int pollingRate)
        {
            List<ParseEnum.ThrashIDS> ids = log.GetBossData().GetBossBehavior().GetAdditionalData(Replay, GetCastLogs(log, 0, log.GetBossData().GetAwareDuration()), log);
            List<AgentItem> aList = log.GetAgentData().GetNPCAgentList().Where(x => ids.Contains(ParseEnum.GetThrashIDS(x.GetID()))).ToList();
            foreach (AgentItem a in aList)
            {
                Mob mob = new Mob(a);
                mob.InitCombatReplay(log, pollingRate, true, false);
                _thrashMobs.Add(mob);
            }
        }

        protected override void SetCombatReplayIcon(ParsedLog log)
        {
            Replay.SetIcon(log.GetBossData().GetBossBehavior().GetReplayIcon());
        }

        public void AddMechanics(ParsedLog log)
        {
            MechanicData mechData = log.GetMechanicData();
            BossData bossData = log.GetBossData();
            List<Mechanic> bossMechanics = bossData.GetBossBehavior().GetMechanics();
            Dictionary<ushort, AbstractMasterPlayer> regroupedMobs = new Dictionary<ushort, AbstractMasterPlayer>();
            // Boons
            List<Mechanic> enemyBoons = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.EnemyBoon || x.GetMechType() == Mechanic.MechType.EnemyBoonStrip).ToList();
            foreach (Mechanic m in enemyBoons)
            {
                Mechanic.SpecialCondition condition = m.GetSpecialCondition();
                foreach (CombatItem c in log.GetBoonData(m.GetSkill()))
                {
                    if (condition != null && !condition(c.Value))
                    {
                        continue;
                    }
                    AbstractMasterPlayer amp = null;
                    if (m.GetMechType() == Mechanic.MechType.EnemyBoon && c.IsBuffRemove == ParseEnum.BuffRemove.None)
                    {
                        if (c.DstInstid == bossData.GetInstid())
                        {
                            amp = this;
                        }
                        else
                        {
                            AgentItem a = log.GetAgentData().GetAgent(c.DstAgent);
                            if (!regroupedMobs.TryGetValue(a.GetID(), out amp))
                            {
                                amp = new DummyPlayer(a);
                                regroupedMobs.Add(a.GetID(), amp);
                            }
                        }
                    }
                    else if (m.GetMechType() == Mechanic.MechType.EnemyBoonStrip && c.IsBuffRemove == ParseEnum.BuffRemove.Manual)
                    {
                        if (c.SrcInstid == bossData.GetInstid())
                        {
                            amp = this;
                        }
                        else
                        {
                            AgentItem a = log.GetAgentData().GetAgent(c.SrcAgent);
                            if (!regroupedMobs.TryGetValue(a.GetID(), out amp))
                            {
                                amp = new DummyPlayer(a);
                                regroupedMobs.Add(a.GetID(), amp);
                            }
                        }
                    }
                    if (amp != null)
                    {
                        mechData[m].Add(new MechanicLog(c.Time - bossData.GetFirstAware(), m, amp));
                    }

                }
            }
            // Casting
            List<Mechanic> enemyCasts = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.EnemyCastEnd || x.GetMechType() == Mechanic.MechType.EnemyCastStart).ToList();
            foreach (Mechanic m in enemyCasts)
            {
                Mechanic.SpecialCondition condition = m.GetSpecialCondition();
                foreach (CombatItem c in log.GetCastDataById(m.GetSkill()))
                {
                    if (condition != null && !condition(c.Value))
                    {
                        continue;
                    }
                    AbstractMasterPlayer amp = null;
                    if ((m.GetMechType() == Mechanic.MechType.EnemyCastStart && c.IsActivation.IsCasting()) || (m.GetMechType() == Mechanic.MechType.EnemyCastEnd && !c.IsActivation.IsCasting()))
                    {
                        if (c.SrcInstid == bossData.GetInstid())
                        {
                            amp = this;
                        }
                        else
                        {
                            AgentItem a = log.GetAgentData().GetAgent(c.SrcAgent);
                            if (!regroupedMobs.TryGetValue(a.GetID(), out amp))
                            {
                                amp = new DummyPlayer(a);
                                regroupedMobs.Add(a.GetID(), amp);
                            }
                        }
                    }
                    if (amp != null)
                    {
                        mechData[m].Add(new MechanicLog(c.Time - bossData.GetFirstAware(), m, amp));
                    }
                }

            }
            // Spawn
            List<Mechanic> spawnMech = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.Spawn).ToList();
            foreach (Mechanic m in spawnMech)
            {
                foreach (AgentItem a in log.GetAgentData().GetNPCAgentList().Where(x => x.GetID() == m.GetSkill()))
                {
                    if (!regroupedMobs.TryGetValue(a.GetID(), out AbstractMasterPlayer amp))
                    {
                        amp = new DummyPlayer(a);
                        regroupedMobs.Add(a.GetID(), amp);
                    }
                    mechData[m].Add(new MechanicLog(a.GetFirstAware() - bossData.GetFirstAware(), m, amp));
                }
            }
        }

        /*protected override void setHealingLogs(ParsedLog log)
        {
            // nothing to do
        }

        protected override void setHealingReceivedLogs(ParsedLog log)
        {
            // nothing to do
        }*/
    }
}