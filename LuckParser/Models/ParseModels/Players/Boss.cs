﻿using LuckParser.Controllers;
using LuckParser.Models.DataModels;
using System;
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

        private List<PhaseData> phases = new List<PhaseData>();
        private List<long> phaseData = new List<long>();
        private CombatReplayMap map = null;
        private List<Mob> thrashMobs = new List<Mob>();

        public List<PhaseData> getPhases(ParsedLog log, bool getAllPhases)
        {

            if (phases.Count == 0)
            {
                long fight_dur = log.getBossData().getAwareDuration();
                if (!getAllPhases)
                {
                    phases.Add(new PhaseData(0, fight_dur));
                    phases[0].setName("Full Fight");
                    return phases;
                }
                getCastLogs(log, 0, fight_dur);
                phases = log.getBossData().getBossBehavior().getPhases(this, log, cast_logs);
            }
            return phases;
        }

        public void addPhaseData(long data)
        {
            phaseData.Add(data);
        }

        public List<long> getPhaseData()
        {
            return phaseData;
        }

        public CombatReplayMap getCombatMap(ParsedLog log)
        {
            if (map == null)
            {
                map = log.getBossData().getBossBehavior().getCombatMap();
            }
            return map;
        }

        public List<Mob> getThrashMobs()
        {
            return thrashMobs;
        }

        // Private Methods

        protected override void setDamagetakenLogs(ParsedLog log)
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

        protected override void setAdditionalCombatReplayData(ParsedLog log, int pollingRate)
        {
            List<ParseEnum.ThrashIDS> ids = log.getBossData().getBossBehavior().getAdditionalData(replay, getCastLogs(log, 0, log.getBossData().getAwareDuration()), log);
            List<AgentItem> aList = log.getAgentData().getNPCAgentList().Where(x => ids.Contains(ParseEnum.getThrashIDS(x.getID()))).ToList();
            foreach (AgentItem a in aList)
            {
                Mob mob = new Mob(a);
                mob.initCombatReplay(log, pollingRate, true, false);
                thrashMobs.Add(mob);
            }
        }

        protected override void setCombatReplayIcon(ParsedLog log)
        {
            replay.setIcon(log.getBossData().getBossBehavior().getReplayIcon());
        }

        public override void addMechanics(ParsedLog log)
        {
            MechanicData mech_data = log.getMechanicData();
            BossData boss_data = log.getBossData();
            List<Mechanic> bossMechanics = boss_data.getBossBehavior().getMechanics();
            List<Mechanic> enemyBoons = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.EnemyBoon).ToList();
            enemyBoons.AddRange(bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.EnemyBoonStrip));
            foreach (Mechanic m in enemyBoons)
            {
                foreach (CombatItem c in log.getBoonData())
                {
                    if (m.GetSkill() == c.getSkillID() && ((m.GetMechType() == Mechanic.MechType.EnemyBoon && c.isBuffremove() == ParseEnum.BuffRemove.None) || (m.GetMechType() == Mechanic.MechType.EnemyBoonStrip && c.isBuffremove() == ParseEnum.BuffRemove.Manual)))
                    {
                        AbstractMasterPlayer amp = null;
                        if (c.getDstInstid() == boss_data.getInstid())
                        {
                            amp = this;
                        }
                        else
                        {
                            amp = new Mob(log.getAgentData().GetAgent(c.getDstAgent()));
                        }
                        mech_data.AddItem(new MechanicLog(c.getTime() - boss_data.getFirstAware(), c.getSkillID(), m.GetName(), c.getValue(), amp, m.GetPlotly()));
                    }
                }
            }
            List<Mechanic> spawnMech = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.Spawn).ToList();
            foreach (Mechanic m in spawnMech)
            {
                foreach (AgentItem a in log.getAgentData().getNPCAgentList().Where(x => x.getID() == m.GetSkill()))
                {
                    AbstractMasterPlayer amp = new Mob(a);
                    mech_data.AddItem(new MechanicLog(a.getFirstAware() - boss_data.getFirstAware(), m.GetSkill(), m.GetName(), 0, amp, m.GetPlotly()));
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