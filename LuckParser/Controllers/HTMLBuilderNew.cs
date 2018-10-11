﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LuckParser.Models.DataModels;
using LuckParser.Models.HtmlModels;
using LuckParser.Models.ParseModels;

namespace LuckParser.Controllers
{
    class HTMLBuilderNew
    {
        private const string _scriptVersion = "0.5";
        private const int _scriptVersionRev = 11;
        private readonly SettingsContainer _settings;

        private readonly ParsedLog _log;

        private readonly Statistics _statistics;

        public HTMLBuilderNew(ParsedLog log, SettingsContainer settings, Statistics statistics)
        {
            _log = log;

            _settings = settings;
            HTMLHelper.Settings = settings;
            GraphHelper.Settings = settings;

            _statistics = statistics;
        }

        private static string FilterStringChars(string str)
        {
            string filtered = "";
            string filter = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
            foreach (char c in str)
            {
                if (filter.Contains(c))
                {
                    filtered += c;
                }
            }
            return filtered;
        }

        private List<int> ConvertGraph(List<Point> points)
        {
            List<int> graph = new List<int>();
            foreach (Point point in points)
            {
                graph.Add(point.Y);
            }
            return graph;
        }

        private double[] BuildBossHealthData(int phaseIndex, Boss target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            int duration = (int)phase.GetDuration("s");
            double[] chart = _statistics.BossHealth[target].Skip((int)phase.Start / 1000).Take(duration + 1).ToArray();
            return chart;
        }

        private BossChartDataDto BuildBossGraphData(int phaseIndex, Boss target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            return new BossChartDataDto
            {
                total = ConvertGraph(GraphHelper.GetTotalDPSGraph(_log, target, phaseIndex, phase, GraphHelper.GraphMode.S1)),
                health = BuildBossHealthData(phaseIndex, target)
            };
        }
        
        /// <summary>
        /// Creates the dps graph
        /// </summary>
        private List<PlayerChartDataDto> BuildPlayerGraphData(int phaseIndex)
        {
            List<PlayerChartDataDto> list = new List<PlayerChartDataDto>();
            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player p in _log.PlayerList)
            {
                PlayerChartDataDto pChar = new PlayerChartDataDto()
                {
                    total = ConvertGraph(GraphHelper.GetTotalDPSGraph(_log, p, phaseIndex, phase, GraphHelper.GraphMode.S1)),
                    bosses = new List<List<int>>()
                };
                foreach (Boss target in phase.Targets)
                {
                    pChar.bosses.Add(ConvertGraph(GraphHelper.GetBossDPSGraph(_log, p, phaseIndex, phase, GraphHelper.GraphMode.S1, target)));
                }
                list.Add(pChar);
            }
            return list;
        }

