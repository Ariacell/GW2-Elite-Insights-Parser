﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuckParser.Models.ParseModels;
using LuckParser.Models.ParseEnums;
using System.Drawing;
using System.Net;
using System.IO.Compression;
using System.ComponentModel;
//recomend CTRL+M+O to collapse all
using LuckParser.Models.DataModels;

//recommend CTRL+M+O to collapse all
namespace LuckParser.Controllers
{
    public class Parser
    {
        private GW2APIController APIController = new GW2APIController();            

        //Main data storage after binary parse
        private LogData log_data;
        private BossData boss_data;
        private AgentData agent_data = new AgentData();
        private SkillData skill_data = new SkillData();
        private CombatData combat_data = new CombatData();
        private MechanicData mech_data = new MechanicData();
        private List<Player> p_list = new List<Player>();
        private Boss boss;

        // Public Methods
        public LogData getLogData()
        {
            return log_data;
        }
        public BossData getBossData()
        {
            return boss_data;
        }

        public ParsedLog GetParsedLog()
        {
            return new ParsedLog(log_data, boss_data, agent_data, skill_data, combat_data, mech_data, p_list, boss);
        }

        //Main Parse method------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Parses the given log
        /// </summary>
        /// <param name="bg">BackgroundWorker handling the log</param>
        /// <param name="row">GridRow object bound to the UI</param>
        /// <param name="evtc">The path to the log to parse</param>
        /// <returns></returns>
        public void ParseLog(GridRow row, string evtc)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (FileStream origstream = new FileStream(evtc, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (evtc.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var arch = new ZipArchive(origstream, ZipArchiveMode.Read))
                        {
                            if (arch.Entries.Count != 1)
                            {
                                throw new CancellationException(row, new InvalidDataException("Invalid Archive"));
                            }
                            using (var data = arch.Entries[0].Open())
                            {
                                data.CopyTo(stream);
                            }
                        }
                    }
                    else
                    {
                        origstream.CopyTo(stream);
                    }
                }
                stream.Position = 0;

