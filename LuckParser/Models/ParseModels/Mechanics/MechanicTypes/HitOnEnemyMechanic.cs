﻿using LuckParser.Controllers;
using LuckParser.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    
    public class HitOnEnemyMechanic : Mechanic
    {

        public HitOnEnemyMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown, List<MechanicChecker> conditions, TriggerRule rule) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown, conditions, rule)
        {
        }

        public HitOnEnemyMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown, List<MechanicChecker> conditions, TriggerRule rule) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown, conditions, rule)
        {
        }

        public HitOnEnemyMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown)
        {
        }

        public HitOnEnemyMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown)
        {
        }

        public override void CheckMechanic(ParsedEvtcContainer evtcContainer, Dictionary<Mechanic, List<MechanicLog>> mechanicLogs, Dictionary<ushort, DummyActor> regroupedMobs)
        {
            CombatData combatData = evtcContainer.CombatData;
            HashSet<ushort> playersIds = evtcContainer.PlayerIDs;
            IEnumerable<AgentItem> agents = evtcContainer.AgentData.GetAgentsByID((ushort)SkillId);
            foreach (AgentItem a in agents)
            {
                List<CombatItem> combatitems = combatData.GetDamageTakenData(a.InstID, a.FirstAware, a.LastAware);
                foreach (CombatItem c in combatitems)
                {
                    if (c.IsBuff > 0 || !c.ResultEnum.IsHit() || !Keep(c, evtcContainer) )
                    {
                        continue;
                    }
                    foreach (Player p in evtcContainer.PlayerList)
                    {
                        if (c.SrcInstid == p.InstID || c.SrcMasterInstid == p.InstID )
                        {
                            mechanicLogs[this].Add(new MechanicLog(evtcContainer.FightData.ToFightSpace(c.Time), this, p));
                        }
                    }
                }
            }
        }
    }
}
