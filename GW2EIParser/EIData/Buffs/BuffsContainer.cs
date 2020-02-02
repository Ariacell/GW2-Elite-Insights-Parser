﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIParser.Parser.ParsedData;
using static GW2EIParser.EIData.Buff;

namespace GW2EIParser.EIData
{
    public class BuffsContainer
    {

        public Dictionary<long, Buff> BuffsByIds { get; }
        public Dictionary<BuffNature, List<Buff>> BuffsByNature { get; }
        public Dictionary<BuffSource, List<Buff>> BuffsBySource { get; }
        public Dictionary<BuffType, List<Buff>> BuffsByType { get; }
        private readonly Dictionary<string, Buff> _buffsByName;
        public Dictionary<int, List<Buff>> BuffsByCapacity { get; }

        private readonly BuffSourceFinder _buffSourceFinder;

        public BuffsContainer(ulong build)
        {
            var currentBuffs = new List<Buff>();
            foreach (List<Buff> buffs in AllBuffs)
            {
                currentBuffs.AddRange(buffs.Where(x => x.MaxBuild > build && build >= x.MinBuild));
            }
            BuffsByIds = currentBuffs.GroupBy(x => x.ID).ToDictionary(x => x.Key, x => x.First());
            BuffsByNature = currentBuffs.GroupBy(x => x.Nature).ToDictionary(x => x.Key, x => x.ToList());
            BuffsBySource = currentBuffs.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.ToList());
            BuffsByType = currentBuffs.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());
            _buffsByName = currentBuffs.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.ToList().Count > 1 ? throw new InvalidOperationException("Error Encountered: Same name present multiple times in buffs - " + x.First().Name) : x.First());
            BuffsByCapacity = currentBuffs.GroupBy(x => x.Capacity).ToDictionary(x => x.Key, x => x.ToList());
            _buffSourceFinder = GetBuffSourceFinder(build, new HashSet<long>(BuffsByNature[BuffNature.Boon].Select(x => x.ID)));
        }

        public Buff GetBuffByName(string name)
        {
            if (_buffsByName.TryGetValue(name, out Buff buff))
            {
                return buff;
            }
            throw new InvalidOperationException("Error Encountered: Buff " + name + " does not exist");
        }

        public AgentItem TryFindSrc(AgentItem dst, long time, long extension, ParsedLog log, long buffID)
        {
            return _buffSourceFinder.TryFindSrc(dst, time, extension, log, buffID);
        }

        // Non shareable buffs
        private List<Buff> GetRemainingBuffsList(BuffSource source)
        {
            return BuffsBySource[source].Where(x => x.Nature == BuffNature.GraphOnlyBuff).ToList();
        }
        public List<Buff> GetRemainingBuffsList(string source)
        {
            return GetRemainingBuffsList(ProfToEnum(source));
        }
    }
}
