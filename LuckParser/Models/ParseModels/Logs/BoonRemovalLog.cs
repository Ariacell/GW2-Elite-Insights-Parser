﻿using LuckParser.Models.DataModels;

namespace LuckParser.Models.ParseModels
{
    public class BoonRemovalLog : BoonLog
    {
        private ParseEnum.BuffRemove _removeType;

        public BoonRemovalLog(long time, ushort srcInstid, long value, ParseEnum.BuffRemove removeType, ParseEnum.IFF iff) : base(time, srcInstid, value, iff)
        {
            _removeType = removeType;
        }

        public override ParseEnum.BuffRemove GetRemoveType()
        {
            return _removeType;
        }
    }
}