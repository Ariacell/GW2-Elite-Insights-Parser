﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuckParser.Models.ParseModels
{
    public class Boon
    {
        // Boon
        

        // Fields
        private String name;
        private int id;
        private String plotlyGroup;
        private String type;
        private int capacity;

        // Constructor abrv does not matter unused
        private Boon(String name, String group, String type, int capacity)
        {
            this.name = name;
            this.plotlyGroup = group;
            this.type = type;
            this.capacity = capacity;
        }
        private Boon(String name, int id, String group, String type, int capacity)
        {
            this.name = name;
            this.id = id;
            this.plotlyGroup = group;
            this.type = type;
            this.capacity = capacity;
        }
        // Public Methods
        public void setID(int id)
        {
            this.id = id;
        }
        public static Boon getEnum(String name)
        {
            foreach (Boon b in getList())
            {
                if (b.getName() == name)
                {
                    return b;
                }
            }
            foreach (Boon b in getAllProfList())
            {
                if (b.getName() == name)
                {
                    return b;
                }
            }
            return null;
        }

        public static List<int> getCondiList() {
            List<int> condiList = new List<int>();
            condiList.Add(736);//Bleeding
            condiList.Add(737);//Burning
            condiList.Add(861);//Confusion
            condiList.Add(723);//Poisin
            condiList.Add(19426);//Torment
            condiList.Add(720);//Blind
            condiList.Add(722);//Chilled
            condiList.Add(721);//Cripplied
            condiList.Add(791);//Fear
            condiList.Add(727);//Immob
            condiList.Add(26766);//Slow
            condiList.Add(27705);//Taunt
            condiList.Add(742);//Weakness
            condiList.Add(738);//Vuln
            return condiList;
        }
        public static string getCondiName(int id) {
            switch (id) {
                case 736:
                    return "Bleeding";
                case 737:
                    return "Burning";
                case 861:
                    return "Confusion";
                case 723:
                    return "Poison";
                case 19426:
                    return "Torment";
                case 720:
                    return "Blinded";
                case 722:
                    return "Chilled";
                case 721:
                    return "Crippled";
                case 791:
                    return "Fear";
                case 727:
                    return "Immobile";
                case 26766:
                    return "Slow";
                case 27705:
                    return "Taunt";
                case 742:
                    return "Weakness";
                case 738:
                    return "Vulnerability";
                case 873:
                    return "Retaliation";
                default:
                    return "UNKNOWN";
            }
        }
        public static List<Boon> getCondiBoonList() {
            List<Boon> boonList = new List<Boon>();
            boonList.Add(new Boon("Bleeding", 736, "main", "intensity", 1500));
            boonList.Add(new Boon("Burning", 737, "main", "intensity", 1500));
            boonList.Add(new Boon("Confusion", 861, "main", "intensity", 1500));
            boonList.Add(new Boon("Poison", 723, "main", "intensity", 1500));
            boonList.Add(new Boon("Torment", 19426, "main", "intensity", 1500));
            boonList.Add(new Boon("Blinded", 737, "main", "duration", 9));
            boonList.Add(new Boon("Chilled", 722, "main", "duration", 5));
            boonList.Add(new Boon("Crippled", 721, "main", "duration", 9));
            boonList.Add(new Boon("Fear", 791, "main", "duration", 9));
            boonList.Add(new Boon("Immobile", 727, "main", "duration", 3));
            boonList.Add(new Boon("Slow", 26766, "main", "duration", 9));
            boonList.Add(new Boon("Weakness", 737, "main", "duration", 5));
            boonList.Add(new Boon("Vulnerability", 738, "main", "intensity", 25));
            return boonList;
        }
        public static List<Boon> getList(){
            List<Boon> boonList = new List<Boon>();
            boonList.Add(new Boon("Might", 740, "main", "intensity", 25));
            boonList.Add(new Boon("Fury", 725, "main", "duration", 9));//or 3m and 30s
            boonList.Add(new Boon("Quickness", 1187, "main", "duration", 5));
            boonList.Add(new Boon("Alacrity", 30328 , "main", "duration", 9));

            boonList.Add(new Boon("Protection", 717, "main", "duration", 5));
            boonList.Add(new Boon("Regeneration", 718, "main", "duration", 5));
            boonList.Add(new Boon("Vigor", 726, "main", "duration", 5));
            boonList.Add(new Boon("Aegis", 743, "main", "duration", 5));
            boonList.Add(new Boon("Stability", 1122, "main", "intensity", 25));
            boonList.Add(new Boon("Swiftness", 719, "main", "duration", 9));
            boonList.Add(new Boon("Retaliation", 873, "main", "duration", 5));
            boonList.Add(new Boon("Resistance", 26980 , "main", "duration", 5));

            boonList.Add(new Boon("Spotter", 14055 , "ranger", "duration", 1));
            boonList.Add(new Boon("Spirit of Frost", 12544 , "ranger", "duration", 1));
            boonList.Add(new Boon("Sun Spirit", 12540, "ranger", "duration", 1));
            boonList.Add(new Boon("Stone Spirit", 12547, "ranger", "duration", 1));
            boonList.Add(new Boon("Storm Spirit", "ranger", "duration", 1));
            boonList.Add(new Boon("Glyph of Empowerment", "ranger", "duration", 1));

            boonList.Add(new Boon("Empower Allies", 14222 , "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Strength", 14417 , "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Discipline", 14449 , "warrior", "duration", 1));

            boonList.Add(new Boon("Soothing Mist", "ele", "duration", 1));

            boonList.Add(new Boon("Pinpoint Distribution", "engie", "duration", 1));

            boonList.Add(new Boon("Vampiric Aura", "necro", "duration", 1));

            boonList.Add(new Boon("Assassin's Presence", "rev", "duration", 1));

            boonList.Add(new Boon("Battle Presence", 17046, "warrior", "duration", 1));//Virtue of Resolve (ID:17046)
            return boonList;
        }

        public static List<Boon> getMainList()
        {
            List<Boon> boonList = new List<Boon>();
            boonList.Add(new Boon("Might",740, "main", "intensity", 25));
            boonList.Add(new Boon("Fury",725, "main", "duration", 9));//or 3m and 30s
            boonList.Add(new Boon("Quickness",1187, "main", "duration", 5));
            boonList.Add(new Boon("Alacrity", 30328, "main", "duration", 9));

            boonList.Add(new Boon("Protection",717, "main", "duration", 5));
            boonList.Add(new Boon("Regeneration",718, "main", "duration", 5));
            boonList.Add(new Boon("Vigor",726, "main", "duration", 5));
            boonList.Add(new Boon("Aegis",743, "main", "duration", 9));
            boonList.Add(new Boon("Stability",1122, "main", "duration", 9));
            boonList.Add(new Boon("Swiftness",719, "main", "duration", 9));
            boonList.Add(new Boon("Retaliation",873, "main", "duration", 9));
            boonList.Add(new Boon("Resistance", 26980, "main", "duration", 5));

            return boonList;
        }
        public static List<Boon> getSharableProfList()
        {
            List<Boon> boonList = new List<Boon>();
            boonList.Add(new Boon("Spotter", 14055, "ranger", "duration",1));
            boonList.Add(new Boon("Spirit of Frost", 12544, "ranger", "duration", 1));
            boonList.Add(new Boon("Sun Spirit", 12540, "ranger", "duration", 1));
            boonList.Add(new Boon("Stone Spirit", 12547, "ranger", "duration", 1));
            boonList.Add(new Boon("Storm Spirit", "ranger", "duration", 1));
            boonList.Add(new Boon("Glyph of Empowerment", "ranger", "duration", 1));
            boonList.Add(new Boon("Empower Allies", 14222, "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Strength", 14417, "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Discipline", 14449, "warrior", "duration", 1));
            boonList.Add(new Boon("Battle Presence", 17046, "warrior", "duration", 1));//Virtue of Resolve (ID:17046)
            boonList.Add(new Boon("Assassin's Presence", "rev", "duration", 1));
            boonList.Add(new Boon("Naturalistic Resonance", "rev", "duration", 1));
            boonList.Add(new Boon("Pinpoint Distribution", "engie", "duration", 1));
            boonList.Add(new Boon("Increased Condition Damage", "engie", "duration", 1));
            boonList.Add(new Boon("Soothing Mist", "ele", "duration", 1));
            boonList.Add(new Boon("Vampiric Aura", "necro", "duration", 1));
            return boonList;
        }
        public static List<Boon> getAllProfList()
        {
            List<Boon> boonList = new List<Boon>();
            boonList.Add(new Boon("Stealth", 13017, "regular", "duration", 1));//13017
            boonList.Add(new Boon("Revealed", 13017, "regular", "duration", 1));//13017
            boonList.Add(new Boon("Superspeed", 5974, "regular", "duration", 1));//5974
            boonList.Add(new Boon("Invulnerability", 801 , "regular", "duration", 1));
            boonList.Add(new Boon("Unblockable", "regular", "duration", 1));
            //Auras
            boonList.Add(new Boon("Chaos Armor", 10332 , "aura", "duration", 1));
            boonList.Add(new Boon("Fire Shield", 5677, "aura", "duration", 1));//5677
            boonList.Add(new Boon("Frost Aura", 5579, "aura", "duration", 1));//5579
            boonList.Add(new Boon("Light Aura", 25518 , "aura", "duration", 1));
            boonList.Add(new Boon("Magnetic Aura", 5684, "aura", "duration", 1));//5684
            boonList.Add(new Boon("Shocking Aura", 5577,"aura", "duration", 1));//5577

            //Race sepecfic
            boonList.Add(new Boon("Take Root",12459, "race", "duration", 1));
            boonList.Add(new Boon("Become the Bear", "race", "duration", 1));
            boonList.Add(new Boon("Become the Raven", "race", "duration", 1));
            boonList.Add(new Boon("Become the Snow Leopard", "race", "duration", 1));
            boonList.Add(new Boon("Become the Wolf", "race", "duration", 1));
            boonList.Add(new Boon("Avatar of Melandru", 12368, "race", "duration", 1));//12368
            boonList.Add(new Boon("Power Suit", "race", "duration", 1));
            boonList.Add(new Boon("Reaper of Grenth", 12366, "race", "duration", 1));//12366
            boonList.Add(new Boon("Charrooka", "race", "duration", 1));

            //Profession specefic effects
            //revenant
            //skills
            boonList.Add(new Boon("Crystal Hibernation",28262, "rev", "duration", 1));
            boonList.Add(new Boon("Vengeful Hammers",27273, "rev", "duration", 1));
            boonList.Add(new Boon("Rite of the Great Dwarf",26596, "rev", "duration", 1));
            boonList.Add(new Boon("Embrace the Darkness",28001, "rev", "duration", 1));
            boonList.Add(new Boon("Enchanted Daggers", 28557,"rev", "intensity", 6));
            boonList.Add(new Boon("Impossible Odds",27581, "rev", "duration", 1));
            //signets
            boonList.Add(new Boon("Facet of Light", "rev", "duration", 1));
            boonList.Add(new Boon("Infuse Light", "rev", "duration", 1));
            boonList.Add(new Boon("Facet of Darkness", "rev", "duration", 1));
            boonList.Add(new Boon("Facet of Elements", "rev", "duration", 1));
            boonList.Add(new Boon("Facet of Strength", "rev", "duration", 1));
            boonList.Add(new Boon("Facet of Chaos", "rev", "duration", 1));
            boonList.Add(new Boon("Facet of Nature", "rev", "duration", 1));
            boonList.Add(new Boon("Naturalistic Resonance", "rev", "duration", 1));
            //attunments
            boonList.Add(new Boon("Legendary Centaur Stance", "rev", "duration", 1));
            boonList.Add(new Boon("Legendary Dragon Stance", "rev", "duration", 1));
            boonList.Add(new Boon("Legendary Dwarf Stance", "rev", "duration", 1));
            boonList.Add(new Boon("Legendary Demon Stance", "rev", "duration", 1));
            boonList.Add(new Boon("Legendary Assassin Stance", "rev", "duration", 1));
            boonList.Add(new Boon("Legendary Renegade Stance", "rev", "duration", 1));
            //summons
            boonList.Add(new Boon("Breakrazor's Bastion", "rev", "duration", 1));
            boonList.Add(new Boon("Razorclaw's Rage", "rev", "duration", 1));
            boonList.Add(new Boon("Soulcleave's Summit", "rev", "duration", 1));
            //traits
            boonList.Add(new Boon("Vicious Lacerations", "rev", "intensity", 5));
            boonList.Add(new Boon("Assassin's Presence", "rev", "duration", 1));
            boonList.Add(new Boon("Expose Defenses",48894, "rev", "duration", 1));
            boonList.Add(new Boon("Invoking Harmony", "rev", "duration", 1));
            boonList.Add(new Boon("Selfless Amplification", "rev", "duration", 1));
            boonList.Add(new Boon("Hardening Persistence", "rev", "intensity", 8));
            boonList.Add(new Boon("Soothing Bastion", "rev", "duration", 1));
            boonList.Add(new Boon("Kalla's Fervor", "rev", "duration", 5));

            //warrior
            //skills
            boonList.Add(new Boon("Riposte", "warrior", "duration", 1));
            boonList.Add(new Boon("Counterattack", "warrior", "duration", 1));
            boonList.Add(new Boon("Flames of War", "warrior", "duration", 1));
            boonList.Add(new Boon("Blood Reckoning", "warrior", "duration", 1));
            boonList.Add(new Boon("Rock Guard", "warrior", "duration", 1));
            boonList.Add(new Boon("Sight beyond Sight", "warrior", "duration", 1));

            //signets
            boonList.Add(new Boon("Healing Signet", "warrior", "duration", 1));
            boonList.Add(new Boon("Dolyak Signet", "warrior", "duration", 1));
            boonList.Add(new Boon("Signet of Fury", "warrior", "duration", 1));
            boonList.Add(new Boon("Signet of Might", "warrior", "duration", 1));
            boonList.Add(new Boon("Signet of Stamina", "warrior", "duration", 1));
            boonList.Add(new Boon("Signet of Rage", "warrior", "duration", 1));
            boonList.Add(new Boon("Signet of Ferocity", "warrior", "duration", 1));
            //summons
            boonList.Add(new Boon("Banner of Strength", "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Discipline", "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Tactics", "warrior", "duration", 1));
            boonList.Add(new Boon("Banner of Defense", "warrior", "duration", 1));
            //stancces
            boonList.Add(new Boon("Shield Stance", "warrior", "duration", 1));
            boonList.Add(new Boon("Berserker's Stance", "warrior", "duration", 1));
            boonList.Add(new Boon("Enduring Pain", "warrior", "duration", 1));
            boonList.Add(new Boon("Balanced Stance", "warrior", "duration", 1));
            boonList.Add(new Boon("Defiant Stance", "warrior", "duration", 1));
            //traits
            boonList.Add(new Boon("Empower Allies", "warrior", "duration", 1));
            boonList.Add(new Boon("Peak Performance", "warrior", "duration", 1));
            boonList.Add(new Boon("Furious Surge", "warrior", "intensity", 25));
            boonList.Add(new Boon("Health Gain per Adrenaline bar Spent", "warrior", "intensity", 3));
            boonList.Add(new Boon("Rousing Resilience", "warrior", "duration", 1));
            boonList.Add(new Boon("Always Angry", "warrior", "duration", 1));
            boonList.Add(new Boon("Full Counter", "warrior", "duration", 1));
            boonList.Add(new Boon("Attacker's Insight", "warrior", "intensity", 5));
            boonList.Add(new Boon("Virtue of Resolve","warrior","duration",1));//Virtue of Resolve (ID:17046)

            //guardian
            //skills
            boonList.Add(new Boon("Zealot's Flame", "guard", "duration", 1));
            boonList.Add(new Boon("Purging Flames", "guard", "duration", 1));
            boonList.Add(new Boon("Litany of Wrath", "guard", "duration", 1));
            boonList.Add(new Boon("Renewed Focus", "guard", "duration", 1));
            //signets
            boonList.Add(new Boon("Signet of Resolve", "guard", "duration", 1));
            boonList.Add(new Boon("Bane Signet", "guard", "duration", 1));
            boonList.Add(new Boon("Signet of Judgment", "guard", "duration", 1));
            boonList.Add(new Boon("Signet of Mercy", "guard", "duration", 1));
            boonList.Add(new Boon("Signet of Wrath", "guard", "duration", 1));
            boonList.Add(new Boon("Signet of Courage", "guard", "duration", 1));
            //traits
            boonList.Add(new Boon("Virute of Justice", "guard", "duration", 1));
            boonList.Add(new Boon("Justice", "guard", "duration", 1));
            boonList.Add(new Boon("Virute of Courage", "guard", "duration", 1));
            boonList.Add(new Boon("Shield of Courage", "guard", "duration", 1));
            boonList.Add(new Boon("Virute of Resolve", "guard", "duration", 1));
            boonList.Add(new Boon("Strength in Numbers", "guard", "duration", 1));
            boonList.Add(new Boon("Invigorated Bulwark", "guard", "intensity", 5));
            boonList.Add(new Boon("Force of Will", "guard", "duration", 1));//not sure if intensity
            boonList.Add(new Boon("Tome of Justice", "guard", "duration", 1));
            boonList.Add(new Boon("Tome of Courage", "guard", "duration", 1));
            boonList.Add(new Boon("Tome of Resolve", "guard", "duration", 1));
            boonList.Add(new Boon("Ashes of the Just", "guard", "intensity", 25));
            boonList.Add(new Boon("Eternal Oasis", "guard", "duration", 1));
            boonList.Add(new Boon("Unbroken Lines", "guard", "duration", 1));
            boonList.Add(new Boon("Quickfire", "guard", "duration", 1));

            //Engie
            //skills
            boonList.Add(new Boon("Static Shield", "engie", "duration", 1));
            boonList.Add(new Boon("Absorb", "engie", "duration", 1));
            boonList.Add(new Boon("A.E.D.", "engie", "duration", 1));
            boonList.Add(new Boon("Elixir S", "engie", "duration", 1));
            boonList.Add(new Boon("Elixir X", "engie", "duration", 1));
            boonList.Add(new Boon("Utility Goggles", "engie", "duration", 1));
            boonList.Add(new Boon("Slick Shoes", "engie", "duration", 1));
            boonList.Add(new Boon("Watchful Eye", "engie", "duration", 1));
            boonList.Add(new Boon("Cooling Vapor", "engie", "duration", 1));
            boonList.Add(new Boon("Photon Wall Deployed", "engie", "duration", 1));
            boonList.Add(new Boon("Spectrum Shield", "engie", "duration", 1));
            boonList.Add(new Boon("Gear Shield", "engie", "duration", 1));
            //Transforms
            boonList.Add(new Boon("Rampage", "engie", "duration", 1));
            boonList.Add(new Boon("Photon Forge", "engie", "duration", 1));
            //Traits
            boonList.Add(new Boon("Laser's Edge", "engie", "duration", 1));
            boonList.Add(new Boon("Afterburner", "engie", "intensity", 5));
            boonList.Add(new Boon("Iron Blooded", "engie", "intensity", 25));
            boonList.Add(new Boon("Streamlined Kits", "engie", "duration", 1));
            boonList.Add(new Boon("Kinetic Charge", "engie", "intensity", 5));
            boonList.Add(new Boon("Pinpoint Distribution", "engie", "duration", 1));
            boonList.Add(new Boon("Increased Condition Damage", "engie", "duration", 1));

            //Ranger
            boonList.Add(new Boon("Celestial Avatar", "ranger", "duration", 1));
            //signets
            boonList.Add(new Boon("Signet of Renewal", "ranger", "duration", 1));
            boonList.Add(new Boon("Signet of Stone", "ranger", "duration", 1));
            boonList.Add(new Boon("Signet of the Hunt", "ranger", "duration", 1));
            boonList.Add(new Boon("Signet of the Wild", "ranger", "duration", 1));
            //Summons
            //Need reloook
            boonList.Add(new Boon("Spirit of Frost", "ranger", "duration", 1));
            boonList.Add(new Boon("Sun Spirit", "ranger", "duration", 1));
            boonList.Add(new Boon("Stone Spirit", "ranger", "duration", 1));
            boonList.Add(new Boon("Storm Spirit", "ranger", "duration", 1));
            //skills
            boonList.Add(new Boon("Attack of Opportunity", "ranger", "duration", 1));
            boonList.Add(new Boon("Call of the Wild", "ranger", "duration", 1));
            boonList.Add(new Boon("Strength of the pack!", "ranger", "duration", 1));
            boonList.Add(new Boon("Sick 'Em!", "ranger", "duration", 1));
            boonList.Add(new Boon("Sharpening Stones", "ranger", "intenstiy", 10));
            boonList.Add(new Boon("Ancestral Grace", "ranger", "duration", 1));
            boonList.Add(new Boon("Glyph of Empowerment", "ranger", "duration", 1));
            boonList.Add(new Boon("Dolyak Stance", "ranger", "duration", 1));
            boonList.Add(new Boon("Griffon Stance", "ranger", "duration", 1));
            boonList.Add(new Boon("Moa Stance", "ranger", "duration", 1));
            boonList.Add(new Boon("Vulture Stance", "ranger", "duration", 1));
            boonList.Add(new Boon("Bear Stance", "ranger", "duration", 1));
            boonList.Add(new Boon("One Wolf Pack", "ranger", "duration", 1));
            boonList.Add(new Boon("Sharpen Spines", "ranger", "intensity", 5));
            //traits
            boonList.Add(new Boon("Spotter", "ranger", "duration", 1));
            boonList.Add(new Boon("Opening Strike", "ranger", "duration", 1));
            boonList.Add(new Boon("Quick Draw", "ranger", "duration", 1));
            boonList.Add(new Boon("On Dodge", "ranger", "duration", 1));
            boonList.Add(new Boon("Natural Mender", "ranger", "duration", 1));
            boonList.Add(new Boon("Lingering Light", "ranger", "duration", 1));
            boonList.Add(new Boon("Deadly", "ranger", "duration", 1));
            boonList.Add(new Boon("Ferocious", "ranger", "duration", 1));
            boonList.Add(new Boon("Supportive", "ranger", "duration", 1));
            boonList.Add(new Boon("Versatile", "ranger", "duration", 1));
            boonList.Add(new Boon("Stout", "ranger", "duration", 1));
            boonList.Add(new Boon("Unstoppable Union", "ranger", "duration", 1));
            boonList.Add(new Boon("Twice as Vicious", "ranger", "duration", 1));

            //thief
            boonList.Add(new Boon("Deadeyes's Gaze", "thief", "duration", 1));
            //signets
            boonList.Add(new Boon("Signet of Malice", "thief", "duration", 1));
            boonList.Add(new Boon("Assassin's Signet", "thief", "duration", 1));//13047 44597
            boonList.Add(new Boon("Infiltrator's Signet", "thief", "duration", 1));//13063
            boonList.Add(new Boon("Signet of Agility", "thief", "duration", 1));//13061
            boonList.Add(new Boon("Signet of Shadows", "thief", "duration", 1));//13059
            //poisins
            boonList.Add(new Boon("Ice Drake Venom", "thief", "intensity", 4));//13095
            boonList.Add(new Boon("Devourer Venom", "thief", "intensity", 2));//
            boonList.Add(new Boon("Skale Venom", "thief", "intensity", 4));//13054
            boonList.Add(new Boon("Spider Venom", "thief", "intensity", 6));//
            boonList.Add(new Boon("Basilisk Venom", "thief", "intensity", 6));//
                                                                             //Physical
            boonList.Add(new Boon("Palm Strike", "thief", "duration", 1));//
            boonList.Add(new Boon("Pulmonary Impact", "thief", "intensity", 2));//

            boonList.Add(new Boon("Infiltration", "thief", "duration", 1));//
            //Transforms
            boonList.Add(new Boon("Dagger Storm", "thief", "duration", 1));//
            boonList.Add(new Boon("Kneeling", "thief", "duration", 1));//
            //traits
            boonList.Add(new Boon("Maleficent Seven", "thief", "duration", 1));
            boonList.Add(new Boon("Hidden Killer", "thief", "duration", 1));//
            boonList.Add(new Boon("Lead Attacks", "thief", "intensity", 15));//
            boonList.Add(new Boon("Instant Reflexes", "thief", "duration", 1));//
            boonList.Add(new Boon("Lotus Training", "thief", "duration", 1));//
            boonList.Add(new Boon("Unhindered Combatant", "thief", "duration", 1));//
            boonList.Add(new Boon("Bounding Dodger", "thief", "duration", 1));//

            //mesmer 
            boonList.Add(new Boon("Distortion", "mes", "duration", 1));
            //signets
            boonList.Add(new Boon("Signet of the Ether", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Domination", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Illusions", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Inspiration", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Midnight", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Humility", "mes", "duration", 1));
            //skills
            boonList.Add(new Boon("Blur", "mes", "duration", 1));
            boonList.Add(new Boon("Mirror", "mes", "duration", 1));
            boonList.Add(new Boon("Echo", "mes", "duration", 1));
            boonList.Add(new Boon("Illusion of Life", "mes", "duration", 1));
            boonList.Add(new Boon("Time Echo", "mes", "duration", 1));
            //traits
            boonList.Add(new Boon("Fencer's Finesse", "mes", "intensity", 10));
            boonList.Add(new Boon("Illusionary Defense", "mes", "intensity", 5));
            boonList.Add(new Boon("Compunding Power", "mes", "intensity", 5));
            boonList.Add(new Boon("Phantasmal Force", "mes", "intensity", 25));
            boonList.Add(new Boon("Mirage Cloak", "mes", "duration", 1));

            //Necro
            //forms
            boonList.Add(new Boon("Lich Form", "mes", "duration", 1));
            boonList.Add(new Boon("Death Shroud", "mes", "duration", 1));
            boonList.Add(new Boon("Reaper's Shroud", "mes", "duration", 1));
            //Signets
            boonList.Add(new Boon("Signet of Vampirism", "mes", "duration", 1));
            boonList.Add(new Boon("Plague Signet", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Spite", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of the Locust", "mes", "duration", 1));
            boonList.Add(new Boon("Signet of Undeath", "mes", "duration", 1));
            //skills
            boonList.Add(new Boon("Spectral Walk", "mes", "duration", 1));
            //traits
            boonList.Add(new Boon("Corrupter's Defense", "mes", "intenstiy", 10));
            boonList.Add(new Boon("Vampiric Aura", "mes", "duration", 1));
            boonList.Add(new Boon("Last Rites", "mes", "duration", 1));
            boonList.Add(new Boon("Sadistic Searing", "mes", "duration", 1));

            
            //ele
            //signets
            boonList.Add(new Boon("Signet of Restoration", "ele", "duration", 1));//739
            boonList.Add(new Boon("Signet of Air", "ele", "duration", 1));//5590
            boonList.Add(new Boon("Signet of Earth", "ele", "duration", 1));//5592
            boonList.Add(new Boon("Signet of Fire", "ele", "duration", 1));//5544
            boonList.Add(new Boon("Signet of Water", "ele", "duration", 1));//5591
            //attunments
            boonList.Add(new Boon("Fire Attunement", "ele", "duration", 1));//5585
            boonList.Add(new Boon("Water Attunement", "ele", "duration", 1));
            boonList.Add(new Boon("Air Attunement", "ele", "duration", 1));//5575
            boonList.Add(new Boon("Earth Attunement", "ele", "duration", 1));//5580
            //forms
            boonList.Add(new Boon("Mist Form", "ele", "duration", 1));//5543
            boonList.Add(new Boon("Ride the Lightning", "ele", "duration", 1));//5588
            boonList.Add(new Boon("Vapor Form", "ele", "duration", 1));
            boonList.Add(new Boon("Tornado", "ele", "duration", 1));//5534
            boonList.Add(new Boon("Whirlpool", "ele", "duration", 1));
            //conjures
            boonList.Add(new Boon("Conjure Earth Attributes", "ele", "duration", 1));//15788
            boonList.Add(new Boon("Conjure Flame Attributes", "ele", "duration", 1));//15789
            boonList.Add(new Boon("Conjure Frost Attributes", "ele", "duration", 1));//15790
            boonList.Add(new Boon("Conjure Lightning Attributes", "ele", "duration", 1));//15791
            boonList.Add(new Boon("Conjure Fire Attributes", "ele", "duration", 1));//15792
                                                    //Extras
            boonList.Add(new Boon("Arcane Power", "ele", "duration", 1));//5582
            boonList.Add(new Boon("Arcane Shield", "ele", "duration", 1));//5640
            boonList.Add(new Boon("Renewal of Fire", "ele", "duration", 1));//5764
            boonList.Add(new Boon("Glyph of Elemental Power", "ele", "duration", 1));//5739 5741 5740 5742
            boonList.Add(new Boon("Rebound", "ele", "duration", 1));//31337
            boonList.Add(new Boon("Rock Barrier", "ele", "duration", 1));//34633 750
            boonList.Add(new Boon("Magnetic Wave", "ele", "duration", 1));//15794
            boonList.Add(new Boon("Obsidian Flesh", "ele", "duration", 1));//5667
            //Traits
            boonList.Add(new Boon("Harmonious Conduit", "ele", "duration", 1));//31353
            boonList.Add(new Boon("Fresh Air", "ele", "duration", 1));//31353
            boonList.Add(new Boon("Soothing Mist", "ele", "duration", 1));
            boonList.Add(new Boon("Lesser Arcane Shield", "ele", "duration", 1));
            boonList.Add(new Boon("Weaver's Prowess", "ele", "duration", 1));
            boonList.Add(new Boon("Elements of Rage", "ele", "duration", 1));
            boonList.Add(new Boon("bleh", "ele", "duration", 1));
    
            return boonList;
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
        public String getPloltyGroup()
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
    }
}