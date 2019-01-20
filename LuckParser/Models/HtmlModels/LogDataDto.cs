﻿using System.Collections.Generic;

namespace LuckParser.Models.HtmlModels
{   
    public class LogDataDto
    { 
        public List<TargetDto> targets = new List<TargetDto>();
        public readonly List<PlayerDto> players = new List<PlayerDto>();
        public readonly List<EnemyDto> enemies = new List<EnemyDto>();
        public readonly List<PhaseDto> phases = new List<PhaseDto>();
        public List<long> boons;
        public List<long> offBuffs;
        public List<long> defBuffs;
        public List<long> dmgCommonModifiersBuffs;
        public Dictionary<string, List<long>> persBuffs;
        
        public List<long> conditions;
        public string encounterDuration;
        public bool success;
        public string fightName;
        public string fightIcon;
        public bool lightTheme;
        public bool noMechanics;
        public bool singleGroup;
    }
}
