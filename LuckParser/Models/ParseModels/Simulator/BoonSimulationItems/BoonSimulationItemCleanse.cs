﻿using LuckParser.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.ParseModels
{
    public class BoonSimulationItemCleanse
    {
        private readonly ushort _provokedBy;
        private readonly long _duration;
        private readonly long _time;

        public BoonSimulationItemCleanse(ushort provokedBy, long duration, long time)
        {
            _provokedBy = provokedBy;
            _duration = duration;
            _time = time;
        }

        public void SetCleanseItem(Dictionary<AbstractActor, Dictionary<long, List<long>>> cleanses, long start, long end, long boonid, ParsedLog log)
        {
            long cleanse = GetCleanseDuration(start, end);
            AbstractActor actor = GeneralHelper.GetActor(_provokedBy, _time, log);
            if (cleanse > 0)
            {
                if (!cleanses.TryGetValue(actor, out var dict))
                {
                    dict = new Dictionary<long, List<long>>();
                    cleanses.Add(actor, dict);
                }
                if (!dict.TryGetValue(boonid, out var list))
                {
                    list = new List<long>();
                    dict.Add(boonid, list);
                }
                list.Add(cleanse);
            }
        }

        public long GetCleanseDuration(long start, long end)
        {
            return (start <= _time && _time <= end) ? _duration : 0;
        }
    }
}
