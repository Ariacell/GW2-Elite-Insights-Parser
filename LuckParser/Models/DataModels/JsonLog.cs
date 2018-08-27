﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using LuckParser.Controllers;
using LuckParser.Models.ParseModels;

namespace LuckParser.Models.DataModels
{
    class JsonLog
    {
        public struct JsonDps
        {
            public int[] AllDps;
            public int[] AllDamage;
            public int[] AllCondiDps;
            public int[] AllCondiDamage;
            public int[] AllPowerDps;
            public int[] AllPowerDamage;

            // Boss
            public int[] BossDps;
            public int[] BossDamage;
            public int[] BossCondiDps;
            public int[] BossCondiDamage;
            public int[] BossPowerDps;
            public int[] BossPowerDamage;

            // Player only
            public int[] PlayerPowerDamage;
            public int[] PlayerBossPowerDamage;
        }

        public struct JsonDefenses
        {
            public long[] DamageTaken;
            public int[] BlockedCount;
            public int[] EvadedCount;
            public int[] InvulnedCount;
            public int[] DamageInvulned;
            public int[] DamageBarrier;
        }

        public struct JsonBoonUptime
        {
            public double[] Uptime;
            public double[] Generation;
            public double[] Overstack;
        }

        public struct JsonSupport
        {
            public int[] Resurrects;
            public float[] ResurrectTime;
            public int[] CondiCleanse;
            public float[] CondiCleanseTime;
        }

        public struct JsonBoss
        {
            public string Name;
            public ushort Id;
            public int TotalHealth;
            public double FinalHealth;
            public double HealthPercentBurned;
            public JsonDps Dps;
            public Dictionary<long, JsonBossBoon> Conditions;
        }

        public struct JsonStats
        {
            // Rates
            public int[] PowerLoopCount;
            public int[] CritablePowerLoopCount;
            public int[] CriticalRate;
            public int[] CriticalDmg;
            public int[] ScholarRate;
            public int[] ScholarDmg;
            public int[] MovingRate;
            public int[] MovingDamage;
            public int[] FlankingRate;
            public int[] GlanceRate;
            public int[] Missed;
            public int[] Interrupts;
            public int[] Invulned;
            public int[] Wasted;
            public double[] TimeWasted;
            public int[] Saved;
            public double[] TimeSaved;
            public double[] AvgBoons;
            public double[] StackDist;

            //Boss only Rates
            public int[] PowerLoopCountBoss;
            public int[] CritablePowerLoopCountBoss;
            public int[] CriticalRateBoss;
            public int[] CriticalDmgBoss;
            public int[] ScholarRateBoss;
            public int[] ScholarDmgBoss;
            public int[] MovingRateBoss;
            public int[] MovingDamageBoss;
            public int[] FlankingRateBoss;
            public int[] GlanceRateBoss;
            public int[] MissedBoss;
            public int[] InterruptsBoss;
            public int[] InvulnedBoss;

            // Counts
            public int[] SwapCount;
            public int[] DownCount;
            public int[] DodgeCount;

            // Misc
            public double[] Died;
            public double[] Dcd;
        }

        public struct JsonPlayer
        {
            public string Character;
            public string Account;
            public int Condition;
            public int Concentration;
            public int Healing;
            public int Toughness;
            public int Group;
            public string Profession;
            public string[] Weapons;
            public JsonDps Dps;
            public JsonStats Stats;
            public JsonDefenses Defenses;
            public JsonSupport Support;
            public Dictionary<long, JsonBoonUptime> SelfBoons;
            public Dictionary<long, JsonBoonUptime> GroupBoons;
            public Dictionary<long, JsonBoonUptime> OffGroupBoons;
            public Dictionary<long, JsonBoonUptime> SquadBoons;
        }

        public struct JsonPhase
        {
            public long Duration;
            public string Name;
        }

        public struct JsonBossBoon
        {
            public double[] Uptime;
            public Dictionary<String, double>[] Generated;
            public Dictionary<String, double>[] Overstacked;
        }

        public struct JsonMechanic
        {
            public long Time;
            public string Player;
            public string Description;
            public long Skill;
        }

        public string EliteInsightsVersion;
        public string ArcVersion;
        public string RecordedBy;
        public string TimeStart;
        public string TimeEnd;
        public string Duration;
        public bool Success;
        public JsonBoss Boss;
        public ArrayList Players;
        public ArrayList Phases;
        public JsonMechanic[] Mechanics;
        public List<Point3D> StackCenterPositions;
    }
}