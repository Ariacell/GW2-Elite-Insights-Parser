﻿using LuckParser.Models.DataModels;

namespace LuckParser.Models.ParseModels
{
    public class BoonApplicationLog : BoonLog
    {

        public BoonApplicationLog(long time, ushort srcInstid, long value, ParseEnum.IFF iff) : base(time,srcInstid,value, iff)
        {
        }

        public override ParseEnum.BuffRemove GetRemoveType()
        {
            return ParseEnum.BuffRemove.None;
        }
    }
}