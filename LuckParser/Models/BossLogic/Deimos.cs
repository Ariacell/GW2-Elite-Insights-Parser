﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Deimos : BossLogic
    {
        public Deimos()
        {
            Mode = ParseMode.Raid;
            MechanicList.AddRange(new List<Mechanic>
            {
                new Mechanic(37716, "Rapid Decay", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'circle-open',color:'rgb(0,0,0)',", "Oil",0), //Rapid Decay (Black expanding oil), Black Oil
            //new Mechanic(37844, "Off Balance", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'cross',color:'rgb(255,0,255)',", "Failed Teleport Break",0), Cast by the drunkard Saul, would be logical to be the forced random teleport but not sure when it's successful or not
            new Mechanic(37846, "Off Balance", Mechanic.MechType.EnemyCastEnd, ParseEnum.BossIDS.Deimos, "symbol:'cross',color:'rgb(255,0,255)',", "Drunkard Teleport",0, (value => value >= 2200)),// Seems to be the ID for the breakbar/cast start. If value is above 2200 (and the is_activation==5), the break apparently failed.
            new Mechanic(38272, "Boon Thief", Mechanic.MechType.EnemyCastEnd, ParseEnum.BossIDS.Deimos, "symbol:'x',color:'rgb(255,0,255)',", "Boon Thief",0, (value => value >= 4400)),// Seems to only be successful if the value is above 4400 (the duration of the breakbar). Also, has no dst_agent so new category might be necessary?
            new Mechanic(38208, "Annihilate", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'hexagon',color:'rgb(255,200,0)',", "Smash",0), //Annihilate (Cascading Pizza attack), Boss Smash
            new Mechanic(37929, "Annihilate", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'hexagon',color:'rgb(255,200,0)',", "Smash",0), //Annihilate (Cascading Pizza attack), Boss Smash
            new Mechanic(37980, "Demonic Shock Wave", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'circle',color:'rgb(255,0,0)',", "10%Smsh",0), //Knockback in 10% Phase, 10% Smash
            new Mechanic(37982, "Demonic Shock Wave", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'circle',color:'rgb(255,0,0)',", "10%Smsh",0), //Knockback in 10% Phase, 10% Smash
            new Mechanic(37733, "Tear Instability", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'diamond',color:'rgb(0,128,128)',", "Tear",0), //Collected a Demonic Tear, Tear
            new Mechanic(37613, "Mind Crush", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Deimos, "symbol:'square',color:'rgb(0,0,255)',", "MCrsh",0,(value => value > 0))), //Hit by Mind Crush without Bubble Protection, Mind Crush // Not properly detected everytime, sometimes people receive mind crush hit but no weak minded afterwards (partly due to green circle immunity). Suggestion: Only mark as hit if it deals non-zero damage (value>=0).
            new Mechanic(38187, "Weak Minded", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'square-open',color:'rgb(200,140,255)',", "WkMind",0), //Weak Minded (Debuff after Mind Crush), Weak Minded
            new Mechanic(37730, "Chosen by Eye of Janthir", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'circle',color:'rgb(0,255,0)',", "Grn",0), //Chosen by the Eye of Janthir, Chosen (Green)
            new Mechanic(38169, "Teleported", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Deimos, "symbol:'circle-open',color:'rgb(0,255,0)',", "TP",0), //Teleport to/from Demonic Realm, Teleport
            new Mechanic(38224, "Unnatural Signet", Mechanic.MechType.EnemyBoon, ParseEnum.BossIDS.Deimos, "symbol:'square-open',color:'rgb(0,255,255)',", "DMGDbf",0)//Double Damage Debuff on Deimos, +100% Dmg Buff
            //mlist.Add("Chosen by Eye of Janthir");
            //mlist.Add("");//tp from drunkard
            //mlist.Add("");//bon currupt from thief
            //mlist.Add("Teleport");//to demonic realm
            });
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
            long fightDuration = log.GetBossData().GetAwareDuration();
            List<PhaseData> phases = GetInitialPhase(log);
            // Determined + additional data on inst change
            CombatItem invulDei = log.GetBoonData().Find(x => x.GetSkillID() == 762 && x.IsBuffremove() == ParseEnum.BuffRemove.None && x.GetDstInstid() == boss.GetInstid());
            if (invulDei != null)
            {
                end = invulDei.GetTime() - log.GetBossData().GetFirstAware();
                phases.Add(new PhaseData(start, end));
                start = (boss.GetPhaseData().Count == 1 ? boss.GetPhaseData()[0] - log.GetBossData().GetFirstAware() : fightDuration);
                castLogs.Add(new CastLog(end, -6, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None));
            }
            if (fightDuration - start > 5000 && start >= phases.Last().GetEnd())
            {
                phases.Add(new PhaseData(start, fightDuration));
            }
            for (int i = 1; i < phases.Count; i++)
            {
                phases[i].SetName("Phase " + i);
            }
            int offsetDei = phases.Count;
            CombatItem teleport = log.GetCombatList().FirstOrDefault(x => x.GetSkillID() == 38169);
            int splits = 0;
            while (teleport != null && splits < 3)
            {
                start = teleport.GetTime() - log.GetBossData().GetFirstAware();
                CombatItem teleportBack = log.GetCombatList().FirstOrDefault(x => x.GetSkillID() == 38169 && x.GetTime() - log.GetBossData().GetFirstAware() > start + 10000);
                if (teleportBack != null)
                {
                    end = teleportBack.GetTime() - log.GetBossData().GetFirstAware();
                }
                else
                {
                    end = fightDuration;
                }
                phases.Add(new PhaseData(start, end));
                splits++;
                teleport = log.GetCombatList().FirstOrDefault(x => x.GetSkillID() == 38169 && x.GetTime() - log.GetBossData().GetFirstAware() > end + 10000);
            }

            string[] namesDeiSplit = new [] { "Thief", "Gambler", "Drunkard" };
            for (int i = offsetDei; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                phase.SetName(namesDeiSplit[i - offsetDei]);
                List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                        ParseEnum.ThrashIDS.Thief,
                        ParseEnum.ThrashIDS.Drunkard,
                        ParseEnum.ThrashIDS.Gambler,
                        ParseEnum.ThrashIDS.GamblerClones,
                        ParseEnum.ThrashIDS.GamblerReal,
                    };
                List<AgentItem> clones = log.GetAgentData().GetNPCAgentList().Where(x => ids.Contains(ParseEnum.GetThrashIDS(x.GetID()))).ToList();
                foreach (AgentItem a in clones)
                {
                    long agentStart = a.GetFirstAware() - log.GetBossData().GetFirstAware();
                    if (phase.InInterval(agentStart))
                    {
                        phase.AddRedirection(a);
                    }
                }

            }
            phases.Sort((x, y) => (x.GetStart() < y.GetStart()) ? -1 : 1);
            return phases;
        }

        public override List<ParseEnum.ThrashIDS> GetAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO: facing information (slam)
            List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                        ParseEnum.ThrashIDS.Saul,
                        ParseEnum.ThrashIDS.Thief,
                        ParseEnum.ThrashIDS.Drunkard,
                        ParseEnum.ThrashIDS.Gambler,
                        ParseEnum.ThrashIDS.GamblerClones,
                        ParseEnum.ThrashIDS.GamblerReal,
                        ParseEnum.ThrashIDS.Greed,
                        ParseEnum.ThrashIDS.Pride
                    };
            List<CastLog> mindCrush = cls.Where(x => x.GetID() == 37613).ToList();
            foreach (CastLog c in mindCrush)
            {
                int start = (int)c.GetTime();
                int end = start + 5000;
                replay.AddCircleActor(new CircleActor(true, end, 180, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.5)"));
                replay.AddCircleActor(new CircleActor(false, 0, 180, new Tuple<int, int>(start, end), "rgba(255, 0, 0, 0.5)"));
                if (!log.GetBossData().IsCM())
                {
                    replay.AddCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>(start, end), "rgba(0, 0, 255, 0.3)", new Point3D(-8421.818f, 3091.72949f, -9.818082e8f, 216)));
                }
            }
            return ids;
        }

        public override void GetAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // teleport zone
            List<CombatItem> tpDeimos = GetFilteredList(log, 37730, p.GetInstid());
            int tpStart = 0;
            foreach (CombatItem c in tpDeimos)
            {
                if (c.IsBuffremove() == ParseEnum.BuffRemove.None)
                {
                    tpStart = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                }
                else
                {
                    int tpEnd = (int)(c.GetTime() - log.GetBossData().GetFirstAware());
                    replay.AddCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>(tpStart, tpEnd), "rgba(0, 150, 0, 0.3)"));
                    replay.AddCircleActor(new CircleActor(true, tpEnd, 180, new Tuple<int, int>(tpStart, tpEnd), "rgba(0, 150, 0, 0.3)"));
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
