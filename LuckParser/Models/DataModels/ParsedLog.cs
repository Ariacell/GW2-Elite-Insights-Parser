﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuckParser.Models.ParseModels;

namespace LuckParser.Models.DataModels
{
    public class ParsedLog
    {
        private LogData _logData;
        private BossData _bossData;
        private AgentData _agentData;
        private SkillData _skillData;
        private CombatData _combatData;
        private MechanicData _mechData;
        private List<Player> _playerList;
        private Boss _boss;

        

        public ParsedLog(LogData logData, BossData bossData, AgentData agentData, SkillData skillData, 
                CombatData combatData, List<Player> playerList, Boss boss)
        {
            _logData = logData;
            _bossData = bossData;
            _agentData = agentData;
            _skillData = skillData;
            _combatData = combatData;
            _playerList = playerList;
            _boss = boss;
            _mechData = new MechanicData(bossData);
        }

        public BossData GetBossData()
        {
            return _bossData;
        }

        public Boss GetBoss()
        {
            return _boss;
        }

        public CombatData GetCombatData()
        {
            return _combatData;
        }

        public AgentData GetAgentData()
        {
            return _agentData;
        }

        public List<Player> GetPlayerList()
        {
            return _playerList;
        }

        public MechanicData GetMechanicData()
        {
            return _mechData;
        }

        public SkillData GetSkillData()
        {
            return _skillData;
        }

        public LogData GetLogData()
        {
            return _logData;
        }

        public CombatData GetCombatList()
        {
            return _combatData;
        }

        public List<CombatItem> GetBoonData()
        {
            return _combatData.GetBoonData();
        }

        public List<CombatItem> GetDamageData()
        {
            return _combatData.GetDamageData();
        }

        public List<CombatItem> GetCastData()
        {
            return _combatData.GetCastData();
        }

        public List<CombatItem> GetDamageTakenData()
        {
            return _combatData.GetDamageTakenData();
        }

        public bool IsBenchmarkMode()
        {
            return _bossData.GetBossBehavior().GetMode() == BossLogic.ParseMode.Golem;
        }

        /*public List<CombatItem> getHealingData()
        {
            return _combatData.getHealingData();
        }

        public List<CombatItem> getHealingReceivedData()
        {
            return _combatData.getHealingReceivedData();
        }*/

        public List<CombatItem> GetMovementData()
        {
            return _combatData.GetMovementData();
        }     
    }
}
