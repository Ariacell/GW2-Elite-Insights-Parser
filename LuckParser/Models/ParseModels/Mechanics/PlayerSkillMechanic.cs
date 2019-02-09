﻿using LuckParser.Controllers;
using LuckParser.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    
    public class PlayerSkillMechanic : Mechanic
    {

        public PlayerSkillMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown, CheckTriggerCondition condition = null) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown, condition)
        {
        }

        public PlayerSkillMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown, CheckTriggerCondition condition = null) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown, condition)
        {
        }

        public override void CheckMechanic(ParsedLog log, Dictionary<ushort, DummyActor> regroupedMobs)
        {
            MechanicData mechData = log.MechanicData;
            CombatData combatData = log.CombatData;
            HashSet<ushort> playersIds = log.PlayerIDs;
            foreach (Player p in log.PlayerList)
            {
                foreach (CombatItem c in log.CombatData.GetCastDataById(SkillId))
                {
                    if (!Keep(c))
                    {
                        continue;
                    }
                    if (c.IsActivation.StartCasting() && c.SrcInstid == p.InstID)
                    {
                        mechData[this].Add(new MechanicLog(log.FightData.ToFightSpace(c.Time), this, p));

                    }
                }
            }
        }
    }
}
