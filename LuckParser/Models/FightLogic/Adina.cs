﻿using LuckParser.Parser;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static LuckParser.Parser.ParseEnum.TrashIDS;
using System.Drawing;

namespace LuckParser.Models.Logic
{
    public class Adina : RaidLogic
    {
        public Adina(ushort triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>()
            {
                new PlayerBoonApplyMechanic(56593, "Radiant Blindness", new MechanicPlotlySetting("circle","rgb(255,0,0)"), "R.Blind", "Blindess applied if looking at Adina", "Radiant Blindness", 0),
                new PlayerBoonApplyMechanic(56586, "Eroding Curse", new MechanicPlotlySetting("cross","rgb(255,0,100)"), "Curse", "Stacking damage debuff from Hand of Erosion", "Eroding Curse", 0),
                new HitOnPlayerMechanic(56648, " Boulder Barrage", new MechanicPlotlySetting("square","rgb(255,0,0)"), "Boulder", "Hit by boulder thrown during pillars", "Boulder Barrage", 0),
            });
            Extension = "adina";
            IconUrl = "https://wiki.guildwars2.com/images/d/d2/Guild_emblem_004.png";
        }

        public override void SpecialParse(FightData fightData, AgentData agentData, List<CombatItem> combatData)
        {
            List<CombatItem> attackTargets = combatData.Where(x => x.IsStateChange == ParseEnum.StateChange.AttackTarget).ToList();
            long first = combatData.Count > 0 ? combatData.First().LogTime : 0;
            long final = combatData.Count > 0 ? combatData.Last().LogTime : 0;
            foreach (CombatItem at in attackTargets)
            {
                AgentItem hand = agentData.GetAgent(at.DstAgent, at.LogTime);
                AgentItem atAgent = agentData.GetAgent(at.SrcAgent, at.LogTime);
                List<CombatItem> attackables = combatData.Where(x => x.IsStateChange == ParseEnum.StateChange.Targetable && x.SrcAgent == atAgent.Agent && x.LogTime <= atAgent.LastAwareLogTime && x.LogTime >= atAgent.FirstAwareLogTime).ToList();
                List<long> attackOn = attackables.Where(x => x.DstAgent == 1 && x.LogTime >= first + 2000).Select(x => x.LogTime).ToList();
                List<long> attackOff = attackables.Where(x => x.DstAgent == 0 && x.LogTime >= first + 2000).Select(x => x.LogTime).ToList();
                List<CombatItem> posFacingHP = combatData.Where(x => x.SrcAgent == hand.Agent && x.LogTime >= hand.FirstAwareLogTime && hand.LastAwareLogTime >= x.LogTime && (x.IsStateChange == ParseEnum.StateChange.Position || x.IsStateChange == ParseEnum.StateChange.Rotation || x.IsStateChange == ParseEnum.StateChange.MaxHealthUpdate)).ToList();
                for (int i = 0; i < attackOn.Count; i++)
                {
                    long start = attackOn[i];
                    long end = final;
                    if (i <= attackOff.Count - 1)
                    {
                        end = attackOff[i];
                    }
                    AgentItem extra = agentData.AddCustomAgent(start, end, AgentItem.AgentType.Gadget, hand.Name, hand.Prof, (ushort)HandOfErosion, hand.Toughness, hand.Healing, hand.Condition, hand.Concentration, hand.HitboxWidth, hand.HitboxHeight);
                    foreach (CombatItem c in combatData.Where(x => x.SrcAgent == hand.Agent &&x.LogTime >= extra.FirstAwareLogTime && x.LogTime <= extra.LastAwareLogTime))
                    {
                        c.OverrideSrcValues(extra.Agent, extra.InstID);
                    }
                    foreach (CombatItem c in combatData.Where(x => x.DstAgent == hand.Agent && x.LogTime >= extra.FirstAwareLogTime && x.LogTime <= extra.LastAwareLogTime))
                    {
                        c.OverrideDstValues(extra.Agent, extra.InstID);
                    }
                    foreach (CombatItem c in posFacingHP)
                    {
                        CombatItem cExtra = new CombatItem(c);
                        cExtra.OverrideTime(extra.FirstAwareLogTime);
                        cExtra.OverrideSrcValues(extra.Agent, extra.InstID);
                        combatData.Add(cExtra);
                    }
                }            
            }
            combatData.Sort((x, y) => x.LogTime.CompareTo(y.LogTime));
        }

