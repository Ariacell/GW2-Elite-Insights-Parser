﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Deimos : RaidLogic
    {
        public Deimos()
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new Mechanic(37716, "Rapid Decay", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'circle-open',color:'rgb(0,0,0)',", "Oil","Rapid Decay (Black expanding oil)", "Black Oil",0),
            new Mechanic(37846, "Off Balance", Mechanic.MechType.EnemyCastStart, ParseEnum.BossIDS.Deimos, "symbol:'diamond-tall',color:'rgb(0,160,150)',", "TP.CC","Off Balance (Saul TP Breakbar)", "Saul TP Start",0),
            new Mechanic(37846, "Off Balance", Mechanic.MechType.EnemyCastEnd, ParseEnum.BossIDS.Deimos, "symbol:'diamond-tall',color:'rgb(255,0,0)',", "TP.CC.Fail","Failed Saul TP CC", "Failed CC",0, (condition => condition.CombatItem.Value >= 2200)),
            new Mechanic(37846, "Off Balance", Mechanic.MechType.EnemyCastEnd, ParseEnum.BossIDS.Deimos, "symbol:'diamond-tall',color:'rgb(0,160,0)',", "TP.CCed","Saul TP CCed", "CCed",0, (condition => condition.CombatItem.Value < 2200)),
            new Mechanic(38272, "Boon Thief", Mechanic.MechType.EnemyCastStart, ParseEnum.BossIDS.Deimos, "symbol:'diamond-wide',color:'rgb(0,160,150)',", "Thief.CC","Boon Thief (Saul Breakbar)", "Boon Thief Start",0),
            new Mechanic(38272, "Boon Thief", Mechanic.MechType.EnemyCastEnd, ParseEnum.BossIDS.Deimos, "symbol:'diamond-wide',color:'rgb(255,0,0)',", "Thief.CC.Fail","Failed Boon Thief CC", "Failed CC",0, (condition => condition.CombatItem.Value >= 4400)),
            new Mechanic(38272, "Boon Thief", Mechanic.MechType.EnemyCastEnd, ParseEnum.BossIDS.Deimos, "symbol:'diamond-wide',color:'rgb(0,160,0)',", "Thief.CCed","Boon Thief CCed", "CCed",0, (condition => condition.CombatItem.Value < 4400)),
            new Mechanic(38208, "Annihilate", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'hexagon',color:'rgb(255,200,0)',", "Smash","Annihilate (Cascading Pizza attack)", "Boss Smash",0),
            new Mechanic(37929, "Annihilate", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'hexagon',color:'rgb(255,200,0)',", "Smash","Annihilate (Cascading Pizza attack)", "Boss Smash",0),
            new Mechanic(37980, "Demonic Shock Wave", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'triangle-right-open',color:'rgb(255,0,0)',", "10%RSmsh","Knockback (right hand) in 10% Phase", "10% Right Smash",0),
            new Mechanic(38046, "Demonic Shock Wave", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'triangle-left-open',color:'rgb(255,0,0)',", "10%LSmash","Knockback (left hand) in 10% Phase", "10% Left Smash",0),
            new Mechanic(37982, "Demonic Shock Wave", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'bowtie',color:'rgb(255,0,0)',", "10%DSmsh","Knockback (both hands) in 10% Phase", "10% Double Smash",0),
            new Mechanic(37733, "Tear Instability", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'diamond',color:'rgb(0,128,128)',", "Tear","Collected a Demonic Tear", "Tear",0),
            new Mechanic(37613, "Mind Crush", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'square',color:'rgb(0,0,255)',", "MCrsh","Hit by Mind Crush without Bubble Protection", "Mind Crush",0,(condition => condition.DamageLog.Damage > 0)),
            new Mechanic(38187, "Weak Minded", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'square-open',color:'rgb(200,140,255)',", "WkMind","Weak Minded (Debuff after Mind Crush)", "Weak Minded",0),
            new Mechanic(37730, "Chosen by Eye of Janthir", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'circle',color:'rgb(0,255,0)',", "Grn","Chosen by the Eye of Janthir", "Chosen (Green)",0), 
            new Mechanic(38169, "Teleported", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'circle-open',color:'rgb(0,255,0)',", "TP","Teleport to/from Demonic Realm", "Teleport",0),
            new Mechanic(38224, "Unnatural Signet", Mechanic.MechType.EnemyBoon, ParseEnum.BossIDS.Deimos, "symbol:'square-open',color:'rgb(0,255,255)',", "DMGDbf","Double Damage Debuff on Deimos", "+100% Dmg Buff",0)
            });
            Extension = "dei";
            IconUrl = "https://wiki.guildwars2.com/images/e/e0/Mini_Ragged_White_Mantle_Figurehead.png";
        }

        public override CombatReplayMap GetCombatMap()
        {
            return new CombatReplayMap("https://i.imgur.com/GCwOVVE.png",
                            Tuple.Create(4400, 5753),
                            Tuple.Create(-9542, 1932, -7004, 5250),
                            Tuple.Create(-27648, -9216, 27648, 12288),
                            Tuple.Create(11774, 4480, 14078, 5376));
        }

        public override List<PhaseData> GetPhases(Boss boss, ParsedLog log, List<CastLog> castLogs)
        {
            long start = 0;
            long end = 0;
            long fightDuration = log.FightData.FightDuration;
            List<PhaseData> phases = GetInitialPhase(log);
            // Determined + additional data on inst change
            CombatItem invulDei = log.GetBoonData(762).Find(x => x.IsBuffRemove == ParseEnum.BuffRemove.None && x.DstInstid == boss.InstID);
            if (invulDei != null)
            {
                end = invulDei.Time - log.FightData.FightStart;
                phases.Add(new PhaseData(start, end));
                start = (boss.PhaseData.Count == 1 ? boss.PhaseData[0] - log.FightData.FightStart : fightDuration);
                castLogs.Add(new CastLog(end, -6, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None));
            }
            if (fightDuration - start > 5000 && start >= phases.Last().End)
            {
                phases.Add(new PhaseData(start, fightDuration));
            }
            for (int i = 1; i < phases.Count; i++)
            {
                phases[i].Name = "Phase " + i;
            }
            int offsetDei = phases.Count;
            CombatItem teleport = log.GetBoonData(38169).FirstOrDefault();
            int splits = 0;
            while (teleport != null && splits < 3)
            {
                start = teleport.Time - log.FightData.FightStart;
                CombatItem teleportBack = log.GetBoonData(38169).FirstOrDefault(x => x.Time - log.FightData.FightStart > start + 10000);
                if (teleportBack != null)
                {
                    end = Math.Min(teleportBack.Time - log.FightData.FightStart, fightDuration);
                }
                else
                {
                    end = fightDuration;
                }
                phases.Add(new PhaseData(start, end));
                splits++;
                teleport = log.GetBoonData(38169).FirstOrDefault(x => x.Time - log.FightData.FightStart > end + 10000);
            }

            string[] namesDeiSplit = new [] { "Thief", "Gambler", "Drunkard" };
            for (int i = offsetDei; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                phase.Name = namesDeiSplit[i - offsetDei];
                List<ParseEnum.TrashIDS> ids = new List<ParseEnum.TrashIDS>
                    {
                        ParseEnum.TrashIDS.Thief,
                        ParseEnum.TrashIDS.Drunkard,
                        ParseEnum.TrashIDS.Gambler,
                        ParseEnum.TrashIDS.GamblerClones,
                        ParseEnum.TrashIDS.GamblerReal,
                    };
                List<AgentItem> clones = log.AgentData.NPCAgentList.Where(x => ids.Contains(ParseEnum.GetTrashIDS(x.ID))).ToList();
                foreach (AgentItem a in clones)
                {
                    long agentStart = a.FirstAware - log.FightData.FightStart;
                    if (phase.InInterval(agentStart))
                    {
                        phase.Redirection.Add(a);
                    }
                }
                phase.OverrideStart(log.FightData.FightStart);

            }
            phases.Sort((x, y) => (x.Start < y.Start) ? -1 : 1);
            return phases;
        }

        public override List<ParseEnum.TrashIDS> GetAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO: facing information (slam)
            List<ParseEnum.TrashIDS> ids = new List<ParseEnum.TrashIDS>
                    {
                        ParseEnum.TrashIDS.Saul,
                        ParseEnum.TrashIDS.Thief,
                        ParseEnum.TrashIDS.Drunkard,
                        ParseEnum.TrashIDS.Gambler,
                        ParseEnum.TrashIDS.GamblerClones,
                        ParseEnum.TrashIDS.GamblerReal,
                        ParseEnum.TrashIDS.Greed,
                        ParseEnum.TrashIDS.Pride,
                        ParseEnum.TrashIDS.Oil,
                        ParseEnum.TrashIDS.Tear
                    };
            List<CastLog> mindCrush = cls.Where(x => x.SkillId == 37613).ToList();
            foreach (CastLog c in mindCrush)
            {
                int start = (int)c.Time;
                int end = start + 5000;
                replay.Actors.Add(new CircleActor(true, end, 180, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.5)"));
                replay.Actors.Add(new CircleActor(false, 0, 180, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.5)"));
                if (!log.FightData.IsCM)
                {
                    replay.Actors.Add(new CircleActor(true, 0, 180, new Tuple<int, int>(start, end), "rgba(0, 0, 255, 0.3)", new Point3D(-8421.818f, 3091.72949f, -9.818082e8f, 216)));
                }
            }
            List<CastLog> annihilate = cls.Where(x => (x.SkillId == 38208) || (x.SkillId == 37929)).ToList();
            foreach (CastLog c in annihilate)
            {
                int start = (int)c.Time;
                int delay = 1000;
                int end = start + 2400;
                int duration = 120;
                Point3D facing = replay.Rotations.FirstOrDefault(x => x.Time >= start);
                if (facing == null)
                {
                    continue;
                }
                for (int i = 0; i < 6; i++)
                {
                    replay.Actors.Add(new PieActor(true, 0, 900, (int)Math.Round(Math.Atan2(-facing.Y, facing.X) * 180 / Math.PI + i * 360 / 10), 360 / 10, new Tuple<int, int>(start + delay + i * duration, end + i * duration), "rgba(255, 200, 0, 0.5)"));
                    replay.Actors.Add(new PieActor(false, 0, 900, (int)Math.Round(Math.Atan2(-facing.Y, facing.X) * 180 / Math.PI + i * 360 / 10), 360 / 10, new Tuple<int, int>(start + delay + i * duration, end + i * 120), "rgba(255, 150, 0, 0.5)"));
                    if (i % 5 != 0)
                    {
                        replay.Actors.Add(new PieActor(true, 0, 900, (int)Math.Round(Math.Atan2(-facing.Y, facing.X) * 180 / Math.PI - i * 360 / 10), 360 / 10, new Tuple<int, int>(start + delay + i * duration, end + i * 120), "rgba(255, 200, 0, 0.5)"));
                        replay.Actors.Add(new PieActor(false, 0, 900, (int)Math.Round(Math.Atan2(-facing.Y, facing.X) * 180 / Math.PI - i * 360 / 10), 360 / 10, new Tuple<int, int>(start + delay + i * duration, end + i * 120), "rgba(255, 150, 0, 0.5)"));
                    }
                }
            }
            return ids;
        }

        public override void GetAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // teleport zone
            List<CombatItem> tpDeimos = GetFilteredList(log, 37730, p.InstID);
            int tpStart = 0;
            foreach (CombatItem c in tpDeimos)
            {
                if (c.IsBuffRemove == ParseEnum.BuffRemove.None)
                {
                    tpStart = (int)(c.Time - log.FightData.FightStart);
                }
                else
                {
                    int tpEnd = (int)(c.Time - log.FightData.FightStart);
                    replay.Actors.Add(new CircleActor(true, 0, 180, new Tuple<int, int>(tpStart, tpEnd), "rgba(0, 150, 0, 0.3)"));
                    replay.Actors.Add(new CircleActor(true, tpEnd, 180, new Tuple<int, int>(tpStart, tpEnd), "rgba(0, 150, 0, 0.3)"));
                }
            }
        }

        public override int IsCM(List<CombatItem> clist, int health)
        {
            return (health > 40e6) ? 1 : 0;
        }

        public override string GetReplayIcon()
        {
            return "https://i.imgur.com/mWfxBaO.png";
        }
    }
}
