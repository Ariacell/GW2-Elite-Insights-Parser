﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models
{
    public class JsonMinions
    {
        public class JsonExtraMinions
        {
            public int ID;
        }

        public string Name;
        public List<JsonSkill> Rotation;
        public JsonExtraMinions ExtraData;
    }
}
