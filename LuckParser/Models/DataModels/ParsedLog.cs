﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuckParser.Models.ParseModels;

namespace LuckParser.Models.DataModels
{
    class ParsedLog
    {
        private LogData log_data;
        private BossData boss_data;
        private AgentData agent_data = new AgentData();
        private SkillData skill_data = new SkillData();
        private CombatData combat_data = new CombatData();
        private MechanicData mech_data = new MechanicData();
        private List<Player> p_list = new List<Player>();
        private Boss boss;

        public ParsedLog(LogData log_data, BossData boss_data, AgentData agent_data, SkillData skill_data, 
                CombatData combat_data, MechanicData mech_data, List<Player> p_list, Boss boss)
        {
            this.log_data = log_data;
            this.boss_data = boss_data;
            this.agent_data = agent_data;
            this.skill_data = skill_data;
            this.combat_data = combat_data;
            this.mech_data = mech_data;
            this.p_list = p_list;
            this.boss = boss;
        }

        public BossData getBossData()
        {
            return boss_data;
        }

        public Boss getBoss()
        {
            return boss;
        }

        public CombatData getCombatData()
        {
            return combat_data;
        }

        public AgentData getAgentData()
        {
            return agent_data;
        }

        public List<Player> getPlayerList()
        {
            return p_list;
        }

        public MechanicData getMechanicData()
        {
            return mech_data;
        }

        public SkillData getSkillData()
        {
            return skill_data;
        }

        public LogData getLogData()
        {
            return log_data;
        }
    }
}
