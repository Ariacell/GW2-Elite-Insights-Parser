﻿using LuckParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LuckParser.Models.Statistics;

namespace LuckParser.Models.ParseModels
{
    public abstract class DamageModifier
    {
        public enum DamageType { All, Power, Condition};
        public enum ModifierSource { All, Necromancer, Elementalist, Mesmer, Warrior, Revenant, Guardian, Thief, Ranger, Engineer};
        public static ModifierSource ProfToEnum(string prof)
        {
            switch (prof)
            {
                case "Druid":
                case "Ranger":
                case "Soulbeast":
                    return ModifierSource.Ranger;
                case "Scrapper":
                case "Holosmith":
                case "Engineer":
                    return ModifierSource.Engineer;
                case "Daredevil":
                case "Deadeye":
                case "Thief":
                    return ModifierSource.Thief;
                case "Weaver":
                case "Tempest":
                case "Elementalist":
                    return ModifierSource.Elementalist;
                case "Mirage":
                case "Chronomancer":
                case "Mesmer":
                    return ModifierSource.Mesmer;
                case "Scourge":
                case "Reaper":
                case "Necromancer":
                    return ModifierSource.Necromancer;
                case "Spellbreaker":
                case "Berserker":
                case "Warrior":
                    return ModifierSource.Warrior;
                case "Firebrand":
                case "Dragonhunter":
                case "Guardian":
                    return ModifierSource.Guardian;
                case "Renegade":
                case "Herald":
                case "Revenant":
                    return ModifierSource.Revenant;
            }
            return ModifierSource.All;
        }

        private DamageType _compareType { get; }
        private DamageType _srcType { get; }
        private bool _withPets { get; }
        protected double GainPerStack { get; }
        protected readonly GainComputer GainComputer;
        public ModifierSource Src { get; }
        protected long ID { get; }
        public string Url { get; }
        public string Name { get; }

        private static readonly GainComputer _byPresence = new GainComputerByPresence();
        private static readonly GainComputer _byStack = new GainComputerByStack();
        private static readonly GainComputer _nonMultiplier = new GainComputerNonMultiplier();

