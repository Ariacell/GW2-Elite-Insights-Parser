﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LuckParser.Models
{
    public abstract class FractalLogic : BossLogic
    {
        protected FractalLogic()
        { 
            Mode = ParseMode.Fractal;
            CanCombatReplay = true;
            MechanicList.AddRange(new List<Mechanic>
            {
            new Mechanic(37695, "Flux Bomb", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Unknown, "symbol:'circle',color:'rgb(150,0,255)',size:10,", "FBmb","Flux Bomb application", "Flux Bomb",0),
            new Mechanic(36393, "Flux Bomb", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Unknown, "symbol:'circle-open',color:'rgb(150,0,255)',size:10,", "FB.dmg","Flux Bomb hit", "Flux Bomb dmg",0),
            new Mechanic(19684, "Fractal Vindicator", Mechanic.MechType.Spawn, ParseEnum.BossIDS.Unknown, "symbol:'star-diamond-open',color:'rgb(0,0,0)',size:10,", "FV.spwn","Fractal Vindicator spawned", "Vindicator spawn",0),
            });
        }

        public override List<PhaseData> GetPhases(Boss boss, ParsedLog log, List<CastLog> castLogs)
        {
            // generic method for fractals
            long start = 0;
            long end = 0;
            long fightDuration = log.GetFightData().FightDuration;
            List<PhaseData> phases = GetInitialPhase(log);
            List<CombatItem> invulsBoss = GetFilteredList(log,762,boss.InstID);        
            for (int i = 0; i < invulsBoss.Count; i++)
            {
                CombatItem c = invulsBoss[i];
                if (c.IsBuffRemove == ParseEnum.BuffRemove.None)
                {
                    end = c.Time - log.GetFightData().FightStart;
                    phases.Add(new PhaseData(start, end));
                    if (i == invulsBoss.Count - 1)
                    {
                        castLogs.Add(new CastLog(end, -5, (int)(fightDuration - end), ParseEnum.Activation.None, (int)(fightDuration - end), ParseEnum.Activation.None));
                    }
                }
                else
                {
                    start = c.Time - log.GetFightData().FightStart;
                    castLogs.Add(new CastLog(end, -5, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None));
                }
            }
            if (fightDuration - start > 5000 && start >= phases.Last().GetEnd())
            {
                phases.Add(new PhaseData(start, fightDuration));
            }
            for (int i = 1; i < phases.Count; i++)
            {
                phases[i].SetName("Phase " + i);
            }
            return phases;
        }

        protected void SetSuccessOnCombatExit(CombatData combatData, LogData logData, FightData bossData, int combatExitCount)
        {
            int combatExits = combatData.Count(x => x.SrcInstid == bossData.InstID && x.IsStateChange == ParseEnum.StateChange.ExitCombat);
            CombatItem lastDamageTaken = combatData.GetDamageTakenData(bossData.InstID).LastOrDefault(x => x.Value > 0);
            if (combatExits == combatExitCount && lastDamageTaken != null)
            {
                logData.Success = true;
                bossData.FightEnd = lastDamageTaken.Time;
            }
        }

        public override void SetSuccess(CombatData combatData, LogData logData, FightData bossData)
        {
            // check reward
            CombatItem reward = combatData.LastOrDefault(x => x.IsStateChange == ParseEnum.StateChange.Reward);
            CombatItem lastDamageTaken = combatData.GetDamageTakenData(bossData.InstID).LastOrDefault(x => x.Value > 0);
            if (lastDamageTaken != null)
            {
                if (reward != null && lastDamageTaken.Time - reward.Time < 100)
                {
                    logData.Success = true;
                    bossData.FightEnd = reward.Time;
                }
                else
                {
                    SetSuccessByDeath(combatData, logData, bossData);
                }
            }
        }

    }
}
