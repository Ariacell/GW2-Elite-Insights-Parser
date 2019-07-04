﻿using LuckParser.Parser;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.Logic
{
    public abstract class RaidLogic : FightLogic
    {
        protected enum FallBackMethod { None, NPC, AttackTarget }

        protected FallBackMethod GenericFallBackMethod = FallBackMethod.NPC;

        protected RaidLogic(ushort triggerID) : base(triggerID)
        {
            Mode = ParseMode.Raid;
        }

        protected virtual List<ushort> GetDeatchCheckIds()
        {
            return new List<ushort>
            {
                TriggerID
            };
        }

        public override void CheckSuccess(CombatData combatData, AgentData agentData, FightData fightData, HashSet<AgentItem> playerAgents)
        {
            HashSet<int> raidRewardsTypes = new HashSet<int>
                {
                    55821,
                    60685,
                    914,
                    22797
                };
            List<RewardEvent> rewards = combatData.GetRewardEvents();
            RewardEvent reward = rewards.FirstOrDefault(x => raidRewardsTypes.Contains(x.RewardType));
            if (reward != null)
            {
                fightData.SetSuccess(true, fightData.ToLogSpace(reward.Time));
            }
            else if (GenericFallBackMethod == FallBackMethod.NPC)
            {
                SetSuccessByDeath(combatData, fightData, playerAgents, true, GetDeatchCheckIds());
            }
            else if (GenericFallBackMethod == FallBackMethod.AttackTarget)
            {
                SetSuccessByAttackTarget(Targets.Find(x => x.ID == TriggerID), combatData, fightData, playerAgents);
            }
        }

        protected override HashSet<ushort> GetUniqueTargetIDs()
        {
            return new HashSet<ushort>
            {
                TriggerID
            };
        }
    }
}