        protected override List<ushort> GetFightTargetsIDs()
        {
            return new List<ushort>()
            {
                (ushort)ParseEnum.TargetIDS.Adina,
                (ushort)HandOfErosion,
                (ushort)HandOfEruption
            };
        }

        public override List<PhaseData> GetPhases(ParsedLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            Target mainTarget = Targets.Find(x => x.ID == (ushort)ParseEnum.TargetIDS.Adina && x.AgentItem.Type == AgentItem.AgentType.NPC);
            if (mainTarget == null)
            {
                throw new InvalidOperationException("Main target of the fight not found");
            }
            phases[0].Targets.Add(mainTarget);
            if (!requirePhases)
            {
                return phases;
            }
            List<AbstractCastEvent> quantumQuakes = mainTarget.GetCastLogs(log, 0, log.FightData.FightDuration).Where(x => x.SkillId == 56035).ToList();
            List<AbstractBuffEvent> invuls = GetFilteredList(log.CombatData, 762, mainTarget, true);
            long start = 0, end = 0;
            for (int i = 0; i < invuls.Count; i++)
            {
                AbstractBuffEvent be = invuls[i];
                if (be is BuffApplyEvent)
                {
                    start = be.Time;
                    if (i == invuls.Count - 1)
                    {
                        phases.Add(new PhaseData(start, log.FightData.FightDuration)
                        {
                            Name = "Split " + (i / 2 + 1)
                        });
                    }
                }
                else
                {
                    end = be.Time;
                    phases.Add(new PhaseData(start, end)
                    {
                        Name = "Split " + (i / 2 + 1)
                    });
                }
            }
            List<PhaseData> mainPhases = new List<PhaseData>();
            start = 0;
            end = 0;
            for (int i = 1; i < phases.Count; i++)
            {
                AbstractCastEvent qQ = quantumQuakes[i - 1];
                end = qQ.Time;
                mainPhases.Add(new PhaseData(start, end)
                {
                    Name = "Phase " + i
                });
                PhaseData split = phases[i];
                AddTargetsToPhase(split, new List<ushort> { (ushort)HandOfErosion, (ushort)HandOfEruption }, log);
                start = split.End;
                if (i == phases.Count - 1 && start != log.FightData.FightDuration)
                {
                    mainPhases.Add(new PhaseData(start, log.FightData.FightDuration)
                    {
                        Name = "Phase " + (i + 1)
                    });
                }
            }
            foreach(PhaseData phase in mainPhases)
            {
                phase.Targets.Add(mainTarget);
            }
            phases.AddRange(mainPhases);
            phases.Sort((x, y) => x.Start.CompareTo(y.Start));
            return phases;
        }


        protected override CombatReplayMap GetCombatMapInternal()
        {
            return new CombatReplayMap("",
                            (800, 800),
                            (-21504, -21504, 24576, 24576),
                            (-21504, -21504, 24576, 24576),
                            (33530, 34050, 35450, 35970));
        }

        public override int IsCM(CombatData combatData, AgentData agentData, FightData fightData)
        {
            Target target = Targets.Find(x => x.ID == (ushort)ParseEnum.TargetIDS.Adina && x.AgentItem.Type == AgentItem.AgentType.NPC);
            if (target == null)
            {
                throw new InvalidOperationException("Target for CM detection not found");
            }
            return (target.GetHealth(combatData) > 23e6) ? 1 : 0;
        }
    }
}
