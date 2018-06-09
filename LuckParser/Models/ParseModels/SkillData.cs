﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuckParser.Models.ParseModels
{
    public class SkillData
    {
        // Fields
        private List<SkillItem> skill_list;
        private Dictionary<int, string> apiMissingID = new Dictionary<int, string>()
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
            // Matthias
            { 34468, "Shield (Human)"},
            { 34427, "Abomination Transformation"},
            { 34510, "Shiled (Abomination)"}
        };

        // Constructors
        public SkillData()
        {
            this.skill_list = new List<SkillItem>();
        }

        // Public Methods
        public void addItem(SkillItem item)
        {
            skill_list.Add(item);
        }

        public String getName(int ID)
        {

            // Custom
            if (apiMissingID.ContainsKey(ID))
            {
                return apiMissingID[ID];
            }

            // Normal
            foreach (SkillItem s in skill_list)
            {
                if (s.getID() == ID)
                {
                    return s.getName();
                }
            }

            // Unknown
            return "uid: " + ID.ToString();
        }

        // Getters
        public List<SkillItem> getSkillList()
        {
            return skill_list;
        }
    }
}