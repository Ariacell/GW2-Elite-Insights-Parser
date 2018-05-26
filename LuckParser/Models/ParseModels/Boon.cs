﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuckParser.Models.ParseModels
{
    public class Boon
    {
        // Boon
        public enum BoonEnum { Condition, Boon, OffensiveBuff, DefensiveBuff, NonShareableBuff, Food, Utility};
        public enum BoonSource { Mixed, Necromancer, Elementalist, Mesmer, Warrior, Revenant, Guardian, Thief, Ranger, Engineer, Item  };

        public static BoonSource ProfToEnum(string prof)
        {
            switch(prof)
            {
                case "Druid":
                case "Ranger":
                case "Soulbeast":
                    return BoonSource.Ranger;
                case "Scrapper":
                case "Holosmith":
                case "Engineer":
                    return BoonSource.Engineer;
                case "Daredevil":
                case "Deadeye":
                case "Thief":
                    return BoonSource.Thief;
                case "Weaver":
                case "Tempest":
                case "Elementalist":
                    return BoonSource.Elementalist;
                case "Mirage":
                case "Chronomancer":
                case "Mesmer":
                    return BoonSource.Mesmer;
                case "Scourge":
                case "Reaper":
                case "Necromancer":
                    return BoonSource.Necromancer;
                case "Spellbreaker":
                case "Berserker":
                case "Warrior":
                    return BoonSource.Warrior;
                case "Firebrand":
                case "Dragonhunter":
                case "Guardian":
                    return BoonSource.Guardian;
                case "Renegade":
                case "Herald":
                case "Revenant":
                    return BoonSource.Revenant;
            }
            return BoonSource.Mixed;
        }

        // Fields
        private String name;
        private int id;
        private BoonEnum priority;
        private BoonSource plotlyGroup;
        private String type;
        private int capacity;
        private string link;


        private Boon(String name, BoonSource group, String type, int capacity, BoonEnum priority)
        {
            this.name = name;
            this.plotlyGroup = group;
            this.type = type;
            this.id = -1;
            this.capacity = capacity;
            this.priority = priority;
            this.link = "";
        }
        private Boon(String name, int id, BoonSource group, String type, int capacity, BoonEnum priority)
        {
            this.name = name;
            this.id = id;
            this.plotlyGroup = group;
            this.type = type;
            this.capacity = capacity;
            this.priority = priority;
            this.link = "";
        }

        private Boon(String name, int id, BoonSource group, String type, int capacity, BoonEnum priority, string link)
        {
            this.name = name;
            this.id = id;
            this.plotlyGroup = group;
            this.type = type;
            this.capacity = capacity;
            this.priority = priority;
            this.link = link;
        }
        // Public Methods

        private static List<Boon> allBoons = new List<Boon>
            {
                //Base boons
                new Boon("Might", 740, BoonSource.Mixed, "intensity", 25, BoonEnum.Boon, "https://wiki.guildwars2.com/images/7/7c/Might.png"),
                new Boon("Fury", 725, BoonSource.Mixed, "duration", 9, BoonEnum.Boon, "https://wiki.guildwars2.com/images/4/46/Fury.png"),//or 3m and 30s
                new Boon("Quickness", 1187, BoonSource.Mixed, "duration", 5, BoonEnum.Boon, "https://wiki.guildwars2.com/images/b/b4/Quickness.png"),
                new Boon("Alacrity", 30328, BoonSource.Mixed, "duration", 9, BoonEnum.Boon, "https://wiki.guildwars2.com/images/thumb/4/4c/Alacrity.png/20px-Alacrity.png"),
                new Boon("Protection", 717, BoonSource.Mixed, "duration", 5, BoonEnum.Boon, "https://wiki.guildwars2.com/images/6/6c/Protection.png"),
                new Boon("Regeneration", 718, BoonSource.Mixed, "duration", 5, BoonEnum.Boon, "https://wiki.guildwars2.com/images/5/53/Regeneration.png"),
                new Boon("Vigor", 726, BoonSource.Mixed, "duration", 5, BoonEnum.Boon, "https://wiki.guildwars2.com/images/f/f4/Vigor.png"),
                new Boon("Aegis", 743, BoonSource.Mixed, "duration", 9, BoonEnum.Boon, "https://wiki.guildwars2.com/images/e/e5/Aegis.png"),
                new Boon("Stability", 1122, BoonSource.Mixed, "intensity", 25, BoonEnum.Boon, "https://wiki.guildwars2.com/images/a/ae/Stability.png"),
                new Boon("Swiftness", 719, BoonSource.Mixed, "duration", 9, BoonEnum.Boon, "https://wiki.guildwars2.com/images/a/af/Swiftness.png"),
                new Boon("Retaliation", 873, BoonSource.Mixed, "duration", 9, BoonEnum.Boon, "https://wiki.guildwars2.com/images/5/53/Retaliation.png"),
                new Boon("Resistance", 26980, BoonSource.Mixed, "duration", 5, BoonEnum.Boon, "https://wiki.guildwars2.com/images/thumb/e/e9/Resistance_40px.png/20px-Resistance_40px.png"),
                // Condis         
                new Boon("Bleeding", 736, BoonSource.Mixed, "intensity", 1500, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/3/33/Bleeding.png/20px-Bleeding.png"),
                new Boon("Burning", 737, BoonSource.Mixed, "intensity", 1500, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/4/45/Burning.png/20px-Burning.png"),
                new Boon("Confusion", 861, BoonSource.Mixed, "intensity", 1500, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/e/e6/Confusion.png/20px-Confusion.png"),
                new Boon("Poison", 723, BoonSource.Mixed, "intensity", 1500, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/0/05/Poison.png/20px-Poison.png"),
                new Boon("Torment", 19426, BoonSource.Mixed, "intensity", 1500, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/0/08/Torment.png/20px-Torment.png"),
                new Boon("Blind", 720, BoonSource.Mixed, "duration", 9, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/3/33/Blinded.png/20px-Blinded.png"),
                new Boon("Chilled", 722, BoonSource.Mixed, "duration", 5, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/a/a6/Chilled.png/20px-Chilled.png"),
                new Boon("Crippled", 721, BoonSource.Mixed, "duration", 9, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/f/fb/Crippled.png/20px-Crippled.png"),
                new Boon("Fear", 791, BoonSource.Mixed, "duration", 9, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/e/e6/Fear.png/20px-Fear.png"),
                new Boon("Immobile", 727, BoonSource.Mixed, "duration", 3, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/3/32/Immobile.png/20px-Immobile.png"),
                new Boon("Slow", 26766, BoonSource.Mixed, "duration", 9, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/f/fb/Slow_40px.png/20px-Slow_40px.png"),
                new Boon("Weakness", 742, BoonSource.Mixed, "duration", 5, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/f/f9/Weakness.png/20px-Weakness.png"),
                new Boon("Taunt", 27705, BoonSource.Mixed, "duration", 5, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/c/cc/Taunt.png/20px-Taunt.png"),
                new Boon("Vulnerability", 738, BoonSource.Mixed, "intensity", 25, BoonEnum.Condition, "https://wiki.guildwars2.com/images/thumb/a/af/Vulnerability.png/20px-Vulnerability.png"),
                new Boon("Retaliation", 873, BoonSource.Mixed, "duration", 9, BoonEnum.Condition, "https://wiki.guildwars2.com/images/5/53/Retaliation.png"),
                // Generic
                new Boon("Stealth", 13017, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Revealed", 890, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Superspeed", 5974, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff, "https://wiki.guildwars2.com/images/1/1a/Super_Speed.png"),
                //new Boon("Invulnerability", 801, BoonSource.Mixed, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/e/eb/Determined.png"),
                new Boon("Unblockable", BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                //Auras
                new Boon("Chaos Armor", 10332, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Fire Shield", 5677, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Frost Aura", 5579, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Light Aura", 25518, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Magnetic Aura", 5684, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Shocking Aura", 5577, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                //race
                new Boon("Take Root", 12459, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Become the Bear",12426, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Become the Raven",12405, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Become the Snow Leopard",12400, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Become the Wolf",12393, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Avatar of Melandru", 12368, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Power Suit",12326, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Reaper of Grenth", 12366, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Charrzooka",43503, BoonSource.Mixed, "duration", 1, BoonEnum.NonShareableBuff),
                ///REVENANT
                //skills
                new Boon("Crystal Hibernation", 28262, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Vengeful Hammers", 27273, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Rite of the Great Dwarf", 26596, BoonSource.Revenant, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/6/69/Rite_of_the_Great_Dwarf.png"),
                new Boon("Embrace the Darkness", 28001, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),// incorrect id?
                new Boon("Enchanted Daggers", 28557, BoonSource.Revenant, "intensity", 6, BoonEnum.NonShareableBuff),
                new Boon("Impossible Odds", 27581, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                //facets
                new Boon("Facet of Light",27336, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Infuse Light",27737, BoonSource.Revenant, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/6/60/Infuse_Light.png"),
                new Boon("Facet of Darkness",28036, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Facet of Elements",28243, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Facet of Strength",27376, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Facet of Chaos",27983, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Facet of Nature",29275, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Naturalistic Resonance", 29379, BoonSource.Revenant, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/e/e9/Facet_of_Nature.png"),
                //legends
                new Boon("Legendary Centaur Stance",27972, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Legendary Dragon Stance",27732, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Legendary Dwarf Stance",27205, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Legendary Demon Stance",27928, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Legendary Assassin Stance",27890, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Legendary Renegade Stance",44272, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                //summons
                new Boon("Breakrazor's Bastion",44682, BoonSource.Revenant, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/a/a7/Breakrazor%27s_Bastion.png"),
                new Boon("Razorclaw's Rage",41016, BoonSource.Revenant, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/7/73/Razorclaw%27s_Rage.png"),
                new Boon("Soulcleave's Summit",45026, BoonSource.Revenant, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/7/78/Soulcleave%27s_Summit.png"),
                //traits
                new Boon("Vicious Lacerations",29395, BoonSource.Revenant, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Assassin's Presence", 26854, BoonSource.Revenant, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/5/54/Assassin%27s_Presence.png"),
                //new Boon("Expose Defenses", 48894, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Invoking Harmony",29025, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Selfless Amplification",30509, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Hardening Persistence",28957, BoonSource.Revenant, "intensity", 8, BoonEnum.NonShareableBuff),
                new Boon("Soothing Bastion",34136, BoonSource.Revenant, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Kalla's Fervor",42883, BoonSource.Revenant, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Improved Kalla's Fervor",45614, BoonSource.Revenant, "intensity", 5, BoonEnum.NonShareableBuff),
                ///WARRIOR
                //skills
                new Boon("Riposte",14434, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Counterattack",14509, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Flames of War", 31708, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Blood Reckoning", 29466 , BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Rock Guard", 34256 , BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Sight beyond Sight",40616, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                //signets
                new Boon("Healing Signet",786, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Dolyak Signet",14458, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Fury",14459, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Might",14444, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Stamina",14478, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Rage",14496, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                //banners
                new Boon("Banner of Strength", 14417, BoonSource.Warrior, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/e/e1/Banner_of_Strength.png/33px-Banner_of_Strength.png"),
                new Boon("Banner of Discipline", 14449, BoonSource.Warrior, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/5/5f/Banner_of_Discipline.png/33px-Banner_of_Discipline.png"),
                new Boon("Banner of Tactics",14450, BoonSource.Warrior, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/3/3f/Banner_of_Tactics.png/33px-Banner_of_Tactics.png"),
                new Boon("Banner of Defense",14543, BoonSource.Warrior, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/f/f1/Banner_of_Defense.png/33px-Banner_of_Defense.png"),
                //stances
                new Boon("Shield Stance",756, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Berserker's Stance",14453, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Enduring Pain",787, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Balanced Stance",34778, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Defiant Stance",21816, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Rampage",14484, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Empower Allies", 14222, BoonSource.Warrior, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/4/4c/Empower_Allies.png/20px-Empower_Allies.png"),
                new Boon("Peak Performance",46853, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Furious Surge", 30204, BoonSource.Warrior, "intensity", 25, BoonEnum.NonShareableBuff),
                new Boon("Health Gain per Adrenaline bar Spent", BoonSource.Warrior, "intensity", 3, BoonEnum.NonShareableBuff),
                new Boon("Rousing Resilience",24383, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Always Angry",34099, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Full Counter",43949, BoonSource.Warrior, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Attacker's Insight",41963, BoonSource.Warrior, "intensity", 5, BoonEnum.NonShareableBuff),
                /// GUARDIAN
                //skills
                new Boon("Zealot's Flame", 9103, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Purging Flames",21672, BoonSource.Guardian, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/2/28/Purging_Flames.png"),
                new Boon("Litany of Wrath",21665, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Renewed Focus",9255, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Ashes of the Just",41957, BoonSource.Guardian, "intensity", 25, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/6/6d/Epilogue-_Ashes_of_the_Just.png"),
                new Boon("Eternal Oasis",44871, BoonSource.Guardian, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/5/5f/Epilogue-_Eternal_Oasis.png"),
                new Boon("Unbroken Lines",43194, BoonSource.Guardian, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/d/d8/Epilogue-_Unbroken_Lines.png"),
                //signets
                new Boon("Signet of Resolve",9220, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Bane Signet",9092, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Judgment",9156, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Mercy",9162, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Wrath",9100, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Courage",29633, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                //virtues
                new Boon("Virtue of Justice", 9114, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Spears of Justice", 29632, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Virtue of Courage", 9113, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Shield of Courage", 29523, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Virtue of Resolve", 9119, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Wings of Resolve", 30308, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Tome of Justice",40530, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Tome of Courage",43508,BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Tome of Resolve",46298, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Strength in Numbers",13796, BoonSource.Guardian, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/7/7b/Strength_in_Numbers.png"),
                new Boon("Invigorated Bulwark",30207, BoonSource.Guardian, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Battle Presence", 17046, BoonSource.Guardian, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/2/27/Battle_Presence.png"),
                new Boon("Force of Will",29485, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),//not sure if intensity
                new Boon("Quickfire",45123, BoonSource.Guardian, "duration", 1, BoonEnum.NonShareableBuff),
                ///ENGINEER
                //skills
                new Boon("Static Shield",6055, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Absorb",6056, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("A.E.D.",21660, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Elixir S",5863, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Elixir X", BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Utility Goggles",5864, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Slick Shoes",5833, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Watchful Eye", BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Cooling Vapor",46444, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Photon Wall Deployed",46094, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Spectrum Shield",43066, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Gear Shield",5997, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                //Transforms
                new Boon("Rampage", BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Photon Forge",43708, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                //Traits
                new Boon("Laser's Edge",44414, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Afterburner",42210, BoonSource.Engineer, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Iron Blooded",49065, BoonSource.Engineer, "intensity", 25, BoonEnum.NonShareableBuff),
                new Boon("Streamlined Kits",18687, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Kinetic Charge",45781, BoonSource.Engineer, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Pinpoint Distribution", 38333, BoonSource.Engineer, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/b/bf/Pinpoint_Distribution.png"),
                new Boon("Heat Therapy",40694, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Overheat", 40397, BoonSource.Engineer, "duration", 1, BoonEnum.NonShareableBuff),
                ///RANGER
                new Boon("Celestial Avatar", 31508, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                //signets
                new Boon("Signet of Renewal",41147, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Stone",12627, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of the Hunt",12626, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of the Wild",12636, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                //spirits
                 //spirits
               // new Boon("Water Spirit", 50386, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/0/06/Water_Spirit.png/33px-Water_Spirit.png"),
                new Boon("Frost Spirit", 12544, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/c/c6/Frost_Spirit.png/33px-Frost_Spirit.png"),
                new Boon("Sun Spirit", 12540, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/d/dd/Sun_Spirit.png/33px-Sun_Spirit.png"),
                new Boon("Stone Spirit", 12547, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/3/35/Stone_Spirit.png/20px-Stone_Spirit.png"),
                //new Boon("Storm Spirit", 50381, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/2/25/Storm_Spirit.png/30px-Storm_Spirit.png"),
                //reworked
                new Boon("Water Spirit", 50386, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/0/06/Water_Spirit.png/33px-Water_Spirit.png"),
                new Boon("Frost Spirit", 50421, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/c/c6/Frost_Spirit.png/33px-Frost_Spirit.png"),
                new Boon("Sun Spirit", 50413, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/d/dd/Sun_Spirit.png/33px-Sun_Spirit.png"),
                new Boon("Stone Spirit", 50415, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/3/35/Stone_Spirit.png/20px-Stone_Spirit.png"),
                new Boon("Storm Spirit", 50381, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/thumb/2/25/Storm_Spirit.png/30px-Storm_Spirit.png"),
                //skills
                new Boon("Attack of Opportunity",12574, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Call of the Wild",36781, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Strength of the pack!",12554, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Sick 'Em!",33902, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Sharpening Stones",12536, BoonSource.Ranger, "intenstiy", 10, BoonEnum.NonShareableBuff),
                new Boon("Ancestral Grace", 31584, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Glyph of Empowerment", 31803, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/thumb/f/f0/Glyph_of_Empowerment.png/33px-Glyph_of_Empowerment.png"),
                new Boon("Dolyak Stance",41815, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/7/71/Dolyak_Stance.png"),
                new Boon("Griffon Stance",46280, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/9/98/Griffon_Stance.png"),
                new Boon("Moa Stance",45038, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/6/66/Moa_Stance.png"),
                new Boon("Vulture Stance",44651, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/8/8f/Vulture_Stance.png"),
                new Boon("Bear Stance",40045, BoonSource.Ranger, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/f/f0/Bear_Stance.png"),
                new Boon("One Wolf Pack",44139, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/3/3b/One_Wolf_Pack.png"),
                new Boon("Sharpen Spines",43266, BoonSource.Ranger, "intensity", 5, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Spotter", 14055, BoonSource.Ranger, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/b/b0/Spotter.png"),
                new Boon("Opening Strike",13988, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Quick Draw",29703, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Light on your feet",30673, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Natural Mender",30449, BoonSource.Ranger, "intensity", 10, BoonEnum.NonShareableBuff),
                new Boon("Lingering Light",32248, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Deadly",44932, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Ferocious",41720, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Supportive",40069, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Versatile",44693, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Stout",40272, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Unstoppable Union",44439, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Twice as Vicious",45600, BoonSource.Ranger, "duration", 1, BoonEnum.NonShareableBuff),
                ///THIEF
                //signets
                new Boon("Signet of Malice",13049, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Assassin's Signet (Passive)",13047, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Assassin's Signet (Active)",44597, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Infiltrator's Signet",13063, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Agility",13061, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Shadows",13059, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                //venoms
                new Boon("Skelk Venom",-1, BoonSource.Thief, "intensity", 4, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/7/75/Skelk_Venom.png"),
                new Boon("Ice Drake Venom",13095, BoonSource.Thief, "intensity", 4, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/7/7b/Ice_Drake_Venom.png"),
                new Boon("Devourer Venom", 13094, BoonSource.Thief, "intensity", 2, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/4/4d/Devourer_Venom.png"),
                new Boon("Skale Venom", 13036, BoonSource.Thief, "intensity", 4, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/1/14/Skale_Venom.png"),
                new Boon("Spider Venom",13036, BoonSource.Thief, "intensity", 6, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/3/39/Spider_Venom.png"),
                new Boon("Basilisk Venom", 13133, BoonSource.Thief, "intensity", 6, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/3/3a/Basilisk_Venom.png"),
                //physical
                new Boon("Palm Strike",30423, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Pulmonary Impact",30510, BoonSource.Thief, "intensity", 2, BoonEnum.NonShareableBuff),
                //weapon
                new Boon("Infiltration",13135, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                //transforms
                new Boon("Dagger Storm",13134, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Kneeling",42869, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Deadeyes's Gaze",46333, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Maleficent Seven",43606, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Hidden Killer",42720, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Lead Attacks",34659, BoonSource.Thief, "intensity", 15, BoonEnum.NonShareableBuff),
                new Boon("Instant Reflexes",34283, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Lotus Training", 32200, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Unhindered Combatant", 32931, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Bounding Dodger", 33162, BoonSource.Thief, "duration", 1, BoonEnum.NonShareableBuff),
                ///MESMER
                //signets
                new Boon("Signet of the Ether", 21751, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Domination",10231, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Illusions",10246, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Inspiration",10235, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Midnight",10233, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Humility",30739, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                //skills
                new Boon("Distortion",10243, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Blur", 10335 , BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Mirror",10357, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Echo",29664, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Illusion of Life", BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                //new Boon("Time Block",30134, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff), What is this?
                new Boon("Time Echo",29582, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Time Anchored",30136, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Fencer's Finesse", 30426 , BoonSource.Mesmer, "intensity", 10, BoonEnum.NonShareableBuff),
                new Boon("Illusionary Defense",49099, BoonSource.Mesmer, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Compunding Power",49058, BoonSource.Mesmer, "intensity", 5, BoonEnum.NonShareableBuff),
                new Boon("Phantasmal Force", 44691 , BoonSource.Mesmer, "intensity", 25, BoonEnum.NonShareableBuff),
                new Boon("Mirage Cloak",40408, BoonSource.Mesmer, "duration", 1, BoonEnum.NonShareableBuff),
                ///NECROMANCER
                //forms
                new Boon("Lich Form",10631, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Death Shroud", 790, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Reaper's Shroud", 29446, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                //signets
                new Boon("Signet of Vampirism",21761, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Plague Signet",10630, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Spite",10621, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of the Locust",10614, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Undeath",10610, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                //skills
                new Boon("Spectral Walk",15083, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Infusing Terror", 30129, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Corrupter's Defense",30845, BoonSource.Necromancer, "intenstiy", 10, BoonEnum.NonShareableBuff),
                new Boon("Vampiric Aura", 30285, BoonSource.Necromancer, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/d/da/Vampiric_Presence.png"),
                new Boon("Last Rites",29726, BoonSource.Necromancer, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/1/1a/Last_Rites_%28effect%29.png"),
                new Boon("Sadistic Searing",43626, BoonSource.Necromancer, "duration", 1, BoonEnum.NonShareableBuff),
                ///ELEMENTALIST
                //signets
                new Boon("Signet of Restoration",739, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Air",5590, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Earth",5592, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Fire",5544, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Signet of Water",5591, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                //attunments
                new Boon("Fire Attunement", 5585, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Water Attunement", 5586, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Air Attunement", 5575, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Earth Attunement", 5580, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                //forms
                new Boon("Mist Form",5543, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Ride the Lightning",5588, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Vapor Form",5620, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Tornado",5534, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Whirlpool", BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                //conjures
                new Boon("Conjure Earth Shield", 15788, BoonSource.Elementalist, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/7/7a/Conjure_Earth_Shield.png"),
                new Boon("Conjure Flame Axe", 15789, BoonSource.Elementalist, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/a/a1/Conjure_Flame_Axe.png"),
                new Boon("Conjure Frost Bow", 15790, BoonSource.Elementalist, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/c/c3/Conjure_Frost_Bow.png"),
                new Boon("Conjure Lightning Hammer", 15791, BoonSource.Elementalist, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/1/1f/Conjure_Lightning_Hammer.png"),
                new Boon("Conjure Fiery Greatsword", 15792, BoonSource.Elementalist, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/e/e2/Conjure_Fiery_Greatsword.png"),
                //skills
                new Boon("Arcane Power",5582, BoonSource.Elementalist, "duration", 1, BoonEnum.OffensiveBuff, "https://wiki.guildwars2.com/images/7/72/Arcane_Power.png"),
                new Boon("Arcane Shield",5640, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Renewal of Fire",5764, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Glyph of Elemental Power (Fire)",5739, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Glyph of Elemental Power (Water)",5741, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Glyph of Elemental Power (Air)",5740, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Glyph of Elemental Power (Earth)",5742, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Rebound",31337, BoonSource.Elementalist, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/0/03/%22Rebound%21%22.png"),
                new Boon("Rock Barrier",34633, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),//750?
                new Boon("Magnetic Wave",15794, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Obsidian Flesh",5667, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                //traits
                new Boon("Harmonious Conduit",31353, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Fresh Air",31353, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Soothing Mist", 5587, BoonSource.Elementalist, "duration", 1, BoonEnum.DefensiveBuff, "https://wiki.guildwars2.com/images/f/f7/Soothing_Mist.png"),
                new Boon("Lesser Arcane Shield",25579, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Weaver's Prowess",42061, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                new Boon("Elements of Rage",42416, BoonSource.Elementalist, "duration", 1, BoonEnum.NonShareableBuff),
                /// FOODS
                new Boon("Plate of Truffle Steak",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/4/4c/Plate_of_Truffle_Steak.png"),
                new Boon("Bowl of Sweet and Spicy Butternut Squash Soup",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/d/df/Bowl_of_Sweet_and_Spicy_Butternut_Squash_Soup.png"),
                new Boon("Red-Lentil Saobosa",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/a/a8/Red-Lentil_Saobosa.png"),
                new Boon("Rare Veggie Pizza",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/a/a0/Rare_Veggie_Pizza.png"),
                new Boon("Bowl of Garlic Kale Sautee",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/0/04/Bowl_of_Garlic_Kale_Sautee.png"),
                new Boon("Koi Cake",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/1/14/Koi_Cake.png"),
                new Boon("Prickly Pear Pie",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/0/0a/Prickly_Pear_Pie.png"),
                new Boon("Bowl of Nopalitos Sauté",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/f/f1/Bowl_of_Nopalitos_Saut%C3%A9.png"),
                new Boon("Delicious Rice Ball",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/5/5d/Delicious_Rice_Ball.png"),
                new Boon("Slice of Allspice Cake",-1, BoonSource.Item, "duration", 1, BoonEnum.Food, "https://wiki.guildwars2.com/images/1/13/Slice_of_Allspice_Cake.png"),
                /// UTILITIES
                new Boon("Superior Sharpening Stone",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/7/78/Superior_Sharpening_Stone.png"),
                new Boon("Master Maintenance Oil",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/5/5b/Master_Maintenance_Oil.png"),
                new Boon("Master Tuning Crystal",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/5/58/Master_Tuning_Crystal.png"),
                new Boon("Toxic Sharpening Stone",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/d/db/Toxic_Sharpening_Stone.png"),
                new Boon("Potent Superior Sharpening Stone",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/7/78/Superior_Sharpening_Stone.png"),
                new Boon("Toxic Maintenance Oil",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/a/a6/Toxic_Maintenance_Oil.png"),
                new Boon("Magnanimous Maintenance Oil",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/5/53/Magnanimous_Maintenance_Oil.png"),
                new Boon("Potent Master Maintenance Oil",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/5/5b/Master_Maintenance_Oil.png"),
                new Boon("Furious Maintenance Oil",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/5/5b/Master_Maintenance_Oil.png"),
                new Boon("Bountiful Maintenance Oil",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/5/5b/Master_Maintenance_Oil.png"),
                new Boon("Toxic Focusing Crystal",-1, BoonSource.Item, "duration", 1, BoonEnum.Utility, "https://wiki.guildwars2.com/images/d/de/Toxic_Focusing_Crystal.png"),
            };
        // Conditions
        public static List<Boon> getCondiBoonList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.Condition).ToList();
        }
        // Boons
        public static List<Boon> getBoonList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.Boon).ToList();
        }
        // Shareable buffs
        public static List<Boon> getOffensiveList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.OffensiveBuff).ToList();
        }
        public static List<Boon> getOffensiveList(BoonSource source)
        {
            return getOffensiveList().Where(x => x.plotlyGroup == source).ToList();
        }
        public static List<Boon> getDefensiveList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.DefensiveBuff).ToList();
        }
        public static List<Boon> getDefensiveList(BoonSource source)
        {
            return getDefensiveList().Where(x => x.plotlyGroup == source).ToList();
        }
        public static List<Boon> getSharableProfList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.OffensiveBuff || x.priority == BoonEnum.DefensiveBuff).ToList();   
        }
        public static List<Boon> getSharableProfList(BoonSource source)
        {
            return getSharableProfList().Where(x => x.plotlyGroup == source).ToList();
        }
        // Foods
        public static List<Boon> getFoodList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.Food).ToList();
        }
        // Utilities
        public static List<Boon> getUtilityList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.Utility).ToList();
        }
        // All buffs
        public static List<Boon> getAllProfList()
        {
            return allBoons.Where(x => x.priority != BoonEnum.Condition && x.priority != BoonEnum.Food && x.priority != BoonEnum.Utility).ToList();
        }
        // Non shareable buffs
        public static List<Boon> getRemainingBuffsList()
        {
            return allBoons.Where(x => x.priority == BoonEnum.NonShareableBuff).ToList();
        }
        public static List<Boon> getRemainingBuffsList(BoonSource source)
        {
            return getRemainingBuffsList().Where(x => x.plotlyGroup == source).ToList();
        }


        // Getters
        public String getName()
        {
            return this.name;
        }
        public int getID()
        {
            return this.id;
        }
        public BoonSource getPloltyGroup()
        {
            return this.plotlyGroup;
        }

        public String getType()
        {
            return this.type;
        }

        public int getCapacity()
        {
            return this.capacity;
        }

        public string getLink()
        {
            return this.link;
        }
    }
}