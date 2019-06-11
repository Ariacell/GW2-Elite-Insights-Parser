﻿using LuckParser.Controllers;
using LuckParser.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    
    public class DamageOnPlayerFirstHitMechanic : DamageOnPlayerMechanic
    {
        protected override bool Keep(AbstractDamageEvent c, ParsedLog log)
        {
            if (GetFirstHit(c.From, log) != c)
            {
                return false;
            }
            return base.Keep(c, log);
        }

        private readonly Dictionary<AgentItem, AbstractDamageEvent> _firstHits = new Dictionary<AgentItem, AbstractDamageEvent>();

        public DamageOnPlayerFirstHitMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown, List<DamageChecker> conditions, TriggerRule rule) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown, conditions, rule)
        {
        }

        public DamageOnPlayerFirstHitMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown, List<DamageChecker> conditions, TriggerRule rule) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown, conditions, rule)
        {
        }

        public DamageOnPlayerFirstHitMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, int internalCoolDown) : this(skillId, inGameName, plotlySetting, shortName, shortName, shortName, internalCoolDown)
        {
        }

        public DamageOnPlayerFirstHitMechanic(long skillId, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown) : base(skillId, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown)
        {
        }

        private AbstractDamageEvent GetFirstHit(AgentItem src, ParsedLog log)
        {
            if (!_firstHits.TryGetValue(src, out AbstractDamageEvent evt))
            {
                AbstractDamageEvent res = log.CombatData.GetDamageData(src).Where(x => x.SkillId == SkillId).FirstOrDefault();
                _firstHits[src] = res;
                return res;
            }
            return evt;
        }
    }
}
