﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static LuckParser.Models.DataModels.ParseEnum.TrashIDS;

namespace LuckParser.Models
{
    public class ConjuredAmalgamate : RaidLogic
    {
        public ConjuredAmalgamate(ushort triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new Mechanic(52173, "Pulverize", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.ConjuredAmalgamate, "symbol:'square',color:'rgb(255,140,0)'", "Plvrz","Pulverize", "Pulverize",0),
                
            });
            Extension = "ca";
            IconUrl = "https://i.imgur.com/eLyIWd2.png";
        }

        protected override CombatReplayMap GetCombatMapInternal()
        {
            return new CombatReplayMap("https://i.imgur.com/9PJB5Ky.png",
                            Tuple.Create(1414, 2601),
                            Tuple.Create(-5064, -15030, -2864, -10830),
                            Tuple.Create(-21504, -21504, 24576, 24576),
                            Tuple.Create(13440, 14336, 15360, 16256));
        }

        protected override List<ushort> GetFightTargetsIDs()
        {
            return new List<ushort>
            {
                (ushort)ParseEnum.BossIDS.ConjuredAmalgamate,
                (ushort)ParseEnum.BossIDS.CARightArm,
                (ushort)ParseEnum.BossIDS.CALeftArm
            };
        }

        protected override List<ParseEnum.TrashIDS> GetTrashMobsIDS()
        {
            return new List<ParseEnum.TrashIDS>()
            {
                ConjuredGreatsword,
                ConjuredShield
            };
        }

        public override void SpecialParse(FightData fightData, AgentData agentData, List<CombatItem> combatData)
        {
            Random rnd = new Random();
            ulong agent = 1;
            while (agentData.AgentValues.Contains(agent))
            {
                agent = (ulong)rnd.Next(0, Int32.MaxValue);
            }
            ushort id = 1;
            while (agentData.InstIDValues.Contains(id))
            {
                id = (ushort)rnd.Next(0, ushort.MaxValue);
            }
            AgentItem sword = new AgentItem(agent, "Conjured Sword\0:Conjured Sword\011", "Sword", AgentItem.AgentType.Player, 0, 0, 0, 0, 20, 20)
            {
                InstID = id,
                LastAware = combatData.Last().Time,
                FirstAware = combatData.First().Time,
                MasterAgent = 0
            };
            agentData.AddCustomAgent(sword);
            foreach(CombatItem cl in combatData)
            {
                if (cl.SkillID == 52370 && cl.IsStateChange == ParseEnum.StateChange.Normal && cl.IsBuffRemove == ParseEnum.BuffRemove.None &&
                                        ((cl.IsBuff == 1 && cl.BuffDmg >= 0 && cl.Value == 0) ||
                                        (cl.IsBuff == 0 && cl.Value >= 0)) && cl.DstInstid != 0 && cl.IFF == ParseEnum.IFF.Foe)
                {
                    cl.SrcAgent = sword.Agent;
                    cl.SrcInstid = sword.InstID;
                    cl.SrcMasterInstid = 0;
                }
            }
        }

        public override void ComputeAdditionalThrashMobData(Mob mob, ParsedLog log)
        {
            switch (mob.ID)
            {
                case (ushort)ConjuredGreatsword:
                case (ushort)ConjuredShield:
                    break;
                default:
                    throw new InvalidOperationException("Unknown ID in ComputeAdditionalData");
            }
        }

        public override List<PhaseData> GetPhases(ParsedLog log, bool requirePhases)
        {
            long start = 0;
            long end = 0;
            List<PhaseData> phases = GetInitialPhase(log);
            Boss ca = Targets.Find(x => x.ID == (ushort)ParseEnum.BossIDS.ConjuredAmalgamate);
            if (ca == null)
            {
                throw new InvalidOperationException("Conjurate Amalgamate not found");
            }
            phases[0].Targets.Add(ca);
            if (!requirePhases)
            {
                return phases;
            }
            List<CombatItem> CAInvul = GetFilteredList(log, 52255, ca.InstID);
            CAInvul.RemoveAll(x => x.IsStateChange == ParseEnum.StateChange.BuffInitial);
            for (int i = 0; i < CAInvul.Count; i++)
            {
                CombatItem invul = CAInvul[i];
                if (invul.IsBuffRemove != ParseEnum.BuffRemove.None)
                {
                    end = Math.Min(invul.Time - log.FightData.FightStart, log.FightData.FightDuration);
                    phases.Add(new PhaseData(start, end));
                    if (i == CAInvul.Count - 1)
                    {
                        phases.Add(new PhaseData(end, log.FightData.FightDuration));
                    }
                }
                else
                {
                    start = Math.Min(invul.Time - log.FightData.FightStart, log.FightData.FightDuration);
                    phases.Add(new PhaseData(end, start));
                    if (i == CAInvul.Count - 1)
                    {
                        phases.Add(new PhaseData(start, log.FightData.FightDuration));
                    }
                }
            }
            for (int i = 1; i < phases.Count; i++)
            {
                string name;
                PhaseData phase = phases[i];
                if (i % 2 == 1)
                {
                    name = "Arm Phase";
                }
                else
                {
                    phase.DrawArea = true;
                    name = "Burn Phase";
                }
                phase.Name = name;
                phase.DrawEnd = true;
                phase.DrawStart = true;
            }
            Boss leftArm = Targets.Find(x => x.ID == (ushort)ParseEnum.BossIDS.CALeftArm);
            if (leftArm != null)
            {
                List<CombatItem> leftArmDown = log.GetBoonData(52430).Where(x => x.IsBuffRemove == ParseEnum.BuffRemove.All && x.SrcInstid == leftArm.InstID).ToList();
                for (int i = 1; i < phases.Count; i += 2)
                {
                    PhaseData phase = phases[i];
                    if (leftArmDown.Exists(x => phase.InInterval(x.Time - log.FightData.FightStart)))
                    {
                        phase.Name = "Left " + phase.Name;
                    }
                }
            }
            Boss rightArm = Targets.Find(x => x.ID == (ushort)ParseEnum.BossIDS.CARightArm);
            if (rightArm != null)
            {
                List<CombatItem> rightArmDown = log.GetBoonData(52430).Where(x => x.IsBuffRemove == ParseEnum.BuffRemove.All && x.SrcInstid == rightArm.InstID).ToList();
                for (int i = 1; i < phases.Count; i += 2)
                {
                    PhaseData phase = phases[i];
                    if (rightArmDown.Exists(x => phase.InInterval(x.Time - log.FightData.FightStart)))
                    {
                        if (phase.Name.Contains("Left"))
                        {
                            phase.Name = "Both Arms Phase";
                        }
                        else
                        {
                            phase.Name = "Right " + phase.Name;
                        }
                    }
                }
            }
            return phases;
        }

        public override void ComputeAdditionalBossData(Boss boss, ParsedLog log)
        {
            switch (boss.ID)
            {
                case (ushort)ParseEnum.BossIDS.ConjuredAmalgamate:
                case (ushort)ParseEnum.BossIDS.CALeftArm:
                case (ushort)ParseEnum.BossIDS.CARightArm:
                    break;
                default:
                    throw new InvalidOperationException("Unknown ID in ComputeAdditionalData");
            }
        }

        public override void ComputeAdditionalPlayerData(Player p, ParsedLog log)
        {

        }

        public override int IsCM(ParsedLog log)
        {
            Boss target = Targets.Find(x => x.ID == (ushort)ParseEnum.BossIDS.ConjuredAmalgamate);
            if (target == null)
            {
                throw new InvalidOperationException("Target for CM detection not found");
            }
            return log.CombatData.GetBoonData(53075).Count > 0 ? 1 : 0;
        }
    }
}
