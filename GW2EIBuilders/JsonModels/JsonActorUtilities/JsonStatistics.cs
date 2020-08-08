﻿using GW2EIEvtcParser.EIData;

namespace GW2EIBuilders.JsonModels
{
    /// <summary>
    /// Class representing general statistics
    /// </summary>
    public static class JsonStatistics
    {
        /// <summary>
        /// Defensive stats
        /// </summary>
        public class JsonDefensesAll
        {
            /// <summary>
            /// Total damage taken
            /// </summary>
            public long DamageTaken { get; }
            /// <summary>
            /// Number of blocks
            /// </summary>
            public int BlockedCount { get; }
            /// <summary>
            /// Number of evades
            /// </summary>
            public int EvadedCount { get; }
            /// <summary>
            /// Number of misses
            /// </summary>
            public int MissedCount { get; }
            /// <summary>
            /// Number of dodges
            /// </summary>
            public int DodgeCount { get; }
            /// <summary>
            /// Number of time an incoming attack was negated by invul
            /// </summary>
            public int InvulnedCount { get; }
            /// <summary>
            /// Damage negated by invul
            /// </summary>
            public int DamageInvulned { get; }
            /// <summary>
            /// Damage done against barrier
            /// </summary>
            public int DamageBarrier { get; }
            /// <summary>
            /// Number of time interrupted
            /// </summary>
            public int InterruptedCount { get; }
            /// <summary>
            /// Number of time downed
            /// </summary>
            public int DownCount { get; }
            /// <summary>
            /// Time passed in downstate
            /// </summary>
            public long DownDuration { get; }
            /// <summary>
            /// Number of time died
            /// </summary>
            public int DeadCount { get; }
            /// <summary>
            /// Time passed in dead state
            /// </summary>
            public long DeadDuration { get; }
            /// <summary>
            /// Number of time disconnected
            /// </summary>
            public int DcCount { get; }
            /// <summary>
            /// Time passed in disconnected state
            /// </summary>
            public long DcDuration { get; }

            internal JsonDefensesAll(FinalDefensesAll defenses)
            {
                DamageTaken = defenses.DamageTaken;
                BlockedCount = defenses.BlockedCount;
                DodgeCount = defenses.DodgeCount;
                MissedCount = defenses.MissedCount;
                EvadedCount = defenses.EvadedCount;
                InvulnedCount = defenses.InvulnedCount;
                DamageInvulned = defenses.DamageInvulned;
                DamageBarrier = defenses.DamageBarrier;
                InterruptedCount = defenses.InterruptedCount;
                DownCount = defenses.DownCount;
                DownDuration = defenses.DownDuration;
                DeadCount = defenses.DeadCount;
                DeadDuration = defenses.DeadDuration;
                DcCount = defenses.DcCount;
                DcDuration = defenses.DcDuration;
            }
        }

        /// <summary>
        /// DPS stats
        /// </summary>
        public class JsonDPS
        {
            /// <summary>
            /// Total dps
            /// </summary>
            public int Dps { get; }
            /// <summary>
            /// Total damage
            /// </summary>
            public int Damage { get; }
            /// <summary>
            /// Total condi dps
            /// </summary>
            public int CondiDps { get; }
            /// <summary>
            /// Total condi damage
            /// </summary>
            public int CondiDamage { get; }
            /// <summary>
            /// Total power dps
            /// </summary>
            public int PowerDps { get; }
            /// <summary>
            /// Total power damage
            /// </summary>
            public int PowerDamage { get; }
            /// <summary>
            /// Total actor only dps
            /// </summary>
            public int ActorDps { get; }
            /// <summary>
            /// Total actor only damage
            /// </summary>
            public int ActorDamage { get; }
            /// <summary>
            /// Total actor only condi dps
            /// </summary>
            public int ActorCondiDps { get; }
            /// <summary>
            /// Total actor only condi damage
            /// </summary>
            public int ActorCondiDamage { get; }
            /// <summary>
            /// Total actor only power dps
            /// </summary>
            public int ActorPowerDps { get; }
            /// <summary>
            /// Total actor only power damage
            /// </summary>
            public int ActorPowerDamage { get; }

            internal JsonDPS(FinalDPS stats)
            {
                Dps = stats.Dps;
                Damage = stats.Damage;
                CondiDps = stats.CondiDps;
                CondiDamage = stats.CondiDamage;
                PowerDps = stats.PowerDps;
                PowerDamage = stats.PowerDamage;

                ActorDps = stats.ActorDps;
                ActorDamage = stats.ActorDamage;
                ActorCondiDps = stats.ActorCondiDps;
                ActorCondiDamage = stats.ActorCondiDamage;
                ActorPowerDps = stats.ActorPowerDps;
                ActorPowerDamage = stats.ActorPowerDamage;
            }

        }

