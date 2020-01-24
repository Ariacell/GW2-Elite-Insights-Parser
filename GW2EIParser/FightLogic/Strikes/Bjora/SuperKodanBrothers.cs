﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIParser.EIData;
using GW2EIParser.Parser;
using GW2EIParser.Parser.ParsedData;
using GW2EIParser.Parser.ParsedData.CombatEvents;

namespace GW2EIParser.Logic
{
    public class SuperKodanBrothers : StrikeMissionLogic
    {
        public SuperKodanBrothers(ushort triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
                new PlayerBuffApplyMechanic(727, "Trapped", new MechanicPlotlySetting("circle","rgb(0,0,255)"), "Trapped","Trapped", "Trapped",2500),
                new EnemyBuffApplyMechanic(58619, "Enrage", new MechanicPlotlySetting("circle","rgb(255,125,0)"), "Enrage","Enrage", "Enrage",1 << 16),
            }
            );
            Extension = "supkodbros";
            Icon = "https://i.imgur.com/lNXXbnC.png";
        }

        protected override void SetSuccessByDeath(CombatData combatData, FightData fightData, HashSet<AgentItem> playerAgents, bool all)
        {
            SetSuccessByDeath(combatData, fightData, playerAgents, all, (ushort)ParseEnum.TargetIDS.ClawOfTheFallen, (ushort)ParseEnum.TargetIDS.VoiceOfTheFallen);
        }

        public override List<PhaseData> GetPhases(ParsedLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            NPC voice = Targets.Find(x => x.ID == (ushort)ParseEnum.TargetIDS.ClawOfTheFallen);
            NPC claw = Targets.Find(x => x.ID == (ushort)ParseEnum.TargetIDS.VoiceOfTheFallen);
            if (voice == null || claw == null)
            {
                throw new InvalidOperationException("Error Encountered: Claw or Voice not found");
            }
            phases[0].Targets.Add(voice);
            phases[0].Targets.Add(claw);
            var fightEnd = log.FightData.FightEnd;
            if (!requirePhases)
            {
                return phases;
            }
            long start = 0;
            int voiceAndClawCount = 0;
            phases.AddRange(GetPhasesByInvul(log, 762, claw, false, true));
            for (int i = 1; i < phases.Count; i++)
            {
                phases[i].Name = "Phase " + i;
                phases[i].Targets.Add(claw);
                phases[i].Targets.Add(voice);
            }
            foreach (NPC voiceAndClaw in Targets.Where(x => x.ID == (ushort)ParseEnum.TargetIDS.VoiceAndClaw)) 
            {
                EnterCombatEvent enterCombat = log.CombatData.GetEnterCombatEvents(voiceAndClaw.AgentItem).FirstOrDefault();
                if (enterCombat != null)
                {
                    DeadEvent death = log.CombatData.GetDeadEvents(voiceAndClaw.AgentItem).FirstOrDefault();
                    var phase = new PhaseData(enterCombat.Time, death != null ? death.Time : fightEnd, "Voice and Claw " + ++voiceAndClawCount);
                    phase.Targets.Add(voiceAndClaw);
                    phases.Add(phase);
                    start = phase.End;
                }
            }
            AbstractBuffEvent enrage = log.CombatData.GetBuffData(58619).FirstOrDefault(x => x is BuffApplyEvent);
            if (enrage != null && start < fightEnd)
            {
                var phase = new PhaseData(enrage.Time, log.FightData.FightEnd, "Enrage");
                phase.Targets.Add(claw.AgentItem == enrage.To ? claw : voice);
                phases.Add(phase);
            }
            phases.Sort((x, y) => x.Start.CompareTo(y.Start));
            return phases;
        }

        public override string GetFightName()
        {
            return "Super Kodan Brothers";
        }

        protected override HashSet<ushort> GetUniqueTargetIDs()
        {
            return new HashSet<ushort>
            {
                (ushort)ParseEnum.TargetIDS.ClawOfTheFallen,
                (ushort)ParseEnum.TargetIDS.VoiceOfTheFallen,
            };
        }

        protected override List<ushort> GetFightTargetsIDs()
        {
            return new List<ushort>
            {
                (ushort)ParseEnum.TargetIDS.VoiceOfTheFallen,
                (ushort)ParseEnum.TargetIDS.ClawOfTheFallen,
                (ushort)ParseEnum.TargetIDS.VoiceAndClaw,
            };
        }
    }
}
