﻿using LuckParser.Controllers;
using LuckParser.Parser;
using LuckParser.Models.HtmlModels;
using LuckParser.Models.ParseModels;
using Newtonsoft.Json;
using NUglify;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LuckParser.Setting;
using LuckParser.Models;
using Newtonsoft.Json.Serialization;

namespace LuckParser.Builders
{
    class HTMLBuilder
    {
        private string _scriptVersion;
        private int _scriptVersionRev;
        private readonly SettingsContainer _settings;

        private readonly ParsedLog _log;

        private readonly string[] _uploadLink;

        private readonly Statistics _statistics;
        private Dictionary<long, Boon> _usedBoons = new Dictionary<long, Boon>();
        private Dictionary<long, SkillItem> _usedSkills = new Dictionary<long, SkillItem>();

        public HTMLBuilder(ParsedLog log, SettingsContainer settings, Statistics statistics, string[] uploadString)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            _scriptVersion = version.Major + "." + version.Minor;
#if PROD
            _scriptVersion += "." + version.Build;
#endif
            _scriptVersionRev = version.Revision;
            _log = log;

            _settings = settings;
            CombatReplayHelper.Settings = settings;

            _statistics = statistics;

            _uploadLink = uploadString;
        }

        public static void UpdateStatisticSwitches(StatisticsCalculator.Switches switches)
        {
            switches.CalculateBoons = true;
            switches.CalculateDPS = true;
            switches.CalculateConditions = true;
            switches.CalculateDefense = true;
            switches.CalculateStats = true;
            switches.CalculateSupport = true;
            switches.CalculateCombatReplay = true;
            switches.CalculateMechanics = true;
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

        private TargetChartDataDto BuildTargetGraphData(int phaseIndex, Target target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            return new TargetChartDataDto
            {
                Total = target.Get1SDamageList(_log, phaseIndex, phase, null),
                Health = _statistics.TargetsHealth[phaseIndex][target]
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
                    Total = p.Get1SDamageList(_log, phaseIndex, phase, null),
                    Targets = new List<List<int>>()
                };
                foreach (Target target in phase.Targets)
                {
                    pChar.Targets.Add(p.Get1SDamageList(_log, phaseIndex, phase, target));
                }
                list.Add(pChar);
            }
            return list;
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

            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalDPS dpsAll = _statistics.DpsAll[player][phaseIndex];
                Statistics.FinalStatsAll stats = _statistics.StatsAll[player][phaseIndex];

                List<object> playerData = new List<object>
                {
                    dpsAll.Damage,
                    dpsAll.Dps,
                    dpsAll.PowerDamage,
                    dpsAll.PowerDps,
                    dpsAll.CondiDamage,
                    dpsAll.CondiDps
                };

                list.Add(playerData);
            }
            return list;
        }
        private List<List<List<object>>> BuildDPSTargetsData(int phaseIndex)
        {
            List<List<List<object>>> list = new List<List<List<object>>>(_log.PlayerList.Count);
            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player player in _log.PlayerList)
            {
                List<List<object>> playerData = new List<List<object>>();

                foreach (Target target in phase.Targets)
                {
                    List<object> tar = new List<object>();
                    playerData.Add(tar);
                    Statistics.FinalDPS dpsTarget = _statistics.DpsTarget[target][player][phaseIndex];
                    tar.Add(dpsTarget.Damage);
                    tar.Add(dpsTarget.Dps);
                    tar.Add(dpsTarget.PowerDamage);
                    tar.Add(dpsTarget.PowerDps);
                    tar.Add(dpsTarget.CondiDamage);
                    tar.Add(dpsTarget.CondiDps);
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

                List<object> playerData = new List<object>
                {
                    stats.PowerLoopCount,
                    stats.CritablePowerLoopCount, 
                    stats.CriticalRate, 
                    stats.CriticalDmg, 
                    
                    stats.ScholarRate, 
                    stats.ScholarDmg,
                    stats.PowerDamage,
                    
                    stats.MovingRate, 
                    stats.MovingDamage, 
                    
                    stats.FlankingRate, 
                    
                    stats.GlanceRate, 

                    stats.Missed, 
                    stats.Interrupts, 
                    stats.Invulned,

                    stats.EagleRate,
                    stats.EagleDmg,
                    stats.FlankingDmg, 
                    // commons
                    stats.TimeWasted, 
                    stats.Wasted, 

                    stats.TimeSaved, 
                    stats.Saved, 

                    stats.SwapCount, 
                    Math.Round(stats.StackDist, 2) 
                };
                list.Add(playerData);
            }
            return list;
        }
        /// <summary>
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="phaseIndex"></param>
        private List<List<List<object>>> BuildDMGStatsTargetsData(int phaseIndex)
        {
            List<List<List<object>>> list = new List<List<List<object>>>();

            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player player in _log.PlayerList)
            {
                List<List<object>> playerData = new List<List<object>>();
                foreach (Target target in phase.Targets)
                {
                    Statistics.FinalStats statsTarget = _statistics.StatsTarget[target][player][phaseIndex];
                    playerData.Add(new List<object>(){
                        statsTarget.PowerLoopCount,

                        statsTarget.CritablePowerLoopCount, 
                        statsTarget.CriticalRate,
                        statsTarget.CriticalDmg, 
                        
                        statsTarget.ScholarRate,
                        statsTarget.ScholarDmg,
                        statsTarget.PowerDamage,
                        
                        statsTarget.MovingRate,
                        statsTarget.MovingDamage,
                        
                        statsTarget.FlankingRate,
                        
                        statsTarget.GlanceRate,

                        statsTarget.Missed,
                        statsTarget.Interrupts,
                        statsTarget.Invulned,

                        statsTarget.EagleRate,
                        statsTarget.EagleDmg,
                        statsTarget.FlankingDmg,
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

                List<object> playerData = new List<object>
                {
                    defenses.DamageTaken,
                    defenses.DamageBarrier,
                    defenses.BlockedCount,
                    defenses.InvulnedCount,
                    defenses.InterruptedCount,
                    defenses.EvadedCount,
                    defenses.DodgeCount
                };

                if (defenses.DownDuration > 0)
                {
                    TimeSpan downDuration = TimeSpan.FromMilliseconds(defenses.DownDuration);
                    playerData.Add(defenses.DownCount);
                    playerData.Add(downDuration.TotalSeconds + " seconds downed, " +Math.Round((downDuration.TotalMilliseconds / phase.GetDuration()) * 100, 1) + "% Downed");
                }
                else
                {
                    playerData.Add(0);
                    playerData.Add("0% downed");
                }

                if (defenses.DeadDuration > 0)
                {
                    TimeSpan deathDuration = TimeSpan.FromMilliseconds(defenses.DeadDuration);
                    playerData.Add(defenses.DeadCount);
                    playerData.Add(deathDuration.TotalSeconds + " seconds dead, " + (100.0 - Math.Round((deathDuration.TotalMilliseconds / phase.GetDuration()) * 100, 1)) + "% Alive");
                }
                else
                {
                    playerData.Add(0);
                    playerData.Add("100% Alive");
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
            bool boonTable = listToUse.Select(x => x.ID).Contains(740);
                
            foreach (Player player in _log.PlayerList)
            {
                BoonData boonData = new BoonData();

                Dictionary<long, Statistics.FinalBuffs> boons = _statistics.SelfBuffs[player][phaseIndex];
                long fightDuration = phases[phaseIndex].GetDuration();
                       
                if (boonTable)
                {
                    boonData.Avg = Math.Round(_statistics.StatsAll[player][phaseIndex].AvgBoons, 1);
                }
                foreach (Boon boon in listToUse)
                {
                    List<object> boonVals = new List<object>();
                    boonData.Data.Add(boonVals);

                    if (boons.TryGetValue(boon.ID, out var uptime))
                    {
                        boonVals.Add(uptime.Uptime);
                        if (boonTable && boon.Type == Boon.BoonType.Intensity && uptime.Presence > 0)
                        {
                            boonVals.Add(uptime.Presence);
                        }
                    }
                }
                list.Add(boonData);
            }
            return list;
        }

        private Dictionary<string, List<Boon>> BuildPersonalBoonData(Dictionary<string, List<long>> dict)
        {
            Dictionary<string, List<Boon>> boonsBySpec = new Dictionary<string, List<Boon>>();
            // Collect all personal buffs by spec
            foreach (var pair in _log.PlayerListBySpec)
            {
                List<Player> players = pair.Value;
                HashSet<long> specBoonIds = new HashSet<long>(Boon.GetRemainingBuffsList(pair.Key).Select(x => x.ID));
                HashSet<Boon> boonToUse = new HashSet<Boon>();
                foreach (Player player in players)
                {
                    for (int i = 0; i < _statistics.Phases.Count; i++)
                    {
                        Dictionary<long, Statistics.FinalBuffs> boons = _statistics.SelfBuffs[player][i];
                        foreach (Boon boon in _statistics.PresentPersonalBuffs[player.InstID])
                        {
                            if (boons.TryGetValue(boon.ID, out var uptime))
                            {
                                if (uptime.Uptime > 0 && specBoonIds.Contains(boon.ID))
                                {
                                    boonToUse.Add(boon);
                                }
                            }
                        }
                    }
                }
                boonsBySpec[pair.Key] = boonToUse.ToList();
            }
            foreach (var pair in boonsBySpec)
            {
                dict[pair.Key] = new List<long>();
                foreach(Boon boon in pair.Value)
                {
                    dict[pair.Key].Add(boon.ID);
                    _usedBoons[boon.ID] = boon;
                }
            }
            return boonsBySpec;
        }

        private List<BoonData> BuildPersonalBuffUptimeData(Dictionary<string, List<Boon>> boonsBySpec, int phaseIndex)
        {
            List<BoonData> list = new List<BoonData>();
            foreach (Player player in _log.PlayerList)
            {
                BoonData boonData = new BoonData();

                Dictionary<long, Statistics.FinalBuffs> boons = _statistics.SelfBuffs[player][phaseIndex];
                long fightDuration = _statistics.Phases[phaseIndex].GetDuration();
                
                foreach (Boon boon in boonsBySpec[player.Prof])
                {
                    List<object> boonVals = new List<object>();
                    boonData.Data.Add(boonVals);
                    if (boons.TryGetValue(boon.ID, out var uptime))
                    {
                        boonVals.Add(uptime.Uptime);
                    } else
                    {
                        boonVals.Add(0);
                    }
                }
                list.Add(boonData);
            }
            return list;
        }

        private void BuildDmgModifiersData(int phaseIndex, List<long> boonToUse, List<List<object[]>> data, List<List<List<object[]>>> dataTargets)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player player in _log.PlayerList)
            {
                List<object[]> pData = new List<object[]>();
                List<List<object[]>> pDataTargets = new List<List<object[]>>();
                data.Add(pData);
                dataTargets.Add(pDataTargets);
                Dictionary<long, List<AbstractMasterActor.ExtraBoonData>> extraBoonDataAll = player.GetExtraBoonData(_log, null);
                foreach(long id in boonToUse)
                {
                    if (extraBoonDataAll.TryGetValue(id, out var extraDataList))
                    {
                        var extraData = extraDataList[phaseIndex];
                        pData.Add(new object[]
                                    {
                                    extraData.HitCount,
                                    extraData.TotalHitCount,
                                    extraData.DamageGain,
                                    extraData.TotalDamage
                                    });
                    }
                    else
                    {
                        pData.Add(new object[]
                                    {
                                    0,
                                    0,
                                    0,
                                    0
                                    });
                    }
                }
                foreach (Target target in phase.Targets)
                {
                    List<object[]> pTarget = new List<object[]>();
                    pDataTargets.Add(pTarget);
                    Dictionary<long, List<AbstractMasterActor.ExtraBoonData>> extraBoonDataTarget = player.GetExtraBoonData(_log, target);
                    foreach (long id in boonToUse)
                    {
                        if (extraBoonDataTarget.TryGetValue(id, out var extraDataList))
                        {
                            var extraData = extraDataList[phaseIndex];
                            pTarget.Add(new object[]
                                        {
                                    extraData.HitCount,
                                    extraData.TotalHitCount,
                                    extraData.DamageGain,
                                    extraData.TotalDamage
                                        });
                        }
                        else
                        {
                            pTarget.Add(new object[]
                                        {
                                    0,
                                    0,
                                    0,
                                    0
                                        });
                        }
                    }
                }
            }
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

                Dictionary<long, Statistics.FinalBuffs> uptimes;
                if (target == "self") uptimes = _statistics.SelfBuffs[player][phaseIndex];
                else if (target == "group") uptimes = _statistics.GroupBuffs[player][phaseIndex];
                else if (target == "off") uptimes = _statistics.OffGroupBuffs[player][phaseIndex];
                else if (target == "squad") uptimes = _statistics.SquadBuffs[player][phaseIndex];
                else throw new InvalidOperationException("unknown target type");

                Dictionary<long, string> rates = new Dictionary<long, string>();
                foreach (Boon boon in listToUse)
                {
                    if (uptimes.TryGetValue(boon.ID, out var uptime))
                    {
                        boonData.Data.Add(new List<object>()
                        {
                            uptime.Generation,
                            uptime.Overstack,
                            uptime.Wasted,
                            uptime.UnknownExtension,
                            uptime.Extension,
                            uptime.Extended
                        });
                    }
                    else
                    {
                        boonData.Data.Add(new List<object>()
                        {
                            0,
                            0,
                            0,
                            0,
                            0
                        });
                    }
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
        private List<object[]> BuildRotationData(AbstractActor p, int phaseIndex)
        {
            List<object[]> list = new List<object[]>();

            PhaseData phase = _statistics.Phases[phaseIndex];
            List<CastLog> casting = p.GetCastLogsActDur(_log, phase.Start, phase.End);
            SkillData skillList = _log.SkillData;
            foreach (CastLog cl in casting)
            {
                if (!_usedSkills.ContainsKey(cl.SkillId)) _usedSkills.Add(cl.SkillId, skillList.Get(cl.SkillId));
                object[] rotEntry = new object[5];
                list.Add(rotEntry);
                double offset = 0.0;
                double start = (cl.Time - phase.Start) / 1000.0;
                rotEntry[0] = start;
                if (start < 0.0)
                {
                    offset = -1000.0 * start;
                    rotEntry[0] = 0;
                }
                rotEntry[1] = cl.SkillId;
                rotEntry[2] = cl.ActualDuration - offset; ;
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
            List<Player.DeathRecap> recaps = p.GetDeathRecaps(_log);
            if (recaps == null)
            {
                return null;
            }
            foreach (Player.DeathRecap deathRecap in recaps)
            {
                DeathRecapDto recap = new DeathRecapDto()
                {
                    Time = deathRecap.Time
                };
                res.Add(recap);
                if (deathRecap.ToKill != null)
                {
                    recap.ToKill = new List<object[]>();
                    foreach (Player.DeathRecap.DeathRecapDamageItem item in deathRecap.ToKill)
                    {
                        recap.ToKill.Add(new object[]
                        {
                            item.Time,
                            item.Skill,
                            item.Damage,
                            item.Src,
                            item.Condi
                        });
                    }
                }
                if (deathRecap.ToDown != null)
                {
                    recap.ToDown = new List<object[]>();
                    foreach (Player.DeathRecap.DeathRecapDamageItem item in deathRecap.ToDown)
                    {
                        recap.ToDown.Add(new object[]
                        {
                            item.Time,
                            item.Skill,
                            item.Damage,
                            item.Src,
                            item.Condi
                        });
                    }
                }
                
            }
            return res;
        }

        private List<object[]> BuildDMGDistBodyData(List<CastLog> casting, List<DamageLog> damageLogs, long finalTotalDamage)
        {
            List<object[]> list = new List<object[]>();
            Dictionary<long, List<CastLog>> castLogsBySkill = casting.GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<long, List<DamageLog>> damageLogsBySkill = damageLogs.GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<long, Boon> conditionsById = _statistics.PresentConditions.ToDictionary(x => x.ID);
            SkillData skillList = _log.SkillData;
            foreach (KeyValuePair<long, List<DamageLog>> entry in damageLogsBySkill)
            {         
                int totaldamage = 0, 
                    mindamage = int.MaxValue, 
                    maxdamage = int.MinValue,
                    hits = 0, 
                    crit = 0,
                    flank = 0, 
                    glance = 0;
                foreach (DamageLog dl in entry.Value)
                {
                    if (dl.Result == ParseEnum.Result.Downed)
                    {
                        continue;
                    }
                    int curdmg = dl.Damage;
                    totaldamage += curdmg;
                    if (curdmg < mindamage) { mindamage = curdmg; }
                    if (curdmg > maxdamage) { maxdamage = curdmg; }
                    hits++;
                    if (dl.Result == ParseEnum.Result.Crit) crit++;
                    if (dl.Result == ParseEnum.Result.Glance) glance++;
                    if (dl.IsFlanking) flank++;
                }

                bool isCondi = conditionsById.ContainsKey(entry.Key) || entry.Key == 873;
                if (isCondi)
                {
                    Boon condi = entry.Key == 873 ? Boon.BoonsByIds[873] : conditionsById[entry.Key];
                    if (!_usedBoons.ContainsKey(condi.ID)) _usedBoons.Add(condi.ID, condi);
                }
                else
                {
                    if (!_usedSkills.ContainsKey(entry.Key)) _usedSkills.Add(entry.Key, skillList.Get(entry.Key));
                }

                int casts = 0, timeswasted = 0, timessaved = 0;
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

                object[] skillData = {
                    isCondi?1:0,
                    entry.Key,
                    totaldamage,
                    mindamage == int.MaxValue ? 0 : mindamage,
                    maxdamage == int.MinValue ? 0 : maxdamage,
                    casts,
                    hits,
                    crit,
                    flank,
                    glance,
                    timeswasted / 1000.0,
                    -timessaved / 1000.0
                };
                list.Add(skillData);
            }

            foreach (KeyValuePair<long, List<CastLog>> entry in castLogsBySkill)
            {
                if (damageLogsBySkill.ContainsKey(entry.Key)) continue;

                if (!_usedSkills.ContainsKey(entry.Key)) _usedSkills.Add(entry.Key, skillList.Get(entry.Key));

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

                object[] skillData = { 0, entry.Key, 0, -1, 0, casts,
                    0, 0, 0, 0, timeswasted / 1000.0, -timessaved / 1000.0 };
                list.Add(skillData);
            }
            return list;
        }

        private DmgDistributionDto _BuildDMGDistData(Statistics.FinalDPS dps, AbstractMasterActor p, Target target, int phaseIndex)
        {
            DmgDistributionDto dto = new DmgDistributionDto();
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<CastLog> casting = p.GetCastLogs(_log, phase.Start, phase.End);
            List<DamageLog> damageLogs = p.GetJustPlayerDamageLogs(target, _log, phase.Start, phase.End);
            dto.TotalDamage = dps.Damage;
            dto.ContributedDamage = damageLogs.Count > 0 ? damageLogs.Sum(x => x.Damage) : 0;
            dto.Distribution = BuildDMGDistBodyData(casting, damageLogs, dto.ContributedDamage);

            return dto;
        }

        /// <summary>
        /// Creates the damage distribution table for a given player
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="p"></param>
        /// <param name="phaseIndex"></param>
        private DmgDistributionDto BuildPlayerDMGDistData(Player p, Target target, int phaseIndex)
        {
            Statistics.FinalDPS dps = target != null ? _statistics.DpsTarget[target][p][phaseIndex] : _statistics.DpsAll[p][phaseIndex];
            return _BuildDMGDistData(dps, p, target, phaseIndex);
        }

        /// <summary>
        /// Creates the damage distribution table for a target
        /// </summary>
        private DmgDistributionDto BuildTargetDMGDistData(Target target, int phaseIndex)
        {
            Statistics.FinalDPS dps = _statistics.TargetDps[target][phaseIndex];
            return _BuildDMGDistData(dps, target, null, phaseIndex);
        }

        private DmgDistributionDto _BuildDMGDistData(Statistics.FinalDPS dps, AbstractMasterActor p, Minions minions, Target target, int phaseIndex)
        {
            DmgDistributionDto dto = new DmgDistributionDto();
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<CastLog> casting = minions.GetCastLogs(_log, phase.Start, phase.End);
            List<DamageLog> damageLogs = minions.GetDamageLogs(target, _log, phase.Start, phase.End);
            dto.ContributedDamage = damageLogs.Count > 0 ? damageLogs.Sum(x => x.Damage) : 0;
            dto.TotalDamage = dps.Damage;
            dto.Distribution = BuildDMGDistBodyData(casting, damageLogs, dto.ContributedDamage);
            return dto;
        }

        /// <summary>
        /// Creates the damage distribution table for a given minion
        /// </summary>
        private DmgDistributionDto BuildPlayerMinionDMGDistData(Player p, Minions minions, Target target, int phaseIndex)
        {
            Statistics.FinalDPS dps = target != null ? _statistics.DpsTarget[target][p][phaseIndex] : _statistics.DpsAll[p][phaseIndex];

            return _BuildDMGDistData(dps, p, minions, target, phaseIndex);
        }

        /// <summary>
        /// Creates the damage distribution table for a given boss minion
        /// </summary>
        private DmgDistributionDto BuildTargetMinionDMGDistData(Target target, Minions minions, int phaseIndex)
        {
            Statistics.FinalDPS dps = _statistics.TargetDps[target][phaseIndex];
            return _BuildDMGDistData(dps, target, minions, null, phaseIndex);
        }

        /// <summary>
        /// Create the damage taken distribution table for a given player
        /// </summary>
        /// <param name="p"></param>
        /// <param name="phaseIndex"></param>
        private DmgDistributionDto BuildDMGTakenDistData(AbstractMasterActor p, int phaseIndex)
        {
            DmgDistributionDto dto = new DmgDistributionDto
            {
                Distribution = new List<object[]>()
            };
            PhaseData phase = _statistics.Phases[phaseIndex];
            List<DamageLog> damageLogs = p.GetDamageTakenLogs(null, _log, phase.Start, phase.End);
            Dictionary<long, List<DamageLog>> damageLogsBySkill = damageLogs.GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            SkillData skillList = _log.SkillData;
            dto.ContributedDamage = damageLogs.Count > 0 ? damageLogs.Sum(x => (long)x.Damage) : 0;
            Dictionary<long, Boon> conditionsById = _statistics.PresentConditions.ToDictionary(x => x.ID);
            foreach (var entry in damageLogsBySkill)
            {
                int totaldamage = 0,
                    mindamage = int.MaxValue,
                    hits = 0,
                    maxdamage = int.MinValue,
                    crit = 0,
                    flank = 0,
                    glance = 0;

                foreach (DamageLog dl in entry.Value)
                {
                    if (dl.Result == ParseEnum.Result.Downed)
                    {
                        continue;
                    }
                    int curdmg = dl.Damage;
                    totaldamage += curdmg;
                    if (curdmg < mindamage) { mindamage = curdmg; }
                    if (curdmg > maxdamage) { maxdamage = curdmg; }
                    hits++;
                    if (dl.Result == ParseEnum.Result.Crit) crit++;
                    if (dl.Result == ParseEnum.Result.Glance) glance++;
                    if (dl.IsFlanking) flank++;
                }

                bool isCondi = conditionsById.ContainsKey(entry.Key) || entry.Key == 873;
                if (isCondi)
                {
                    Boon condi = entry.Key == 873 ? Boon.BoonsByIds[873] : conditionsById[entry.Key];
                    if (!_usedBoons.ContainsKey(condi.ID)) _usedBoons.Add(condi.ID, condi);
                }
                else
                {
                    if (!_usedSkills.ContainsKey(entry.Key)) _usedSkills.Add(entry.Key, skillList.Get(entry.Key));
                }
                object[] row = new object[12] {
                    isCondi ? 1 : 0, // isCondi
                    entry.Key,
                    totaldamage,
                    mindamage == int.MaxValue ? 0 : mindamage,
                    maxdamage == int.MinValue ? 0 : maxdamage,
                    0,
                    hits,
                    crit,
                    flank,
                    glance,
                    0,
                    0
                };
                dto.Distribution.Add(row);
            }

            return dto;
        }

        private List<BoonChartDataDto> BuildBoonGraphData(AbstractMasterActor p, int phaseIndex)
        {
            List<BoonChartDataDto> list = new List<BoonChartDataDto>();
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, BoonsGraphModel> boonGraphData = p.GetBoonGraphs(_log).ToDictionary(x => x.Key, x => x.Value);
            foreach (Boon buff in _statistics.PresentBoons)
            {
                if (boonGraphData.TryGetValue(buff.ID, out BoonsGraphModel bgm))
                {
                    BoonChartDataDto graph = BuildBoonGraph(bgm, phase);
                    if (graph != null) list.Add(graph);
                }
                boonGraphData.Remove(buff.ID);
            }
            foreach (Boon buff in _statistics.PresentConditions)
            {
                if (boonGraphData.TryGetValue(buff.ID, out BoonsGraphModel bgm))
                {
                    BoonChartDataDto graph = BuildBoonGraph(bgm, phase);
                    if (graph != null) list.Add(graph);
                }
                boonGraphData.Remove(buff.ID);
            }
            foreach (Boon buff in _statistics.PresentOffbuffs)
            {
                if (boonGraphData.TryGetValue(buff.ID, out BoonsGraphModel bgm))
                {
                    BoonChartDataDto graph = BuildBoonGraph(bgm, phase);
                    if (graph != null) list.Add(graph);
                }
                boonGraphData.Remove(buff.ID);
            }
            foreach (Boon buff in _statistics.PresentDefbuffs)
            {
                if (boonGraphData.TryGetValue(buff.ID, out BoonsGraphModel bgm))
                {
                    BoonChartDataDto graph = BuildBoonGraph(bgm, phase);
                    if (graph != null) list.Add(graph);
                }
                boonGraphData.Remove(buff.ID);
            }
            foreach (BoonsGraphModel bgm in boonGraphData.Values)
            {
                BoonChartDataDto graph = BuildBoonGraph(bgm, phase);
                if (graph != null) list.Add(graph);
            }
            if (p.GetType() == typeof(Player))
            {
                foreach (Target mainTarget in _log.FightData.GetMainTargets(_log))
                {
                    boonGraphData = mainTarget.GetBoonGraphs(_log);
                    foreach (BoonsGraphModel bgm in boonGraphData.Values.Reverse().Where(x => x.Boon.Name == "Compromised" || x.Boon.Name == "Unnatural Signet" || x.Boon.Name == "Fractured - Enemy"))
                    {
                        BoonChartDataDto graph = BuildBoonGraph(bgm, phase);
                        if (graph != null) list.Add(graph);
                    }
                }
            }
            list.Reverse();
            return list;
        }

        private BoonChartDataDto BuildBoonGraph(BoonsGraphModel bgm, PhaseData phase)
        {
            //TODO line: {shape: 'hv'}
            List<BoonsGraphModel.Segment> bChart = bgm.BoonChart.Where(x => x.End >= phase.Start && x.Start <= phase.End
            ).ToList();
            if (bChart.Count == 0 || (bChart.Count == 1 && bChart.First().Value == 0))
            {
                return null;
            }
            BoonChartDataDto dto = new BoonChartDataDto
            {
                Id = bgm.Boon.ID,
                Visible = (bgm.Boon.Name == "Might" || bgm.Boon.Name == "Quickness" || bgm.Boon.Name == "Vulnerability"),
                Color = GeneralHelper.GetLink("Color-" + bgm.Boon.Name),
                States = new List<object[]>(bChart.Count + 1)
            };
            _usedBoons[bgm.Boon.ID] = bgm.Boon;
            foreach (BoonsGraphModel.Segment seg in bChart)
            {
                double segStart = Math.Round(Math.Max(seg.Start - phase.Start, 0) / 1000.0, 3);
                dto.States.Add(new object[] { segStart, seg.Value });
            }
            BoonsGraphModel.Segment lastSeg = bChart.Last();
            double segEnd = Math.Round(Math.Min(lastSeg.End - phase.Start, phase.End - phase.Start) / 1000.0, 3);
            dto.States.Add(new object[] { segEnd, lastSeg.Value });

            return dto;
        }

        private List<FoodDto> BuildPlayerFoodData(Player p)
        {
            List<FoodDto> list = new List<FoodDto>();
            List<Player.Consumable> consume = p.GetConsumablesList(_log, 0, _log.FightData.FightDuration);

            foreach(Player.Consumable entry in consume)
            {
                FoodDto dto = new FoodDto
                {
                    Time = entry.Time / 1000.0,
                    Duration = entry.Duration / 1000.0,
                    Stack = entry.Stack,
                    Id = entry.Item.ID,
                    Dimished = (entry.Item.ID == 46587 || entry.Item.ID == 46668)
                };
                _usedBoons[entry.Item.ID] = entry.Item;
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
            HashSet<Mechanic> presMech = _log.MechanicData.GetPresentPlayerMechs(0);
            PhaseData phase = _statistics.Phases[phaseIndex];

            foreach (Player p in _log.PlayerList)
            {
                List<int[]> playerData = new List<int[]>(presMech.Count);
                foreach (Mechanic mech in presMech)
                {
                    long timeFilter = 0;
                    int filterCount = 0;
                    List<MechanicLog> mls = _log.MechanicData[mech].Where(x => x.Actor.InstID == p.InstID && phase.InInterval(x.Time)).ToList();
                    int count = mls.Count;
                    foreach (MechanicLog ml in mls)
                    {
                        if (mech.InternalCooldown != 0 && ml.Time - timeFilter < mech.InternalCooldown)//ICD check
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
            HashSet<Mechanic> presMech = _log.MechanicData.GetPresentEnemyMechs(0);
            PhaseData phase = _statistics.Phases[phaseIndex];
            foreach (DummyActor p in _log.MechanicData.GetEnemyList(0))
            {
                List<int[]> enemyData = new List<int[]>(presMech.Count);
                foreach (Mechanic mech in presMech)
                {
                    long timeFilter = 0;
                    int filterCount = 0;
                    List<MechanicLog> mls = _log.MechanicData[mech].Where(x => x.Actor.InstID == p.InstID && phase.InInterval(x.Time)).ToList();
                    int count = mls.Count;
                    foreach (MechanicLog ml in mls)
                    {
                        if (mech.InternalCooldown != 0 && ml.Time - timeFilter < mech.InternalCooldown)//ICD check
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
        
        private List<MechanicDto> BuildMechanics()
        {
            List<MechanicDto> mechanicDtos = new List<MechanicDto>();
            HashSet<Mechanic> playerMechs = _log.MechanicData.GetPresentPlayerMechs(0);
            HashSet<Mechanic> enemyMechs = _log.MechanicData.GetPresentEnemyMechs(0);
            foreach (Mechanic mech in _log.MechanicData.GetPresentMechanics(0))
            {
                List<MechanicLog> mechanicLogs = _log.MechanicData[mech];
                MechanicDto dto = new MechanicDto
                {
                    Name = mech.FullName,
                    ShortName = mech.ShortName,
                    Description = mech.Description,
                    PlayerMech = playerMechs.Contains(mech),
                    EnemyMech = enemyMechs.Contains(mech)
                };
                mechanicDtos.Add(dto);
            }
            return mechanicDtos;
        }

        private List<MechanicChartDataDto> BuildMechanicsChartData()
        {
            List<MechanicChartDataDto> mechanicsChart = new List<MechanicChartDataDto>();
            HashSet<Mechanic> playerMechs = _log.MechanicData.GetPresentPlayerMechs(0);
            HashSet<Mechanic> enemyMechs = _log.MechanicData.GetPresentEnemyMechs(0);
            foreach (Mechanic mech in _log.MechanicData.GetPresentMechanics(0))
            {
                List<MechanicLog> mechanicLogs = _log.MechanicData[mech];
                MechanicChartDataDto dto = new MechanicChartDataDto
                {
                    Color = mech.PlotlySetting.color,
                    Symbol = mech.PlotlySetting.symbol,
                    Size = mech.PlotlySetting.size,
                    Visible = (mech.SkillId == SkillItem.DeathId || mech.SkillId == SkillItem.DownId),
                    Points = BuildMechanicGraphPointData(mechanicLogs, mech.IsEnemyMechanic)
                };
                mechanicsChart.Add(dto);
            }
            return mechanicsChart;
        }

        private List<List<List<double>>> BuildMechanicGraphPointData(List<MechanicLog> mechanicLogs, bool enemyMechanic)
        {
            List<List<List<double>>> list = new List<List<List<double>>>();
            foreach (PhaseData phase in _statistics.Phases)
            {
                List<List<double>> phaseData = new List<List<double>>();
                list.Add(phaseData);
                if (!enemyMechanic)
                {
                    Dictionary<DummyActor, int> playerIndex = new Dictionary<DummyActor, int>();
                    for (var p = 0; p < _log.PlayerList.Count; p++)
                    {
                        playerIndex.Add(_log.PlayerList[p], p);
                        phaseData.Add(new List<double>());
                    }
                    foreach (MechanicLog ml in mechanicLogs.Where(x => phase.InInterval(x.Time)))
                    {
                        double time = (ml.Time - phase.Start) / 1000.0;
                        if (playerIndex.TryGetValue(ml.Actor, out int p))
                        {
                            phaseData[p].Add(time);
                        }
                    }
                } else
                {
                    Dictionary<DummyActor, int> targetIndex = new Dictionary<DummyActor, int>();
                    for (var p = 0; p < phase.Targets.Count; p++)
                    {
                        targetIndex.Add(phase.Targets[p], p);
                        phaseData.Add(new List<double>());
                    }
                    phaseData.Add(new List<double>());
                    foreach (MechanicLog ml in mechanicLogs.Where(x => phase.InInterval(x.Time)))
                    {
                        double time = (ml.Time - phase.Start) / 1000.0;
                        if (targetIndex.TryGetValue(ml.Actor, out int p))
                        {
                            phaseData[p].Add(time);
                        }
                        else
                        {
                            phaseData[phaseData.Count - 1].Add(time);
                        }
                    }
                }
            }
            return list;
        }

        private List<BoonData> BuildTargetCondiData(int phaseIndex, Target target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, Statistics.FinalTargetBuffs> conditions = _statistics.TargetBuffs[target][phaseIndex];
            List<BoonData> list = new List<BoonData>();

            foreach (Player player in _log.PlayerList)
            {
                BoonData playerData = new BoonData
                {
                    Data = new List<List<object>>()
                };

                foreach (Boon boon in _statistics.PresentConditions)
                {
                    List<object> boonData = new List<object>();
                    if (conditions.TryGetValue(boon.ID, out var toUse))
                    {
                        boonData.Add(toUse.Generated[player]);
                        boonData.Add(toUse.Overstacked[player]);
                        boonData.Add(toUse.Wasted[player]);
                        boonData.Add(toUse.UnknownExtension[player]);
                        boonData.Add(toUse.Extension[player]);
                        boonData.Add(toUse.Extended[player]);
                    }
                    playerData.Data.Add(boonData);
                }
                list.Add(playerData);
            }
            return list;
        }

        private BoonData BuildTargetCondiUptimeData(int phaseIndex, Target target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, Statistics.FinalTargetBuffs> conditions = _statistics.TargetBuffs[target][phaseIndex];
            long fightDuration = phase.GetDuration();
            BoonData tagetData = new BoonData
            {
                Data = new List<List<object>>()
            };
            tagetData.Avg = Math.Round(_statistics.AvgTargetConditions[target][phaseIndex], 1);
            foreach (Boon boon in _statistics.PresentConditions)
            {
                List<object> boonData = new List<object>();

                if (conditions.TryGetValue(boon.ID, out var uptime))
                {
                    boonData.Add(uptime.Uptime);
                    if (boon.Type != Boon.BoonType.Duration && uptime.Presence > 0)
                    {
                        boonData.Add(uptime.Presence);
                    }
                }

                tagetData.Data.Add(boonData);
            }
            return tagetData;
        }

        private BoonData BuildTargetBoonData(int phaseIndex, Target target)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            Dictionary<long, Statistics.FinalTargetBuffs> conditions = _statistics.TargetBuffs[target][phaseIndex];
            long fightDuration = phase.GetDuration();
            BoonData targetData = new BoonData
            {
                Data = new List<List<object>>()
            };
            targetData.Avg = Math.Round(_statistics.AvgTargetBoons[target][phaseIndex], 1);
            foreach (Boon boon in _statistics.PresentBoons)
            {
                List<object> boonData = new List<object>();
                if (conditions.TryGetValue(boon.ID, out var uptime))
                {
                    boonData.Add(uptime.Uptime);
                    if (boon.Type != Boon.BoonType.Duration && uptime.Presence > 0)
                    {
                        boonData.Add(uptime.Presence);
                    }
                }

                targetData.Data.Add(boonData);
            }
            return targetData;
        }

        private string ReplaceVariables(string html)
        {
            html = html.Replace("${bootstrapTheme}", !_settings.LightTheme ? "slate" : "yeti");

            html = html.Replace("${encounterStart}", _log.LogData.LogStart);
            html = html.Replace("${encounterEnd}", _log.LogData.LogEnd);
            html = html.Replace("${evtcVersion}", _log.LogData.BuildVersion);
            html = html.Replace("${fightID}", _log.FightData.ID.ToString());
            html = html.Replace("${eiVersion}", Application.ProductVersion);
            html = html.Replace("${recordedBy}", _log.LogData.PoV.Split(':')[0]);

            string uploadString = "";
            if (_settings.UploadToDPSReports)
            {
                uploadString += "<p>DPS Reports Link (EI): <a href=\"" + _uploadLink[0] + "\">" + _uploadLink[0] + "</a></p>";
            }
            if (_settings.UploadToDPSReportsRH)
            {
                uploadString += "<p>DPS Reports Link (RH): <a href=\"" + _uploadLink[1] + "\">" + _uploadLink[1] + "</a></p>";
            }
            if (_settings.UploadToRaidar)
            {
                uploadString += "<p>Raidar Link: <a href=\"" + _uploadLink[2] + "\">" + _uploadLink[2] + "</a></p>";
            }
            html = html.Replace("<!--${UploadLinks}-->", uploadString);

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

            html = html.Replace("<!--${Css}-->", BuildCss(path));
            html = html.Replace("<!--${Js}-->", BuildEIJs(path));
            html = html.Replace("<!--${JsCRLink}-->", BuildCRLinkJs(path));

            html = html.Replace("'${logDataJson}'", BuildLogData());

            html = html.Replace("<!--${Details}-->", BuildDetails());
            html = html.Replace("<!--${Maps}-->", BuildMaps());
#if DEBUG
            html = html.Replace("<!--${Vue}-->", "<script src=\"https://cdn.jsdelivr.net/npm/vue@2.5.17/dist/vue.js\"></script>");
#else
            html = html.Replace("<!--${Vue}-->", "<script src=\"https://cdn.jsdelivr.net/npm/vue@2.5.17/dist/vue.min.js\"></script>");
#endif

            html = html.Replace("'${graphDataJson}'", BuildGraphJson());

            html = html.Replace("<!--${CombatReplayScript}-->", BuildCombatReplayScript(path));
            html = html.Replace("<!--${CombatReplayBody}-->", BuildCombatReplayContent());
            sw.Write(html);
            return;       
        }

        private string BuildCombatReplayScript(string path)
        {
            if (!_settings.ParseCombatReplay || !_log.FightData.Logic.CanCombatReplay)
            {
                return "";
            }
            CombatReplayMap map = _log.FightData.Logic.GetCombatMap();
            if (Properties.Settings.Default.HtmlExternalScripts)
            {
#if DEBUG
                string jsFileName = "EliteInsights-CR-" + _scriptVersion + ".js";
#else
                string jsFileName = "EliteInsights-CR-" + _scriptVersion + ".min.js";
#endif
                string jsPath = Path.Combine(path, jsFileName);
                try
                {
                    using (var fs = new FileStream(jsPath, FileMode.Create, FileAccess.Write))
                    using (var scriptWriter = new StreamWriter(fs, Encoding.UTF8))
                    {
#if DEBUG
                        scriptWriter.Write(Properties.Resources.combatreplay_js);
#else
                        scriptWriter.Write(Uglify.Js(Properties.Resources.combatreplay_js, GeneralHelper.JSMinifySettings).Code);
#endif
                    }
                } catch (IOException)
                {
                }
                string content = "<script src=\"./" + jsFileName + "?version=" + _scriptVersionRev + "\"></script>\n";
                content += "<script>"+ CombatReplayHelper.GetDynamicCombatReplayScript(_log, _settings.PollingRate, map)+ "</script>";
                return content;
            }
            else
            {
                return CombatReplayHelper.CreateCombatReplayScript(_log, map, _settings.PollingRate);
            }
        }

        private string BuildCombatReplayContent()
        {
            if (!_settings.ParseCombatReplay || !_log.FightData.Logic.CanCombatReplay)
            {
                return "";
            }
            CombatReplayMap map = _log.FightData.Logic.GetCombatMap();
            (int width, int height) canvasSize = map.GetPixelMapSize();
            return CombatReplayHelper.CreateCombatReplayInterface(canvasSize, _log);
        }

        private string BuildTemplates(string script)
        {
            string tmplScript = script; 
            Dictionary<string, string> templates = new Dictionary<string, string>()
            {
                {"${tmplBuffStats}",Properties.Resources.tmplBuffStats },
                {"${tmplBuffStatsTarget}",Properties.Resources.tmplBuffStatsTarget },
                {"${tmplBuffTable}",Properties.Resources.tmplBuffTable },
                {"${tmplDamageDistPlayer}",Properties.Resources.tmplDamageDistPlayer },
                {"${tmplDamageDistTable}",Properties.Resources.tmplDamageDistTable },
                {"${tmplDamageDistTarget}",Properties.Resources.tmplDamageDistTarget },
                {"${tmplDamageModifierTable}",Properties.Resources.tmplDamageModifierTable },
                {"${tmplDamageTable}",Properties.Resources.tmplDamageTable },
                {"${tmplDamageTaken}",Properties.Resources.tmplDamageTaken },
                {"${tmplDeathRecap}",Properties.Resources.tmplDeathRecap },
                {"${tmplDefenseTable}",Properties.Resources.tmplDefenseTable },
                {"${tmplEncounter}",Properties.Resources.tmplEncounter },
                {"${tmplFood}",Properties.Resources.tmplFood },
                {"${tmplGameplayTable}",Properties.Resources.tmplGameplayTable },
                {"${tmplGeneralLayout}",Properties.Resources.tmplGeneralLayout },
                {"${tmplMechanicsTable}",Properties.Resources.tmplMechanicsTable },
                {"${tmplPersonalBuffTable}",Properties.Resources.tmplPersonalBuffTable },
                {"${tmplPhase}",Properties.Resources.tmplPhase },
                {"${tmplPlayers}",Properties.Resources.tmplPlayers },
                {"${tmplPlayerStats}",Properties.Resources.tmplPlayerStats },
                {"${tmplPlayerTab}",Properties.Resources.tmplPlayerTab },
                {"${tmplSimpleRotation}",Properties.Resources.tmplSimpleRotation },
                {"${tmplSupportTable}",Properties.Resources.tmplSupportTable },
                {"${tmplTargets}",Properties.Resources.tmplTargets },
                {"${tmplTargetStats}",Properties.Resources.tmplTargetStats },
                {"${tmplTargetTab}",Properties.Resources.tmplTargetTab },
                {"${tmplDPSGraph}",Properties.Resources.tmplDPSGraph },
                {"${tmplGraphStats}",Properties.Resources.tmplGraphStats },
                {"${tmplPlayerTabGraph}",Properties.Resources.tmplPlayerTabGraph },
                {"${tmplRotationLegend}",Properties.Resources.tmplRotationLegend },
                {"${tmplTargetTabGraph}",Properties.Resources.tmplTargetTabGraph },
                {"${tmplTargetData}",Properties.Resources.tmplTargetData },
            };
            foreach (var entry in templates)
            {
#if DEBUG
                tmplScript = tmplScript.Replace(entry.Key, entry.Value);
#else
                tmplScript = tmplScript.Replace(entry.Key, Regex.Replace(entry.Value, @"\t|\n|\r", ""));
#endif
            }
            return tmplScript;
        }

        private string BuildCRTemplates(string script)
        {
            string tmplScript = script;
            Dictionary<string, string> CRtemplates = new Dictionary<string, string>()
                {
                    {"${tmplCombatReplayDamageData}", Properties.Resources.tmplCombatReplayDamageData },
                    {"${tmplCombatReplayStatusData}", Properties.Resources.tmplCombatReplayStatusData },
                    {"${tmplCombatReplayDamageTable}", Properties.Resources.tmplCombatReplayDamageTable },
                    {"${tmplCombatReplayPlayerBuffStats}", Properties.Resources.tmplCombatReplayPlayerBuffStats },
                    {"${tmplCombatReplayPlayerStats}", Properties.Resources.tmplCombatReplayPlayerStats },
                    {"${tmplCombatReplayPlayerStatus}", Properties.Resources.tmplCombatReplayPlayerStatus },
                    {"${tmplCombatReplayPlayerRotation}", Properties.Resources.tmplCombatReplayPlayerRotation },
                };
            foreach (var entry in CRtemplates)
            {
                tmplScript = tmplScript.Replace(entry.Key, Regex.Replace(entry.Value, @"\t|\n|\r", ""));
            }
            return tmplScript;
        }

        private string BuildCss(string path)
        {
#if DEBUG
            string scriptContent = Properties.Resources.ei_css;
#else
            string scriptContent = Uglify.Css(Properties.Resources.ei_css).Code;
#endif
            if (Properties.Settings.Default.HtmlExternalScripts)
            {
#if DEBUG
                string cssFilename = "EliteInsights-" + _scriptVersion + ".css";
#else
                string cssFilename = "EliteInsights-" + _scriptVersion + ".min.css";
#endif
                string cssPath = Path.Combine(path, cssFilename);
                try
                {
                    using (var fs = new FileStream(cssPath, FileMode.Create, FileAccess.Write))
                    using (var scriptWriter = new StreamWriter(fs, Encoding.UTF8))
                    {
                        scriptWriter.Write(scriptContent);
                    }
                }
                catch (IOException)
                {
                }
                return "<link rel=\"stylesheet\" type=\"text/css\" href=\"./"+ cssFilename + "?version="+_scriptVersionRev+"\">";
            }
            else
            {
                return "<style type=\"text/css\">\r\n" + scriptContent + "\r\n</style>";
            }
        }

        private string BuildEIJs(string path)
        {
            List<string> orderedScripts = new List<string>()
            {
                Properties.Resources.globalJS,
                Properties.Resources.commonsJS,
                Properties.Resources.headerJS,
                Properties.Resources.layoutJS,
                Properties.Resources.generalStatsJS,
                Properties.Resources.buffStatsJS,
                Properties.Resources.graphsJS,
                Properties.Resources.mechanicsJS,
                Properties.Resources.targetStatsJS,
                Properties.Resources.playerStatsJS,
                Properties.Resources.ei_js
            };
            string scriptContent = orderedScripts[0];
            for (int i = 1; i < orderedScripts.Count; i++)
            {
                scriptContent += orderedScripts[i];
            }
            scriptContent = BuildTemplates(scriptContent);
#if !DEBUG
            scriptContent = Uglify.Js(scriptContent, GeneralHelper.JSMinifySettings).Code;
#endif
            if (Properties.Settings.Default.HtmlExternalScripts)
            {
#if DEBUG
                string scriptFilename = "EliteInsights-" + _scriptVersion + ".js";
#else
                string scriptFilename = "EliteInsights-" + _scriptVersion +".min.js";
#endif
                string scriptPath = Path.Combine(path, scriptFilename);
                try
                {
                    using (var fs = new FileStream(scriptPath, FileMode.Create, FileAccess.Write))
                    using (var scriptWriter = new StreamWriter(fs, Encoding.UTF8))
                    {
                        scriptWriter.Write(scriptContent);
                    }
                }
                catch (IOException)
                {
                }
                return "<script src=\"./" + scriptFilename + "?version=" + _scriptVersionRev + "\"></script>";
            }
            else
            {
                return "<script>\r\n" + scriptContent + "\r\n</script>";
            }
        }

        private string BuildCRLinkJs(string path)
        {
            if (!(_settings.ParseCombatReplay && _log.FightData.Logic.CanCombatReplay))
            {
                return "";
            }
            string scriptContent = Properties.Resources.combatReplayStatsJS;
            scriptContent = BuildCRTemplates(scriptContent);
#if !DEBUG
            scriptContent = Uglify.Js(scriptContent).Code;
#endif
            if (Properties.Settings.Default.HtmlExternalScripts)
            {
#if DEBUG
                string scriptFilename = "EliteInsights-CRLink-" + _scriptVersion + ".js";
#else
                string scriptFilename = "EliteInsights-CRLink-" + _scriptVersion +".min.js";
#endif
                string scriptPath = Path.Combine(path, scriptFilename);
                try
                {
                    using (var fs = new FileStream(scriptPath, FileMode.Create, FileAccess.Write))
                    using (var scriptWriter = new StreamWriter(fs, Encoding.UTF8))
                    {
                        scriptWriter.Write(scriptContent);
                    }
                }
                catch (IOException)
                {
                }
                return "<script src=\"./" + scriptFilename + "?version=" + _scriptVersionRev + "\"></script>";
            }
            else
            {
                return "<script>\r\n" + scriptContent + "\r\n</script>";
            }
        }

        private void BuildWeaponSets(PlayerDto playerDto, Player player)
        {
            string[] weps = player.GetWeaponsArray(_log);
            List<string> firstSet = new List<string>();
            List<string> secondSet = new List<string>();
            for (int j = 0; j < weps.Length; j++)
            {
                var wep = weps[j];
                if (wep != null)
                {
                    if (wep != "2Hand")
                    {
                        if (j > 1)
                        {
                            secondSet.Add(wep);
                        }
                        else
                        {
                            firstSet.Add(wep);
                        }
                    }
                }
                else
                {
                    if (j > 1)
                    {
                        secondSet.Add("Unknown");
                    }
                    else
                    {
                        firstSet.Add("Unknown");
                    }
                }
            }
            if (firstSet[0] == "Unknown" && firstSet[1] == "Unknown")
            {
                playerDto.FirstSet = new List<string>();
            }
            else
            {
                playerDto.FirstSet = firstSet;
            }
            if (secondSet[0] == "Unknown" && secondSet[1] == "Unknown")
            {
                playerDto.SecondSet = new List<string>();
            }
            else
            {
                playerDto.SecondSet = secondSet;
            }
        }

        private string BuildGraphJson()
        {
            ChartDataDto chartData = new ChartDataDto();
            List<PhaseChartDataDto> phaseChartData = new List<PhaseChartDataDto>();
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                PhaseChartDataDto phaseData = new PhaseChartDataDto()
                {
                    Players = BuildPlayerGraphData(i)
                };
                foreach(Target target in _statistics.Phases[i].Targets)
                {
                    phaseData.Targets.Add(BuildTargetGraphData(i, target));
                }

                phaseChartData.Add(phaseData);
             }
            chartData.Phases = phaseChartData;
            chartData.Mechanics = BuildMechanicsChartData();
            return ToJson(chartData);
        }

        private string BuildLogData()
        {
            LogDataDto logData = new LogDataDto();
            foreach(Player player in _log.PlayerList)
            {
                PlayerDto playerDto = new PlayerDto()
                {
                    Group = player.Group,
                    Name = player.Character,
                    Acc = player.Account.TrimStart(':'),
                    Profession = player.Prof,
                    Condi = player.Condition,
                    Conc = player.Concentration,
                    Heal = player.Healing,
                    Tough = player.Toughness,
                    ColTarget = GeneralHelper.GetLink("Color-" + player.Prof),
                    ColCleave = GeneralHelper.GetLink("Color-" + player.Prof + "-NonBoss"),
                    ColTotal = GeneralHelper.GetLink("Color-" + player.Prof + "-Total"),
                    IsConjure = (player.Account == ":Conjured Sword"),
                };
                BuildWeaponSets(playerDto, player);
                if (_settings.ParseCombatReplay && _log.FightData.Logic.CanCombatReplay && !playerDto.IsConjure)
                {
                    playerDto.CombatReplayID = player.GetCombatReplayID();
                }
                foreach (KeyValuePair<string, Minions> pair in player.GetMinions(_log))
                {
                    playerDto.Minions.Add(new MinionDto()
                    {
                        Id = pair.Value.MinionID,
                        Name = pair.Key.TrimEnd(" \0".ToArray())
                    });
                }

                logData.Players.Add(playerDto);
            }

            foreach(DummyActor enemy in _log.MechanicData.GetEnemyList(0))
            {
                logData.Enemies.Add(new EnemyDto() { Name = enemy.Character });
            }

            foreach (Target target in _log.FightData.Logic.Targets)
            {
                TargetDto targetDto = new TargetDto()
                {
                    Name = target.Character,
                    Icon = GeneralHelper.GetNPCIcon(target.ID),
                    Health = target.Health,
                    HbHeight = target.HitboxHeight,
                    HbWidth = target.HitboxWidth,
                    Tough = target.Toughness
                };
                if (_settings.ParseCombatReplay && _log.FightData.Logic.CanCombatReplay)
                {
                    targetDto.CombatReplayID = target.GetCombatReplayID();
                }
                if (_log.FightData.Success)
                {
                    targetDto.Percent = 100;
                    targetDto.HpLeft = 0;
                }
                else
                {
                    if (target.HealthOverTime.Count > 0)
                    {
                        targetDto.Percent = Math.Round(100.0 - target.HealthOverTime[target.HealthOverTime.Count - 1].Y * 0.01, 2);
                        targetDto.HpLeft = (int)Math.Floor(target.HealthOverTime[target.HealthOverTime.Count - 1].Y * 0.01);
                    }
                }
                foreach (KeyValuePair<string, Minions> pair in target.GetMinions(_log))
                {
                    targetDto.Minions.Add(new MinionDto() { Id = pair.Value.MinionID, Name = pair.Key.TrimEnd(" \0".ToArray()) });
                }
                logData.Targets.Add(targetDto);
            }
            //
            logData.PersBuffs = new Dictionary<string, List<long>>();
            Dictionary<string, List<Boon>> persBuffDict = BuildPersonalBoonData(logData.PersBuffs);
            logData.Boons = new List<long>();
            foreach (Boon boon in _statistics.PresentBoons)
            {
                logData.Boons.Add(boon.ID);
                _usedBoons[boon.ID] = boon;
            }
            logData.Conditions = new List<long>();
            foreach (Boon boon in _statistics.PresentConditions)
            {
                logData.Conditions.Add(boon.ID);
                _usedBoons[boon.ID] = boon;
            }
            logData.OffBuffs = new List<long>();
            foreach (Boon boon in _statistics.PresentOffbuffs)
            {
                logData.OffBuffs.Add(boon.ID);
                _usedBoons[boon.ID] = boon;
            }
            logData.DefBuffs = new List<long>();
            foreach (Boon boon in _statistics.PresentDefbuffs)
            {
                logData.DefBuffs.Add(boon.ID);
                _usedBoons[boon.ID] = boon;
            }
            HashSet<long> dmgCommonModifiersBuffs = new HashSet<long>();
            foreach (Player p in _log.PlayerList)
            {
                Dictionary<long, List<AbstractMasterActor.ExtraBoonData>> toCheck = p.GetExtraBoonData(_log, null);
                dmgCommonModifiersBuffs.UnionWith(toCheck.Keys);
            }
            logData.DmgCommonModifiersBuffs = dmgCommonModifiersBuffs.ToList();
            //
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                PhaseData phaseData = _statistics.Phases[i];
                PhaseDto phaseDto = new PhaseDto()
                {
                    Name = phaseData.Name,
                    Duration = phaseData.GetDuration(),
                    Start = phaseData.Start / 1000.0,
                    End = phaseData.End / 1000.0,
                    DpsStats = BuildDPSData(i),
                    DpsStatsTargets = BuildDPSTargetsData(i),
                    DmgStatsTargets = BuildDMGStatsTargetsData(i),
                    DmgStats = BuildDMGStatsData(i),
                    DefStats = BuildDefenseData(i),
                    HealStats = BuildSupportData(i),
                    BoonStats = BuildBuffUptimeData(_statistics.PresentBoons, i),
                    OffBuffStats = BuildBuffUptimeData(_statistics.PresentOffbuffs, i),
                    DefBuffStats = BuildBuffUptimeData(_statistics.PresentDefbuffs, i),
                    PersBuffStats = BuildPersonalBuffUptimeData(persBuffDict, i),
                    BoonGenSelfStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "self"),
                    BoonGenGroupStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "group"),
                    BoonGenOGroupStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "off"),
                    BoonGenSquadStats = BuildBuffGenerationData(_statistics.PresentBoons, i, "squad"),
                    OffBuffGenSelfStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "self"),
                    OffBuffGenGroupStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "group"),
                    OffBuffGenOGroupStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "off"),
                    OffBuffGenSquadStats = BuildBuffGenerationData(_statistics.PresentOffbuffs, i, "squad"),
                    DefBuffGenSelfStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "self"),
                    DefBuffGenGroupStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "group"),
                    DefBuffGenOGroupStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "off"),
                    DefBuffGenSquadStats = BuildBuffGenerationData(_statistics.PresentDefbuffs, i, "squad"),
                    TargetsCondiStats = new List<List<BoonData>>(),
                    TargetsCondiTotals = new List<BoonData>(),
                    TargetsBoonTotals = new List<BoonData>(),
                    MechanicStats = BuildPlayerMechanicData(i),
                    EnemyMechanicStats = BuildEnemyMechanicData(i)
                };
                foreach (Target target in phaseData.Targets)
                {
                    phaseDto.Targets.Add(_log.FightData.Logic.Targets.IndexOf(target));
                }
                BuildDmgModifiersData(i, logData.DmgCommonModifiersBuffs, phaseDto.DmgModifiersCommon, phaseDto.DmgModifiersTargetsCommon);
                foreach (Target target in phaseData.Targets)
                {
                    phaseDto.TargetsCondiStats.Add(BuildTargetCondiData(i, target));
                    phaseDto.TargetsCondiTotals.Add(BuildTargetCondiUptimeData(i, target));
                    phaseDto.TargetsBoonTotals.Add(HasBoons(i, target) ? BuildTargetBoonData(i, target) : null);
                }
                // add phase markup to full fight graph
                phaseDto.MarkupLines = new List<double>();
                phaseDto.MarkupAreas = new List<AreaLabelDto>();
                for (int j = 1; j < _statistics.Phases.Count; j++)
                {
                    PhaseData curPhase = _statistics.Phases[j];
                    if (curPhase.Start < phaseData.Start || curPhase.End > phaseData.End || 
                        (curPhase.Start == phaseData.Start && curPhase.End == phaseData.End ))
                    {
                        continue;
                    }
                    if (phaseDto.SubPhases == null)
                    {
                        phaseDto.SubPhases = new List<int>();
                    }
                    phaseDto.SubPhases.Add(j); 
                    long start = curPhase.Start - phaseData.Start;
                    long end = curPhase.End - phaseData.Start;
                    if (curPhase.DrawStart) phaseDto.MarkupLines.Add(start / 1000.0);
                    if (curPhase.DrawEnd) phaseDto.MarkupLines.Add(end / 1000.0);
                    AreaLabelDto phaseArea = new AreaLabelDto
                    {
                        Start = start / 1000.0,
                        End = end / 1000.0,
                        Label = curPhase.Name,
                        Highlight = curPhase.DrawArea
                    };
                    phaseDto.MarkupAreas.Add(phaseArea);
                }
                if (phaseDto.MarkupAreas.Count == 0) phaseDto.MarkupAreas = null;
                if (phaseDto.MarkupLines.Count == 0) phaseDto.MarkupLines = null;
                logData.Phases.Add(phaseDto);
            }
            //
            double fightDuration = _log.FightData.FightDuration / 1000.0;
            TimeSpan duration = TimeSpan.FromSeconds(fightDuration);
            string durationString = duration.Minutes + "m " + duration.Seconds + "s " + duration.Milliseconds + "ms";
            if (duration.Hours > 0)
            {
                durationString = duration.Hours + "h " + durationString;
            }
            logData.EncounterDuration = durationString;
            logData.Success = _log.FightData.Success;
            logData.FightName = FilterStringChars(_log.FightData.Name);
            logData.FightIcon = _log.FightData.Logic.IconUrl;
            logData.CombatReplay = (_settings.ParseCombatReplay && _log.FightData.Logic.CanCombatReplay);
            logData.LightTheme = _settings.LightTheme;
            logData.SingleGroup = _log.PlayerList.Where(x => x.Account != ":Conjured Sword").Select(x => x.Group).Distinct().Count() == 1;
            logData.NoMechanics = _log.FightData.Logic.MechanicList.Count == 3;
            return ToJson(logData);
        }

        private bool HasBoons(int phaseIndex, Target target)
        {
            Dictionary<long, Statistics.FinalTargetBuffs> conditions = _statistics.TargetBuffs[target][phaseIndex];
            foreach (Boon boon in _statistics.PresentBoons)
            {
                if (conditions.TryGetValue(boon.ID, out var uptime))
                {
                    if (uptime.Uptime > 0.0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string BuildDetails()
        {
            string scripts = "";
            for (var i = 0; i < _log.PlayerList.Count; i++) {
                Player player = _log.PlayerList[i];
                string playerScript = "logData.players[" + i + "].details = " + ToJson(BuildPlayerData(player)) + ";\r\n";
                scripts += playerScript;
            }
            for (int i = 0; i < _log.FightData.Logic.Targets.Count; i++)
            {
                Target target = _log.FightData.Logic.Targets[i];
                string targetScript = "logData.targets[" + i + "].details = " + ToJson(BuildTargetData(target)) + ";\r\n";
                scripts += targetScript;
            }
            return "<script>\r\n"+scripts + "\r\n</script>";
        }

        private string BuildMaps()
        {
            string skillsScript = "var usedSkills = " + ToJson(AssembleSkills(_usedSkills.Values)) + ";" +
                "var skillMap = {};" +
                "$.each(usedSkills, function(i, skill) {" +
                    "skillMap['s'+skill.id]=skill;" +
                "});";
            string boonsScript = "var usedBoons = " + ToJson(AssembleBoons(_usedBoons.Values)) + ";" +
                "var buffMap = {};" +
                "$.each(usedBoons, function(i, boon) {" +
                    "buffMap['b'+boon.id]=boon;" +
                "});";
            string mechanicsScript = "var mechanicMap = " + ToJson(BuildMechanics()) + ";";
            return "<script>\r\n" + skillsScript + "\r\n" + boonsScript + "\r\n" + mechanicsScript + "\r\n</script>";
        }

        private PlayerDetailsDto BuildPlayerData(Player player)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                DmgDistributions = new List<DmgDistributionDto>(),
                DmgDistributionsTargets = new List<List<DmgDistributionDto>>(),
                DmgDistributionsTaken = new List<DmgDistributionDto>(),
                BoonGraph = new List<List<BoonChartDataDto>>(),
                Rotation = new List<List<object[]>>(),
                Food = BuildPlayerFoodData(player),
                Minions = new List<PlayerDetailsDto>(),
                DeathRecap = BuildDeathRecap(player)
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                dto.Rotation.Add(BuildRotationData(player, i));
                dto.DmgDistributions.Add(BuildPlayerDMGDistData(player, null, i));
                List<DmgDistributionDto> dmgTargetsDto = new List<DmgDistributionDto>();
                foreach (Target target in _statistics.Phases[i].Targets)
                {
                    dmgTargetsDto.Add(BuildPlayerDMGDistData(player, target, i));
                }
                dto.DmgDistributionsTargets.Add(dmgTargetsDto);
                dto.DmgDistributionsTaken.Add(BuildDMGTakenDistData(player, i));
                dto.BoonGraph.Add(BuildBoonGraphData(player, i));
            }
            foreach (KeyValuePair<string, Minions> pair in player.GetMinions(_log))
            {
                dto.Minions.Add(BuildPlayerMinionsData(player, pair.Value));
            }

            return dto;
        }

        private PlayerDetailsDto BuildPlayerMinionsData(Player player, Minions minion)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                DmgDistributions = new List<DmgDistributionDto>(),
                DmgDistributionsTargets = new List<List<DmgDistributionDto>>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                List<DmgDistributionDto> dmgTargetsDto = new List<DmgDistributionDto>();
                foreach (Target target in _statistics.Phases[i].Targets)
                {
                    dmgTargetsDto.Add(BuildPlayerMinionDMGDistData(player, minion, target, i));
                }
                dto.DmgDistributionsTargets.Add(dmgTargetsDto);
                dto.DmgDistributions.Add(BuildPlayerMinionDMGDistData(player, minion, null, i));
            }
            return dto;
        }

        private PlayerDetailsDto BuildTargetData(Target target)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                DmgDistributions = new List<DmgDistributionDto>(),
                DmgDistributionsTaken = new List<DmgDistributionDto>(),
                BoonGraph = new List<List<BoonChartDataDto>>(),
                Rotation = new List<List<object[]>>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                if (_statistics.Phases[i].Targets.Contains(target))
                {
                    dto.DmgDistributions.Add(BuildTargetDMGDistData(target, i));
                    dto.DmgDistributionsTaken.Add(BuildDMGTakenDistData(target, i));
                    dto.Rotation.Add(BuildRotationData(target, i));
                    dto.BoonGraph.Add(BuildBoonGraphData(target, i));
                } else
                {
                    dto.DmgDistributions.Add(new DmgDistributionDto());
                    dto.DmgDistributionsTaken.Add(new DmgDistributionDto());
                    dto.Rotation.Add(new List<object[]>());
                    dto.BoonGraph.Add(new List<BoonChartDataDto>());
                }
            }

            dto.Minions = new List<PlayerDetailsDto>();
            foreach (KeyValuePair<string, Minions> pair in target.GetMinions(_log))
            {
                dto.Minions.Add(BuildTargetsMinionsData(target, pair.Value));
            }
            return dto;
        }

        private PlayerDetailsDto BuildTargetsMinionsData(Target target, Minions minion)
        {
            PlayerDetailsDto dto = new PlayerDetailsDto
            {
                DmgDistributions = new List<DmgDistributionDto>()
            };
            for (int i = 0; i < _statistics.Phases.Count; i++)
            {
                if (_statistics.Phases[i].Targets.Contains(target))
                {
                    dto.DmgDistributions.Add(BuildTargetMinionDMGDistData(target, minion, i));
                }
                else
                {
                    dto.DmgDistributions.Add(new DmgDistributionDto());
                }
            }
            return dto;
        }

        private List<BoonDto> AssembleBoons(ICollection<Boon> boons)
        {
            List<BoonDto> dtos = new List<BoonDto>();
            foreach (Boon boon in boons)
            {
                dtos.Add(new BoonDto()
                {
                    Id = boon.ID,
                    Name = boon.Name,
                    Icon = boon.Link,
                    Stacking = (boon.Type == Boon.BoonType.Intensity),
                    Consumable = (boon.Nature == Boon.BoonNature.Consumable),
                    Enemy = (boon.Source == Boon.BoonSource.Enemy)
                });
            }
            return dtos;
        }

        private List<SkillDto> AssembleSkills(ICollection<SkillItem> skills)
        {
            List<SkillDto> dtos = new List<SkillDto>();
            foreach (SkillItem skill in skills)
            {
                GW2APISkill apiSkill = skill.ApiSkill;
                SkillDto dto = new SkillDto() {
                    Id = skill.ID,
                    Name = skill.Name,
                    Icon = skill.Icon,
                    Aa = (apiSkill?.Slot == "Weapon_1")
                };
                dtos.Add(dto);
            }
            return dtos;
        }
 
        private string ToJson(object value)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = contractResolver
            };
            return JsonConvert.SerializeObject(value, settings);
        }
    }
}
