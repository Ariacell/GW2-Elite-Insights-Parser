﻿using System.Collections.Generic;
using System.IO;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.EIData
{
    public class Buff : IVersionable
    {


        public const long NumberOfConditionsID = -3;
        public const long NumberOfBoonsID = -2;
        public const long NumberOfActiveCombatMinions = -17;
        public const long NoBuff = -4;

        // Weaver attunements
        public const long FireWater = -5;
        public const long FireAir = -6;
        public const long FireEarth = -7;
        public const long WaterFire = -8;
        public const long WaterAir = -9;
        public const long WaterEarth = -10;
        public const long AirFire = -11;
        public const long AirWater = -12;
        public const long AirEarth = -13;
        public const long EarthFire = -14;
        public const long EarthWater = -15;
        public const long EarthAir = -16;

        public const long FireDual = 43470;
        public const long WaterDual = 41166;
        public const long AirDual = 42264;
        public const long EarthDual = 44857;

        // Boon
        public enum BuffNature { 
            Condition, 
            Boon, 
            OffensiveBuffTable, 
            DefensiveBuffTable,
            SupportBuffTable,
            GraphOnlyBuff, 
            Consumable, 
            Unknow 
        };

        public enum BuffType
        {
            Duration = 0,
            Intensity = 1,
            Unknown = 2,
        }

        // Fields
        public string Name { get; }
        public long ID { get; }
        public BuffNature Nature { get; }
        public ParserHelper.Source Source { get; }
        private BuffStackType _stackType { get; }
        public BuffType Type {    
            get {
                switch (_stackType)
                {
                    case BuffStackType.Queue:
                    case BuffStackType.Regeneration:
                    case BuffStackType.Force:
                        return BuffType.Duration;
                    case BuffStackType.Stacking:
                    case BuffStackType.StackingConditionalLoss:
                        return BuffType.Intensity;
                    default:
                        return BuffType.Unknown;
                }
            }
        }

        public BuffInfoEvent BuffInfo { get; private set; }

        private ulong _maxBuild { get; } = ulong.MaxValue;
        private ulong _minBuild { get; } = ulong.MinValue;
        public int Capacity { get; private set; }
        public string Link { get; }

        /// <summary>
        /// Buff constructor
        /// </summary>
        /// <param name="name">The name of the boon</param>
        /// <param name="id">The id of the buff</param>
        /// <param name="source">Source of the buff <see cref="ParserHelper.Source"/></param>
        /// <param name="type">Stack Type of the buff<see cref="BuffStackType"/></param>
        /// <param name="capacity">Maximun amount of buff in stack</param>
        /// <param name="nature">Nature of the buff, dictates in which category the buff will appear <see cref="BuffNature"/></param>
        /// <param name="link">URL to the icon of the buff</param>
        public Buff(string name, long id, ParserHelper.Source source, BuffStackType type, int capacity, BuffNature nature, string link)
        {
            Name = name;
            ID = id;
            Source = source;
            _stackType = type;
            Capacity = capacity;
            Nature = nature;
            Link = link;
        }

        public Buff(string name, long id, ParserHelper.Source source, BuffNature nature, string link) : this(name, id, source, BuffStackType.Force, 1, nature, link)
        {
        }

        public Buff(string name, long id, ParserHelper.Source source, BuffStackType type, int capacity, BuffNature nature, string link, ulong minBuild, ulong maxBuild) : this(name, id, source, type, capacity, nature, link)
        {
            _maxBuild = maxBuild;
            _minBuild = minBuild;
        }

        public Buff(string name, long id, ParserHelper.Source source, BuffNature nature, string link, ulong minBuild, ulong maxBuild) : this(name, id, source, BuffStackType.Force, 1, nature, link, minBuild, maxBuild)
        {
        }

        public Buff(string name, long id, string link)
        {
            Name = name;
            ID = id;
            Source = ParserHelper.Source.Unknown;
            _stackType = BuffStackType.Unknown;
            Capacity = 1;
            Nature = BuffNature.Unknow;
            Link = link;
        }

        internal static Buff CreateCustomConsumable(string name, long id, string link, int capacity)
        {
            return new Buff(name + " " + id, id, ParserHelper.Source.Item, capacity > 1 ? BuffStackType.Stacking : BuffStackType.Force, capacity, BuffNature.Consumable, link);
        }

        internal void AttachBuffInfoEvent(BuffInfoEvent buffInfoEvent, ParserController operation)
        {
            if (buffInfoEvent.BuffID != ID)
            {
                return;
            }
            BuffInfo = buffInfoEvent;
            if (Capacity != buffInfoEvent.MaxStacks)
            {
                operation.UpdateProgressWithCancellationCheck("Adjusted capacity for " + Name + " from " + Capacity + " to " + buffInfoEvent.MaxStacks);
                if (buffInfoEvent.StackingType != _stackType)
                {
                    //_stackType = buffInfoEvent.StackingType; // might be unreliable due to its absence on some logs
                    operation.UpdateProgressWithCancellationCheck("Incoherent stack type for " + Name + ": is " + _stackType + " but expected " + buffInfoEvent.StackingType);
                }
                Capacity = buffInfoEvent.MaxStacks;
            }
        }
        internal AbstractBuffSimulator CreateSimulator(ParsedEvtcLog log)
        {
            if (!log.CombatData.HasStackIDs)
            {
                StackingLogic logicToUse;
                switch (_stackType)
                {
                    case BuffStackType.Queue:
                        logicToUse = new QueueLogic();
                        break;
                    case BuffStackType.Regeneration:
                        logicToUse = new HealingLogic();
                        break;
                    case BuffStackType.Force:
                        logicToUse = new ForceOverrideLogic();
                        break;
                    case BuffStackType.Stacking:
                    case BuffStackType.StackingConditionalLoss:
                        logicToUse = new OverrideLogic();
                        break;
                    case BuffStackType.Unknown:
                    default:
                        throw new InvalidDataException("Buffs can not be typless");
                }
                switch (Type)
                {
                    case BuffType.Intensity: return new BuffSimulatorIntensity(Capacity, log, logicToUse);
                    case BuffType.Duration: return new BuffSimulatorDuration(Capacity, log, logicToUse);
                    case BuffType.Unknown:
                        throw new InvalidDataException("Buffs can not be stackless");
                }
            }
            switch (Type)
            {
                case BuffType.Intensity: 
                    return new BuffSimulatorIDIntensity(log);
                case BuffType.Duration: 
                    return new BuffSimulatorIDDuration(log);
                case BuffType.Unknown:
                default:
                    throw new InvalidDataException("Buffs can not be stackless");
            }
        }

        internal static BuffSourceFinder GetBuffSourceFinder(ulong version, HashSet<long> boonIds)
        {
            if (version > 99526)
            {
                return new BuffSourceFinder01102019(boonIds);
            }
            if (version > 95112)
            {
                return new BuffSourceFinder05032019(boonIds);
            }
            return new BuffSourceFinder11122018(boonIds);
        }

        public bool Available(ulong gw2Build)
        {
            return gw2Build < _maxBuild && gw2Build >= _minBuild;
        }
    }
}
