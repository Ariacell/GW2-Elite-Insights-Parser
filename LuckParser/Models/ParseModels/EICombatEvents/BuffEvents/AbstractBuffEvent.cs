﻿using LuckParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.ParseModels
{
    public abstract class AbstractBuffEvent : AbstractCombatEvent
    {
        public AbstractBuffEvent(CombatItem evtcItem, long offset) : base(evtcItem.Time, offset)
        {

        }
    }
}