        private void GetRoles()
        {
            //tags: tank,healer,dps(power/condi)
            //Roles:greenteam,green split,cacnoneers,flakkiter,eater,KCpusher,agony,epi,handkiter,golemkiter,orbs
        }
        /// <summary>
        /// Creates the dps table
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<object>> BuildDPSData(int phaseIndex)
        {
            List<List<object>> list = new List<List<object>>(_log.PlayerList.Count);
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<string[]> footerList = new List<string[]>();

            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalDPS dpsAll = _statistics.DpsAll[player][phaseIndex];
                Statistics.FinalStatsAll stats = _statistics.StatsAll[player][phaseIndex];

                List<object> playerData = new List<object>();

                foreach (Boss target in phase.Targets)
                {
                    Statistics.FinalDPS dpsBoss = _statistics.DpsBoss[target][player][phaseIndex];
                    playerData.Add(dpsBoss.Damage);
                    playerData.Add(dpsBoss.Dps);
                    playerData.Add(dpsBoss.PowerDamage);
                    playerData.Add(dpsBoss.PowerDps);
                    playerData.Add(dpsBoss.CondiDamage);
                    playerData.Add(dpsBoss.CondiDps);
                }

                playerData.Add(dpsAll.Damage);
                playerData.Add(dpsAll.Dps);
                playerData.Add(dpsAll.PowerDamage);
                playerData.Add(dpsAll.PowerDps);
                playerData.Add(dpsAll.CondiDamage);
                playerData.Add(dpsAll.CondiDps);
                playerData.Add(stats.DownCount);


                if (stats.Died != 0.0)
                {
                    if (stats.Died < 0)
                    {
                        playerData.Add("");
                        playerData.Add(-stats.Died + " time(s)");
                    }
                    else
                    {
                        TimeSpan timedead = TimeSpan.FromMilliseconds(stats.Died);
                        playerData.Add(timedead + "(" + Math.Round((timedead.TotalMilliseconds / phase.GetDuration()) * 100, 1) + "% Alive)"); //28
                        playerData.Add(timedead.Minutes + " m " + timedead.Seconds + " s");
                    }
                }
                else
                {
                    playerData.Add("Never died");
                    playerData.Add("");
                }

                list.Add(playerData);
            }
            return list;
        }
        /// <summary>
        /// Creates the damage stats table
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<object>> BuildDMGStatsData(int phaseIndex)
        {
            List<List<object>> list = new List<List<object>>();
            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalStatsAll stats = _statistics.StatsAll[player][phaseIndex];
                Statistics.FinalDPS dps = _statistics.DpsAll[player][phaseIndex];

                List<object> playerData = new List<object>
                {
                    stats.PowerLoopCount, //0
                    stats.CritablePowerLoopCount, //1
                    Math.Round((double)(stats.CriticalRate) / stats.CritablePowerLoopCount * 100, 1), //2
                    stats.CriticalRate, //3
                    stats.CriticalDmg, //4

                    Math.Round((double)(stats.ScholarRate) / stats.PowerLoopCount * 100, 1), //5
                    stats.ScholarRate, //6
                    stats.ScholarDmg, //7
                    Math.Round(100.0 * (dps.PlayerPowerDamage / (double)(dps.PlayerPowerDamage - stats.ScholarDmg) - 1.0), 3), //8

                    Math.Round((double)(stats.MovingRate) / stats.PowerLoopCount * 100, 1), //9
                    stats.MovingRate, //10
                    stats.MovingDamage, //11
                    Math.Round(100.0 * (dps.PlayerPowerDamage / (double)(dps.PlayerPowerDamage - stats.MovingDamage) - 1.0), 3), //12

                    Math.Round(stats.FlankingRate / (double)stats.PowerLoopCount * 100, 1), //13
                    stats.FlankingRate, //14

                    Math.Round(stats.GlanceRate / (double)stats.PowerLoopCount * 100, 1), //15
                    stats.GlanceRate, //16

                    stats.Missed, //17
                    stats.Interrupts, //18
                    stats.Invulned, //19

                    stats.TimeWasted, //20
                    stats.Wasted, //21

                    stats.TimeSaved, //22
                    stats.Saved, //23

                    stats.SwapCount, //24
                    Math.Round(stats.StackDist, 2), //25
                    stats.DownCount //26
                };

                if (stats.Died != 0.0)
                {
                    if (stats.Died < 0)
                    {
                        playerData.Add(-stats.Died + " time(s)"); //27
                        playerData.Add(""); //28
                    }
                    else
                    {
                        TimeSpan timedead = TimeSpan.FromMilliseconds(stats.Died);
                        playerData.Add(timedead.Minutes + " m " + timedead.Seconds + " s"); //27
                        playerData.Add(timedead + "(" + Math.Round((timedead.TotalMilliseconds / phase.GetDuration()) * 100, 1) + "% Alive)"); //28
                    }
                }
                else
                {
                    playerData.Add(""); //27
                    playerData.Add("Never died"); //28
                }
                list.Add(playerData);
            }
            return list;
        }
        /// <summary>
        /// Creates the damage stats table for hits on just boss
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<object>> BuildDMGStatsBossData(int phaseIndex)
        {
            List<List<object>> list = new List<List<object>>();

            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player player in _log.PlayerList)
            {
                List<object> playerData = new List<object>();
                foreach (Boss target in phase.Targets)
                {
                    Statistics.FinalStats statsBoss = _statistics.StatsBoss[target][player][phaseIndex];
                    Statistics.FinalDPS dpsBoss = _statistics.DpsBoss[target][player][phaseIndex];
                    playerData.AddRange(new List<object>(){
                        statsBoss.PowerLoopCount, //0
                        statsBoss.CritablePowerLoopCount, //1
                        Math.Round((double)(statsBoss.CriticalRate) / statsBoss.CritablePowerLoopCount * 100, 1), //2
                        statsBoss.CriticalRate, //3
                        statsBoss.CriticalDmg, //4

                        Math.Round((double)(statsBoss.ScholarRate) / statsBoss.PowerLoopCount * 100, 1), //5
                        statsBoss.ScholarRate, //6
                        statsBoss.ScholarDmg, //7
                        Math.Round(100.0 * (dpsBoss.PlayerPowerDamage / (double)(dpsBoss.PlayerPowerDamage - statsBoss.ScholarDmg) - 1.0), 3), //8

                        Math.Round((double)(statsBoss.MovingRate) / statsBoss.PowerLoopCount * 100, 1), //9
                        statsBoss.MovingRate, //10
                        statsBoss.MovingDamage, //11
                        Math.Round(100.0 * (dpsBoss.PlayerPowerDamage / (double)(dpsBoss.PlayerPowerDamage - statsBoss.MovingDamage) - 1.0), 3), //12

                        Math.Round(statsBoss.FlankingRate / (double)statsBoss.PowerLoopCount * 100, 1), //13
                        statsBoss.FlankingRate, //14

                        Math.Round(statsBoss.GlanceRate / (double)statsBoss.PowerLoopCount * 100, 1), //15
                        statsBoss.GlanceRate, //16

                        statsBoss.Missed, //17
                        statsBoss.Interrupts, //18
                        statsBoss.Invulned //19
                    });
                }
                list.Add(playerData);
            }
            return list;
        }
        /// <summary>
        /// Creates the defense table
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<object>> BuildDefenseData(int phaseIndex)
        {
            List<List<object>> list = new List<List<object>>();

            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalDefenses defenses = _statistics.Defenses[player][phaseIndex];
                Statistics.FinalStatsAll stats = _statistics.StatsAll[player][phaseIndex];

                List<object> playerData = new List<object>
                {
                    defenses.DamageTaken,
                    defenses.DamageBarrier,
                    defenses.BlockedCount,
                    0,
                    defenses.InvulnedCount,
                    defenses.EvadedCount,
                    stats.DodgeCount,
                    stats.DownCount
                };

                if (stats.Died != 0.0)
                {
                    if (stats.Died < 0)
                    {
                        playerData.Add(-stats.Died + " time(s)");
                        playerData.Add("");
                    }
                    else
                    {
                        TimeSpan timedead = TimeSpan.FromMilliseconds(stats.Died);
                        playerData.Add(timedead.Minutes + " m " + timedead.Seconds + " s");
                        playerData.Add(timedead + "(" + Math.Round((timedead.TotalMilliseconds / phase.GetDuration()) * 100, 1) + "% Alive)");
                    }
                }
                else
                {
                    playerData.Add("");
                    playerData.Add("Never died");
                }

                list.Add(playerData);
            }

            return list;
        }
        /// <summary>
        /// Creates the support table
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<object>> BuildSupportData(int phaseIndex)
        {
            List<List<object>> list = new List<List<object>>();

            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalSupport support = _statistics.Support[player][phaseIndex];
                List<object> playerData = new List<object>(4)
                {
                    support.CondiCleanse,
                    support.CondiCleanseTime,
                    support.Resurrects,
                    support.ResurrectTime
                };
                list.Add(playerData);
            }
            return list;
        }
        /// <summary>
        /// Create the buff uptime table
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="listToUse"></param>
        /// <param name="tableId"></param>
        /// <param name="phaseIndex"></param>
        private List<BoonData> BuildBuffUptimeData(List<Boon> listToUse, int phaseIndex)
        {
            List<PhaseData> phases = _statistics.Phases;
            List<BoonData> list = new List<BoonData>();

            List<List<string>> footList = new List<List<string>>();
            
            HashSet<int> intensityBoon = new HashSet<int>();
            bool boonTable = listToUse.Select(x => x.ID).Contains(740);
                
            foreach (Player player in _log.PlayerList)
            {
                BoonData boonData = new BoonData();

                Dictionary<long, Statistics.FinalBoonUptime> boons = _statistics.SelfBoons[player][phaseIndex];
                Dictionary<long,List<AbstractMasterPlayer.ExtraBoonData>> extraBoonData = player.GetExtraBoonData(_log, null); Dictionary<long, List<AbstractMasterPlayer.ExtraBoonData>> extraBoonDataBoss = player.GetExtraBoonData(_log, _log.Boss);
                List<string> boonArrayToList = new List<string>
                {
                    player.Group.ToString()
                };
                long fightDuration = phases[phaseIndex].GetDuration();
                int count = 0;
                       
                if (boonTable)
                {
                    boonData.avg = Math.Round(_statistics.StatsAll[player][phaseIndex].AvgBoons, 1);
                }
                foreach (Boon boon in listToUse)
                {
                    List<object> boonVals = new List<object>();
                    boonData.val.Add(boonVals);

                    boonVals.Add(boons[boon.ID].Uptime);

                    if (boon.Type == Boon.BoonType.Intensity)
                    {
                        intensityBoon.Add(count);
                    }

                    if (extraBoonData.TryGetValue(boon.ID, out var list1))
                    {
                        var extraData = list1[phaseIndex];
                        string text = extraData.HitCount + " out of " + extraData.TotalHitCount + " hits<br>Pure Damage: " + extraData.DamageGain + "<br>Effective Damage Increase: " + extraData.Percent + "%";
                        string tooltip = "<big><b>All</b></big><br>" + text;
                        if (extraBoonDataBoss.TryGetValue(boon.ID, out var list2))
                        {
                            extraData = list2[phaseIndex];
                            text = extraData.HitCount + " out of " + extraData.TotalHitCount + " hits<br>Pure Damage: " + extraData.DamageGain + "<br>Effective Damage Increase: " + extraData.Percent + "%";
                            tooltip += "<br><big><b>Boss</b></big><br>" + text;
                        }
                        boonVals.Add(0);
                        boonVals.Add(tooltip);
                    }
                    else
                    {
                        if (boonTable && boon.Type == Boon.BoonType.Intensity && boons[boon.ID].Presence > 0)
                        {
                            boonVals.Add(boons[boon.ID].Presence);
                        }
                    }                                
                    boonArrayToList.Add(boons[boon.ID].Uptime.ToString());                        
                    count++;
                }
                        
                //gather data for footer
                footList.Add(boonArrayToList);
                list.Add(boonData);
            }
            return list;
        }
        /// <summary>
        /// Create the self buff generation table
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="listToUse"></param>
        /// <param name="tableId"></param>
        /// <param name="phaseIndex"></param>
        private List<BoonData> BuildBuffGenerationData(List<Boon> listToUse, int phaseIndex, string target)
        {
            List<BoonData> list = new List<BoonData>();

            foreach (Player player in _log.PlayerList)
            {
                BoonData boonData = new BoonData();

                Dictionary<long, Statistics.FinalBoonUptime> uptimes;
                if (target == "self") uptimes = _statistics.SelfBoons[player][phaseIndex];
                else if (target == "group") uptimes = _statistics.GroupBoons[player][phaseIndex];
                else if (target == "off") uptimes = _statistics.OffGroupBoons[player][phaseIndex];
                else if (target == "squad") uptimes = _statistics.SquadBoons[player][phaseIndex];
                else throw new InvalidOperationException("unknown target type");

                Dictionary<long, string> rates = new Dictionary<long, string>();
                foreach (Boon boon in listToUse)
                {
                    Statistics.FinalBoonUptime uptime = uptimes[boon.ID];
                    List<object> val = new List<object>(2)
                    {
                        uptime.Generation,
                        uptime.Overstack
                    };
                    boonData.val.Add(val);
                }
                list.Add(boonData);
            }
            return list;
        }

        /// <summary>
        /// Creates the rotation tab for a given player
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="p"></param>
        /// <param name="simpleRotSize"></param>
        /// <param name="phaseIndex"></param>
        private List<double[]> BuildSimpleRotationTabData(AbstractPlayer p, int phaseIndex, Dictionary<long, SkillItem> usedSkills)
        {
            List<double[]> list = new List<double[]>();

            PhaseData phase = _statistics.Phases[phaseIndex];
            List<CastLog> casting = p.GetCastLogs(_log, phase.Start, phase.End);
            SkillData skillList = _log.SkillData;
            foreach (CastLog cl in casting)
            {
                if (!usedSkills.ContainsKey(cl.SkillId)) usedSkills.Add(cl.SkillId, skillList.GetOrDummy(cl.SkillId));
                double[] rotEntry = new double[5];
                list.Add(rotEntry);
                rotEntry[0] = (cl.Time - phase.Start) / 1000.0;
                rotEntry[1] = cl.SkillId;
                rotEntry[2] = cl.ActualDuration;
                rotEntry[3] = EncodeEndActivation(cl.EndActivation);
                rotEntry[4] = cl.StartActivation == ParseEnum.Activation.Quickness ? 1 : 0;
            }
            return list;
        }

        private int EncodeEndActivation(ParseEnum.Activation endActivation)
        {
            switch (endActivation)
            {
                case ParseEnum.Activation.CancelFire: return 1;
                case ParseEnum.Activation.CancelCancel: return 2;
                case ParseEnum.Activation.Reset: return 3;
                default: return 0;
            }
        }

        /// <summary>
        /// Creates the death recap tab for a given player
        /// </summary>
        /// <param name="sw">Stream writer</param>
        /// <param name="p">The player</param>
        private List<DeathRecapDto> BuildDeathRecap(Player p)
        {
            List<DeathRecapDto> res = new List<DeathRecapDto>();
            long start = _log.FightData.FightStart;
            long end = _log.FightData.FightEnd;
            List<CombatItem> deads = _log.CombatData.GetStates(p.InstID, ParseEnum.StateChange.ChangeDead, start, end);
            List<CombatItem> downs = _log.CombatData.GetStates(p.InstID, ParseEnum.StateChange.ChangeDown, start, end);
            long lastTime = start;
            List<DamageLog> damageLogs = p.GetDamageTakenLogs(_log, 0, _log.FightData.FightDuration);
            foreach (CombatItem dead in deads)
            {
                DeathRecapDto recap = new DeathRecapDto()
                {
                    time = (int)(dead.Time - start)
                };
                CombatItem down = downs.LastOrDefault(x => x.Time <= dead.Time && x.Time >= lastTime);
                if (down != null)
                {
                    List<DamageLog> damageToDown = damageLogs.Where(x => x.Time < down.Time - start && x.Damage > 0 && x.Time > lastTime - start).ToList();
                    recap.toDown = damageToDown.Count > 0 ? new List<object[]>() : null;
                    int damage = 0;
                    for (int i = damageToDown.Count - 1; i > 0; i--)
                    {
                        DamageLog dl = damageToDown[i];
                        AgentItem ag = _log.AgentData.GetAgentByInstID(dl.SrcInstId, dl.Time + start);
                        object[] item = new object[] {
                            dl.Time,
                            dl.SkillId,
                            dl.Damage,
                            ag != null ? ag.Name.Replace("\u0000", "").Split(':')[0] : ""
                        };
                        damage += dl.Damage;
                        recap.toDown.Add(item);
                        if (damage > 20000)
                        {
                            break;
                        }
                    }
                    List<DamageLog> damageToKill = damageLogs.Where(x => x.Time > down.Time - start && x.Time < dead.Time - start && x.Damage > 0 && x.Time > lastTime - start).ToList();
                    recap.toKill = damageToKill.Count > 0 ? new List<object[]>() : null;
                    for (int i = damageToKill.Count - 1; i > 0; i--)
                    {
                        DamageLog dl = damageToKill[i];
                        AgentItem ag = _log.AgentData.GetAgentByInstID(dl.SrcInstId, dl.Time + start);
                        object[] item = new object[] {
                            dl.Time,
                            dl.SkillId,
                            dl.Damage,
                            ag != null ? ag.Name.Replace("\u0000", "").Split(':')[0] : ""
                        };
                        recap.toKill.Add(item);
                    }
                }
                else
                {
                    recap.toDown = null;
                    List<DamageLog> damageToKill = damageLogs.Where(x => x.Time < dead.Time - start && x.Damage > 0 && x.Time > lastTime - start).ToList();
                    recap.toKill = damageToKill.Count > 0 ? new List<object[]>() : null;
                    int damage = 0;
                    for (int i = damageToKill.Count - 1; i > 0; i--)
                    {
                        DamageLog dl = damageToKill[i];
                        AgentItem ag = _log.AgentData.GetAgentByInstID(dl.SrcInstId, dl.Time + start);
                        object[] item = new object[] {
                            dl.Time,
                            dl.SkillId,
                            dl.Damage,
                            ag != null ? ag.Name.Replace("\u0000", "").Split(':')[0] : ""
                        };
                        damage += dl.Damage;
                        recap.toKill.Add(item);
                        if (damage > 20000)
                        {
                            break;
                        }
                    }
                }
                lastTime = dead.Time;
                res.Add(recap);
            }
            return res.Count > 0 ? res : null;
        }

        private List<double[]> BuildDMGDistBodyData(List<CastLog> casting, List<DamageLog> damageLogs, long finalTotalDamage,
            Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            List<double[]> list = new List<double[]>();
            Dictionary<long, List<CastLog>> castLogsBySkill = casting.GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<long, List<DamageLog>> damageLogsBySkill = damageLogs.GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<long, Boon> conditionsById = _statistics.PresentConditions.ToDictionary(x => x.ID);
            SkillData skillList = _log.SkillData;
            foreach (KeyValuePair<long, List<DamageLog>> entry in damageLogsBySkill)
            {
                int totaldamage = 0, mindamage = 0, maxdamage = 0, casts = 0, hits = 0, crit = 0, flank = 0, glance = 0, timeswasted = 0, timessaved = 0;
                foreach (DamageLog dl in entry.Value)
                {
                    int curdmg = dl.Damage;
                    totaldamage += curdmg;
                    if (0 == mindamage || curdmg < mindamage) { mindamage = curdmg; }
                    if (0 == maxdamage || curdmg > maxdamage) { maxdamage = curdmg; }
                    hits++;
                    if (dl.Result == ParseEnum.Result.Crit) crit++;
                    if (dl.Result == ParseEnum.Result.Glance) glance++;
                    if (dl.IsFlanking == 1) flank++;
                }

                bool isCondi = conditionsById.ContainsKey(entry.Key) || entry.Key == 873;
                if (isCondi)
                {
                    Boon condi = entry.Key == 873 ? Boon.BoonsByIds[873] : conditionsById[entry.Key];
                    if (!usedBoons.ContainsKey(condi.ID)) usedBoons.Add(condi.ID, condi);
                }
                else
                {
                    if (!usedSkills.ContainsKey(entry.Key)) usedSkills.Add(entry.Key, skillList.GetOrDummy(entry.Key));
                }

                if (!isCondi && castLogsBySkill.TryGetValue(entry.Key, out List<CastLog> clList))
                {

                    casts = clList.Count;
                    foreach (CastLog cl in clList)
                    {
                        if (cl.EndActivation == ParseEnum.Activation.CancelCancel) timeswasted += cl.ActualDuration;
                        if (cl.EndActivation == ParseEnum.Activation.CancelFire && cl.ActualDuration < cl.ExpectedDuration)
                        {
                            timessaved += cl.ExpectedDuration - cl.ActualDuration;
                        }
                    }
                }

                double[] skillData = {
                    isCondi?1:0,
                    entry.Key,
                    finalTotalDamage > 0 ? Math.Round(100.0 * totaldamage / finalTotalDamage,3) : 0,
                    totaldamage, mindamage, maxdamage,
                    casts, hits, crit, flank, glance,
                    timeswasted / 1000.0,
                    -timessaved / 1000.0};
                list.Add(skillData);
            }

            foreach (KeyValuePair<long, List<CastLog>> entry in castLogsBySkill)
            {
                if (damageLogsBySkill.ContainsKey(entry.Key)) continue;

                if (!usedSkills.ContainsKey(entry.Key)) usedSkills.Add(entry.Key, skillList.GetOrDummy(entry.Key));

                int casts = entry.Value.Count;
                int timeswasted = 0, timessaved = 0;
                foreach (CastLog cl in entry.Value)
                {
                    if (cl.EndActivation == ParseEnum.Activation.CancelCancel) timeswasted += cl.ActualDuration;
                    if (cl.EndActivation == ParseEnum.Activation.CancelFire && cl.ActualDuration < cl.ExpectedDuration)
                    {
                        timessaved += cl.ExpectedDuration - cl.ActualDuration;
                    }
                }

                double[] skillData = { 0, entry.Key, 0, 0, 0, 0, casts,
                    0, 0, 0, 0, timeswasted / 1000.0, -timessaved / 1000.0 };
                list.Add(skillData);
            }
            return list;
        }

        private DmgDistributionDto _BuildDMGDistData(Statistics.FinalDPS dps, AbstractMasterPlayer p, Boss boss, int phaseIndex,
            Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            DmgDistributionDto dto = new DmgDistributionDto();
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<CastLog> casting = p.GetCastLogs(_log, phase.Start, phase.End);
            List<DamageLog> damageLogs = p.GetJustPlayerDamageLogs(boss, _log, phase.Start, phase.End);
            int totalDamage = dps.Damage;
            dto.totalDamage = damageLogs.Count > 0 ? damageLogs.Sum(x => x.Damage) : 0;
            if (totalDamage > 0){
                dto.contribution = Math.Round(100.0 * dto.totalDamage / totalDamage,3);
            }

            dto.data = BuildDMGDistBodyData(casting, damageLogs, dto.totalDamage, usedSkills, usedBoons);

            return dto;
        }

        /// <summary>
        /// Creates the damage distribution table for a given player
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="p"></param>
        /// <param name="toBoss"></param>
        /// <param name="phaseIndex"></param>
        private DmgDistributionDto BuildPlayerDMGDistData(Player p, Boss boss, int phaseIndex,
            Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            Statistics.FinalDPS dps = boss != null ? _statistics.DpsBoss[boss][p][phaseIndex] : _statistics.DpsAll[p][phaseIndex];
            return _BuildDMGDistData(dps, p, boss, phaseIndex, usedSkills, usedBoons);
        }

        /// <summary>
        /// Creates the damage distribution table for a the boss
        /// </summary>
        private DmgDistributionDto BuildBossDMGDistData(Boss p, int phaseIndex, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            Statistics.FinalDPS dps = _statistics.BossDps[_log.Boss][phaseIndex];
            return _BuildDMGDistData(dps, p, null, phaseIndex, usedSkills, usedBoons);
        }

        private DmgDistributionDto _BuildDMGDistData(Statistics.FinalDPS dps, AbstractMasterPlayer p, Minions minions, Boss boss, int phaseIndex, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            DmgDistributionDto dto = new DmgDistributionDto();
            int totalDamage = dps.Damage;
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<CastLog> casting = minions.GetCastLogs(_log, phase.Start, phase.End);
            List<DamageLog> damageLogs = minions.GetDamageLogs(boss, _log, phase.Start, phase.End);
            int finalTotalDamage = damageLogs.Count > 0 ? damageLogs.Sum(x => x.Damage) : 0;

            if (totalDamage > 0)
            {
                dto.contribution = Math.Round(100.0 * finalTotalDamage / totalDamage, 2);
            }
            dto.data = BuildDMGDistBodyData(casting, damageLogs, finalTotalDamage, usedSkills, usedBoons);
            return dto;
        }

        /// <summary>
        /// Creates the damage distribution table for a given minion
        /// </summary>
        private DmgDistributionDto BuildPlayerMinionDMGDistData(Player p, Minions minions, Boss boss, int phaseIndex, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            Statistics.FinalDPS dps = boss != null ? _statistics.DpsBoss[boss][p][phaseIndex] : _statistics.DpsAll[p][phaseIndex];

            return _BuildDMGDistData(dps, p, minions, boss, phaseIndex, usedSkills, usedBoons);
        }

        /// <summary>
        /// Creates the damage distribution table for a given boss minion
        /// </summary>
        private DmgDistributionDto CreateBossMinionDMGDistTable(Boss p, Minions minions, int phaseIndex, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            Statistics.FinalDPS dps = _statistics.BossDps[_log.Boss][phaseIndex];
            return _BuildDMGDistData(dps, p, minions, null, phaseIndex, usedSkills, usedBoons);
        }

        /// <summary>
        /// Create the damage taken distribution table for a given player
        /// </summary>
        /// <param name="p"></param>
        /// <param name="phaseIndex"></param>
        private DmgDistributionDto BuildDMGTakenDistData(Player p, int phaseIndex, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            DmgDistributionDto dto = new DmgDistributionDto
            {
                data = new List<double[]>()
            };
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<DamageLog> damageLogs = p.GetDamageTakenLogs(_log, phase.Start, phase.End);
            SkillData skillList = _log.SkillData;
            dto.totalDamage = damageLogs.Count > 0 ? damageLogs.Sum(x => (long)x.Damage) : 0;

            HashSet<long> usedIDs = new HashSet<long>();
            List<Boon> condiRetal = new List<Boon>(_statistics.PresentConditions)
            {
                Boon.BoonsByIds[873]
            };
            foreach (Boon condi in condiRetal)
            {
                long condiID = condi.ID;
                int totaldamage = 0;
                int mindamage = 0;
                int hits = 0;
                int maxdamage = 0;
                usedIDs.Add(condiID);
                foreach (DamageLog dl in damageLogs.Where(x => x.SkillId == condiID))
                {
                    int curdmg = dl.Damage;
                    totaldamage += curdmg;
                    if (0 == mindamage || curdmg < mindamage) { mindamage = curdmg; }
                    if (0 == maxdamage || curdmg > maxdamage) { maxdamage = curdmg; }
                    hits++;

                }
                int avgdamage = (int)(totaldamage / (double)hits);
                if (totaldamage > 0)
                {
                    double[] row = new double[13] {
                        1, // isCondi
                        condi.ID,
                        Math.Round(100 * (double)totaldamage / dto.totalDamage, 2),
                        totaldamage,
                        mindamage, maxdamage,
                        hits, hits,
                        0, 0, 0, 0, 0 //crit, flank, glance, timeswasted, timessaved
                    };
                    dto.data.Add(row);
                    if (!usedBoons.ContainsKey(condi.ID)) usedBoons.Add(condi.ID, condi);
                }
            }

            foreach (int id in damageLogs.Where(x => !usedIDs.Contains(x.SkillId)).Select(x => (int)x.SkillId).Distinct())
            {//foreach casted skill
                SkillItem skill = skillList.Get(id);

                if (skill != null)
                {
                    if (!usedSkills.ContainsKey(id)) usedSkills.Add(id, skill);

                    int totaldamage = 0;
                    int mindamage = 0;
                    int hits = 0;
                    int maxdamage = 0;
                    int crit = 0;
                    int flank = 0;
                    int glance = 0;
                    foreach (DamageLog dl in damageLogs.Where(x => x.SkillId == id))
                    {
                        int curdmg = dl.Damage;
                        totaldamage += curdmg;
                        if (0 == mindamage || curdmg < mindamage) { mindamage = curdmg; }
                        if (0 == maxdamage || curdmg > maxdamage) { maxdamage = curdmg; }
                        if (curdmg >= 0) { hits++; };
                        ParseEnum.Result result = dl.Result;
                        if (result == ParseEnum.Result.Crit) { crit++; } else if (result == ParseEnum.Result.Glance) { glance++; }
                        if (dl.IsFlanking == 1) { flank++; }
                    }

                    double[] row = new double[13] {
                        0, // isCondi
                        skill.ID,
                        Math.Round(100 * (double)totaldamage / dto.totalDamage, 2),
                        totaldamage,
                        mindamage, maxdamage,
                        hits, hits,
                        crit, flank, glance,
                        0, 0
                    };
                    dto.data.Add(row);
                }
            }

            return dto;
        }

        private List<BoonChartDataDto> BuildPlayerBoonGraphData(AbstractMasterPlayer p, int phaseIndex)
        {
            List<BoonChartDataDto> list = new List<BoonChartDataDto>();
            PhaseData phase = _statistics.Phases[phaseIndex];
            if (_statistics.PresentBoons.Count > 0)
            {
                Dictionary<long, BoonsGraphModel> boonGraphData = p.GetBoonGraphs(_log);
                foreach (BoonsGraphModel bgm in boonGraphData.Values.Reverse())
                {
                    BoonChartDataDto graph = BuildPlayerTabBoonGraph(bgm, phase);
                    if (graph != null) list.Add(graph);
                }
                boonGraphData = _log.Boss.GetBoonGraphs(_log);
                //TODO add to used boon list?
                foreach (BoonsGraphModel bgm in boonGraphData.Values.Reverse().Where(x => x.BoonName == "Compromised" || x.BoonName == "Unnatural Signet" || x.BoonName == "Fractured - Enemy"))
                {
                    BoonChartDataDto graph = BuildPlayerTabBoonGraph(bgm, phase);
                    if (graph != null) list.Add(graph);
                }
            }
            return list;
        }

        private BoonChartDataDto BuildPlayerTabBoonGraph(BoonsGraphModel bgm, PhaseData phase)
        {
            //TODO line: {shape: 'hv'}
            long roundedEnd = phase.Start + 1000*phase.GetDuration("s");
            List<BoonsGraphModel.Segment> bChart = bgm.BoonChart.Where(x => x.End >= phase.Start && x.Start <= roundedEnd).ToList();
            if (bChart.Count == 0 || (bChart.Count == 1 && bChart.First().Value == 0))
            {
                return null;
            }
            BoonChartDataDto dto = new BoonChartDataDto
            {
                name = bgm.BoonName,
                visible = bgm.BoonName == "Might" || bgm.BoonName == "Quickness",
                color = HTMLHelper.GetLink("Color-" + bgm.BoonName),
                data = new List<double[]>(bChart.Count + 1)
            };

            foreach (BoonsGraphModel.Segment seg in bChart)
            {
                double segStart = Math.Round(Math.Max(seg.Start - phase.Start, 0) / 1000.0, 3);
                dto.data.Add(new double[] { segStart, seg.Value });
            }
            BoonsGraphModel.Segment lastSeg = bChart.Last();
            double segEnd = Math.Round(Math.Min(lastSeg.End - phase.Start, roundedEnd - phase.Start) / 1000.0, 3);
            dto.data.Add(new double[] { segEnd, lastSeg.Value });

            return dto;
        }

        private List<FoodDto> BuildPlayerFoodData(Player p, int phaseIndex)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<FoodDto> list = new List<FoodDto>();
            List<Tuple<Boon, long, int>> consume = p.GetConsumablesList(_log, phase.Start, phase.End);

            foreach(Tuple<Boon, long, int> entry in consume)
            {
                FoodDto dto = new FoodDto
                {
                    time = entry.Item2 / 1000.0,
                    duration = entry.Item3 / 1000.0,
                    name = entry.Item1.Name,
                    icon = entry.Item1.Link,
                    dimished = entry.Item1.ID == 46587 || entry.Item1.ID == 46668
                };
                list.Add(dto);
            }

            return list;
        }

        /// <summary>
        /// Creates the mechanics table of the fight
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<int[]>> BuildPlayerMechanicData(int phaseIndex)
        {
            List<List<int[]>> list = new List<List<int[]>>();
            HashSet<Mechanic> presMech = _log.MechanicData.GetPresentMechanics(0);
            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player p in _log.PlayerList)
            {
                List<int[]> playerData = new List<int[]>(presMech.Count);
                foreach (Mechanic mech in presMech)
                {
                    long timeFilter = 0;
                    int filterCount = 0;
                    List<MechanicLog> mls = _log.MechanicData[mech].Where(x => x.Player.InstID == p.InstID && phase.InInterval(x.Time)).ToList();
                    int count = mls.Count;
                    foreach (MechanicLog ml in mls)
                    {
                        if (mech.InternalCooldown != 0 && ml.Time - timeFilter < mech.SkillId)//ICD check
                        {
                            filterCount++;
                        }
                        timeFilter = ml.Time;

                    }
                    int[] mechEntry = {count - filterCount,count};
                    playerData.Add(mechEntry);
                }
                list.Add(playerData);
            }
            return list;
        }

        private List<List<int[]>> BuildEnemyMechanicData(int phaseIndex)
        {
            List<List<int[]>> list = new List<List<int[]>>();
            HashSet<Mechanic> presMech = _log.MechanicData.GetPresentMechanics(0);
            PhaseData phase = _statistics.Phases[phaseIndex];
            foreach (AbstractMasterPlayer p in _log.MechanicData.GetEnemyList(0))
            {
                List<int[]> enemyData = new List<int[]>(presMech.Count);
                foreach (Mechanic mech in presMech)
                {
                    long timeFilter = 0;
                    int filterCount = 0;
                    List<MechanicLog> mls = _log.MechanicData[mech].Where(x => x.Player.InstID == p.InstID && phase.InInterval(x.Time)).ToList();
                    int count = mls.Count;
                    foreach (MechanicLog ml in mls)
                    {
                        if (mech.InternalCooldown != 0 && ml.Time - timeFilter < mech.SkillId)//ICD check
                        {
                            filterCount++;
                        }
                        timeFilter = ml.Time;

                    }
                    enemyData.Add(new int[] { count - filterCount, count });
                }
                list.Add(enemyData);
            }
            return list;
        }
        
        private List<MechanicDto> BuildMechanicGraphData()
        {
            List<MechanicDto> mechanicDtos = new List<MechanicDto>();
            HashSet<Mechanic> playerMechs = _log.MechanicData.GetPresentPlayerMechs(0);
            HashSet<Mechanic> enemyMechs = _log.MechanicData.GetPresentEnemyMechs(0);
            foreach (Mechanic mech in _log.MechanicData.GetPresentMechanics(0))
            {
                List<MechanicLog> mechanicLogs = _log.MechanicData[mech];
                MechanicDto dto = new MechanicDto
                {
                    name = mech.PlotlyName,
                    shortName = mech.ShortName,
                    description = mech.Description,
                    color = mech.PlotlyColor,
                    symbol = mech.PlotlySymbol,
                    visible = (mech.SkillId == -2 || mech.SkillId == -3),
                    data = BuildMechanicData(mechanicLogs),
                    playerMech = playerMechs.Contains(mech),
                    enemyMech = enemyMechs.Contains(mech)
                };
                mechanicDtos.Add(dto);
            }
            return mechanicDtos;
        }

        private List<List<List<double>>> BuildMechanicData(List<MechanicLog> mechanicLogs)
        {
            List<List<List<double>>> list = new List<List<List<double>>>();
            foreach (PhaseData phase in _statistics.Phases)
            {
                List<List<double>> phaseData = new List<List<double>>();
                list.Add(phaseData);
                Dictionary<long, int> playerIndexByInstId = new Dictionary<long, int>();
                for (var p = 0; p < _log.PlayerList.Count; p++)
                {
                    playerIndexByInstId.Add(_log.PlayerList[p].InstID, p);
                    phaseData.Add(new List<double>());
                }
                playerIndexByInstId.Add(_log.Boss.InstID, _log.PlayerList.Count);
                phaseData.Add(new List<double>());
                foreach (MechanicLog ml in mechanicLogs.Where(x => phase.InInterval(x.Time)))
                {
                    double time = (ml.Time - phase.Start) / 1000.0;
                    if (playerIndexByInstId.TryGetValue(ml.Player.InstID, out int p))
                    {
                        phaseData[p].Add(time);
                    } else
                    {
                        phaseData[phaseData.Count - 1].Add(time);
                    }
                }
            }
            return list;
        }

        private List<BoonData> BuildBossCondiData(int phaseIndex, Boss target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[target][phaseIndex];
            List<BoonData> list = new List<BoonData>();

            foreach (Player player in _log.PlayerList)
            {
                BoonData playerData = new BoonData
                {
                    val = new List<List<object>>()
                };

                foreach (Boon boon in _statistics.PresentConditions)
                {
                    List<object> boonData = new List<object>();
                    Statistics.FinalBossBoon toUse = conditions[boon.ID];
                    boonData.Add(toUse.Generated[player]);
                    boonData.Add(toUse.Overstacked[player]);
                    playerData.val.Add(boonData);
                }
                list.Add(playerData);
            }
            return list;
        }

        private BoonData BuildBossCondiUptimeData(int phaseIndex, Boss target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[target][phaseIndex];
            Dictionary<long, long> condiPresence = target.GetCondiPresence(_log, phaseIndex);
            long fightDuration = phase.GetDuration();
            BoonData bossData = new BoonData
            {
                val = new List<List<object>>()
            };
            bossData.avg = Math.Round(_statistics.AvgBossConditions[target][phaseIndex], 1);
            foreach (Boon boon in _statistics.PresentConditions)
            {
                List<object> boonData = new List<object>
                {
                    conditions[boon.ID].Uptime
                };

                if (boon.Type != Boon.BoonType.Duration && condiPresence.TryGetValue(boon.ID, out long presenceTime))
                {
                    boonData.Add(Math.Round(100.0 * presenceTime / fightDuration, 1));
                }

                bossData.val.Add(boonData);
            }
            return bossData;
        }

        private BoonData BuildBossBoonData(int phaseIndex, Boss target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[target][phaseIndex];
            long fightDuration = phase.GetDuration();
            BoonData bossData = new BoonData
            {
                val = new List<List<object>>()
            };
            bossData.avg = Math.Round(_statistics.AvgBossBoons[target][phaseIndex], 1);
            foreach (Boon boon in _statistics.PresentBoons)
            {
                List<object> boonData = new List<object>
                {
                    conditions[boon.ID].Uptime
                };

                if (boon.Type != Boon.BoonType.Duration && conditions[boon.ID].Presence > 0)
                {
                    boonData.Add(conditions[boon.ID].Presence);
                }

                bossData.val.Add(boonData);
            }
            return bossData;
        }

        private void CreateEstimateTabs(StreamWriter sw, int phaseIndex)
        {
            sw.Write("<ul class=\"nav nav-tabs\">");
            {
                sw.Write("<li class=\"nav-item\">" +
                            "<a class=\"nav-link\" data-toggle=\"tab\" href=\"#est_role" + phaseIndex + "\">Roles</a>" +
                        "</li>" +

                        "<li class=\"nav-item\">" +
                            "<a class=\"nav-link\" data-toggle=\"tab\" href=\"#est_cc" + phaseIndex + "\">CC</a>" +
                        "</li>" +
                         "<li class=\"nav-item\">" +
                            "<a class=\"nav-link\" data-toggle=\"tab\" href=\"#est" + phaseIndex + "\">Maybe more</a>" +
                        "</li>");
            }
            sw.Write("</ul>");
            sw.Write("<div id=\"myTabContent\" class=\"tab-content\">");
            {
                sw.Write("<div class=\"tab-pane fade show active\" id=\"est_role" + phaseIndex + "\">");
                {
                    //Use cards
                }
                sw.Write("</div>");
                sw.Write("<div class=\"tab-pane fade show active\" id=\"est_cc" + phaseIndex + "\">");
                {
                }
                sw.Write("</div>");
                sw.Write("<div class=\"tab-pane fade show active\" id=\"est" + phaseIndex + "\">");
                {
                }
                sw.Write("</div>");
            }
            sw.Write("</div>");
        }
        /// <summary>
        /// Creates the combat replay tab
        /// </summary>
        /// <param name="sw">Stream writer</param>
        private void CreateReplayTable(StreamWriter sw)
        {
            CombatReplayMap map = _log.FightData.Logic.GetCombatMap();
            Tuple<int, int> canvasSize = map.GetPixelMapSize();
            HTMLHelper.WriteCombatReplayInterface(sw, canvasSize, _log);
            HTMLHelper.WriteCombatReplayScript(sw, _log, canvasSize, map, _settings.PollingRate);
        }

        private string ReplaceVariables(string html)
        {
            double fightDuration = _log.FightData.FightDuration / 1000.0;
            TimeSpan duration = TimeSpan.FromSeconds(fightDuration);
            string durationString = duration.Minutes + "m " + duration.Seconds + "s " + duration.Milliseconds + "ms";
            if (duration.Hours > 0)
            {
                durationString = duration.Hours + "h " + durationString;
            }

            int encounterPercent = 0;
            double healthLeft = 100;
            
            if (_log.LogData.Success)
            {
                encounterPercent = 100;
                healthLeft = 0;
            }
            else
            {
                if (_log.Boss.HealthOverTime.Count > 0)
                {
                    healthLeft = Math.Round(_log.Boss.HealthOverTime[_log.Boss.HealthOverTime.Count - 1].Y * 0.01, 2);
                    encounterPercent = (int)Math.Floor(100.0 - _log.Boss.HealthOverTime[_log.Boss.HealthOverTime.Count - 1].Y * 0.01);
                }
            }

            html = html.Replace("${bootstrapTheme}", !_settings.LightTheme ? "slate" : "cosmo");

            html = html.Replace("${encounterStart}", _log.LogData.LogStart);
            html = html.Replace("${encounterEnd}", _log.LogData.LogEnd);
            html = html.Replace("${encounterDuration}", durationString);
            html = html.Replace("${encounterResult}", _log.LogData.Success ? "Success": "Fail");
            html = html.Replace("${encounterResultCss}", _log.LogData.Success ? "text-success" : "text-warning");
            html = html.Replace("${encounterPercent}", encounterPercent.ToString());
            html = html.Replace("${evtcVersion}", _log.LogData.BuildVersion);
            html = html.Replace("${bossID}", _log.FightData.ID.ToString());
            html = html.Replace("${fightName}", FilterStringChars(_log.FightData.Name));
            html = html.Replace("${bossHealth}", _log.Boss.Health.ToString());
            html = html.Replace("${bossHealthLeft}", healthLeft.ToString());
            html = html.Replace("${fightIcon}", _log.FightData.Logic.IconUrl);
            html = html.Replace("${eiVersion}", Application.ProductVersion);
            html = html.Replace("${recordedBy}", _log.LogData.PoV.Split(':')[0]);

            return html;
        }

        /// <summary>
        /// Creates the whole html
        /// </summary>
        /// <param name="sw">Stream writer</param>
        public void CreateHTML(StreamWriter sw, string path)
        {
            string html = Properties.Resources.template_html;
            html = ReplaceVariables(html);

            html = html.Replace("<!--${flomixCss}-->", BuildFlomixCss(path));
            html = html.Replace("<!--${flomixJs}-->", BuildFlomixJs(path));

            html = html.Replace("${logDataJson}", BuildLogData());

            html = html.Replace("<!--${playerData}-->", BuildDetails());

            html = html.Replace("${graphDataJson}", BuildGraphJson());

            html = html.Replace("<!--${combatReplay}-->", BuildCombatReplayContent());

            sw.Write(html);
            return;       
        }

        private string BuildCombatReplayContent()
        {
            if (!_settings.ParseCombatReplay || !_log.FightData.Logic.CanCombatReplay)
            {
                return "";
            }
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write("<div id=\"replay_template\">");
                    CreateReplayTable(sw);
                    sw.Write("</div>");
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private string BuildFlomixCss(string path)
        {
            string scriptContent = Properties.Resources.flomix_ei_css;
            string cssFilename = "flomix-ei-" + _scriptVersion + ".css";
            if (Properties.Settings.Default.NewHtmlExternalScripts)
            {
                string cssPath = Path.Combine(path, cssFilename);
                using (var fs = new FileStream(cssPath, FileMode.Create, FileAccess.Write))
                using (var scriptWriter = new StreamWriter(fs))
                {
                    scriptWriter.Write(scriptContent);
                }
                return "<link rel=\"stylesheet\" type=\"text/css\" href=\"./"+ cssFilename + "?version="+_scriptVersionRev+"\">";
            }
            else
            {
                return "<style type=\"text/css\">\r\n" + scriptContent + "\r\n</style>";
            }
        }

        private string BuildFlomixJs(string path)
        {
            string scriptContent = BuildJavascript();
            string scriptFilename = "flomix-ei-" + _scriptVersion + ".js";
            if (Properties.Settings.Default.NewHtmlExternalScripts)
            {
                string scriptPath = Path.Combine(path, scriptFilename);
                using (var fs = new FileStream(scriptPath, FileMode.Create, FileAccess.Write))
                using (var scriptWriter = new StreamWriter(fs))
                {
                    scriptWriter.Write(scriptContent);
                }
                return "<script src=\"./" + scriptFilename + "?version=" + _scriptVersionRev + "\"></script>";
            }
            else
            {
                return "<script>\r\n" + scriptContent + "\r\n</script>";
            }
        }

        private string BuildGraphJson()
        {
            List<PhaseChartDataDto> chartData = new List<PhaseChartDataDto>();
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                PhaseChartDataDto phaseData = new PhaseChartDataDto()
                {
                    players = BuildPlayerGraphData(i)
                };
                foreach(Boss target in _statistics.Phases[i].Targets)
                {
                    phaseData.bosses.Add(BuildBossGraphData(i, target));
                }

                chartData.Add(phaseData);
             }
            return ToJson(chartData, typeof(List<PhaseChartDataDto>));
        }

        private string BuildLogData()
        {
            LogDataDto data = new LogDataDto();
            foreach(Player player in _log.PlayerList)
            {
                PlayerDto playerDto = new PlayerDto(
                    player.Group,
                    player.Character,
                    player.Account.TrimStart(':'),
                    player.Prof)
                {
                    condi = player.Condition,
                    conc = player.Concentration,
                    heal = player.Healing,
                    tough = player.Toughness,
                    weapons = player.GetWeaponsArray(_log),
                    colBoss = HTMLHelper.GetLink("Color-" + player.Prof),
                    colCleave = HTMLHelper.GetLink("Color-" + player.Prof + "-NonBoss"),
                    colTotal = HTMLHelper.GetLink("Color-" + player.Prof + "-Total"),
                    isConjure = player.Account == ":Conjured Sword",
                    deathRecap = BuildDeathRecap(player)
                };

                foreach (KeyValuePair<string, Minions> pair in player.GetMinions(_log))
                {
                    playerDto.minions.Add(new MinionDto(pair.Value.MinionID, pair.Key.TrimEnd(" \0".ToArray())));
                }

                data.players.Add(playerDto);
            }

            foreach(AbstractMasterPlayer enemy in _log.MechanicData.GetEnemyList(0))
            {
                data.enemies.Add(new EnemyDto(enemy.Character));
            }

            foreach (Boss target in _log.FightData.Logic.Targets)
            {
                BossDto tar = new BossDto(target.ID, target.Character, GeneralHelper.GetNPCIcon(target.ID))
                {
                    health = target.Health,
                    hbHeight = target.HitboxHeight,
                    hbWidth = target.HitboxWidth
                };
                foreach (KeyValuePair<string, Minions> pair in target.GetMinions(_log))
                {
                    tar.minions.Add(new MinionDto(pair.Value.MinionID, pair.Key.TrimEnd(" \0".ToArray())));
                }
                data.bosses.Add(tar);
            }

            data.flags.simpleRotation = _settings.SimpleRotation;
            data.flags.dark = !_settings.LightTheme;
            data.flags.combatReplay = _settings.ParseCombatReplay && _log.FightData.Logic.CanCombatReplay;

            data.graphs.Add(new GraphDto("full", "Full"));
            data.graphs.Add(new GraphDto("s10", "10s"));
            data.graphs.Add(new GraphDto("s30", "30s"));
            data.graphs.Add(new GraphDto("phase", "Phase"));

            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                PhaseData phaseData = _statistics.Phases[i];
                PhaseDto phaseDto = new PhaseDto(phaseData.Name, phaseData.GetDuration("s"));
                phaseDto.start = phaseData.Start / 1000.0;
                phaseDto.end = phaseData.End / 1000.0;
                foreach (Boss target in phaseData.Targets)
                {
                    phaseDto.bosses.Add(_log.FightData.Logic.Targets.IndexOf(target));
                }
                data.phases.Add(phaseDto);
                phaseDto.dpsStats = BuildDPSData(i);
                phaseDto.dmgStatsBoss = BuildDMGStatsBossData(i);
                phaseDto.dmgStats = BuildDMGStatsData(i);
                phaseDto.defStats = BuildDefenseData(i);
                phaseDto.healStats = BuildSupportData(i);

                phaseDto.boonStats = BuildBuffUptimeData(_statistics.PresentBoons, i);
                phaseDto.offBuffStats = BuildBuffUptimeData(_statistics.PresentOffbuffs, i);
                phaseDto.defBuffStats = BuildBuffUptimeData(_statistics.PresentDefbuffs, i);

                phaseDto.boonGenSelfStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "self");
                phaseDto.boonGenGroupStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "group");
                phaseDto.boonGenOGroupStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "off");
                phaseDto.boonGenSquadStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "squad");

                phaseDto.offBuffGenSelfStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "self");
                phaseDto.offBuffGenGroupStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "group");
                phaseDto.offBuffGenOGroupStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "off");
                phaseDto.offBuffGenSquadStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "squad");

                phaseDto.defBuffGenSelfStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "self");
                phaseDto.defBuffGenGroupStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "group");
                phaseDto.defBuffGenOGroupStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "off");
                phaseDto.defBuffGenSquadStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "squad");

                phaseDto.bossCondiStats = new List<List<BoonData>>();
                phaseDto.bossCondiTotals = new List<BoonData>();
                phaseDto.bossBoonTotals = new List<BoonData>();
                phaseDto.bossHasBoons = new List<bool>();
                foreach (Boss target in phaseData.Targets)
                {
                    phaseDto.bossCondiStats.Add(BuildBossCondiData(i, target));
                    phaseDto.bossCondiTotals.Add(BuildBossCondiUptimeData(i, target));
                    phaseDto.bossBoonTotals.Add(BuildBossBoonData(i, target));
                    phaseDto.bossHasBoons.Add(HasBoons(i, target));
                }

                phaseDto.mechanicStats = BuildPlayerMechanicData(i);
                phaseDto.enemyMechanicStats = BuildEnemyMechanicData(i);

                phaseDto.deaths = new List<long>();

                foreach (Player player in _log.PlayerList)
                {
                    phaseDto.deaths.Add(player.GetDeath(_log, phaseData.Start, phaseData.End));
                }

                // add phase markup to full fight graph
                if (i == 0)
                {
                    phaseDto.markupLines = new List<double>();
                    phaseDto.markupAreas = new List<AreaLabelDto>();
                    for(int j = 1; j < _statistics.Phases.Count; j++)
                    {
                        PhaseData curPhase = _statistics.Phases[j];
                        if (curPhase.DrawStart) phaseDto.markupLines.Add(curPhase.Start/1000.0);
                        if (curPhase.DrawEnd) phaseDto.markupLines.Add(curPhase.End / 1000.0);
                        AreaLabelDto phaseArea = new AreaLabelDto
                        {
                            start = curPhase.Start / 1000.0,
                            end = curPhase.End / 1000.0,
                            label = curPhase.Name,
                            highlight = curPhase.DrawArea
                        };
                        phaseDto.markupAreas.Add(phaseArea);
                    }
                }
            }


            data.boons = AssembleBoons(_statistics.PresentBoons);
            data.offBuffs = AssembleBoons(_statistics.PresentOffbuffs);
            data.defBuffs = AssembleBoons(_statistics.PresentDefbuffs);
            data.mechanics = BuildMechanicGraphData();

            data.bossCondis = AssembleBoons(_statistics.PresentConditions);
            data.bossBoons = AssembleBoons(_statistics.PresentBoons);

            return ToJson(data, typeof(LogDataDto));
        }

        private bool HasBoons(int phaseIndex, Boss target)
        {
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[target][phaseIndex];
            foreach (Boon boon in _statistics.PresentBoons)
            {
                if (conditions[boon.ID].Uptime > 0.0)
                {
                    return true;
                }
            }
            return false;
        }

        private string BuildDetails()
        {
            Dictionary<long, SkillItem> usedSkills = new Dictionary<long, SkillItem>();
            Dictionary<long, Boon> usedBoons = new Dictionary<long, Boon>();
            string scripts = "";
            for (var i = 0; i < _log.PlayerList.Count; i++) {
                Player player = _log.PlayerList[i];
                string playerScript = "data.players[" + i + "].details = " + ToJson(BuildPlayerData(player, usedSkills, usedBoons), typeof(PlayerDetailsDto)) + ";\r\n";
                scripts += playerScript;
            }
            for (int i = 0; i < _log.FightData.Logic.Targets.Count; i++)
            {
                Boss boss = _log.FightData.Logic.Targets[i];
                string bossScript = "data.bosses[" + i + "].details = " + ToJson(BuildBossData(boss, usedSkills, usedBoons), typeof(PlayerDetailsDto)) + ";\r\n";
                scripts += bossScript;
            }
            string skillsScript = "var usedSkills = " + ToJson(AssembleSkills(usedSkills.Values), typeof(ICollection<SkillDto>)) + ";" +
                "data.skillMap = {};" +
                "$.each(usedSkills, function(i, skill) {" +
                "data.skillMap['s'+skill.id]=skill;" +
                "});";
            string boonsScript = "var usedBoons = " + ToJson(AssembleBoons(usedBoons.Values), typeof(ICollection<BoonDto>)) + ";" +
                "data.boonMap = {};" +
                "$.each(usedBoons, function(i, boon) {" +
                "data.boonMap['b'+boon.id]=boon;" +
                "});";
            return "<script>\r\n"+ skillsScript+"\r\n"+boonsScript+"\r\n"+scripts + "\r\n</script>";
        }

        private PlayerDetailsDto BuildPlayerData(Player player, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                dmgDistributions = new List<DmgDistributionDto>(),
                dmgDistributionsBosses = new List<List<DmgDistributionDto>>(),
                dmgDistributionsTaken = new List<DmgDistributionDto>(),
                boonGraph = new List<List<BoonChartDataDto>>(),
                rotation = new List<List<double[]>>(),
                food = new List<List<FoodDto>>(),
                minions = new List<PlayerDetailsDto>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                dto.rotation.Add(BuildSimpleRotationTabData(player, i, usedSkills));
                dto.dmgDistributions.Add(BuildPlayerDMGDistData(player, null, i, usedSkills, usedBoons));
                List<DmgDistributionDto> dmgBossesDto = new List<DmgDistributionDto>();
                foreach (Boss target in _statistics.Phases[i].Targets)
                {
                    dmgBossesDto.Add(BuildPlayerDMGDistData(player, target, i, usedSkills, usedBoons));
                }
                dto.dmgDistributionsBosses.Add(dmgBossesDto);
                dto.dmgDistributionsTaken.Add(BuildDMGTakenDistData(player, i, usedSkills, usedBoons));
                dto.boonGraph.Add(BuildPlayerBoonGraphData(player, i));
                dto.food.Add(BuildPlayerFoodData(player, i));
            }
            foreach (KeyValuePair<string, Minions> pair in player.GetMinions(_log))
            {
                dto.minions.Add(BuildPlayerMinionsData(player, pair.Value, usedSkills, usedBoons));
            }

            return dto;
        }

        private PlayerDetailsDto BuildPlayerMinionsData(Player player, Minions minion, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                dmgDistributions = new List<DmgDistributionDto>(),
                dmgDistributionsBosses = new List<List<DmgDistributionDto>>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                List<DmgDistributionDto> dmgBossesDto = new List<DmgDistributionDto>();
                foreach (Boss target in _statistics.Phases[i].Targets)
                {
                    dmgBossesDto.Add(BuildPlayerMinionDMGDistData(player, minion, target, i, usedSkills, usedBoons));
                }
                dto.dmgDistributionsBosses.Add(dmgBossesDto);
                dto.dmgDistributions.Add(BuildPlayerMinionDMGDistData(player, minion, null, i, usedSkills, usedBoons));
            }
            return dto;
        }

        private PlayerDetailsDto BuildBossData(Boss boss, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                dmgDistributions = new List<DmgDistributionDto>(),
                boonGraph = new List<List<BoonChartDataDto>>(),
                rotation = new List<List<double[]>>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                if (_statistics.Phases[i].Targets.Contains(boss))
                {
                    dto.dmgDistributions.Add(BuildBossDMGDistData(boss, i, usedSkills, usedBoons));
                    dto.rotation.Add(BuildSimpleRotationTabData(boss, i, usedSkills));
                    dto.boonGraph.Add(BuildPlayerBoonGraphData(boss, i));
                } else
                {
                    dto.dmgDistributions.Add(new DmgDistributionDto());
                    dto.rotation.Add(new List<double[]>());
                    dto.boonGraph.Add(new List<BoonChartDataDto>());
                }
            }

            dto.minions = new List<PlayerDetailsDto>();
            foreach (KeyValuePair<string, Minions> pair in boss.GetMinions(_log))
            {
                dto.minions.Add(BuildBossMinionsData(boss, pair.Value, usedSkills, usedBoons));
            }
            return dto;
        }

        private PlayerDetailsDto BuildBossMinionsData(Boss boss, Minions minion, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Boon> usedBoons)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                dmgDistributions = new List<DmgDistributionDto>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                if (_statistics.Phases[i].Targets.Contains(boss))
                {
                    dto.dmgDistributions.Add(CreateBossMinionDMGDistTable(boss, minion, i, usedSkills, usedBoons));
                }
                else
                {
                    dto.dmgDistributions.Add(new DmgDistributionDto());
                }
            }
            return dto;
        }

        private List<BoonDto> AssembleBoons(ICollection<Boon> boons)
        {
            List<BoonDto> dtos = new List<BoonDto>();
            foreach (Boon boon in boons)
            {
                dtos.Add(new BoonDto(
                                boon.ID,
                                boon.Name,
                                boon.Link,
                                boon.Type == Boon.BoonType.Intensity
                                )
                        );
            }
            return dtos;
        }

        private List<SkillDto> AssembleSkills(ICollection<SkillItem> skills)
        {
            List<SkillDto> dtos = new List<SkillDto>();
            foreach (SkillItem skill in skills)
            {
                GW2APISkill apiSkill = skill.ApiSkill;
                SkillDto dto = new SkillDto(skill.ID, skill.Name, apiSkill?.icon, apiSkill?.slot == "Weapon_1");
                if (skill.ID == SkillItem.WeaponSwapId) dto.icon = "https://wiki.guildwars2.com/images/c/ce/Weapon_Swap_Button.png";
                else if (skill.ID == SkillItem.ResurrectId) dto.icon = "https://wiki.guildwars2.com/images/3/3d/Downed_ally.png";
                else if (skill.ID == SkillItem.BandageId) dto.icon = "https://wiki.guildwars2.com/images/0/0c/Bandage.png";
                else if (skill.ID == SkillItem.DodgeId) dto.icon = "https://wiki.guildwars2.com/images/b/b2/Dodge.png";
                dtos.Add(dto);
            }
            return dtos;
        }
 
        private string ToJson(object value, Type type)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            MemoryStream memoryStream = new MemoryStream();
            ser.WriteObject(memoryStream, value);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private string EscapeJsrender(string template)
        {
            // escape single quotation marks
            string escaped = template.Replace(@"\", @"\\");
            escaped = template.Replace("'", @"\'");
            // remove line breaks
            escaped = Regex.Replace(escaped, @"\s*\r?\n\s*", "");
            return escaped;
        }

        private string BuildTemplateJS(string name, string code)
        {
            return "\r\nvar "+ name + " = $.templates('"+name+"', '"+EscapeJsrender(code)+"');";
        }

        private string BuildJavascript()
        {
            string javascript = Properties.Resources.flomix_ei_js;
            javascript+= BuildTemplateJS("tmplTabs", Properties.Resources.tmplTabs);
            javascript += BuildTemplateJS("tmplPlayerCells", Properties.Resources.tmplPlayerCells);
            javascript += BuildTemplateJS("tmplDpsTable", Properties.Resources.tmplDpsTable);
            javascript += BuildTemplateJS("tmplBoonTable", Properties.Resources.tmplBoonTable);
            javascript += BuildTemplateJS("tmplSupTable", Properties.Resources.tmplSupTable);
            javascript += BuildTemplateJS("tmplDefTable", Properties.Resources.tmplDefTable);

            javascript += BuildTemplateJS("tmplDmgTable", Properties.Resources.tmplDmgTable);
            javascript += BuildTemplateJS("tmplDmgDistTable", Properties.Resources.tmplDmgDistTable);
            javascript += BuildTemplateJS("tmplDmgTakenTable", Properties.Resources.tmplDmgTakenTable);
            javascript += BuildTemplateJS("tmplMechanicTable", Properties.Resources.tmplMechanicTable);
            javascript += BuildTemplateJS("tmplCompTable", Properties.Resources.tmplCompTable);

            return javascript;
        }
    }
}
