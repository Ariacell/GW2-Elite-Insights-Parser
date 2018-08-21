﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Matthias : BossLogic
    {
        public Matthias()
        {
            Mode = ParseMode.Raid;
            MechanicList.AddRange(new List<Mechanic>
            {

            new Mechanic(34380, "Oppressive Gaze", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'hexagram',color:'rgb(255,0,0)',", "Hadouken",0),//human
            new Mechanic(34371, "Oppressive Gaze", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'hexagram',color:'rgb(255,0,0)',", "Hadouken",0),//abom
            new Mechanic(34480, "Blood Shards", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'diamond-wide-open',color:'rgb(255,0,255)',", "Rapid Fire",0),//human
            new Mechanic(34440, "Blood Shards", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'diamond-wide-open',color:'rgb(255,0,255)',", "Rapid Fire",0),//abom
            new Mechanic(34404, "Shards of Rage", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'star-diamond',color:'rgb(255,0,0)',", "Shards",1000),//human
            new Mechanic(34411, "Shards of Rage", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'star-diamond',color:'rgb(255,0,0)',", "Shards",1000),//abom
            new Mechanic(34466, "Fiery Vortex", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'triangle-down-open',color:'rgb(255,200,0)',", "Tornado",250), 
            new Mechanic(34543, "Thunder", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'triangle-up-open',color:'rgb(0,255,255)',", "Storm",0),
            new Mechanic(34450, "Unstable Blood Magic", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'diamond',color:'rgb(255,0,0)',", "Well",0),
            new Mechanic(34336, "Well of the Profane", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'diamond-open',color:'rgb(255,0,0)',", "W.Dmg",0),
            //Knockdown from Icy Patches: Cannot find a combat event for exactly this. There is however a slow application (ID 26766) of exactly 10 seconds to the player that gets knocked down. Maybe could be tracked by something like that. Also, Icy Patches throw snowballs (ID 34447) but I am not sure if that is related as there seems to be no target.
            new Mechanic(34416, "Corruption", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'circle',color:'rgb(255,140,0)',", "Corruption",0),
            new Mechanic(34473, "Corruption", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'circle-open',color:'rgb(255,140,0)',", "C.Dmg",0),
            new Mechanic(34442, "Sacrifice", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'circle',color:'rgb(128,0,128)',", "Sacrifice",0),
            new Mechanic(34367, "Unbalanced", Mechanic.MechType.PlayerBoonRemove, ParseEnum.BossIDS.Matthias, "symbol:'square',color:'rgb(200,140,255)',", "Knockdown",5000,(value => value > 0)), //Does this only trigger on actual Knockdown or also when just reaching 10 stacks and letting the debuff time out? Maybe check is_buffremove==3.
            //new Mechanic(34422, "Blood Fueled", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'square',color:'rgb(255,0,0)',", "Ate Reflects(good)",0),//human //Applied at the same time as Backflip Shards since it is the buff applied by them, can be omitted imho
            //new Mechanic(34428, "Blood Fueled", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'square',color:'rgb(255,0,0)',", "Ate Reflects(good)",0),//abom
            new Mechanic(34376, "Blood Shield", Mechanic.MechType.EnemyBoon, ParseEnum.BossIDS.Matthias, "symbol:'octagon',color:'rgb(255,0,0)',", "Bubble",0),//human
            new Mechanic(34518, "Blood Shield", Mechanic.MechType.EnemyBoon, ParseEnum.BossIDS.Matthias, "symbol:'octagon',color:'rgb(255,0,0)',", "Bubble",0),//abom
            new Mechanic(34511, "Zealous Benediction", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'circle',color:'rgb(255,200,0)',", "Bombs",0),
            new Mechanic(26766, "Icy Patch", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Matthias, "symbol:'circle-open',color:'rgb(0,0,255)',", "Icy KD",0,(value => value == 10000)),
            //Track Zealous Benediction? (Bombs) Application? (ID 34511) Hit? (ID 34528) The hit on allies is not registered since it's percentage based, I think
            new Mechanic(34413, "Surrender", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Matthias, "symbol:'circle-cross-open',color:'rgb(0,0,0)',", "Spirit",0)
            });
        }

        public override CombatReplayMap GetCombatMap()
        {
            return new CombatReplayMap("https://i.imgur.com/3X0YveK.png",
                            Tuple.Create(880, 880),
                            Tuple.Create(-7248, 4585, -4625, 7207),
                            Tuple.Create(-12288, -27648, 12288, 27648),
                            Tuple.Create(2688, 11906, 3712, 14210));
        }

        public override List<PhaseData> GetPhases(Boss boss, ParsedLog log, List<CastLog> castLogs)
        {
            long start = 0;
            long end = 0;
            long fightDuration = log.GetBossData().GetAwareDuration();
            List<PhaseData> phases = GetInitialPhase(log);
            // Special buff cast check
            CombatItem heatWave = log.GetCombatList().Find(x => x.GetSkillID() == 34526);
            List<long> phaseStarts = new List<long>();
            if (heatWave != null)
            {
                phaseStarts.Add(heatWave.GetTime() - log.GetBossData().GetFirstAware());
                CombatItem downPour = log.GetCombatList().Find(x => x.GetSkillID() == 34554);
                if (downPour != null)
                {
                    phaseStarts.Add(downPour.GetTime() - log.GetBossData().GetFirstAware());
                    CastLog abo = castLogs.Find(x => x.GetID() == 34427);
                    if (abo != null)
                    {
                        phaseStarts.Add(abo.GetTime());
                    }
                }
            }
            foreach (long t in phaseStarts)
            {
                end = t;
                phases.Add(new PhaseData(start, end));
                // make sure stuff from the precedent phase mix witch each other
                start = t + 1;
            }
            if (fightDuration - start > 5000 && start >= phases.Last().GetEnd())
            {
                phases.Add(new PhaseData(start, fightDuration));
            }
            string[] namesMat = new [] { "Ice Phase", "Fire Phase", "Storm Phase", "Abomination Phase" };
            for (int i = 1; i < phases.Count; i++)
            {
                phases[i].SetName(namesMat[i - 1]);
            }
            return phases;
        }

        public override List<ParseEnum.ThrashIDS> GetAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO: needs facing information for hadouken
            List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                        ParseEnum.ThrashIDS.Spirit,
                        ParseEnum.ThrashIDS.Spirit2,
                        ParseEnum.ThrashIDS.IcePatch,
                        ParseEnum.ThrashIDS.Tornado,
                        ParseEnum.ThrashIDS.Storm
                    };
            List<CastLog> humanShield = cls.Where(x => x.GetID() == 34468).ToList();
            List<int> humanShieldRemoval = log.GetBoonData().Where(x => x.GetSkillID() == 34518 && x.IsBuffremove() == ParseEnum.BuffRemove.All).Select(x => (int)(x.GetTime() - log.GetBossData().GetFirstAware())).Distinct().ToList();
            for (var i = 0; i < humanShield.Count; i++)
            {
                var shield = humanShield[i];
                if (i < humanShieldRemoval.Count)
                {
                    int removal = humanShieldRemoval[i];
                    replay.AddCircleActor(new CircleActor(true, 0, 250, new Tuple<int, int>((int)shield.GetTime(), removal), "rgba(255, 0, 255, 0.5)"));
                } else
                {
                    replay.AddCircleActor(new CircleActor(true, 0, 250, new Tuple<int, int>((int)shield.GetTime(), (int)log.GetBossData().GetAwareDuration()), "rgba(255, 0, 255, 0.5)"));
                }
            }
            List<CastLog> aboShield = cls.Where(x => x.GetID() == 34510).ToList();
            List<int> aboShieldRemoval = log.GetBoonData().Where(x => x.GetSkillID() == 34376 && x.IsBuffremove() == ParseEnum.BuffRemove.All).Select(x => (int)(x.GetTime() - log.GetBossData().GetFirstAware())).Distinct().ToList();
            for (var i = 0; i < aboShield.Count; i++)
            {
                var shield = aboShield[i];
                if (i < aboShieldRemoval.Count)
                {
                    int removal = aboShieldRemoval[i];
                    replay.AddCircleActor(new CircleActor(true, 0, 250, new Tuple<int, int>((int)shield.GetTime(), removal), "rgba(255, 0, 255, 0.5)"));
                }
                else
                {
                    replay.AddCircleActor(new CircleActor(true, 0, 250, new Tuple<int, int>((int)shield.GetTime(), (int)log.GetBossData().GetAwareDuration()), "rgba(255, 0, 255, 0.5)"));
                }
            }
            List<CastLog> rageShards = cls.Where(x => x.GetID() == 34404 || x.GetID() == 34411).ToList();
            foreach (CastLog c in rageShards)
            {
                int start = (int)c.GetTime();
                int end = start + c.GetActDur();
                replay.AddCircleActor(new CircleActor(false, 0, 300, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.5)"));
                replay.AddCircleActor(new CircleActor(true, end, 300, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.5)"));
            }
            return ids;
        }

        public override void GetAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // Corruption
            List<CombatItem> corruptedMatthias = GetFilteredList(log, 34416, p.GetInstid());
            corruptedMatthias.AddRange(GetFilteredList(log, 34473, p.GetInstid()));
            int corruptedMatthiasStart = 0;
            foreach (CombatItem c in corruptedMatthias)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    corruptedMatthiasStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int corruptedMatthiasEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                    replay.AddCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>(corruptedMatthiasStart, corruptedMatthiasEnd), "rgba(255, 150, 0, 0.5)"));
                    Point3D wellPosition = replay.GetPositions().FirstOrDefault(x => x.Time > corruptedMatthiasEnd);
                    if (wellPosition != null)
                    {
                        replay.AddCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>(corruptedMatthiasEnd, corruptedMatthiasEnd + 100000), "rgba(0, 0, 0, 0.3)", wellPosition));
                        replay.AddCircleActor(new CircleActor(true, corruptedMatthiasEnd + 100000, 180, new Tuple<int, int>(corruptedMatthiasEnd, corruptedMatthiasEnd + 100000), "rgba(0, 0, 0, 0.3)", wellPosition));
                    }
                }
            }
            // Well of profane
            List<CombatItem> wellMatthias = GetFilteredList(log, 34450, p.GetInstid());
            int wellMatthiasStart = 0;
            foreach (CombatItem c in wellMatthias)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    wellMatthiasStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int wellMatthiasEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                    replay.AddCircleActor(new CircleActor(false, 0, 120, new Tuple<int, int>(wellMatthiasStart, wellMatthiasEnd), "rgba(150, 255, 80, 0.5)"));
                    replay.AddCircleActor(new CircleActor(true, wellMatthiasStart + 9000, 120, new Tuple<int, int>(wellMatthiasStart, wellMatthiasEnd), "rgba(150, 255, 80, 0.5)"));
                    Point3D wellPosition = replay.GetPositions().FirstOrDefault(x => x.Time > wellMatthiasEnd);
                    if (wellPosition != null)
                    {
                        replay.AddCircleActor(new CircleActor(true, 0, 300, new Tuple<int, int>(wellMatthiasEnd, wellMatthiasEnd + 90000), "rgba(255, 0, 50, 0.5)", wellPosition));
                    }
                }
            }
            // Sacrifice
            List<CombatItem> sacrificeMatthias = GetFilteredList(log, 34442, p.GetInstid());
            int sacrificeMatthiasStart = 0;
            foreach (CombatItem c in sacrificeMatthias)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    sacrificeMatthiasStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int sacrificeMatthiasEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                    replay.AddCircleActor(new CircleActor(true, 0, 120, new Tuple<int, int>(sacrificeMatthiasStart, sacrificeMatthiasEnd), "rgba(0, 150, 250, 0.2)"));
                    replay.AddCircleActor(new CircleActor(true, sacrificeMatthiasStart + 10000, 120, new Tuple<int, int>(sacrificeMatthiasStart, sacrificeMatthiasEnd), "rgba(0, 150, 250, 0.35)"));
                }
            }
            // Bombs
            List<CombatItem> zealousBenediction = log.GetBoonData().Where(x => x.GetSkillID() == 34511 && ((x.GetDstInstid() == p.GetInstid() && x.IsBuffremove() == ParseEnum.BuffRemove.None))).ToList();
            foreach (CombatItem c in zealousBenediction)
            {
                int zealousStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware()) ;
                int zealousEnd = zealousStart + 5000;
                replay.AddCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>(zealousStart, zealousEnd), "rgba(200, 150, 0, 0.2)"));
                replay.AddCircleActor(new CircleActor(true, zealousEnd, 180, new Tuple<int, int>(zealousStart, zealousEnd), "rgba(200, 150, 0, 0.4)"));
            }
        }

        public override string GetReplayIcon()
        {
            return "https://i.imgur.com/3uMMmTS.png";
        }
    }
}
