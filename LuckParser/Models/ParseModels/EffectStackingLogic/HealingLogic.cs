﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuckParser.Models.DataModels;

namespace LuckParser.Models.ParseModels
{
    public class HealingLogic : StackingLogic
    {
        protected struct CompareHealing
        {
            private ParsedLog log;

            public CompareHealing(ParsedLog log)
            {
                this.log = log;
            }

            public int Compare(BoonSimulator.BoonStackItem x, BoonSimulator.BoonStackItem y)
            {
                List<Player> players = log.GetPlayerList();
                Player a = players.Find(p => p.GetInstid() == x.src);
                Player b = players.Find(p => p.GetInstid() == y.src);
                if (a == null || b == null)
                {
                    return 0;
                }
                return a.GetHealing() < b.GetHealing() ? 1 : -1;
            }
        }
        public override void Sort(ParsedLog log, List<BoonSimulator.BoonStackItem> stacks)
        {
            CompareHealing comparator = new CompareHealing(log);
            stacks.Sort(comparator.Compare);        
        }

        public override bool StackEffect(ParsedLog log, BoonSimulator.BoonStackItem toAdd, List<BoonSimulator.BoonStackItem> stacks, List<BoonSimulationItem> simulation)
        {
            
            for (int i = 1; i < stacks.Count; i++)
            {
                if (stacks[i].boon_duration < toAdd.boon_duration)
                {
                    long overstackValue = stacks[i].overstack + stacks[i].boon_duration;
                    ushort srcValue = stacks[i].src;
                    for (int j = simulation.Count - 1; j >= 0; j--)
                    {
                        if (simulation[j].AddOverstack(srcValue, overstackValue))
                        {
                            break;
                        }
                    }
                    stacks[i] = toAdd;
                    Sort(log, stacks);
                    return true;
                }
            }
            return false;
        }
    }
}
