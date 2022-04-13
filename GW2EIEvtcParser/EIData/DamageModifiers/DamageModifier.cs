﻿using System;
using System.Collections.Generic;
using System.IO;
using GW2EIEvtcParser.EncounterLogic;
using GW2EIEvtcParser.Interfaces;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ParserHelper;

namespace GW2EIEvtcParser.EIData
{
    public abstract class DamageModifier : IVersionable
    {

        public enum DamageModifierMode { PvE, sPvP, WvW, All, sPvPWvW };
        public enum DamageSource { All, NoPets };

        private DamageType _compareType { get; }
        private DamageType _srcType { get; }
        private DamageSource _dmgSrc { get; }
        protected double GainPerStack { get; }
        internal GainComputer GainComputer { get; }
        private ulong _minBuild { get; } = GW2Builds.StartOfLife;
        private ulong _maxBuild { get; } = GW2Builds.EndOfLife;
        public bool Multiplier => GainComputer.Multiplier;
        public bool SkillBased => GainComputer.SkillBased;

        public bool Approximate { get; protected set; } = false;
        public ParserHelper.Source Src { get; }
        public string Icon { get; }
        public string Name { get; }
        public int ID { get; }
        public string Tooltip { get; protected set; }
        internal delegate bool DamageLogChecker(AbstractHealthDamageEvent dl, ParsedEvtcLog log);

        protected DamageModifierMode Mode { get; } = DamageModifierMode.All;
        internal DamageLogChecker DLChecker { get; }


        internal static readonly GainComputerByPresence ByPresence = new GainComputerByPresence();
        internal static readonly GainComputerByMultiPresence ByMultiPresence = new GainComputerByMultiPresence();
        internal static readonly GainComputerNonMultiplier ByPresenceNonMultiplier = new GainComputerNonMultiplier();
        internal static readonly GainComputerBySkill BySkill = new GainComputerBySkill();
        internal static readonly GainComputerByStack ByStack = new GainComputerByStack();
        internal static readonly GainComputerByMultiplyingStack ByMultipliyingStack = new GainComputerByMultiplyingStack();
        internal static readonly GainComputerByAbsence ByAbsence = new GainComputerByAbsence();

        internal DamageModifier(string name, string tooltip, DamageSource damageSource, double gainPerStack, DamageType srctype, DamageType compareType, ParserHelper.Source src, string icon, GainComputer gainComputer, DamageLogChecker dlChecker, ulong minBuild, ulong maxBuild, DamageModifierMode mode)
        {
            Tooltip = tooltip;
            Name = name;
            ID = Name.GetHashCode();
            _dmgSrc = damageSource;
            GainPerStack = gainPerStack;
            _compareType = compareType;
            _srcType = srctype;
            Src = src;
            Icon = icon;
            GainComputer = gainComputer;
            DLChecker = dlChecker;
            _maxBuild = maxBuild;
            _minBuild = minBuild;
            Mode = mode;
            switch (_dmgSrc)
            {
                case DamageSource.All:
                    Tooltip += "<br>Actor + Minions";
                    break;
                case DamageSource.NoPets:
                    Tooltip += "<br>No Minions";
                    break;
            }
            switch (_srcType)
            {
                case DamageType.All:
                    Tooltip += "<br>All Damage";
                    throw new InvalidDataException("No known damage modifier that modifies every outgoing damage");
                case DamageType.Power:
                    Tooltip += "<br>Power Damage";
                    break;
                case DamageType.Strike:
                    Tooltip += "<br>Strike Damage";
                    break;
                case DamageType.StrikeAndCondition:
                    Tooltip += "<br>Strike and Condition Damage";
                    break;
                case DamageType.StrikeAndConditionAndLifeLeech:
                    Tooltip += "<br>Strike, Condition and Life Leech Damage";
                    break;
                case DamageType.Condition:
                    Tooltip += "<br>Condition Damage";
                    break;
                default:
                    throw new NotImplementedException("Not implemented damage type " + _srcType);
            }
            switch (_compareType)
            {
                case DamageType.All:
                    Tooltip += "<br>Compared against All Damage";
                    break;
                case DamageType.Power:
                    Tooltip += "<br>Compared against Power Damage";
                    break;
                case DamageType.Strike:
                    Tooltip += "<br>Compared against Strike Damage";
                    break;
                case DamageType.StrikeAndCondition:
                    Tooltip += "<br>Compared against Strike and Condition Damage";
                    break;
                case DamageType.StrikeAndConditionAndLifeLeech:
                    Tooltip += "<br>Compared against Strike, Condition and Life Leech Damage";
                    break;
                case DamageType.Condition:
                    Tooltip += "<br>Compared against Condition Damage";
                    break;
                default:
                    throw new NotImplementedException("Not implemented damage type " + _compareType);
            }
            if (!Multiplier)
            {
                Tooltip += "<br>Non multiplier";
            }
        }
        public bool Available(ulong gw2Build)
        {
            return gw2Build < _maxBuild && gw2Build >= _minBuild;
        }

