﻿namespace LuckParser.Models.ParseModels
{
    public class MechanicLog
    {
        // Fields
        private long _time;
        private Mechanic _mechanic;
        private AbstractMasterPlayer _player;

        public MechanicLog(long time, Mechanic mechanic,
        AbstractMasterPlayer player)
        {
            _time = time;
            _mechanic = mechanic;
            _player = player;
        }
        //getters
        public long GetTime()
        {
            return _time;
        }
        public long GetSkill()
        {
            return _mechanic.GetSkill();
        }
        public AbstractMasterPlayer GetPlayer()
        {
            return _player;
        }
        public string GetName()
        {
            return _mechanic.GetName();
        }
        public string GetPlotly()
        {
            return _mechanic.GetPlotly();
        }
    }
}
