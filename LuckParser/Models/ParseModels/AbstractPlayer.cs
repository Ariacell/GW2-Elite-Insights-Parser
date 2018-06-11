﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LuckParser.Models.ParseModels
{
    public abstract class AbstractPlayer
    {
        protected ushort instid;
        private String character;
        // Boons
        private BoonDistribution boon_distribution = new BoonDistribution();
        private Dictionary<int, long> boon_presence = new Dictionary<int, long>();
        private Dictionary<int, BoonsGraphModel> boon_points = new Dictionary<int, BoonsGraphModel>();
        // DPS
        protected List<DamageLog> damage_logs = new List<DamageLog>();
        private List<DamageLog> damage_logsFiltered = new List<DamageLog>();
        // Minions
        private List<ushort> combatMinionIDList = new List<ushort>();
        private Dictionary<AgentItem, List<DamageLog>> minion_damage_logs = new Dictionary<AgentItem, List<DamageLog>>();
        private Dictionary<AgentItem, List<DamageLog>> minion_damage_logsFiltered = new Dictionary<AgentItem, List<DamageLog>>();
        // Taken damage
        protected List<DamageLog> damageTaken_logs = new List<DamageLog>();
        // Casts
        protected List<CastLog> cast_logs = new List<CastLog>();
        // Constructor
        public AbstractPlayer(AgentItem agent)
        {
            String[] name = agent.getName().Split('\0');
            character = name[0];
            instid = agent.getInstid();
        }
        // Getters
        public ushort getInstid()
        {
            return instid;
        }
        public string getCharacter()
        {
            return character;
        }
        public long getDeath(BossData bossData, List<CombatItem> combatList, long start, long end)
        {
            long offset = bossData.getFirstAware();
            CombatItem dead = combatList.FirstOrDefault(x => x.getSrcInstid() == instid && x.isStateChange().getEnum() == "CHANGE_DEAD" && x.getTime() >= start + offset && x.getTime() <= end + offset);
            if (dead != null && dead.getTime() > 0)
            {
                return dead.getTime();
            }
            return 0;
        }    
        public List<DamageLog> getDamageLogs(int instidFilter, BossData bossData, List<CombatItem> combatList, AgentData agentData, long start, long end)//isntid = 0 gets all logs if specefied sets and returns filterd logs
        {
            if (damage_logs.Count == 0)
            {
                setDamageLogs(bossData, combatList, agentData);
            }


            if (damage_logsFiltered.Count == 0)
            {
                setFilteredLogs(bossData, combatList, agentData);
            }
            if (instidFilter == 0)
            {
                return damage_logs.Where(x => x.getTime() >= start && x.getTime() <= end).ToList();
            }
            else
            {
                return damage_logsFiltered.Where( x => x.getTime() >= start && x.getTime() <= end).ToList();
            }
        }
        public List<DamageLog> getDamageTakenLogs(BossData bossData, List<CombatItem> combatList, AgentData agentData, MechanicData m_data, long start, long end)
        {
            if (damageTaken_logs.Count == 0)
            {
                setDamagetakenLogs(bossData, combatList, agentData, m_data);
            }
            return damageTaken_logs.Where(x => x.getTime() >= start && x.getTime() <= end).ToList();
        }
        public BoonDistribution getBoonDistribution(BossData bossData, SkillData skillData, List<CombatItem> combatList)
        {
            if (boon_distribution.Count == 0)
            {
                setBoonDistribution(bossData, skillData, combatList);
            }
            return boon_distribution;
        }
        public Dictionary<int, BoonsGraphModel> getBoonGraphs(BossData bossData, SkillData skillData, List<CombatItem> combatList)
        {
            if (boon_distribution.Count == 0)
            {
                setBoonDistribution(bossData, skillData, combatList);
            }
            return boon_points;
        }
        public Dictionary<int, long> getBoonPresence(BossData bossData, SkillData skillData, List<CombatItem> combatList)
        {
            if (boon_distribution.Count == 0)
            {
                setBoonDistribution(bossData, skillData, combatList);
            }
            return boon_presence;
        }
        public List<CastLog> getCastLogs(BossData bossData, List<CombatItem> combatList, AgentData agentData, long start, long end)
        {
            if (cast_logs.Count == 0)
            {
                setCastLogs(bossData, combatList, agentData);
            }
            return cast_logs.Where(x => x.getTime() >= start && x.getTime() <= end).ToList();

        }
        public Dictionary<AgentItem, List<DamageLog>> getMinionsDamageLogs(int instidFilter, BossData bossData, List<CombatItem> combatList, AgentData agentData)
        {
            if (minion_damage_logs.Count == 0)
            {
                // make sure the keys matches
                foreach (AgentItem agent in minion_damage_logsFiltered.Keys)
                {
                    minion_damage_logs[agent] = new List<DamageLog>();
                }
                setMinionsDamageLogs(0, bossData, combatList, agentData, minion_damage_logs);
            }

            if (minion_damage_logsFiltered.Count == 0)
            {
                // make sure the keys matches
                foreach (AgentItem agent in minion_damage_logs.Keys)
                {
                    minion_damage_logsFiltered[agent] = new List<DamageLog>();
                }
                setMinionsDamageLogs(bossData.getInstid(), bossData, combatList, agentData, minion_damage_logsFiltered);
            }
            if (instidFilter == 0)
            {
                return minion_damage_logs;
            }
            else
            {
                return minion_damage_logsFiltered;
            }
        }
        public List<DamageLog> getJustPlayerDamageLogs(int instidFilter, BossData bossData, List<CombatItem> combatList, AgentData agentData, long start, long end)
        {
            List<ushort> minionList = getCombatMinionList(bossData, combatList, agentData);
            return getDamageLogs(instidFilter, bossData, combatList, agentData, start, end).Where(x => !minionList.Contains(x.getInstidt())).ToList();
        }
        // privates
        protected void addDamageLog(long time, ushort instid, CombatItem c, List<DamageLog> toFill)
        {
            LuckParser.Models.ParseEnums.StateChange state = c.isStateChange();
            if (instid == c.getDstInstid() && c.getIFF().getEnum() == "FOE")
            {
                if (state.getEnum() == "NORMAL" && c.isBuffremove().getID() == 0)
                {
                    if (c.isBuff() == 1 && c.getBuffDmg() != 0)//condi
                    {
                        toFill.Add(new DamageLogCondition(time, c));
                    }
                    else if (c.isBuff() == 0 && c.getValue() != 0)//power
                    {
                        toFill.Add(new DamageLogPower(time, c));
                    }
                    else if (c.getResult().getID() == 5 || c.getResult().getID() == 6 || c.getResult().getID() == 7)
                    {//Hits that where blinded, invulned, interupts
                        toFill.Add(new DamageLogPower(time, c));
                    }
                }
            }
        }
        // Setters
        protected abstract void setDamageLogs(BossData bossData, List<CombatItem> combatList, AgentData agentData);
        protected void setDamageTakenLog(long time, ushort instid, CombatItem c)
        {
            LuckParser.Models.ParseEnums.StateChange state = c.isStateChange();
            if (instid == c.getSrcInstid())
            {
                if (state.getID() == 0)
                {
                    if (c.isBuff() == 1 && c.getBuffDmg() != 0)
                    {
                        //inco,ing condi dmg not working or just not present?
                        // damagetaken.Add(c.getBuffDmg());
                        damageTaken_logs.Add(new DamageLogCondition(time, c));
                    }
                    else if (c.isBuff() == 0 && c.getValue() != 0)
                    {
                        damageTaken_logs.Add(new DamageLogPower(time, c));

                    }
                    else if (c.isBuff() == 0 && c.getValue() == 0)
                    {
                        damageTaken_logs.Add(new DamageLogPower(time, c));
                    }
                }
            }
        }
        private void setFilteredLogs(BossData bossData, List<CombatItem> combatList, AgentData agentData)
        {
            long time_start = bossData.getFirstAware();
            foreach (CombatItem c in combatList)
            {
                if (instid == c.getSrcInstid() || instid == c.getSrcMasterInstid())//selecting player
                {
                    long time = c.getTime() - time_start;
                    addDamageLog(time, bossData.getInstid(), c, damage_logsFiltered);
                }
            }
        }
        private void setBoonDistribution(BossData bossData, SkillData skillData, List<CombatItem> combatList)
        {
            BoonMap to_use = getBoonMap(bossData, skillData, combatList, true);
            List<Boon> boon_to_use = Boon.getAllBuffList();
            boon_to_use.AddRange(Boon.getCondiBoonList());
            long dur = bossData.getLastAware() - bossData.getFirstAware();
            int fight_duration = (int)(dur) / 1000;
            // Init boon presence points
            BoonsGraphModel boon_presence_points = new BoonsGraphModel("Number of Boons");
            for (int i = 0; i <= fight_duration; i++)
            {
                boon_presence_points.getBoonChart().Add(new Point(i, 0));
            }
            foreach (Boon boon in boon_to_use)
            {
                int boonid = boon.getID();
                if (to_use.ContainsKey(boonid))
                {
                    List<BoonLog> logs = to_use[boonid];
                    if (logs.Count == 0)
                    {
                        continue;
                    }
                    if (boon_distribution.ContainsKey(boonid))
                    {
                        continue;
                    }
                    boon_distribution[boonid] = new Dictionary<ushort, OverAndValue>();
                    BoonSimulator simulator = boon.getSimulator();
                    simulator.simulate(logs, dur);
                    long death = getDeath(bossData, combatList, 0, dur);
                    if (death > 0)
                    {
                        simulator.trim(death - bossData.getFirstAware());
                    }
                    else
                    {
                        simulator.trim(dur);
                    }
                    List<BoonSimulationItem> simulation = simulator.getSimulationResult();
                    foreach (BoonSimulationItem simul in simulation)
                    {
                        if (!boon_presence.ContainsKey(boonid))
                        {
                            boon_presence[boonid] = simul.getItemDuration();
                        }
                        else
                        {
                            boon_presence[boonid] += simul.getItemDuration();
                        }
                        foreach (ushort src in simul.getSrc())
                        {
                            if (!boon_distribution[boonid].ContainsKey(src))
                            {
                                boon_distribution[boonid][src] = new OverAndValue(simul.getDuration(src), simul.getOverstack(src));
                            }
                            else
                            {
                                OverAndValue toModify = boon_distribution[boonid][src];
                                toModify.value += simul.getDuration(src);
                                toModify.overstack += simul.getOverstack(src);
                                boon_distribution[boonid][src] = toModify;
                            }
                        }
                    }
                    // full precision
                    List<Point> toFill = new List<Point>();                   
                    List<Point> toFillPresence = new List<Point>();
                    for (int i = 0; i < dur + 1; i++)
                    {
                        toFill.Add(new Point(i, 0));
                        toFillPresence.Add(new Point(i, 0));
                    }
                    foreach (BoonSimulationItem simul in simulation)
                    {
                        int start = (int)simul.getStart();
                        int end = (int)simul.getEnd();
                        
                        for (int i = start; i <= end; i++)
                        {
                            toFill[i] = new Point(i, simul.getStack(i));
                            toFillPresence[i] = new Point(i, simul.getItemDuration() > 0 ? 1 : 0);
                        }
                    }
                    // reduce precision to seconds
                    List<Point> reducedPrecision = new List<Point>();
                    List<Point> boonPresence = boon_presence_points.getBoonChart();
                    for (int i = 0; i <= fight_duration; i++)
                    {
                        reducedPrecision.Add(new Point(i, toFill[1000 * i].Y));
                        if (Boon.getBoonList().Select(x => x.getID()).Contains(boonid))
                            boonPresence[i] = new Point(i, boonPresence[i].Y + toFillPresence[1000 * i].Y);
                    }
                    boon_points[boonid] = new BoonsGraphModel(boon.getName(), reducedPrecision);
                }
            }
            boon_points[-2] = boon_presence_points;
        }
        private void setCastLogs(BossData bossData, List<CombatItem> combatList, AgentData agentData)
        {
            long time_start = bossData.getFirstAware();
            CastLog curCastLog = null;

            foreach (CombatItem c in combatList)
            {
                LuckParser.Models.ParseEnums.StateChange state = c.isStateChange();
                if (state.getID() == 0)
                {
                    if (instid == c.getSrcInstid())//selecting player as caster
                    {
                        if (c.isActivation().getID() > 0)
                        {
                            if (c.isActivation().getID() < 3)
                            {
                                long time = c.getTime() - time_start;
                                curCastLog = new CastLog(time, c.getSkillID(), c.getValue(), c.isActivation());
                            }
                            else
                            {
                                if (curCastLog != null)
                                {
                                    if (curCastLog.getID() == c.getSkillID())
                                    {
                                        curCastLog = new CastLog(curCastLog.getTime(), curCastLog.getID(), curCastLog.getExpDur(), curCastLog.startActivation(), c.getValue(), c.isActivation());
                                        cast_logs.Add(curCastLog);
                                        curCastLog = null;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (state.getID() == 11)
                {//Weapon swap
                    if (instid == c.getSrcInstid())//selecting player as caster
                    {
                        if ((int)c.getDstAgent() == 4 || (int)c.getDstAgent() == 5)
                        {
                            long time = c.getTime() - time_start;
                            curCastLog = new CastLog(time, -2, (int)c.getDstAgent(), c.isActivation());
                            cast_logs.Add(curCastLog);
                            curCastLog = null;
                        }
                    }
                }
            }
        }
        private void setMinionsDamageLogs(int instidFilter, BossData bossData, List<CombatItem> combatList, AgentData agentData, Dictionary<AgentItem, List<DamageLog>> toFill)
        {
            List<ushort> minionList = getCombatMinionList(bossData, combatList, agentData);
            foreach (int petid in minionList)
            {
                AgentItem agent = agentData.getNPCAgentList().FirstOrDefault(x => x.getInstid() == petid);
                if (agent != null)
                {
                    List<DamageLog> damageLogs = getDamageLogs(instidFilter, bossData, combatList, agentData, 0, bossData.getAwareDuration()).Where(x => x.getSrcAgent() == agent.getAgent()).ToList();
                    if (damageLogs.Count == 0)
                    {
                        continue;
                    }
                    AgentItem key = toFill.Keys.ToList().FirstOrDefault(x => x.getName() == agent.getName());
                    if (key == null)
                    {
                        toFill[agent] = damageLogs;
                    }
                    else
                    {
                        toFill[key].AddRange(damageLogs);
                    }
                }
            }
        }
        protected abstract void setDamagetakenLogs(BossData bossData, List<CombatItem> combatList, AgentData agentData, MechanicData m_data);
        // private getters
        private BoonMap getBoonMap(BossData bossData, SkillData skillData, List<CombatItem> combatList, bool add_condi)
        {
            BoonMap boon_map = new BoonMap();
            boon_map.add(Boon.getAllBuffList());
            // This only happens for bosses
            if (add_condi)
            {
                boon_map.add(Boon.getCondiBoonList());
            }
            // Fill in Boon Map
            long time_start = bossData.getFirstAware();
            long fight_duration = bossData.getLastAware() - time_start;

            foreach (CombatItem c in combatList)
            {
                if (c.isBuff() != 1 || !boon_map.ContainsKey(c.getSkillID()))
                {
                    continue;
                }
                long time = c.getTime() - time_start;
                ushort dst = c.isBuffremove().getID() == 0 ? c.getDstInstid() : c.getSrcInstid();
                if (instid == dst && time >= 0 && time <= fight_duration)
                {
                    ushort src = c.getSrcMasterInstid() > 0 ? c.getSrcMasterInstid() : c.getSrcInstid();
                    if (c.isBuffremove().getID() == 0)
                    {
                        boon_map[c.getSkillID()].Add(new BoonLog(time, src, c.getValue(), 0));
                    }
                    else if (Boon.removePermission(c.getSkillID(), c.isBuffremove().getID(), c.getIFF().getID()))
                    {
                        if (c.isBuffremove().getID() == 1)//All
                        {
                            List<BoonLog> loglist = boon_map[c.getSkillID()];
                            for (int cnt = loglist.Count() - 1; cnt >= 0; cnt--)
                            {
                                BoonLog curBL = loglist[cnt];
                                if (curBL.getTime() + curBL.getValue() > time)
                                {
                                    long subtract = (curBL.getTime() + curBL.getValue()) - time;
                                    loglist[cnt].addValue(-subtract);
                                    // add removed as overstack
                                    loglist[cnt].addOverstack((ushort)subtract);
                                }
                            }

                        }
                        else if (c.isBuffremove().getID() == 2)//Single
                        {
                            List<BoonLog> loglist = boon_map[c.getSkillID()];
                            int cnt = loglist.Count() - 1;
                            BoonLog curBL = loglist[cnt];
                            if (curBL.getTime() + curBL.getValue() > time)
                            {
                                long subtract = (curBL.getTime() + curBL.getValue()) - time;
                                loglist[cnt].addValue(-subtract);
                                // add removed as overstack
                                loglist[cnt].addOverstack((ushort)subtract);
                            }
                        }
                        else if (c.isBuffremove().getID() == 3)//Manuel
                        {
                            List<BoonLog> loglist = boon_map[c.getSkillID()];
                            for (int cnt = loglist.Count() - 1; cnt >= 0; cnt--)
                            {
                                BoonLog curBL = loglist[cnt];
                                long ctime = curBL.getTime() + curBL.getValue();
                                if (ctime > time)
                                {
                                    long subtract = (curBL.getTime() + curBL.getValue()) - time;
                                    loglist[cnt].addValue(-subtract);
                                    // add removed as overstack
                                    loglist[cnt].addOverstack((ushort)subtract);
                                    break;
                                }
                            }

                        }
                    }

                }
            }
            return boon_map;
        }
        private List<ushort> getCombatMinionList(BossData bossData, List<CombatItem> combatList, AgentData agentData)
        {
            if (combatMinionIDList.Count == 0)
            {
                combatMinionIDList = combatList.Where(x => x.getSrcMasterInstid() == instid && ((x.getValue() != 0 && x.isBuff() == 0) || (x.isBuff() == 1 && x.getBuffDmg() != 0))).Select(x => x.getSrcInstid()).Distinct().ToList();
            }
            return combatMinionIDList;
        }

    }
}
