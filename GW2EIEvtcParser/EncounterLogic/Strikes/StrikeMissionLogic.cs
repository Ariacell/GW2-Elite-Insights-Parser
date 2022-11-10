﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EncounterLogic.EncounterCategory;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal abstract class StrikeMissionLogic : FightLogic
    {

        protected StrikeMissionLogic(int triggerID) : base(triggerID)
        {
            Mode = ParseMode.Instanced10;
            EncounterCategoryInformation.Category = FightCategory.Strike;
            EncounterID |= EncounterIDs.EncounterMasks.StrikeMask;
        }

        protected virtual void SetSuccessByDeath(CombatData combatData, FightData fightData, IReadOnlyCollection<AgentItem> playerAgents, bool all)
        {
            SetSuccessByDeath(combatData, fightData, playerAgents, all, GetSuccessCheckIds());
        }

        internal override long GetFightOffset(FightData fightData, AgentData agentData, List<CombatItem> combatData)
        {
            CombatItem logStartNPCUpdate = combatData.FirstOrDefault(x => x.IsStateChange == ArcDPSEnums.StateChange.LogStartNPCUpdate);
            if (logStartNPCUpdate != null)
            {
                return GetEnterCombatTime(fightData, agentData, combatData);
            }
            return GetGenericFightOffset(fightData);
        }

        protected override HashSet<int> GetUniqueNPCIDs()
        {
            return new HashSet<int>
            {
                GenericTriggerID
            };
        }
    }
}
