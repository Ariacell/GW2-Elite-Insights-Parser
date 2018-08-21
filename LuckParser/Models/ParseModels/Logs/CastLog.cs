﻿using LuckParser.Models.DataModels;

namespace LuckParser.Models.ParseModels
{
    public  class CastLog
    {
        // Fields
        private long time;
        private long skill_id;
        private int exp_dur;
        private int act_dur;
        private ParseEnum.Activation start_activation;
        private ParseEnum.Activation end_activation;


        // Constructor
        public CastLog(long time, long skill_id, int exp_dur, ParseEnum.Activation start_activation,int act_dur, ParseEnum.Activation end_activation)
        {
            this.time = time;
            this.skill_id = skill_id;
            this.exp_dur = exp_dur;
            this.start_activation = start_activation;
            this.act_dur = act_dur;
            this.end_activation = end_activation;
        }
        //start cast log
        public CastLog(long time, long skill_id, int exp_dur, ParseEnum.Activation start_activation)
        {
            this.time = time;
            this.skill_id = skill_id;
            this.exp_dur = exp_dur;
            this.start_activation = start_activation;
            
        }

        // setters
        public void SetEndStatus(int act_dur, ParseEnum.Activation end_activation)
        {
            this.act_dur = act_dur;
            this.end_activation = end_activation;
        }

        // Getters
        public long GetTime()
        {
            return time;
        }
        public long GetID()
        {
            return skill_id;
        }
        public int GetExpDur()
        {
            return exp_dur;
        }
        public ParseEnum.Activation StartActivation()
        {
            return start_activation;
        }
        public int GetActDur()
        {
            return act_dur;
        }
        public ParseEnum.Activation EndActivation()
        {
            return end_activation;
        }
    }
}

