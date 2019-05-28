﻿using LuckParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.ParseModels
{
    public abstract class CastStartEvent : AbstractCastEvent
    {
        public CastStartEvent(CombatItem evtcItem) : base(evtcItem)
        {

        }
    }
}
