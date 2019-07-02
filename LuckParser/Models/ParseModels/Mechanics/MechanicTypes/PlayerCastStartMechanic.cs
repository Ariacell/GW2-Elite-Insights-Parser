﻿using LuckParser.Controllers;
using LuckParser.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    
    public class PlayerCastStartMechanic : CastMechanic
    {

        public PlayerCastStartMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown, List<CastChecker> conditions) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown, conditions)
        {
        }

        public PlayerCastStartMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown)
        {
        }

        public PlayerCastStartMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown)
        {
        }

        public PlayerCastStartMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown, List<CastChecker> conditions) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown, conditions)
        {
        }

        public override void CheckMechanic(ParsedLog log, Dictionary<Mechanic, List<MechanicEvent>> mechanicLogs, Dictionary<ushort, DummyActor> regroupedMobs)
        {
            CombatData combatData = log.CombatData;
            foreach (Player p in log.PlayerList)
            {
                foreach (AbstractCastEvent c in log.CombatData.GetCastDataById(SkillId))
                {
                    if (c.Caster == p.AgentItem && Keep(c, log))
                    {
                        mechanicLogs[this].Add(new MechanicEvent(GetTime(c), this, p));

                    }
                }
            }
        }
    }
}