        internal bool Keep(FightLogic.ParseMode mode, EvtcParserSettings parserSettings)
        {
            // Remove target and approx based damage mods from PvP contexts
            if (this is BuffDamageModifierTarget || Approximate)
            {
                if (mode == FightLogic.ParseMode.WvW || mode == FightLogic.ParseMode.sPvP)
                {
                    return false;
                }
            }
            if (Mode == DamageModifierMode.All)
            {     
                return true;
            }
            switch (mode)
            {
                case FightLogic.ParseMode.Unknown:
                case FightLogic.ParseMode.FullInstance:
                case FightLogic.ParseMode.Instanced5:
                case FightLogic.ParseMode.Instanced10:
                case FightLogic.ParseMode.Benchmark:
                    return Mode == DamageModifierMode.PvE;
                case FightLogic.ParseMode.WvW:
                    return (Mode == DamageModifierMode.WvW || Mode == DamageModifierMode.sPvPWvW);
                case FightLogic.ParseMode.sPvP:
                    return Mode == DamageModifierMode.sPvP || Mode == DamageModifierMode.sPvPWvW;
            }
            return false;
        }

        public int GetTotalDamage(AbstractSingleActor actor, ParsedEvtcLog log, AbstractSingleActor t, long start, long end)
        {
            FinalDPS damageData = actor.GetDPSStats(t, log, start, end);
            switch (_compareType)
            {
                case DamageType.All:
                    return _dmgSrc == DamageSource.All ? damageData.Damage : damageData.ActorDamage;
                case DamageType.Condition:
                    return _dmgSrc == DamageSource.All ? damageData.CondiDamage : damageData.ActorCondiDamage;
                case DamageType.Power:
                    return _dmgSrc == DamageSource.All ? damageData.PowerDamage : damageData.ActorPowerDamage;
                case DamageType.Strike:
                    return _dmgSrc == DamageSource.All ? damageData.StrikeDamage : damageData.ActorStrikeDamage;
                case DamageType.StrikeAndCondition:
                    return _dmgSrc == DamageSource.All ? damageData.StrikeDamage + damageData.CondiDamage : damageData.ActorStrikeDamage + damageData.ActorCondiDamage;
                case DamageType.StrikeAndConditionAndLifeLeech:
                    return _dmgSrc == DamageSource.All ? damageData.StrikeDamage + damageData.CondiDamage + damageData.LifeLeechDamage : damageData.ActorStrikeDamage + damageData.ActorCondiDamage + damageData.ActorLifeLeechDamage;
                default:
                    throw new NotImplementedException("Not implemented damage type " + _compareType);
            }
        }

        public IReadOnlyList<AbstractHealthDamageEvent> GetHitDamageEvents(AbstractSingleActor actor, ParsedEvtcLog log, AbstractSingleActor t, long start, long end)
        {
            return _dmgSrc == DamageSource.All ? actor.GetHitDamageEvents(t, log, start, end, _srcType) : actor.GetJustActorHitDamageEvents(t, log, start, end, _srcType);
        }

        internal abstract List<DamageModifierEvent> ComputeDamageModifier(AbstractSingleActor actor, ParsedEvtcLog log);

