﻿using LuckParser.Models.DataModels;
using System;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public class BoonSimulatorDuration : BoonSimulator
    {
        
        // Constructor
        public BoonSimulatorDuration(int capacity, ParsedLog log, StackingLogic logic) : base(capacity, log, logic)
        {
        }

        // Public Methods
    
        public override void Update(long time_passed)
        {
            if (boon_stack.Count > 0)
            {
                var toAdd = new BoonSimulationItemDuration(boon_stack[0]);
                if (simulation.Count > 0)
                {
                    var last = simulation.Last();
                    if (last.GetEnd() > toAdd.GetStart())
                    {
                        last.SetEnd(toAdd.GetStart());
                    }
                }
                simulation.Add(toAdd);
                boon_stack[0] = new BoonStackItem(boon_stack[0], time_passed, time_passed);
                long diff = time_passed - Math.Abs(Math.Min(boon_stack[0].boon_duration, 0));
                for (int i = 1; i < boon_stack.Count; i++)
                {
                    boon_stack[i] = new BoonStackItem(boon_stack[i], diff, 0);
                }
                if (boon_stack[0].boon_duration <= 0)
                {
                    // Spend leftover time
                    long leftover = Math.Abs(boon_stack[0].boon_duration);
                    boon_stack.RemoveAt(0);
                    Update(leftover);
                }
            }
        }      
    }
}