        /// <summary>
        /// Gameplay stats
        /// </summary>
        public class JsonGameplayStats
        {
            /// <summary>
            /// Number of damage hit
            /// </summary>
            public int TotalDamageCount { get; }
            /// <summary>
            /// Number of direct damage hit
            /// </summary>
            public int DirectDamageCount { get; }
            /// <summary>
            /// Number of connected direct damage hit
            /// </summary>
            public int ConnectedDirectDamageCount { get; }
            /// <summary>
            /// Number of critable hit
            /// </summary>
            public int CritableDirectDamageCount { get; }
            /// <summary>
            /// Number of crit
            /// </summary>
            public int CriticalRate { get; }
            /// <summary>
            /// Total critical damage
            /// </summary>
            public int CriticalDmg { get; }
            /// <summary>
            /// Number of hits while flanking
            /// </summary>
            public int FlankingRate { get; }
            /// <summary>
            /// Number of glanced hits
            /// </summary>
            public int GlanceRate { get; }
            /// <summary>
            /// Number of missed hits
            /// </summary>
            public int Missed { get; }
            /// <summary>
            /// Number of evaded hits
            /// </summary>
            public int Evaded { get; }
            /// <summary>
            /// Number of blocked hits
            /// </summary>
            public int Blocked { get; }
            /// <summary>
            /// Number of hits that interrupted a skill
            /// </summary>
            public int Interrupts { get; }
            /// <summary>
            /// Number of hits against invulnerable targets
            /// </summary>
            public int Invulned { get; }
            /// <summary>
            /// Damage negated by invul
            /// </summary>
            public int DamageInvulned { get; }

            internal JsonGameplayStats(FinalGameplayStats stats)
            {
                TotalDamageCount = stats.TotalDamageCount;
                DirectDamageCount = stats.DirectDamageCount;
                ConnectedDirectDamageCount = stats.ConnectedDirectDamageCount;
                CritableDirectDamageCount = stats.CritableDirectDamageCount;
                CriticalRate = stats.CriticalCount;
                CriticalDmg = stats.CriticalDmg;
                FlankingRate = stats.FlankingCount;
                GlanceRate = stats.GlanceCount;
                Missed = stats.Missed;
                Blocked = stats.Blocked;
                Evaded = stats.Evaded;
                Interrupts = stats.Interrupts;
                Invulned = stats.Invulned;
                DamageInvulned = stats.DamageInvulned;
            }

            internal JsonGameplayStats(FinalGameplayStatsAll stats) : this(stats as FinalGameplayStats)
            {
            }
        }

        /// <summary>
        /// Gameplay stats
        /// </summary>
        public class JsonGameplayStatsAll : JsonGameplayStats
        {
            /// <summary>
            /// Number of time you interrupted your cast
            /// </summary>
            public int Wasted { get; }
            /// <summary>
            /// Time wasted by interrupting your cast
            /// </summary>
            public double TimeWasted { get; }
            /// <summary>
            /// Number of time you skipped an aftercast
            /// </summary>
            public int Saved { get; }
            /// <summary>
            /// Time saved while skipping aftercast
            /// </summary>
            public double TimeSaved { get; }
            /// <summary>
            /// Distance to the epicenter of the squad
            /// </summary>
            public double StackDist { get; }
            /// <summary>
            /// Distance to the commander of the squad. Only when a player with commander tag is present
            /// </summary>
            public double DistToCom { get; }
            /// <summary>
            /// Average amount of boons
            /// </summary>
            public double AvgBoons { get; }
            /// <summary>
            /// Average amount of boons over active time
            /// </summary>
            public double AvgActiveBoons { get; }
            /// <summary>
            /// Average amount of conditions
            /// </summary>
            public double AvgConditions { get; }
            /// <summary>
            /// Average amount of conditions over active time
            /// </summary>
            public double AvgActiveConditions { get; }
            /// <summary>
            /// Number of time a weapon swap happened
            /// </summary>
            public int SwapCount { get; }

            internal JsonGameplayStatsAll(FinalGameplayStatsAll stats) : base(stats)
            {
                Wasted = stats.Wasted;
                TimeWasted = stats.TimeWasted;
                Saved = stats.Saved;
                TimeSaved = stats.TimeSaved;
                StackDist = stats.StackDist;
                DistToCom = stats.DistToCom;
                AvgBoons = stats.AvgBoons;
                AvgActiveBoons = stats.AvgActiveBoons;
                AvgConditions = stats.AvgConditions;
                AvgActiveConditions = stats.AvgActiveConditions;
                SwapCount = stats.SwapCount;
            }
        }

        /// <summary>
        /// Support stats
        /// </summary>
        public class JsonPlayerSupport
        {
            /// <summary>
            /// Number of time ressurected someone
            /// </summary>
            public long Resurrects { get; }
            /// <summary>
            /// Time passed on ressurecting
            /// </summary>
            public double ResurrectTime { get; }
            /// <summary>
            /// Number of time a condition was removed, self excluded
            /// </summary>
            public long CondiCleanse { get; }
            /// <summary>
            /// Total time of condition removed, self excluded
            /// </summary>
            public double CondiCleanseTime { get; }
            /// <summary>
            /// Number of time a condition was removed from self
            /// </summary>
            public long CondiCleanseSelf { get; }
            /// <summary>
            /// Total time of condition removed from self
            /// </summary>
            public double CondiCleanseTimeSelf { get; }
            /// <summary>
            /// Number of time a boon was removed
            /// </summary>
            public long BoonStrips { get; }
            /// <summary>
            /// Total time of boons removed from self
            /// </summary>
            public double BoonStripsTime { get; }

            internal JsonPlayerSupport(FinalPlayerSupport stats)
            {
                Resurrects = stats.Resurrects;
                ResurrectTime = stats.ResurrectTime;
                CondiCleanse = stats.CondiCleanse;
                CondiCleanseTime = stats.CondiCleanseTime;
                CondiCleanseSelf = stats.CondiCleanseSelf;
                CondiCleanseTimeSelf = stats.CondiCleanseTimeSelf;
                BoonStrips = stats.BoonStrips;
                BoonStripsTime = stats.BoonStripsTime;
            }
        }
    }
}
