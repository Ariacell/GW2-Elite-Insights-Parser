﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIParser.Parser;
using GW2EIParser.Parser.ParsedData;
using GW2EIParser.Parser.ParsedData.CombatEvents;
using static GW2EIParser.EIData.Buff;

namespace GW2EIParser.EIData
{
    public class BuffsContainer
    {
        private List<Buff> _allBuffs;
        public Dictionary<long, Buff> BuffsByIds { get; }
        public Dictionary<BuffNature, List<Buff>> BuffsByNature { get; }
        public Dictionary<GeneralHelper.Source, List<Buff>> BuffsBySource { get; }
        public Dictionary<BuffType, List<Buff>> BuffsByType { get; }
        private readonly Dictionary<string, Buff> _buffsByName;
        public Dictionary<int, List<Buff>> BuffsByCapacity { get; }

        private readonly BuffSourceFinder _buffSourceFinder;

        private readonly ulong _build;

        public BuffsContainer(ulong build, CombatData combatData, OperationController operation)
        {
            _build = build;
            _allBuffs = new List<Buff>();
            foreach (List<Buff> buffs in AllBuffs)
            {
                _allBuffs.AddRange(buffs.Where(x => x.MaxBuild > build && build >= x.MinBuild));
            }
            BuffsByNature = _allBuffs.GroupBy(x => x.Nature).ToDictionary(x => x.Key, x => x.ToList());
            // TODO: add unknown consumables here if any
#if DEBUG
            var seenUnknowns = new HashSet<byte>();
            foreach (Buff buff in _allBuffs)
            {
                BuffDataEvent buffDataEvt = combatData.GetBuffDataEvent(buff.ID);
                if (buffDataEvt != null)
                {
                    foreach (BuffDataEvent.BuffFormula formula in buffDataEvt.FormulaList)
                    {
                        if (formula.Attr1 == ParseEnum.BuffAttribute.Unknown && !seenUnknowns.Contains(formula.DebugAttr1))
                        {
                            seenUnknowns.Add(formula.DebugAttr1);
                            operation.UpdateProgress("Unknown Formula " + formula.DebugAttr1 + " for " + buff.ID + " " + buff.Name);
                        }
                    }
                }
            }
#endif
            _buffsByName = _allBuffs.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.ToList().Count > 1 ? throw new InvalidOperationException("Same name present multiple times in buffs - " + x.First().Name) : x.First());
            _buffSourceFinder = GetBuffSourceFinder(_build, new HashSet<long>(BuffsByNature[BuffNature.Boon].Select(x => x.ID)));
            BuffsByCapacity = _allBuffs.GroupBy(x => x.Capacity).ToDictionary(x => x.Key, x => x.ToList());
            BuffsByType = _allBuffs.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());
            BuffsBySource = _allBuffs.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.ToList());
            BuffsByIds = _allBuffs.GroupBy(x => x.ID).ToDictionary(x => x.Key, x => x.First());
        }

        public Buff GetBuffByName(string name)
        {
            if (_buffsByName.TryGetValue(name, out Buff buff))
            {
                return buff;
            }
            throw new InvalidOperationException("Buff " + name + " does not exist");
        }

        public AgentItem TryFindSrc(AgentItem dst, long time, long extension, ParsedLog log, long buffID)
        {
            return _buffSourceFinder.TryFindSrc(dst, time, extension, log, buffID);
        }

        // Non shareable buffs
        public List<Buff> GetRemainingBuffsList(string source)
        {
            var result = new List<Buff>();
            foreach (GeneralHelper.Source src in GeneralHelper.ProfToEnum(source))
            {
                if (BuffsBySource.TryGetValue(src, out List<Buff> list))
                {
                    result.AddRange(list.Where(x => x.Nature == BuffNature.GraphOnlyBuff));
                }
            }
            return result;
        }
    }
}
