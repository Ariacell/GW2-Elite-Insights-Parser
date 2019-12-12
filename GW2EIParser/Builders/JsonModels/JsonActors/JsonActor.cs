﻿using System.Collections.Generic;
using System.Linq;
using GW2EIParser.EIData;
using GW2EIParser.Parser.ParsedData;
using static GW2EIParser.Builders.JsonModels.JsonStatistics;

namespace GW2EIParser.Builders.JsonModels
{
    /// <summary>
    /// Base class for Players and Targets
    /// </summary>
    /// <seealso cref="JsonPlayer"/> 
    /// <seealso cref="JsonNPC"/>
    public abstract class JsonActor
    {

        /// <summary>
        /// Name of the actor
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Condition damage score
        /// </summary>
        public uint Condition { get; }
        /// <summary>
        /// Concentration score
        /// </summary>
        public uint Concentration { get; }
        /// <summary>
        /// Healing Power score
        /// </summary>
        public uint Healing { get; }
        /// <summary>
        /// Toughness score
        /// </summary>
        public uint Toughness { get; }
        /// <summary>
        /// Height of the hitbox
        /// </summary>
        public uint HitboxHeight { get; }
        /// <summary>
        /// Width of the hitbox
        /// </summary>
        public uint HitboxWidth { get; }
        /// <summary>
        /// List of minions
        /// </summary>
        /// <seealso cref="JsonMinions"/>
        public List<JsonMinions> Minions { get; }

        /// <summary>
        /// Array of Total DPS stats \n
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDPS"/>
        public JsonDPS[] DpsAll { get; }
        /// <summary>
        /// Stats against all  \n
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonGameplayStatsAll"/>
        public JsonGameplayStatsAll[] StatsAll { get; }
        /// <summary>
        /// Defensive stats \n
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDefensesAll"/>
        public JsonDefensesAll[] Defenses { get; }
        /// <summary>
        /// Total Damage distribution array \n
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        public List<JsonDamageDist>[] TotalDamageDist { get; }
        /// <summary>
        /// Damage taken array
        /// Length == # of phases
        /// </summary>
        /// <seealso cref="JsonDamageDist"/>
        public List<JsonDamageDist>[] TotalDamageTaken { get; }
        /// <summary>
        /// Rotation data
        /// </summary>
        /// <seealso cref="JsonRotation"/>
        public List<JsonRotation> Rotation { get; }
        /// <summary>
        /// Array of int representing 1S damage points \n
        /// Length == # of phases
        /// </summary>
        /// <remarks>
        /// If the duration of the phase in seconds is non integer, the last point of this array will correspond to the last point  \n
        /// ex: duration === 15250ms, the array will have 17 elements [0, 1000,...,15000,15250]
        /// </remarks>
        public List<int>[] Damage1S { get; }
        /// <summary>
        /// Array of int[2] that represents the number of conditions status \n
        /// Value[i][0] will be the time, value[i][1] will be the number of conditions present from value[i][0] to value[i+1][0] \n
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight \n
        /// </summary>
        public List<int[]> ConditionsStates { get; }
        /// <summary>
        /// Array of int[2] that represents the number of boons status \n
        /// Value[i][0] will be the time, value[i][1] will be the number of boons present from value[i][0] to value[i+1][0] \n
        /// If i corresponds to the last element that means the status did not change for the remainder of the fight
        /// </summary>
        public List<int[]> BoonsStates { get; }


        protected JsonActor(AbstractSingleActor actor, ParsedLog log, Dictionary<string, JsonLog.SkillDesc> skillDesc, Dictionary<string, JsonLog.BuffDesc> buffDesc)
        {
            List<PhaseData> phases = log.FightData.GetPhases(log);
            //
            Name = actor.Character;
            Toughness = actor.Toughness;
            Healing = actor.Healing;
            Concentration = actor.Concentration;
            Condition = actor.Condition;
            HitboxHeight = actor.HitboxHeight;
            HitboxWidth = actor.HitboxWidth;
            //
            DpsAll = actor.GetDPSAll(log).Select(x => new JsonDPS(x)).ToArray();
            StatsAll = actor.GetGameplayStats(log).Select(x => new JsonGameplayStatsAll(x)).ToArray();
            Defenses = actor.GetDefenses(log).Select(x => new JsonDefensesAll(x)).ToArray();
            //
            Dictionary<long, Minions> minionsList = actor.GetMinions(log);
            if (minionsList.Values.Any())
            {
                Minions = minionsList.Values.Select(x => new JsonMinions(x, log, skillDesc, buffDesc)).ToList();
            }
            //
            var skillByID = actor.GetCastLogs(log, 0, log.FightData.FightEnd).GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            if (skillByID.Any())
            {
                Rotation = JsonRotation.BuildJsonRotationList(skillByID, skillDesc);
            }
            //
            Damage1S = new List<int>[phases.Count];
            for (int i = 0; i < phases.Count; i++)
            {
                Damage1S[i] = actor.Get1SDamageList(log, i, phases[i], null);
            }
            //
            TotalDamageDist = BuildDamageDistData(actor, null, phases, log, skillDesc, buffDesc);
            TotalDamageTaken = BuildDamageTakenDistData(actor, null, phases, log, skillDesc, buffDesc);
            //
            BoonsStates = JsonBuffsUptime.GetBuffStates(actor.GetBuffGraphs(log)[ProfHelper.NumberOfBoonsID]);
            ConditionsStates = JsonBuffsUptime.GetBuffStates(actor.GetBuffGraphs(log)[ProfHelper.NumberOfConditionsID]);
            //
        }

        protected static List<JsonDamageDist>[] BuildDamageDistData(AbstractSingleActor actor, NPC target, List<PhaseData> phases, ParsedLog log, Dictionary<string, JsonLog.SkillDesc> skillDesc, Dictionary<string, JsonLog.BuffDesc> buffDesc)
        {
            var res = new List<JsonDamageDist>[phases.Count];
            for (int i = 0; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                res[i] = JsonDamageDist.BuildJsonDamageDistList(actor.GetDamageLogs(target, log, phase).Where(x => !x.HasDowned).GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList()), log, skillDesc, buffDesc);
            }
            return res;
        }

        protected static List<JsonDamageDist>[] BuildDamageTakenDistData(AbstractSingleActor actor, NPC target, List<PhaseData> phases, ParsedLog log, Dictionary<string, JsonLog.SkillDesc> skillDesc, Dictionary<string, JsonLog.BuffDesc> buffDesc)
        {
            var res = new List<JsonDamageDist>[phases.Count];
            for (int i = 0; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                res[i] = JsonDamageDist.BuildJsonDamageDistList(actor.GetDamageTakenLogs(target, log, phase.Start, phase.End).Where(x => !x.HasDowned).GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList()), log, skillDesc, buffDesc);
            }
            return res;
        }
    }
}
