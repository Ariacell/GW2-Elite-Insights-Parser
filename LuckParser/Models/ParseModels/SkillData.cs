﻿using System;
using System.Collections.Generic;

namespace LuckParser.Models.ParseModels
{
    public class SkillData : List<SkillItem>
    {
        // Fields
        private Dictionary<long, string> apiMissingID = new Dictionary<long, string>()
        {
            {1066, "Resurrect"},
            {1175, "Bandage" },
            {65001, "Dodge" },
            // Gorseval
            {31834,"Ghastly Rampage" },
            {31759,"Protective Shadow" },
            {31466,"Ghastly Rampage (Begin)" },
            // Sabetha
            {31372, "Shadow Step" },
            // Slothasor
            {34547, "Tantrum Start" },
            {34515, "Sleeping" },
            {34340, "Fear Me!" },
            // Matthias
            { 34468, "Shield (Human)"},
            { 34427, "Abomination Transformation"},
            { 34510, "Shield (Abomination)"},
            // Generic
            {-5, "Phase out" },
            // Deimos
            {-6, "Roleplay" },
            // Dhuum
            {47396, "Major Soul Split" },
            // Keep Construct
            {35048, "Magic Blast Charge" }
        };

        // Constructors
        public SkillData()
        {
        }

        // Public Methods

        public String GetName(long ID)
        {

            // Custom
            if (apiMissingID.ContainsKey(ID))
            {
                return apiMissingID[ID];
            }

            // Normal
            foreach (SkillItem s in this)
            {
                if (s.GetID() == ID)
                {
                    return s.GetName();
                }
            }

            // Unknown
            return "uid: " + ID.ToString();
        }    
    }
}