        private static readonly List<DamageModifier> _allDamageModifier = new List<DamageModifier>() {
            /// commons
            new DamageLogDamageModifier(Boon.ScholarRune,"Scholar Rune", false, 5.0, DamageType.Power, DamageType.Power, ModifierSource.All,"https://wiki.guildwars2.com/images/2/2b/Superior_Rune_of_the_Scholar.png", x => x.IsNinety, _byPresence ),
            new DamageLogDamageModifier(Boon.EagleRune, "Eagle Rune", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.All,"https://wiki.guildwars2.com/images/9/9b/Superior_Rune_of_the_Eagle.png", x => x.IsFifty, _byPresence ),
            new DamageLogDamageModifier(Boon.MovingBuff, "Moving", false, 5.0, DamageType.Power, DamageType.Power, ModifierSource.All,"https://wiki.guildwars2.com/images/1/1c/Bowl_of_Seaweed_Salad.png", x => x.IsMoving, _byPresence ),
            new DamageLogDamageModifier(Boon.ThiefRune, "Thief Rune", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.All,"https://wiki.guildwars2.com/images/9/96/Superior_Rune_of_the_Thief.png", x => x.IsFlanking , _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Vulnerability"], false, 1.0, DamageType.All, DamageType.All, ModifierSource.All, _byStack),
            new BuffDamageModifier(Boon.BoonsByName["Frost Spirit"], false, 5.0, DamageType.Power, DamageType.Power, ModifierSource.All, _byPresence),
            new DamageLogDamageModifier(Boon.BoonsByName["Soulcleave's Summit"], 0, false, DamageType.Power, DamageType.Power, ModifierSource.All, _byPresence),
            new DamageLogDamageModifier(Boon.BoonsByName["One Wolf Pack"], 0, false, DamageType.Power, DamageType.Power, ModifierSource.All, _byPresence),
            new DamageLogDamageModifier(Boon.BoonsByName["Static Charge"], 0, false, DamageType.Power, DamageType.Power, ModifierSource.All, _byPresence),
            //new BuffDamageModifier(Boon.BoonsByName["Glyph of Empowerment"], false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.All, _nonMultiplier),
            new BuffDamageModifierTarget(Boon.BoonsByName["Unnatural Signet"], false, 200.0, DamageType.Power, DamageType.Power, ModifierSource.All, _byPresence),
            new BuffDamageModifierTarget(Boon.BoonsByName["Compromised"], false, 75.0, DamageType.Power, DamageType.Power, ModifierSource.All, _byStack),
            new BuffDamageModifierTarget(Boon.BoonsByName["Fractured - Enemy"], false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.All, _byStack),
            new BuffDamageModifier(Boon.BoonsByName["Blood Fueled"], false, 20.0, DamageType.Power, DamageType.Power, ModifierSource.All, _byStack),
            new BuffDamageModifier(Boon.BoonsByName["Blood Fueled Abo"], false, 20.0, DamageType.Power, DamageType.Power, ModifierSource.All, _byStack),
            /// Revenant
            new DamageLogDamageModifier(Boon.BoonsByName["Impossible Odds"], 0, false, DamageType.Power, DamageType.Power, ModifierSource.Revenant, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Vicious Lacerations"], false, 3.0, DamageType.Power, DamageType.Power, ModifierSource.Revenant, _byStack),
            new BuffDamageModifier(Boon.BoonsByName["Retaliation"].ID, "Vicious Reprisal", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Revenant, _byPresence, "https://wiki.guildwars2.com/images/c/cf/Vicious_Reprisal.png"),
            new BuffDamageModifier(Boon.BoonsByName["Number of Boons"].ID, "Reinforced Potency", false, 1.0, DamageType.Power, DamageType.Power, ModifierSource.Revenant, _byStack, "https://wiki.guildwars2.com/images/0/0a/Envoy_of_Sustenance.png"),
            new BuffDamageModifier(Boon.BoonsByName["Fury"].ID, "Ferocious Aggression", false, 7.0, DamageType.All, DamageType.All, ModifierSource.Revenant, _byPresence, "https://wiki.guildwars2.com/images/e/ec/Ferocious_Aggression.png"),
            new BuffDamageModifier(Boon.BoonsByName["Kalla's Fervor"], false, 2.0, DamageType.Condition, DamageType.Condition, ModifierSource.Revenant, _byStack),
            new BuffDamageModifier(Boon.BoonsByName["Improved Kalla's Fervor"], false, 3.0, DamageType.Condition, DamageType.Condition, ModifierSource.Revenant, _byStack),
            new BuffDamageModifierTarget(Boon.BoonsByName["Vulnerability"].ID, "Targeted Destruction", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Revenant, _byPresence, "https://wiki.guildwars2.com/images/e/ed/Targeted_Destruction.png"),
            new DamageLogDamageModifier(Boon.SwiftTermination, "Swift Termination", false, 20.0, DamageType.Power, DamageType.Power, ModifierSource.Revenant,"https://wiki.guildwars2.com/images/b/bb/Swift_Termination.png", x => x.IsFifty, _byPresence),
            /// Warrior
            new BuffDamageModifier(Boon.BoonsByName["Peak Performance"], false, 20.0, DamageType.Power, DamageType.Power, ModifierSource.Warrior, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Always Angry"], false, 7.0, DamageType.All, DamageType.All, ModifierSource.Warrior, _byStack),
            new BuffDamageModifierTarget(Boon.BoonsByName["Weakness"].ID, "Cull the Weak", false, 7.0, DamageType.Power, DamageType.Power, ModifierSource.Warrior, _byPresence, "https://wiki.guildwars2.com/images/7/72/Cull_the_Weak.png"),
            new BuffDamageModifier(Boon.BoonsByName["Number of Boons"].ID, "Empowered", false, 1.0, DamageType.Power, DamageType.Power, ModifierSource.Warrior, _byStack, "https://wiki.guildwars2.com/images/c/c2/Empowered.png"),
            new BuffDamageModifierTarget(Boon.BoonsByName["Number of Boons"].ID, "Destruction of the Empowered", false, 3.0, DamageType.Power, DamageType.Power, ModifierSource.Warrior, _byStack, "https://wiki.guildwars2.com/images/5/5c/Destruction_of_the_Empowered.png"),
            /// Guardian
            new BuffDamageModifierTarget(Boon.BoonsByName["Burning"].ID, "Fiery Wrath", false, 7.0, DamageType.Power, DamageType.Power, ModifierSource.Guardian, _byPresence, "https://wiki.guildwars2.com/images/7/70/Fiery_Wrath.png"),
            new BuffDamageModifier(Boon.BoonsByName["Retaliation"].ID, "Retribution", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Guardian, _byStack, "https://wiki.guildwars2.com/images/d/d7/Retribution_%28trait%29.png"),
            new BuffDamageModifier(Boon.BoonsByName["Aegis"].ID, "Unscathed Contender", false, 20.0, DamageType.Power, DamageType.Power, ModifierSource.Guardian, _byPresence, "https://wiki.guildwars2.com/images/b/b4/Unscathed_Contender.png"),
            new BuffDamageModifier(Boon.BoonsByName["Number of Boons"].ID, "Power of the Virtuous", false, 1.0, DamageType.Power, DamageType.Power, ModifierSource.Guardian, _byStack, "https://wiki.guildwars2.com/images/b/b4/Unscathed_Contender.png"),
            new BuffDamageModifierTarget(Boon.BoonsByName["Crippled"].ID, "Zealot's Aggression", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Guardian, _byPresence, "https://wiki.guildwars2.com/images/b/b4/Unscathed_Contender.png"),
            /// ENGINEER
            new BuffDamageModifier(Boon.BoonsByName["Laser's Edge"], false, 15.0, DamageType.Power, DamageType.Power, ModifierSource.Engineer, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Thermal Vision"], false, 5.0, DamageType.Condition, DamageType.Condition, ModifierSource.Engineer, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Vigor"].ID, "Excessive Energy", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Engineer, _byPresence, "https://wiki.guildwars2.com/images/1/1f/Excessive_Energy.png"),
            new BuffDamageModifierTarget(Boon.BoonsByName["Vulnerability"].ID, "Shaped Charge", false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Engineer, _byPresence, "https://wiki.guildwars2.com/images/f/f3/Explosive_Powder.png"),
            new BuffDamageModifierTarget(Boon.BoonsByName["Number of Conditions"].ID, "Modified Ammunition", false, 2.0, DamageType.Power, DamageType.Power, ModifierSource.Engineer, _byStack, "https://wiki.guildwars2.com/images/9/94/Modified_Ammunition.png"),
            /// RANGER
            new BuffDamageModifier(Boon.BoonsByName["Sic 'Em!"], false, 40.0, DamageType.Power, DamageType.Power, ModifierSource.Ranger, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Light on your Feet"], false, 10.0, DamageType.Power, DamageType.Power, ModifierSource.Ranger, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Twice as Vicious"], false, 5.0, DamageType.All, DamageType.All, ModifierSource.Ranger, _byPresence),
            new BuffDamageModifier(Boon.BoonsByName["Number of Boons"].ID, "Bountiful Hunter", true, 1.0, DamageType.Power, DamageType.Power, ModifierSource.Ranger, _byStack, "https://wiki.guildwars2.com/images/2/25/Bountiful_Hunter.png"),
            new BuffDamageModifier(Boon.BoonsByName["Fury"].ID, "Furious Strength", true, 1.0, DamageType.Power, DamageType.Power, ModifierSource.Ranger, _byStack, "https://wiki.guildwars2.com/images/c/ca/Furious_Strength.png"),
            // TODO Predator's Onslaught
        };

        public static Dictionary<ModifierSource, List<DamageModifier>> DamageModifiersPerSource = _allDamageModifier.GroupBy(x => x.Src).ToDictionary(x => x.Key, x => x.ToList());

        protected DamageModifier(long id, string name, bool withPets,  double gainPerStack, DamageType srctype, DamageType compareType, ModifierSource src, string url, GainComputer gainComputer)
        {
            ID = id;
            Name = name;
            _withPets = withPets;
            GainPerStack = gainPerStack;
            _compareType = compareType;
            _srcType = srctype;
            Src = src;
            Url = url;
            GainComputer = gainComputer;
        }

        protected (int total, int count)  GetTotalDamageData(Player p, ParsedLog log, Target t, PhaseData phase)
        {
            List<DamageLog> dls = new List<DamageLog>();
            switch (_compareType)
            {
                case DamageType.All:
                    dls = _withPets ? p.GetDamageLogs(t,log, phase.Start, phase.End) : p.GetJustPlayerDamageLogs(t, log, phase.Start, phase.End);
                    break;
                case DamageType.Condition:
                    dls = (_withPets ? p.GetDamageLogs(t, log, phase.Start, phase.End) : p.GetJustPlayerDamageLogs(t, log, phase.Start, phase.End)).Where(x => x.IsIndirectDamage).ToList();
                    break;
                case DamageType.Power:
                    dls = (_withPets ? p.GetDamageLogs(t, log, phase.Start, phase.End) : p.GetJustPlayerDamageLogs(t, log, phase.Start, phase.End)).Where(x => !x.IsIndirectDamage).ToList();
                    break;
            }
            return (dls.Sum(x => x.Damage), dls.Count);
        }

        protected List<DamageLog> GetDamageLogs(Player p, ParsedLog log, Target t, PhaseData phase)
        {
            switch (_srcType)
            {
                case DamageType.All:
                    return _withPets ? p.GetDamageLogs(t, log, phase.Start, phase.End) : p.GetJustPlayerDamageLogs(t, log, phase.Start, phase.End);
                case DamageType.Condition:
                    return (_withPets ? p.GetDamageLogs(t, log, phase.Start, phase.End) : p.GetJustPlayerDamageLogs(t, log, phase.Start, phase.End)).Where(x => x.IsIndirectDamage).ToList();
                case DamageType.Power:
                default:
                    return (_withPets ? p.GetDamageLogs(t, log, phase.Start, phase.End) : p.GetJustPlayerDamageLogs(t, log, phase.Start, phase.End)).Where(x => !x.IsIndirectDamage).ToList();
            }
        }

        public abstract void ComputeDamageModifier(Dictionary<long, List<ExtraBoonData>> data, Dictionary<Target, Dictionary<long, List<ExtraBoonData>>> dataTarget, Player p, ParsedLog log);
    }
}
