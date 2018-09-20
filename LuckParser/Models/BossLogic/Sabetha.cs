﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Sabetha : RaidLogic
    {
        public Sabetha()
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new Mechanic(34108, "Shell-Shocked", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Sabetha, "symbol:'circle-open',color:'rgb(0,128,0)',", "Lnchd","Shell-Shocked (launched up to cannons)", "Shell-Shocked",0),
            new Mechanic(31473, "Sapper Bomb", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Sabetha, "symbol:'circle',color:'rgb(0,128,0)',", "SBmb","Got a Sapper Bomb", "Sapper Bomb",0),
            new Mechanic(31485, "Time Bomb", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Sabetha, "symbol:'circle-open',color:'rgb(255,0,0)',", "TBmb","Got a Timed Bomb (Expanding circle)", "Timed Bomb",0),
            new Mechanic(31332, "Firestorm", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Sabetha, "symbol:'square',color:'rgb(255,0,0)',", "Flmwll","Firestorm (killed by Flamewall)", "Flamewall",0),
            new Mechanic(31544, "Flak Shot", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Sabetha, "symbol:'hexagram-open',color:'rgb(255,140,0)',", "Flak","Flak Shot (Fire Patches)", "Flak Shot",0),
            new Mechanic(31643, "Cannon Barrage", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Sabetha, "symbol:'circle',color:'rgb(255,200,0)',", "Cannon","Cannon Barrage (stood in AoE)", "Cannon Shot",0),
            new Mechanic(31761, "Flame Blast", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Sabetha, "symbol:'triangle-left-open',color:'rgb(255,200,0)',", "Flmthrwr","Flame Blast (Karde's Flamethrower)", "Flamethrower (Karde)",0),
            new Mechanic(31408, "Kick", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Sabetha, "symbol:'triangle-right',color:'rgb(255,0,255)',", "Kick","Kicked by Bandit", "Bandit Kick",0) 
            // Hit by Time Bomb could be implemented by checking if a person is affected by ID 31324 (1st Time Bomb) or 34152 (2nd Time Bomb, only below 50% boss HP) without being attributed a bomb (ID: 31485) 3000ms before (+-50ms). I think the actual heavy hit isn't logged because it may be percentage based. Nothing can be found in the logs.
            });
            Extension = "sab";
            IconUrl = "https://wiki.guildwars2.com/images/5/54/Mini_Sabetha.png";
        }

        protected override CombatReplayMap GetCombatMapInternal()
        {
            return new CombatReplayMap("https://i.imgur.com/FwpMbYf.png",
                            Tuple.Create(2790, 2763),
                            Tuple.Create(-8587, -162, -1601, 6753),
                            Tuple.Create(-15360, -36864, 15360, 39936),
                            Tuple.Create(3456, 11012, 4736, 14212));
        }

        public override List<PhaseData> GetPhases(ParsedLog log, bool requirePhases)
        {
            long start = 0;
            long end = 0;
            long fightDuration = log.FightData.FightDuration;
            List<PhaseData> phases = GetInitialPhase(log);
            if (!requirePhases)
            {
                return phases;
            }
            // Invul check
            List<CombatItem> invulsSab = GetFilteredList(log, 757, log.Boss.InstID);
            for (int i = 0; i < invulsSab.Count; i++)
            {
                CombatItem c = invulsSab[i];
                if (c.IsBuffRemove == ParseEnum.BuffRemove.None)
                {
                    end = c.Time - log.FightData.FightStart;
                    phases.Add(new PhaseData(start, end));
                    if (i == invulsSab.Count - 1)
                    {
                        log.Boss.AddCustomCastLog(new CastLog(end, -5, (int)(fightDuration - end), ParseEnum.Activation.None, (int)(fightDuration - end), ParseEnum.Activation.None), log);
                    }
                }
                else
                {
                    start = c.Time - log.FightData.FightStart;
                    phases.Add(new PhaseData(end, start));
                    log.Boss.AddCustomCastLog(new CastLog(end, -5, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None), log);
                }
            }
            if (fightDuration - start > 5000 && start >= phases.Last().End)
            {
                phases.Add(new PhaseData(start, fightDuration));
            }
            string[] namesSab = new [] { "Phase 1", "Kernan", "Phase 2", "Knuckles", "Phase 3", "Karde", "Phase 4" };
            for (int i = 1; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                phase.Name = namesSab[i - 1];
                phase.DrawArea = i % 2 == 1;
                phase.DrawStart = i % 2 == 1 && i > 1;
                phase.DrawEnd = i % 2 == 1 && i < 7;
                if (i == 2 || i == 4 || i == 6)
                {
                    List<ParseEnum.TrashIDS> ids = new List<ParseEnum.TrashIDS>
                    {
                       ParseEnum.TrashIDS.Kernan,
                       ParseEnum.TrashIDS.Knuckles,
                       ParseEnum.TrashIDS.Karde,
                    };
                    List<AgentItem> champs = log.AgentData.NPCAgentList.Where(x => ids.Contains(ParseEnum.GetTrashIDS(x.ID))).ToList();
                    foreach (AgentItem a in champs)
                    {
                        long agentStart = a.FirstAware - log.FightData.FightStart;
                        if (phase.InInterval(agentStart))
                        {
                            phase.Redirection.Add(a);
                        }
                    }
                    phase.OverrideStart(log.FightData.FightStart);
                }
            }
            return phases;
        }

        protected override List<ParseEnum.TrashIDS> GetTrashMobsIDS()
        {
            return new List<ParseEnum.TrashIDS>
            {
                ParseEnum.TrashIDS.Kernan,
                ParseEnum.TrashIDS.Knuckles,
                ParseEnum.TrashIDS.Karde,
                ParseEnum.TrashIDS.BanditSapper,
                ParseEnum.TrashIDS.BanditThug,
                ParseEnum.TrashIDS.BanditArsonist
            };
        }

        public override void ComputeAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // timed bombs
            List<CombatItem> timedBombs = log.GetBoonData(31485).Where(x => x.DstInstid == p.InstID && x.IsBuffRemove == ParseEnum.BuffRemove.None).ToList();
            foreach (CombatItem c in timedBombs)
            {
                int start = (int)(c.Time - log.FightData.FightStart);
                int end = start + 3000;
                replay.Actors.Add(new CircleActor(false, 0, 280, new Tuple<int, int>(start, end), "rgba(255, 150, 0, 0.5)"));
                replay.Actors.Add(new CircleActor(true, end, 280, new Tuple<int, int>(start, end), "rgba(255, 150, 0, 0.5)"));
            }
            // Sapper bombs
            List<CombatItem> sapperBombs = GetFilteredList(log, 31473, p.InstID);
            int sapperStart = 0;
            foreach (CombatItem c in sapperBombs)
            {
                if (c.IsBuffRemove == ParseEnum.BuffRemove.None)
                {
                    sapperStart = (int)(c.Time - log.FightData.FightStart);
                }
                else
                {
                    int sapperEnd = (int)(c.Time - log.FightData.FightStart); replay.Actors.Add(new CircleActor(false, 0, 180, new Tuple<int, int>(sapperStart, sapperEnd), "rgba(200, 255, 100, 0.5)"));
                    replay.Actors.Add(new CircleActor(true, sapperStart + 5000, 180, new Tuple<int, int>(sapperStart, sapperEnd), "rgba(200, 255, 100, 0.5)"));
                }
            }
        }

        public override string GetReplayIcon()
        {
            return "https://i.imgur.com/UqbFp9S.png";
        }
    }
}
