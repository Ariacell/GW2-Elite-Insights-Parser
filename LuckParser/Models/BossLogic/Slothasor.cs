﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Slothasor : BossLogic
    {
        public Slothasor()
        {
            Mode = ParseMode.Raid;
            MechanicList.AddRange(new List<Mechanic>
            {
            new Mechanic(34479, "Tantrum", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Slothasor, "symbol:'circle-open',color:'rgb(255,200,0)',", "Tantrum",0),
            new Mechanic(34387, "Volatile Poison", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Slothasor, "symbol:'circle-open',color:'rgb(255,0,0)',", "Poison (Action Key)",0),
            new Mechanic(34481, "Volatile Poison", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Slothasor, "symbol:'circle',color:'rgb(255,0,0)',", "Poison (Area Damage)",0),
            new Mechanic(34516, "Halitosis", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Slothasor, "symbol:'triangle-right',color:'rgb(255,140,0)',", "Flame Breath",0),
            new Mechanic(34482, "Spore Release", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Slothasor, "symbol:'pentagon',color:'rgb(255,0,0)',", "Shake",0),
            new Mechanic(34362, "Magic Transformation", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Slothasor, "symbol:'diamond-tall',color:'rgb(0,255,255)',", "Slub Transform",0),
            //new Mechanic(34496, "Nauseated", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Slothasor, "symbol:'diamond-tall-open',color:'rgb(200,140,255)',", "Slub CD",0), //can be skipped imho, identical person and timestamp as Slub Transform
            new Mechanic(34508, "Fixated", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Slothasor, "symbol:'star',color:'rgb(255,0,255)',", "Fixated",0),
            new Mechanic(34565, "Toxic Cloud", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Slothasor, "symbol:'pentagon-open',color:'rgb(0,128,0)',", "Floor Poison (Green)",0),
            new Mechanic(34537, "Toxic Cloud", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Slothasor, "symbol:'pentagon-open',color:'rgb(0,128,0)',", "Floor Poison (Green)",0)
            });
        }

        public override CombatReplayMap GetCombatMap()
        {
            return new CombatReplayMap("https://i.imgur.com/aLHcYSF.png",
                            Tuple.Create(1688, 2581),
                            Tuple.Create(5822, -3491, 9549, 2205),
                            Tuple.Create(-12288, -27648, 12288, 27648),
                            Tuple.Create(2688, 11906, 3712, 14210));
        }

        public override List<ParseEnum.ThrashIDS> GetAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO:facing information (breath)
            List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
            {
                ParseEnum.ThrashIDS.Slubling1,
                ParseEnum.ThrashIDS.Slubling2,
                ParseEnum.ThrashIDS.Slubling3,
                ParseEnum.ThrashIDS.Slubling4
            };
            List<CastLog> sleepy = cls.Where(x => x.GetID() == 34515).ToList();
            foreach (CastLog c in sleepy)
            {
                replay.AddCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>((int)c.GetTime(), (int)c.GetTime() + c.GetActDur()), "rgba(0, 180, 255, 0.3)"));
            }

            List<CastLog> tantrum = cls.Where(x => x.GetID() == 34547).ToList();
            foreach (CastLog c in tantrum)
            {
                int start = (int)c.GetTime();
                int end = start + c.GetActDur();
                replay.AddCircleActor(new CircleActor(false, 0, 300, new Tuple<int, int>(start, end), "rgba(255, 150, 0, 0.4)"));
                replay.AddCircleActor(new CircleActor(true, end, 300, new Tuple<int, int>(start, end), "rgba(255, 150, 0, 0.4)"));
            }
            List<CastLog> shakes = cls.Where(x => x.GetID() == 34482).ToList();
            foreach (CastLog c in shakes)
            {
                int start = (int)c.GetTime();
                int end = start + c.GetActDur();
                replay.AddCircleActor(new CircleActor(false, 0, 700, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.4)"));
                replay.AddCircleActor(new CircleActor(true, end, 700, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.4)"));
            }
            return ids;
        }

        public override void GetAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // Poison
            List<CombatItem> poisonToDrop = GetFilteredList(log, 34387, p.GetInstid());
            int toDropStart = 0;
            foreach (CombatItem c in poisonToDrop)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    toDropStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int toDropEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware()); replay.AddCircleActor(new CircleActor(false, 0, 180, new Tuple<int, int>(toDropStart, toDropEnd), "rgba(255, 255, 100, 0.5)"));
                    replay.AddCircleActor(new CircleActor(true, toDropStart + 8000, 180, new Tuple<int, int>(toDropStart, toDropEnd), "rgba(255, 255, 100, 0.5)"));
                    Point3D poisonPos = replay.GetPositions().FirstOrDefault(x => x.Time > toDropEnd);
                    if (poisonPos != null)
                    {
                        replay.AddCircleActor(new CircleActor(true, toDropStart + 90000, 900, new Tuple<int, int>(toDropEnd, toDropEnd + 90000), "rgba(255, 0, 0, 0.3)", poisonPos));
                    }
                }
            }
            // Transformation
            List<CombatItem> slubTrans = GetFilteredList(log, 34362, p.GetInstid());
            int transfoStart = 0;
            foreach (CombatItem c in slubTrans)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    transfoStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int transfoEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                    replay.AddCircleActor(new CircleActor(true, 0, 160, new Tuple<int, int>(transfoStart, transfoEnd), "rgba(0, 80, 255, 0.3)"));
                }
            }
            // fixated
            List<CombatItem> fixatedSloth = GetFilteredList(log, 34508, p.GetInstid());
            int fixatedSlothStart = 0;
            foreach (CombatItem c in fixatedSloth)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    fixatedSlothStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int fixatedSlothEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                    replay.AddCircleActor(new CircleActor(true, 0, 120, new Tuple<int, int>(fixatedSlothStart, fixatedSlothEnd), "rgba(255, 80, 255, 0.3)"));
                }
            }
        }

        public override string GetReplayIcon()
        {
            return "https://i.imgur.com/h1xH3ER.png";
        }
    }
}
