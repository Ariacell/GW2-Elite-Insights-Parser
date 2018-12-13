﻿using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public class BoonDistributionItem
    {
        public long Value { get; set; }
        public long Overstack { get; set; }

        public BoonDistributionItem(long value, long overstack)
        {
            Value = value;
            Overstack = overstack;
        }
    }

    public class BoonDistribution : Dictionary<long, Dictionary<ushort, BoonDistributionItem>>
    {
        public long GetUptime(long boonid)
        {
            if (!ContainsKey(boonid))
            {
                return 0;
            }
            return this[boonid].Sum(x => x.Value.Value);
        }

        public long GetGeneration(long boonid, ushort src)
        {
            if (!ContainsKey(boonid) || src == 0)
            {
                return 0;
            }
            return this[boonid].Where( x => src == x.Key).Sum(x => x.Value.Value);
        }

        public long GetOverstack(long boonid, ushort src)
        {
            if (!ContainsKey(boonid) || src == 0)
            {
                return 0;
            }
            return this[boonid].Where(x => src == x.Key).Sum(x => x.Value.Overstack);
        }
    }
}