        internal static readonly List<DamageModifier> ItemDamageModifiers = new List<DamageModifier>
        {
            new DamageLogDamageModifier("Moving Bonus","Seaweed Salad (and the likes) – 5% while moving", DamageSource.NoPets, 5.0, DamageType.Strike, DamageType.Strike, Source.Item,"https://wiki.guildwars2.com/images/1/1c/Bowl_of_Seaweed_Salad.png", (x, log) => x.IsMoving, ByPresence, DamageModifierMode.All),
            new BuffDamageModifier(SkillIDs.FractalOffensive, "Fractal Offensive", "3% per stack", DamageSource.NoPets, 3.0, DamageType.StrikeAndCondition, DamageType.All, Source.Item, ByStack, "https://wiki.guildwars2.com/images/thumb/8/8d/Mist_Offensive_Potion.png/40px-Mist_Offensive_Potion.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.WritOfMasterfulMalice, "Writ of Masterful Malice", "200 condition damage over 90% HP", DamageSource.NoPets, 0.0, DamageType.Condition, DamageType.Condition, Source.Item, ByPresenceNonMultiplier,"https://wiki.guildwars2.com/images/2/20/Writ_of_Masterful_Malice.png",DamageModifierMode.All, (x, log) => x.IsOverNinety),
            new BuffDamageModifier(SkillIDs.WritOfMasterfulStrength, "Writ of Masterful Strength", "200 power over 90% HP", DamageSource.NoPets, 0.0, DamageType.Strike, DamageType.Strike, Source.Item, ByPresenceNonMultiplier,"https://wiki.guildwars2.com/images/2/2b/Writ_of_Masterful_Strength.png",DamageModifierMode.All, (x, log) => x.IsOverNinety),
        };
        internal static readonly List<DamageModifier> GearDamageModifiers = new List<DamageModifier>
        {
            new DamageLogDamageModifier("Scholar Rune", "5% over 90% HP", DamageSource.NoPets, 5.0, DamageType.Strike, DamageType.Strike, Source.Gear,"https://wiki.guildwars2.com/images/2/2b/Superior_Rune_of_the_Scholar.png", (x, log) => x.IsOverNinety, ByPresence, 93543, GW2Builds.EndOfLife, DamageModifierMode.All ),
            new DamageLogDamageModifier("Scholar Rune", "10% over 90% HP", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.Strike, Source.Gear,"https://wiki.guildwars2.com/images/2/2b/Superior_Rune_of_the_Scholar.png", (x, log) => x.IsOverNinety, ByPresence, 0, 93543, DamageModifierMode.All ),
            new DamageLogDamageModifier("Eagle Rune", "10% if target <50% HP", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.Strike, Source.Gear,"https://wiki.guildwars2.com/images/9/9b/Superior_Rune_of_the_Eagle.png", (x, log) => x.AgainstUnderFifty, ByPresence, 93543, GW2Builds.EndOfLife, DamageModifierMode.All),
            new DamageLogDamageModifier("Eagle Rune", "6% if target <50% HP", DamageSource.NoPets, 6.0, DamageType.Strike, DamageType.Strike, Source.Gear,"https://wiki.guildwars2.com/images/9/9b/Superior_Rune_of_the_Eagle.png", (x, log) => x.AgainstUnderFifty, ByPresence , 0 , 93543, DamageModifierMode.All),
            new DamageLogDamageModifier("Thief Rune", "10% while flanking", DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.Strike, Source.Gear,"https://wiki.guildwars2.com/images/9/96/Superior_Rune_of_the_Thief.png", (x, log) => x.IsFlanking , ByPresence, DamageModifierMode.All),
            new BuffDamageModifier(SkillIDs.Might, "Strength Rune", "5% under might",  DamageSource.NoPets, 5.0, DamageType.Strike, DamageType.Strike, Source.Gear, ByPresence, "https://wiki.guildwars2.com/images/2/2b/Superior_Rune_of_Strength.png", DamageModifierMode.All),
            new BuffDamageModifier(SkillIDs.FireAura, "Fire Rune", "10% under fire aura",  DamageSource.NoPets, 10.0, DamageType.Strike, DamageType.Strike, Source.Gear, ByPresence, "https://wiki.guildwars2.com/images/4/4a/Superior_Rune_of_the_Fire.png", 93543 , GW2Builds.EndOfLife, DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Burning, "Flame Legion Rune", "7% on burning target",  DamageSource.NoPets, 7.0, DamageType.Strike, DamageType.Strike, Source.Gear, ByPresence, "https://wiki.guildwars2.com/images/4/4a/Superior_Rune_of_the_Flame_Legion.png", DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.NumberOfBoons, "Spellbreaker Rune", "7% on boonless target",  DamageSource.NoPets, 7.0, DamageType.Strike, DamageType.Strike, Source.Gear, ByAbsence, "https://wiki.guildwars2.com/images/1/1a/Superior_Rune_of_the_Spellbreaker.png", DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Chilled, "Ice Rune", "7% on chilled target",  DamageSource.NoPets, 7.0, DamageType.Strike, DamageType.Strike, Source.Gear, ByPresence, "https://wiki.guildwars2.com/images/7/78/Superior_Rune_of_the_Ice.png", DamageModifierMode.All),
            new BuffDamageModifier(SkillIDs.Fury, "Rage Rune", "5% under fury",  DamageSource.NoPets, 5.0, DamageType.Strike, DamageType.Strike, Source.Gear, ByPresence, "https://wiki.guildwars2.com/images/9/9e/Superior_Rune_of_Rage.png", DamageModifierMode.All),
        };
        internal static readonly List<DamageModifier> CommonDamageModifiers = new List<DamageModifier>
        {
            new BuffDamageModifierTarget(SkillIDs.Exposed31589, "Exposed", "50%", DamageSource.All, 50.0, DamageType.StrikeAndCondition, DamageType.All, Source.Common, ByPresence, "https://wiki.guildwars2.com/images/6/6b/Exposed.png",0, GW2Builds.May2021Balance, DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Exposed31589, "Exposed (Strike)", "30%", DamageSource.All, 30.0, DamageType.Strike, DamageType.All, Source.Common, ByPresence, "https://wiki.guildwars2.com/images/6/6b/Exposed.png", GW2Builds.May2021Balance, GW2Builds.March2022Balance2, DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Exposed31589, "Exposed (Condition)", "100%", DamageSource.All, 100.0, DamageType.Condition, DamageType.All, Source.Common, ByPresence, "https://wiki.guildwars2.com/images/6/6b/Exposed.png", GW2Builds.May2021Balance, GW2Builds.March2022Balance2, DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Exposed31589, "Exposed (Strike)", "10%", DamageSource.All, 10.0, DamageType.Strike, DamageType.All, Source.Common, ByPresence, "https://wiki.guildwars2.com/images/6/6b/Exposed.png", GW2Builds.March2022Balance2, GW2Builds.EndOfLife, DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Exposed31589, "Exposed (Condition)", "20%", DamageSource.All, 20.0, DamageType.Condition, DamageType.All, Source.Common, ByPresence, "https://wiki.guildwars2.com/images/6/6b/Exposed.png", GW2Builds.March2022Balance2, GW2Builds.EndOfLife, DamageModifierMode.All),
            new BuffDamageModifierTarget(SkillIDs.Vulnerability, "Vulnerability", "1% per Stack", DamageSource.All, 1.0, DamageType.StrikeAndCondition, DamageType.All, Source.Common, ByStack, "https://wiki.guildwars2.com/images/a/af/Vulnerability.png", DamageModifierMode.All),
            new BuffApproximateDamageModifier(SkillIDs.FrostSpirit, "Frost Spirit", "5%", DamageSource.NoPets, 5.0, DamageType.Strike, DamageType.All, Source.Common, ByPresence, "https://wiki.guildwars2.com/images/thumb/c/c6/Frost_Spirit.png/33px-Frost_Spirit.png", 88541, GW2Builds.EndOfLife, DamageModifierMode.All),
            new DamageLogDamageModifier("Soulcleave's Summit", "per hit (no ICD)", DamageSource.NoPets, 0, DamageType.Power, DamageType.All, Source.Common,"https://wiki.guildwars2.com/images/7/78/Soulcleave%27s_Summit.png", ((x, log) => x.SkillId == SkillIDs.SoulcleavesSummit), BySkill, 0, GW2Builds.May2021Balance, DamageModifierMode.All),
            new DamageLogDamageModifier("Soulcleave's Summit", "per hit (1s ICD per target)", DamageSource.NoPets, 0, DamageType.Power, DamageType.All, Source.Common,"https://wiki.guildwars2.com/images/7/78/Soulcleave%27s_Summit.png", ((x, log) => x.SkillId == SkillIDs.SoulcleavesSummit), BySkill, GW2Builds.May2021Balance, GW2Builds.EndOfLife, DamageModifierMode.All),
            new DamageLogDamageModifier("One Wolf Pack", "per hit (max. once every 0.25s)", DamageSource.NoPets, 0, DamageType.Power, DamageType.All, Source.Common, "https://wiki.guildwars2.com/images/3/3b/One_Wolf_Pack.png", ((x, log) => x.SkillId == SkillIDs.OneWolfPackSkill), BySkill, DamageModifierMode.All),
        };
        internal static readonly List<DamageModifier> FightSpecificDamageModifiers = new List<DamageModifier>
        {
            new BuffDamageModifier(SkillIDs.ViolentCurrents, "Violent Currents", "5% per stack", DamageSource.NoPets, 5.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByStack, "https://wiki.guildwars2.com/images/0/06/Violent_Currents.png", DamageModifierMode.PvE),
            new BuffDamageModifierTarget(SkillIDs.UnnaturalSignet,"Unnatural Signet", "100%", DamageSource.All, 100.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByPresence, "https://wiki.guildwars2.com/images/2/20/Unnatural_Signet.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.EmpoweredStatueOfDeath,"Empowered (Statue of Death)", "50%", DamageSource.NoPets, 50.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByPresence, "https://wiki.guildwars2.com/images/d/de/Empowered_%28Statue_of_Death%29.png", DamageModifierMode.PvE),
            new BuffDamageModifierTarget(SkillIDs.Compromised, "Compromised", "75% per stack", DamageSource.All, 75.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByStack, "https://wiki.guildwars2.com/images/4/48/Compromised.png", DamageModifierMode.PvE),
            new BuffDamageModifierTarget(SkillIDs.ErraticEnergy, "Erratic Energy", "5% per stack", DamageSource.All, 5.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByStack, "https://wiki.guildwars2.com/images/4/45/Unstable.png", DamageModifierMode.PvE),
            new BuffDamageModifierTarget(SkillIDs.FracturedEnemy, "Fractured - Enemy", "10% per stack", DamageSource.All, 10.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByStack, "https://wiki.guildwars2.com/images/d/d3/Blood_Fueled.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.BloodFueled, "Blood Fueled", "10% per stack", DamageSource.NoPets, 10.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByStack, "https://wiki.guildwars2.com/images/d/d3/Blood_Fueled.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.BloodFueledAbo, "Blood Fueled Abo", "10% per stack", DamageSource.NoPets, 10.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByStack, "https://wiki.guildwars2.com/images/d/d3/Blood_Fueled.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.FractalSavant,"Fractal Savant", "1%", DamageSource.NoPets, 1.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByPresence, "https://wiki.guildwars2.com/images/c/cb/Malign_9_Agony_Infusion.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.FractalProdigy,"Fractal Prodigy", "2%", DamageSource.NoPets, 2.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByPresence, "https://wiki.guildwars2.com/images/1/11/Mighty_9_Agony_Infusion.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.FractalChampion,"Fractal Champion", "4%", DamageSource.NoPets, 4.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByPresence, "https://wiki.guildwars2.com/images/3/3d/Precise_9_Agony_Infusion.png", DamageModifierMode.PvE),
            new BuffDamageModifier(SkillIDs.FractalGod,"Fractal God", "7%", DamageSource.NoPets, 7.0, DamageType.StrikeAndCondition, DamageType.All, Source.FightSpecific, ByPresence, "https://wiki.guildwars2.com/images/2/22/Healing_9_Agony_Infusion.png", DamageModifierMode.PvE),
        };

    }
}
