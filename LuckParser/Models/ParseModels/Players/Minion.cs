﻿using LuckParser.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.ParseModels
{
    public class Minion : AbstractPlayer
    {

        public Minion(ushort master, AgentItem agent) : base(agent)
        {
        }

        protected override void setDamageLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            long min_time = Math.Max(time_start, agent.getFirstAware());
            long max_time = Math.Min(log.getBossData().getLastAware(), agent.getLastAware());
            foreach (CombatItem c in log.getDamageData())
            {
                if (agent.getInstid() == c.getSrcInstid() && c.getTime() > min_time && c.getTime() < max_time)//selecting minion as caster
                {
                    long time = c.getTime() - time_start;
                    foreach (AgentItem item in log.getAgentData().getNPCAgentList())
                    {//selecting all
                        addDamageLog(time, item.getInstid(), c, damage_logs);
                    }                
                }
            }
        }

        protected override void setFilteredLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            long min_time = Math.Max(time_start, agent.getFirstAware());
            long max_time = Math.Min(log.getBossData().getLastAware(), agent.getLastAware());
            foreach (CombatItem c in log.getDamageData())
            {
                if (agent.getInstid() == c.getSrcInstid() && c.getTime() > min_time && c.getTime() < max_time)//selecting player
                {
                    long time = c.getTime() - time_start;
                    addDamageLog(time, log.getBossData().getInstid(), c, damage_logsFiltered);
                }
            }
        }

        protected override void setCastLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            CastLog curCastLog = null;

            long min_time = Math.Max(time_start, agent.getFirstAware());
            long max_time = Math.Min(log.getBossData().getLastAware(), agent.getLastAware());
            foreach (CombatItem c in log.getCombatList())
            {
                if (!(c.getTime() > min_time && c.getTime() < max_time))
                {
                    continue;
                }
                ParseEnum.StateChange state = c.isStateChange();
                if (state == ParseEnum.StateChange.Normal)
                {
                    if (agent.getInstid() == c.getSrcInstid())//selecting player as caster
                    {
                        if (c.isActivation() != ParseEnum.Activation.None)
                        {
                            if (ParseEnum.casting(c.isActivation()))
                            {
                                long time = c.getTime() - time_start;
                                curCastLog = new CastLog(time, c.getSkillID(), c.getValue(), c.isActivation());
                            }
                            else
                            {
                                if (curCastLog != null)
                                {
                                    if (curCastLog.getID() == c.getSkillID())
                                    {
                                        curCastLog = new CastLog(curCastLog.getTime(), curCastLog.getID(), curCastLog.getExpDur(), curCastLog.startActivation(), c.getValue(), c.isActivation());
                                        cast_logs.Add(curCastLog);
                                        curCastLog = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void setDamagetakenLogs(ParsedLog log)
        {
            // nothing to do
            return;
        }
    }
}
