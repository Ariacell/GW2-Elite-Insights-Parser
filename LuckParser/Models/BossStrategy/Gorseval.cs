﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models
{
    public class Gorseval : BossStrategy
    {
        public Gorseval() : base()
        {
            mode = ParseMode.Raid;
            mechanicList = new List<Mechanic>
            {
            new Mechanic(31875, "Spectral Impact", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Gorseval, "symbol:'hexagram',color:'rgb(255,0,0)',", "Slam",4),
            new Mechanic(31623, "Ghastly Prison", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Gorseval, "symbol:'circle',color:'rgb(255,140,0)',", "Egg",0),
            new Mechanic(31498, "Spectral Darkness", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Gorseval, "symbol:'circle',color:'rgb(0,0,255)',", "Orb Debuff",0),
            new Mechanic(31722, "Spirited Fusion", Mechanic.MechType.EnemyBoon, ParseEnum.BossIDS.Gorseval, "symbol:'square',color:'rgb(255,140,0)',", "Ate Spirit",0),
            new Mechanic(31720, "Kick", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Gorseval, "symbol:'triangle-right',color:'rgb(255,0,255)',", "Kicked by Spirit",0)
            //new Mechanic(31834, "Ghastly Rampage", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Gorseval, "symbol:'circle',color:'rgb(0,0,0)',", "Stood in black",2), //stood in black? Trigger via (25 stacks) vuln (ID 738) application would be possible
            };
        }

        public override CombatReplayMap getCombatMap()
        {
            return new CombatReplayMap("https://i.imgur.com/nTueZcX.png",
                            Tuple.Create(1354, 1415),
                            Tuple.Create(-623, -6754, 3731, -2206),
                            Tuple.Create(-15360, -36864, 15360, 39936),
                            Tuple.Create(3456, 11012, 4736, 14212));
        }

        public override List<PhaseData> getPhases(Boss boss, ParsedLog log, List<CastLog> cast_logs)
        {
            long start = 0;
            long end = 0;
            long fight_dur = log.getBossData().getAwareDuration();
            List<PhaseData> phases = getInitialPhase(log);
            // Ghostly protection check
            List<CombatItem> invulsGorse = log.getCombatList().Where(x => x.getSkillID() == 31790 && x.isBuffremove() != ParseEnum.BuffRemove.Manual).ToList();
            for (int i = 0; i < invulsGorse.Count; i++)
            {
                CombatItem c = invulsGorse[i];
                if (c.isBuffremove() == ParseEnum.BuffRemove.None)
                {
                    end = c.getTime() - log.getBossData().getFirstAware();
                    phases.Add(new PhaseData(start, end));
                    if (i == invulsGorse.Count - 1)
                    {
                        cast_logs.Add(new CastLog(end, -5, (int)(fight_dur - end), ParseEnum.Activation.None, (int)(fight_dur - end), ParseEnum.Activation.None));
                    }
                }
                else
                {
                    start = c.getTime() - log.getBossData().getFirstAware();
                    phases.Add(new PhaseData(end, start));
                    cast_logs.Add(new CastLog(end, -5, (int)(start - end), ParseEnum.Activation.None, (int)(start - end), ParseEnum.Activation.None));
                }
            }
            if (fight_dur - start > 5000 && start >= phases.Last().getEnd())
            {
                phases.Add(new PhaseData(start, fight_dur));
            }
            string[] namesGorse = new string[] { "Phase 1", "Split 1", "Phase 2", "Split 2", "Phase 3" };
            for (int i = 1; i < phases.Count; i++)
            {
                phases[i].setName(namesGorse[i - 1]);
            }
            return phases;
        }

        public override List<ParseEnum.ThrashIDS> getAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO: doughnuts (rampage)
            List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                        ParseEnum.ThrashIDS.ChargedSoul
                    };
            List<CastLog> blooms = cls.Where(x => x.getID() == 31616).ToList();
            foreach (CastLog c in blooms)
            {
                int start = (int)c.getTime();
                int end = start + c.getActDur();
                replay.addCircleActor(new CircleActor(true, c.getExpDur() + (int)c.getTime(), 600, new Tuple<int, int>(start, end), "rgba(255, 125, 0, 0.5)"));
                replay.addCircleActor(new CircleActor(false, 0, 600, new Tuple<int, int>(start, end), "rgba(255, 125, 0, 0.5)"));
            }
            List<PhaseData> phases = log.getBoss().getPhases(log, true);
            if (phases.Count > 1)
            {
                List<CastLog> rampage = cls.Where(x => x.getID() == 31834).ToList();
                foreach (CastLog c in rampage)
                {
                    int start = (int)c.getTime();
                    int end = start + c.getActDur();
                    replay.addCircleActor(new CircleActor(true, 0, 180, new Tuple<int, int>(start, end), "rgba(0, 125, 255, 0.3)"));
                    // or spawn -> 3 secs -> explosion -> 0.5 secs -> fade -> 0.5  secs-> next
                    int ticks = (int)Math.Ceiling(c.getActDur() / 4000.0);
                    int phaseIndex = 1;
                    for (phaseIndex = 1; phaseIndex < phases.Count; phaseIndex++)
                    {
                        if (phases[phaseIndex].inInterval(start))
                        {
                            break;
                        }
                    }
                    Point3D pos = log.getBoss().getCombatReplay().getPositions().FirstOrDefault(x => x.time >= start);
                    if (pos == null)
                    {
                        break;
                    }
                    List<string> patterns = new List<string>();
                    switch (phaseIndex)
                    {
                        case 1:
                            patterns = new List<string>
                            {
                                "2+3",
                                "2+3+4",
                                "1+4",
                                "1+2",
                                "1+3",
                                "Full"
                            };
                            break;
                        case 3:
                            patterns = new List<string>
                            {
                                "2+3+4",
                                "1+4",
                                "1+3+4",
                                "1+2",
                                "1+2+3",
                                "Full"
                            };
                            break;
                        case 5:
                            patterns = new List<string>
                            {
                                "1+4",
                                "1+2",
                                "2+3",
                                "3+4",
                                "3+4",
                                "Full"
                            };
                            break;
                        default:
                            throw new Exception("how the fuck");
                    }
                    for (int i = 0; i < ticks; i++)
                    {
                        int tickStart = start + 4000 * i;
                        int explosion = tickStart + 3000;
                        int tickEnd = tickStart + 3500;
                        string pattern = patterns[i];
                        if (pattern.Contains("1"))
                        {
                            replay.addCircleActor(new CircleActor(true, explosion, 300, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.2)", pos));
                            replay.addCircleActor(new CircleActor(true,0 , 300, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.4)", pos));
                        }
                        if (pattern.Contains("2"))
                        {
                            replay.addDoughnutActor(new DoughnutActor(explosion, 300, 600, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.2)", pos));
                            replay.addDoughnutActor(new DoughnutActor(0, 300,600, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.4)", pos));
                        }
                        if (pattern.Contains("3"))
                        {
                            replay.addDoughnutActor(new DoughnutActor(explosion, 600, 900, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.2)", pos));
                            replay.addDoughnutActor(new DoughnutActor(0, 600, 900, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.4)", pos));
                        }
                        if (pattern.Contains("4"))
                        {
                            replay.addDoughnutActor(new DoughnutActor(explosion, 900, 1200, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.2)", pos));
                            replay.addDoughnutActor(new DoughnutActor(0, 900, 1200, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.4)", pos));
                        }
                        if (pattern.Contains("Full"))
                        {
                            replay.addCircleActor(new CircleActor(true, explosion, 1500, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.2)", log.getBoss().getCombatReplay().getPositions().First()));
                            replay.addCircleActor(new CircleActor(true, 0, 1500, new Tuple<int, int>(tickStart, tickEnd), "rgba(0, 0, 0, 0.4)", log.getBoss().getCombatReplay().getPositions().First()));
                        }
                    }
                }
            }

            return ids;
        }

        public override string getReplayIcon()
        {
            return "https://i.imgur.com/5hmMq12.png";
        }
    }
}
