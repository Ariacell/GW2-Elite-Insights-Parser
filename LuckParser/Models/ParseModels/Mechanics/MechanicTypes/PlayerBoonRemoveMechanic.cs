﻿using LuckParser.Controllers;
using LuckParser.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{

    public class PlayerBoonRemoveMechanic : Mechanic
    {

        public PlayerBoonRemoveMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown, List<MechanicChecker> conditions, TriggerRule rule) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown, conditions, rule)
        {
        }

        public PlayerBoonRemoveMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown, List<MechanicChecker> conditions, TriggerRule rule) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown, conditions, rule)
        {
        }

        public PlayerBoonRemoveMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown)
        {
        }

        public PlayerBoonRemoveMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown)
        {
        }

        public override void CheckMechanic(ParsedLog log, Dictionary<Mechanic, List<MechanicLog>> mechanicLogs, Dictionary<ushort, DummyActor> regroupedMobs)
        {
            CombatData combatData = log.CombatData;
            HashSet<ushort> playersIds = log.PlayerIDs;

            foreach (Player p in log.PlayerList)
            {
                foreach (AbstractBuffEvent c in log.CombatData.GetBoonData(SkillId))
                {
                    if (c is BuffRemoveManualEvent && p.AgentItem == c.To /*&& Keep(c, log)*/)
                    {
                        mechanicLogs[this].Add(new MechanicLog(c.Time, this, p));
                    }
                }
            }
        }
    }
}
