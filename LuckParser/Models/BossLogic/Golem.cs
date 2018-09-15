﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Golem : BossLogic
    {
        public Golem()
        {
            Mode = ParseMode.Golem;          
        }

        public override List<PhaseData> GetPhases(Boss boss, ParsedLog log, List<CastLog> castLogs)
        {
            List<PhaseData> phases = GetInitialPhase(log);          
            return phases;
        }

        public override void SetSuccess(CombatData combatData, LogData logData, FightData fightData, List<Player> pList)
        {
            CombatItem pov = combatData.FirstOrDefault(x => x.IsStateChange == ParseEnum.StateChange.PointOfView);
            if (pov != null)
            {
                // to make sure that the logging starts when the PoV starts attacking (in case there is a slave with them)
                CombatItem enterCombat = combatData.FirstOrDefault(x => x.SrcAgent == pov.SrcAgent && x.IsStateChange == ParseEnum.StateChange.EnterCombat);
                if (enterCombat != null)
                {
                    fightData.FightStart = enterCombat.Time;
                }
            }
            CombatItem lastDamageTaken = combatData.GetDamageTakenData(fightData.InstID).LastOrDefault(x => x.Value > 0 || x.BuffDmg > 0);
            if (lastDamageTaken != null)
            {
                fightData.FightEnd = lastDamageTaken.Time;
            }
            if (fightData.HealthOverTime.Count > 0)
            {
                logData.Success = fightData.HealthOverTime.Last().Y < 200;
            }
        }
    }
}