                try
                {
                    row.BgWorker.ThrowIfCanceled(row, "Cancelled");
                    row.BgWorker.UpdateProgress(row, "15% - Parsing boss data...", 15);
                    parseBossData(stream);
                    row.BgWorker.ThrowIfCanceled(row, "Cancelled");
                    row.BgWorker.UpdateProgress(row, "20% - Parsing agent data...", 20);
                    parseAgentData(stream);
                    row.BgWorker.ThrowIfCanceled(row, "Cancelled");
                    row.BgWorker.UpdateProgress(row, "25% - Parsing skill data...", 25);
                    parseSkillData(stream);
                    row.BgWorker.ThrowIfCanceled(row, "Cancelled");
                    row.BgWorker.UpdateProgress(row, "30% - Parsing combat list...", 30);
                    parseCombatList(stream);
                    row.BgWorker.ThrowIfCanceled(row, "Cancelled");
                    row.BgWorker.UpdateProgress(row, "35% - Pairing data...", 35);
                    fillMissingData(stream);
                    row.BgWorker.ThrowIfCanceled(row, "Cancelled");
                }
                catch (Exception ex)
                {
                    if (ex is CancellationException)
                    {
                        throw ex;
                    }

                    throw new CancellationException(row, ex);
                }
            }
        }

        //sub Parse methods
        /// <summary>
        /// Parses boss related data
        /// </summary>
        private void parseBossData(MemoryStream stream)
        {
            // 12 bytes: arc build version
            String build_version = ParseHelper.getString(stream, 12);
            this.log_data = new LogData(build_version);

            // 1 byte: skip
            ParseHelper.safeSkip(stream, 1);

            // 2 bytes: boss instance ID
            ushort instid = ParseHelper.getShort(stream);

            // 1 byte: position
            ParseHelper.safeSkip(stream, 1);

            //Save
            // TempData["Debug"] = build_version +" "+ instid.ToString() ;
            this.boss_data = new BossData(instid);
        }
        /// <summary>
        /// Parses agent related data
        /// </summary>
        private void parseAgentData(MemoryStream stream)
        {
            // 4 bytes: player count
            int player_count = ParseHelper.getInt(stream);

            // 96 bytes: each player
            for (int i = 0; i < player_count; i++)
            {
                // 8 bytes: agent
                ulong agent = ParseHelper.getULong(stream);

                // 4 bytes: profession
                uint prof = ParseHelper.getUInt(stream);

                // 4 bytes: is_elite
                uint is_elite = ParseHelper.getUInt(stream);

                // 2 bytes: toughness
                int toughness = ParseHelper.getShort(stream);
                // skip concentration
                ParseHelper.safeSkip(stream, 2);
                // 2 bytes: healing
                int healing = ParseHelper.getShort(stream);
                ParseHelper.safeSkip(stream, 2);
                // 2 bytes: condition
                int condition = ParseHelper.getShort(stream);
                ParseHelper.safeSkip(stream, 2);
                // 68 bytes: name
                String name = ParseHelper.getString(stream, 68);
                //Save
                Agent a = new Agent(agent, name, prof, is_elite);
                string agent_prof = "";
                if (a != null)
                {
                    agent_prof = a.getProf(this.log_data.getBuildVersion(), APIController);
                    // NPC
                    if (agent_prof == "NPC")
                    {
                        agent_data.addItem( new AgentItem(agent, name, a.getName() + ":" + prof.ToString().PadLeft(5, '0')), agent_prof);//a.getName() + ":" + String.format("%05d", prof)));
                    }
                    // Gadget
                    else if (agent_prof == "GDG")
                    {
                        agent_data.addItem( new AgentItem(agent, name, a.getName() + ":" + (prof & 0x0000ffff).ToString().PadLeft(5, '0')), agent_prof);//a.getName() + ":" + String.format("%05d", prof & 0x0000ffff)));
                    }
                    // Player
                    else
                    {
                        agent_data.addItem( new AgentItem(agent, name, agent_prof, toughness, healing, condition), agent_prof);
                    }
                }
                // Unknown
                else
                {
                    agent_data.addItem( new AgentItem(agent, name, prof.ToString(), toughness, healing, condition), agent_prof);
                }
            }

        }
        /// <summary>
        /// Parses skill related data
        /// </summary>
        private void parseSkillData(MemoryStream stream)
        {
            GW2APIController apiController = new GW2APIController();
            // 4 bytes: player count
            int skill_count = ParseHelper.getInt(stream);
            //TempData["Debug"] += "Skill Count:" + skill_count.ToString();
            // 68 bytes: each skill
            for (int i = 0; i < skill_count; i++)
            {
                // 4 bytes: skill ID
                int skill_id = ParseHelper.getInt(stream);

                // 64 bytes: name
                String name = ParseHelper.getString(stream, 64);
                String nameTrim = name.Replace("\0", "");
                int n;
                bool isNumeric = int.TryParse(nameTrim, out n);//check to see if name was id
                if (n == skill_id && skill_id != 0)
                {
                    //was it a known boon?
                    foreach (Boon b in Boon.getBoonList())
                    {
                        if (skill_id == b.getID())
                        {
                            nameTrim = b.getName();
                        }
                    }
                }
                //Save

                SkillItem skill = new SkillItem(skill_id, nameTrim);

                skill.SetGW2APISkill(apiController);
                skill_data.addItem(skill);
            }
        }
        /// <summary>
        /// Parses combat related data
        /// </summary>
        private void parseCombatList(MemoryStream stream)
        {
            // 64 bytes: each combat
            while (stream.Length - stream.Position >= 64)
            {
                // 8 bytes: time
                long time = ParseHelper.getLong(stream);

                // 8 bytes: src_agent
                ulong src_agent = ParseHelper.getULong(stream);

                // 8 bytes: dst_agent
                ulong dst_agent = ParseHelper.getULong(stream);

                // 4 bytes: value
                int value = ParseHelper.getInt(stream);

                // 4 bytes: buff_dmg
                int buff_dmg = ParseHelper.getInt(stream);

                // 2 bytes: overstack_value
                ushort overstack_value = ParseHelper.getShort(stream);

                // 2 bytes: skill_id
                ushort skill_id = ParseHelper.getShort(stream);

                // 2 bytes: src_instid
                ushort src_instid = ParseHelper.getShort(stream);

                // 2 bytes: dst_instid
                ushort dst_instid = ParseHelper.getShort(stream);

                // 2 bytes: src_master_instid
                ushort src_master_instid = ParseHelper.getShort(stream);

                // 9 bytes: garbage
                ParseHelper.safeSkip(stream, 9);

                // 1 byte: iff
                //IFF iff = IFF.getEnum(f.read());
                IFF iff = new IFF(Convert.ToByte(stream.ReadByte())); //Convert.ToByte(stream.ReadByte());

                // 1 byte: buff
                ushort buff = (ushort)stream.ReadByte();

                // 1 byte: result
                //Result result = Result.getEnum(f.read());
                ParseEnum.Result result = ParseEnum.getResult(Convert.ToByte(stream.ReadByte()));

                // 1 byte: is_activation
                //Activation is_activation = Activation.getEnum(f.read());
                ParseEnum.Activation is_activation = ParseEnum.getActivation(Convert.ToByte(stream.ReadByte()));

                // 1 byte: is_buffremove
                //BuffRemove is_buffremove = BuffRemove.getEnum(f.read());
                ParseEnum.BuffRemove is_buffremoved = ParseEnum.getBuffRemove(Convert.ToByte(stream.ReadByte()));

                // 1 byte: is_ninety
                ushort is_ninety = (ushort)stream.ReadByte();

                // 1 byte: is_fifty
                ushort is_fifty = (ushort)stream.ReadByte();

                // 1 byte: is_moving
                ushort is_moving = (ushort)stream.ReadByte();

                // 1 byte: is_statechange
                //StateChange is_statechange = StateChange.getEnum(f.read());
                ParseEnum.StateChange is_statechange = ParseEnum.getStateChange(Convert.ToByte(stream.ReadByte()));

                // 1 byte: is_flanking
                ushort is_flanking = (ushort)stream.ReadByte();

                // 1 byte: is_flanking
                ushort is_shields = (ushort)stream.ReadByte();
                // 2 bytes: garbage
                ParseHelper.safeSkip(stream, 2);

                //save
                // Add combat
                combat_data.addItem(new CombatItem(time, src_agent, dst_agent, value, buff_dmg, overstack_value, skill_id,
                        src_instid, dst_instid, src_master_instid, iff, buff, result, is_activation, is_buffremoved,
                        is_ninety, is_fifty, is_moving, is_statechange, is_flanking, is_shields));
            }
        }

        /// <summary>
        /// Parses all the data again and link related stuff to each other
        /// </summary>
        private void fillMissingData(MemoryStream stream)
        {
            // Set Agent instid, first_aware and last_aware
            List<CombatItem> combat_list = combat_data.getCombatList();
            foreach (CombatItem c in combat_list)
            {
                foreach (AgentItem a in agent_data.getAllAgentsList())
                {
                    if (a.getInstid() == 0 && a.getAgent() == c.getSrcAgent() && c.isStateChange() == ParseEnum.StateChange.Normal)
                    {
                        a.setInstid(c.getSrcInstid());
                    }
                    if (a.getInstid() != 0 && a.getAgent() == c.getSrcAgent())
                    {
                        if (a.getFirstAware() == 0)
                        {
                            a.setFirstAware(c.getTime());
                        } else
                        {
                            a.setLastAware(c.getTime());
                        }
                    }
                }
            }

            foreach (CombatItem c in combat_list)
            {
                if (c.getSrcMasterInstid() != 0)
                {
                    AgentItem master = agent_data.getAllAgentsList().Find(x => x.getInstid() == c.getSrcMasterInstid() && x.getFirstAware() < c.getTime() && c.getTime() < x.getLastAware());
                    if (master != null)
                    {
                        AgentItem minion = agent_data.getAllAgentsList().Find(x => x.getAgent() == c.getSrcAgent() && x.getFirstAware() < c.getTime() && c.getTime() < x.getLastAware());
                        if (minion != null)
                        {
                            minion.setMasterAgent(master.getAgent());
                        }
                    }
                }
            }

            agent_data.clean();

            // Set Boss data agent, instid, first_aware, last_aware and name
            List<AgentItem> NPC_list = agent_data.getNPCAgentList();
            HashSet<ulong> multiple_boss = new HashSet<ulong>();
            foreach (AgentItem NPC in NPC_list)
            {
                if (NPC.getProf().EndsWith(boss_data.getID().ToString()))
                {
                    if (boss_data.getAgent() == 0)
                    {
                        boss_data.setAgent(NPC.getAgent());
                        boss_data.setInstid(NPC.getInstid());
                        boss_data.setFirstAware(NPC.getFirstAware());
                        boss_data.setName(NPC.getName());
                        boss_data.setTough(NPC.getToughness());
                    }
                    multiple_boss.Add(NPC.getAgent());
                    boss_data.setLastAware(NPC.getLastAware());
                }
            }
            if (multiple_boss.Count > 1)
            {
                agent_data.cleanInstid(boss_data.getInstid());
            }
            
            AgentItem bossAgent = agent_data.GetAgent(boss_data.getAgent());
            boss = new Boss(bossAgent);
            List<Point> bossHealthOverTime = new List<Point>();

            // Grab values threw combat data
            foreach (CombatItem c in combat_list)
            {
                if (c.getSrcInstid() == boss_data.getInstid() && c.isStateChange() == ParseEnum.StateChange.MaxHealthUpdate)//max health update
                {
                    boss_data.setHealth((int)c.getDstAgent());

                }
                if (c.isStateChange() == ParseEnum.StateChange.PointOfView && log_data.getPOV() == "N/A")//Point of View
                {
                    ulong pov_agent = c.getSrcAgent();
                    foreach (AgentItem p in agent_data.getPlayerAgentList())
                    {
                        if (pov_agent == p.getAgent())
                        {
                            log_data.setPOV(p.getName());
                        }
                    }

                }
                else if (c.isStateChange() == ParseEnum.StateChange.LogStart)//Log start
                {
                    log_data.setLogStart(c.getValue());
                }
                else if (c.isStateChange() == ParseEnum.StateChange.LogEnd)//log end
                {
                    log_data.setLogEnd(c.getValue());

                }
                //set health update
                if (c.getSrcInstid() == boss_data.getInstid() && c.isStateChange() == ParseEnum.StateChange.HealthUpdate)
                {
                    bossHealthOverTime.Add(new Point ( (int)(c.getTime() - boss_data.getFirstAware()), (int)c.getDstAgent() ));
                }

            }


            // Dealing with second half of Xera | ((22611300 * 0.5) + (25560600 * 0.5)

            if (boss_data.getID() == 16246)
            {
                int xera_2_instid = 0;
                foreach (AgentItem NPC in NPC_list)
                {
                    if (NPC.getProf().Contains("16286"))
                    {
                        bossHealthOverTime = new List<Point>();//reset boss health over time
                        xera_2_instid = NPC.getInstid();
                        boss_data.setHealth(24085950);
                        boss.addPhaseData(boss_data.getLastAware());
                        boss.addPhaseData(NPC.getFirstAware());
                        boss_data.setLastAware(NPC.getLastAware());
                        foreach (CombatItem c in combat_list)
                        {
                            if (c.getSrcInstid() == xera_2_instid)
                            {
                                c.setSrcInstid(boss_data.getInstid());
                            }
                            if (c.getDstInstid() == xera_2_instid)
                            {
                                c.setDstInstid(boss_data.getInstid());
                            }
                            //set health update
                            if (c.getSrcInstid() == boss_data.getInstid() && c.isStateChange() == ParseEnum.StateChange.HealthUpdate)
                            {
                                bossHealthOverTime.Add(new Point ( (int)(c.getTime() - boss_data.getFirstAware()), (int)c.getDstAgent() ));
                            }
                        }
                        break;
                    }
                }
            }
            //Dealing with Deimos split
            if (boss_data.getID() == 17154)
            {
                int deimos_2_instid = 0;
                foreach (AgentItem NPC in agent_data.getGadgetAgentList())
                {
                    if (NPC.getProf().Contains("08467") || NPC.getProf().Contains("08471"))
                    {
                        deimos_2_instid = NPC.getInstid();
                        long oldAware = boss_data.getLastAware();
                        if (NPC.getLastAware() < boss_data.getLastAware())
                        {
                            // No split
                            break;
                        }
                        boss.addPhaseData(NPC.getFirstAware() >= oldAware ? NPC.getFirstAware() : oldAware);
                        boss_data.setLastAware(NPC.getLastAware());
                        //List<CombatItem> fuckyou = combat_list.Where(x => x.getDstInstid() == deimos_2_instid ).ToList().Sum(x);
                        //int stop = 0;
                        foreach (CombatItem c in combat_list)
                        {
                            if (c.getTime() > oldAware)
                            {
                                if (c.getSrcInstid() == deimos_2_instid)
                                {
                                    c.setSrcInstid(boss_data.getInstid());

                                }
                                if (c.getDstInstid() == deimos_2_instid)
                                {
                                    c.setDstInstid(boss_data.getInstid());
                                }
                            }

                        }
                        break;
                    }
                }
            }
            boss_data.setHealthOverTime(bossHealthOverTime);//after xera in case of change

            // Re parse to see if the boss is dead and update last aware
            foreach (CombatItem c in combat_list)
            {
                //set boss dead
                if (c.isStateChange() == ParseEnum.StateChange.Reward)//got reward
                {
                    log_data.setBossKill(true);
                    boss_data.setLastAware(c.getTime());
                    break;
                }
                //set boss dead
                if (c.getSrcInstid() == boss_data.getInstid() && c.isStateChange() == ParseEnum.StateChange.ChangeDead && !log_data.getBosskill())//change dead
                {
                    log_data.setBossKill(true);
                    boss_data.setLastAware(c.getTime());
                }

            }

            //players
            if (p_list.Count == 0)
            {

                //Fix Disconected players
                List<AgentItem> playerAgentList = agent_data.getPlayerAgentList();

                foreach (AgentItem playerAgent in playerAgentList)
                {
                    List<CombatItem> lp = combat_data.getStates(playerAgent.getInstid(), ParseEnum.StateChange.Despawn, boss_data.getFirstAware(), boss_data.getLastAware());
                    Player player = new Player(playerAgent);
                    bool skip = false;
                    foreach (Player p in p_list)
                    {
                        if (p.getAccount() == player.getAccount())//is this a copy of original?
                        {
                            skip = true;
                        }
                    }
                    if (skip)
                    {
                        continue;
                    }
                    if (lp.Count > 0)
                    {
                        //make all actions of other isntances to original instid
                        int extra_login_Id = 0;
                        foreach (AgentItem extra in NPC_list)
                        {
                            if (extra.getAgent() == playerAgent.getAgent())
                            {

                                extra_login_Id = extra.getInstid();


                                foreach (CombatItem c in combat_list)
                                {
                                    if (c.getSrcInstid() == extra_login_Id)
                                    {
                                        c.setSrcInstid(playerAgent.getInstid());
                                    }
                                    if (c.getDstInstid() == extra_login_Id)
                                    {
                                        c.setDstInstid(playerAgent.getInstid());
                                    }

                                }
                                break;
                            }
                        }

                        player.SetDC(lp[0].getTime());
                        p_list.Add(player);
                    }
                    else//didnt dc
                    {
                        if (player.GetDC() == 0)
                        {
                            p_list.Add(player);
                        }

                    }
                }

            }
            // Sort
            p_list = p_list.OrderBy(a => int.Parse(a.getGroup())).ToList();//p_list.Sort((a, b)=>int.Parse(a.getGroup()) - int.Parse(b.getGroup()))
        }

    }
}
