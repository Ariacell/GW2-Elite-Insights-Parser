﻿using LuckParser.Controllers;
using LuckParser.Models.DataModels;
using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public class Player : AbstractMasterPlayer
    {
        // Fields
        private String account;
        private int group;
        private long dcd = 0;//time in ms the player dcd
       
        private List<Tuple<Boon,long>> consumeList = new List<Tuple<Boon, long>>();
        //weaponslist
        private string[] weapons_array;

        // Constructors
        public Player(AgentItem agent, bool noSquad) : base(agent)
        {
            String[] name = agent.getName().Split('\0');
            account = name[1];
            group = noSquad ? 1 : int.Parse(name[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        // Getters

        public string GetAccount()
        {
            return account;
        }
    
        public int GetGroup()
        {
            return group;
        }

        public int GetToughness()
        {
            return agent.getToughness();
        }

        public int GetHealing()
        {
            return agent.getHealing();
        }

        public int GetCondition()
        {
            return agent.getCondition();
        }

        public int GetConcentration()
        {
            return agent.getConcentration();
        }
        // Public methods
        public int[] GetCleanses(ParsedLog log, long start, long end) {
            long time_start = log.GetBossData().getFirstAware();
            int[] cleanse = { 0, 0 };
            foreach (CombatItem c in log.GetCombatList().Where(x=>x.isStateChange() == ParseEnum.StateChange.Normal && x.isBuff() == 1 && x.getTime() >= (start + time_start) && x.getTime() <= (end + time_start)))
            {
                if (c.isActivation() == ParseEnum.Activation.None)
                {
                    if ((agent.getInstid() == c.getDstInstid() || agent.getInstid() == c.getDstMasterInstid()) && c.getIFF() == ParseEnum.IFF.Friend && (c.isBuffremove() != ParseEnum.BuffRemove.None))
                    {
                        long time = c.getTime() - time_start;
                        if (time > 0)
                        {
                            if (Boon.getCondiBoonList().Exists(x=>x.getID() == c.getSkillID()))
                            {
                                cleanse[0]++;
                                cleanse[1] += c.getBuffDmg();
                            }
                        }
                    }
                }
            }
            return cleanse;
        }
        public int[] GetReses(ParsedLog log, long start, long end)
        {
            List<CastLog> cls = GetCastLogs(log, start, end);
            int[] reses = { 0, 0 };
            foreach (CastLog cl in cls) {
                if (cl.getID() == 1066)
                {
                    reses[0]++;
                    reses[1] += cl.getActDur();
                }
            }
            return reses;
        }
        public string[] GetWeaponsArray(ParsedLog log)
        {
            if (weapons_array == null)
            {
                EstimateWeapons( log);
            }
            return weapons_array;
        }
        
        public long GetDC()
        {
            return dcd;
        }
        public void SetDC(long value)
        {
            dcd = value;
        }
        public List<Tuple<Boon, long>> GetConsumablesList(ParsedLog log, long start, long end)
        {
            if (consumeList.Count == 0)
            {
                SetConsumablesList(log);
            }
            return consumeList.Where(x => x.Item2 >= start && x.Item2 <= end).ToList() ;
        }
        
        // Private Methods
        private void EstimateWeapons(ParsedLog log)
        {
            string[] weapons = new string[4];//first 2 for first set next 2 for second set
            SkillData s_list = log.GetSkillData();
            List<CastLog> casting = GetCastLogs(log, 0, log.GetBossData().getAwareDuration());      
            int swapped = 0;//4 for first set and 5 for next
            long swappedTime = 0;
            List<CastLog> swaps = casting.Where(x => x.getID() == -2).Take(2).ToList();
            // If the player never swapped, assume they are on their first set
            if (swaps.Count == 0)
            {
                swapped = 4;
            }
            // if the player swapped once, check on which set they started
            else if (swaps.Count == 1)
            {
                swapped = swaps.First().getExpDur() == 4 ? 5 : 4;
            }
            foreach (CastLog cl in casting)
            {
                GW2APISkill apiskill = null;
                SkillItem skill = s_list.FirstOrDefault(x => x.getID() == cl.getID());
                if (skill != null)
                {
                    apiskill = skill.GetGW2APISkill();
                }
                if (apiskill != null && cl.getTime() > swappedTime)
                {
                    if (apiskill.type == "Weapon" && apiskill.professions.Count() > 0 && (apiskill.categories == null || (apiskill.categories.Count() == 1 && apiskill.categories[0] == "Phantasm")))
                    {
                        if (apiskill.weapon_type == "Greatsword" || apiskill.weapon_type == "Staff" || apiskill.weapon_type == "Rifle" || apiskill.weapon_type == "Longbow" || apiskill.weapon_type == "Shortbow" || apiskill.weapon_type == "Hammer")
                        {
                            if (swapped == 4)
                            {
                                weapons[0] = apiskill.weapon_type;
                                weapons[1] = "2Hand";
                                continue;
                            }
                            else if (swapped == 5)
                            {
                                weapons[2] = apiskill.weapon_type;
                                weapons[3] = "2Hand";
                                continue;
                            }
                            continue;
                        }//2 handed
                        if (apiskill.weapon_type == "Focus" || apiskill.weapon_type == "Shield" || apiskill.weapon_type == "Torch" || apiskill.weapon_type == "Warhorn")
                        {
                            if (swapped == 4)
                            {

                                weapons[1] = apiskill.weapon_type;
                                continue;
                            }
                            else if (swapped == 5)
                            {

                                weapons[3] = apiskill.weapon_type;
                                continue;
                            }
                            continue;
                        }//OffHand
                        if (apiskill.weapon_type == "Axe" || apiskill.weapon_type == "Dagger" || apiskill.weapon_type == "Mace" || apiskill.weapon_type == "Pistol" || apiskill.weapon_type == "Sword" || apiskill.weapon_type == "Scepter")
                        {
                            if (apiskill.slot == "Weapon_1" || apiskill.slot == "Weapon_2" || apiskill.slot == "Weapon_3")
                            {
                                if (swapped == 4)
                                {

                                    weapons[0] = apiskill.weapon_type;
                                    continue;
                                }
                                else if (swapped == 5)
                                {

                                    weapons[2] = apiskill.weapon_type;
                                    continue;
                                }
                                continue;
                            }
                            if (apiskill.slot == "Weapon_4" || apiskill.slot == "Weapon_5")
                            {
                                if (swapped == 4)
                                {

                                    weapons[1] = apiskill.weapon_type;
                                    continue;
                                }
                                else if (swapped == 5)
                                {

                                    weapons[3] = apiskill.weapon_type;
                                    continue;
                                }
                                continue;
                            }
                        }// 1 handed
                    }

                }
                else if (cl.getID() == -2)
                {
                    //wepswap  
                    swapped = cl.getExpDur();
                    swappedTime = cl.getTime();
                    continue;
                }
            }
            weapons_array = weapons;
        }    
        
        protected override void SetDamagetakenLogs(ParsedLog log)
        {
            long time_start = log.GetBossData().getFirstAware();               
            foreach (CombatItem c in log.GetDamageTakenData()) {
                if (agent.getInstid() == c.getDstInstid() && c.getTime() > log.GetBossData().getFirstAware() && c.getTime() < log.GetBossData().getLastAware()) {//selecting player as target
                    long time = c.getTime() - time_start;
                    AddDamageTakenLog(time, c);
                }
            }
        }  
        private void SetConsumablesList(ParsedLog log)
        {
            List<Boon> foodBoon = Boon.getFoodList();
            List<Boon> utilityBoon = Boon.getUtilityList();
            long time_start = log.GetBossData().getFirstAware();
            long fight_duration = log.GetBossData().getLastAware() - time_start;
            foreach (CombatItem c in log.GetBoonData())
            {
                if ( c.isBuffremove() != ParseEnum.BuffRemove.None || (c.isBuff() != 18 && c.isBuff() != 1) || agent.getInstid() != c.getDstInstid())
                {
                    continue;
                }
                var food = foodBoon.FirstOrDefault(x => x.getID() == c.getSkillID());
                var utility = utilityBoon.FirstOrDefault(x => x.getID() == c.getSkillID());
                if (food == null && utility == null)
                {
                    continue;
                }
                long time = c.getTime() - time_start;
                if (time <= fight_duration)
                {
                    consumeList.Add(new Tuple<Boon, long>(food ?? utility, time)); 
                }
            }
        }

        protected override void SetAdditionalCombatReplayData(ParsedLog log, int pollingRate)
        {
            // Down and deads
            List<CombatItem> status = log.GetCombatData().GetStates(GetInstid(), ParseEnum.StateChange.ChangeDown, log.GetBossData().getFirstAware(), log.GetBossData().getLastAware());
            status.AddRange(log.GetCombatData().GetStates(GetInstid(), ParseEnum.StateChange.ChangeUp, log.GetBossData().getFirstAware(), log.GetBossData().getLastAware()));
            status.AddRange(log.GetCombatData().GetStates(GetInstid(), ParseEnum.StateChange.ChangeDead, log.GetBossData().getFirstAware(), log.GetBossData().getLastAware()));
            status.AddRange(log.GetCombatData().GetStates(GetInstid(), ParseEnum.StateChange.Spawn, log.GetBossData().getFirstAware(), log.GetBossData().getLastAware()));
            status.AddRange(log.GetCombatData().GetStates(GetInstid(), ParseEnum.StateChange.Despawn, log.GetBossData().getFirstAware(), log.GetBossData().getLastAware()));
            status = status.OrderBy(x => x.getTime()).ToList();
            List<Tuple<long, long>> dead = new List<Tuple<long, long>>();
            List<Tuple<long, long>> down = new List<Tuple<long, long>>();
            List<Tuple<long, long>> dc = new List<Tuple<long, long>>();
            for (var i = 0; i < status.Count -1;i++)
            {
                CombatItem cur = status[i];
                CombatItem next = status[i + 1];
                if (cur.isStateChange().IsDown())
                {
                    down.Add(new Tuple<long, long>(cur.getTime() - log.GetBossData().getFirstAware(), next.getTime() - log.GetBossData().getFirstAware()));
                } else if (cur.isStateChange().IsDead())
                {
                    dead.Add(new Tuple<long, long>(cur.getTime() - log.GetBossData().getFirstAware(), next.getTime() - log.GetBossData().getFirstAware()));
                } else if (cur.isStateChange().IsDespawn())
                {
                    dc.Add(new Tuple<long, long>(cur.getTime() - log.GetBossData().getFirstAware(), next.getTime() - log.GetBossData().getFirstAware()));
                }
            }
            // check last value
            if (status.Count > 0)
            {
                CombatItem cur = status.Last();
                if (cur.isStateChange().IsDown())
                {
                    down.Add(new Tuple<long, long>(cur.getTime() - log.GetBossData().getFirstAware(), log.GetBossData().getAwareDuration()));
                }
                else if (cur.isStateChange().IsDead())
                {
                    dead.Add(new Tuple<long, long>(cur.getTime() - log.GetBossData().getFirstAware(), log.GetBossData().getAwareDuration()));
                }
                else if (cur.isStateChange().IsDespawn())
                {
                    dc.Add(new Tuple<long, long>(cur.getTime() - log.GetBossData().getFirstAware(), log.GetBossData().getAwareDuration()));
                }
            }
            replay.setStatus(down, dead, dc);
            // Boss related stuff
            log.GetBossData().getBossBehavior().getAdditionalPlayerData(replay, this, log);
        }

        protected override void SetCombatReplayIcon(ParsedLog log)
        {
            replay.setIcon(HTMLHelper.GetLink(GetProf()));
        }

        public override void AddMechanics(ParsedLog log)
        {
            MechanicData mech_data = log.GetMechanicData();
            BossData boss_data = log.GetBossData();
            CombatData combat_data = log.GetCombatData();
            List<Mechanic> bossMechanics = boss_data.getBossBehavior().getMechanics();
            long start = boss_data.getFirstAware();
            long end = boss_data.getLastAware();
            // Player status
            List<Mechanic> playerStatus = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.PlayerStatus).ToList();
            foreach (Mechanic mech in playerStatus)
            {
                List<CombatItem> toUse = new List<CombatItem>();
                switch (mech.GetSkill()) {
                    case -2:
                        toUse = combat_data.GetStates(GetInstid(), ParseEnum.StateChange.ChangeDead, start, end);                 
                        break;
                    case -3:
                        toUse = combat_data.GetStates(GetInstid(), ParseEnum.StateChange.ChangeDown, start, end);
                        break;
                    case 1066:
                        toUse = log.GetCastData().Where(x => x.getSkillID() == 1066 && x.getSrcInstid() == GetInstid() && x.isActivation().IsCasting()).ToList();
                        break;
                    default:
                        break;
                }
                foreach (CombatItem pnt in toUse)
                {
                    mech_data[mech].Add(new MechanicLog(pnt.getTime() - start, mech, this));
                }

            }
            //Player hit
            List<DamageLog> dls = GetDamageTakenLogs(log, 0, boss_data.getAwareDuration());
            List<Mechanic> skillOnPlayer = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.SkillOnPlayer).ToList();
            foreach (Mechanic mech in skillOnPlayer)
            {
                Mechanic.SpecialCondition condition = mech.GetSpecialCondition();
                foreach (DamageLog dLog in dls)
                {
                    if (condition != null && !condition(dLog.getDamage()))
                    {
                        continue;
                    }
                    if (dLog.getID() == mech.GetSkill() && dLog.getResult().IsHit())
                    {
                        mech_data[mech].Add(new MechanicLog(dLog.getTime(), mech, this));

                    }
                }
            }
            // Player boon
            List<Mechanic> playerBoon = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.PlayerBoon || x.GetMechType() == Mechanic.MechType.PlayerOnPlayer || x.GetMechType() == Mechanic.MechType.PlayerBoonRemove).ToList();
            foreach (Mechanic mech in playerBoon)
            {
                Mechanic.SpecialCondition condition = mech.GetSpecialCondition();
                foreach (CombatItem c in log.GetBoonData())
                {
                    if (condition != null && !condition(c.getValue()))
                    {
                        continue;
                    }
                    if (mech.GetMechType() == Mechanic.MechType.PlayerBoonRemove)
                    {
                        if (c.getSkillID() == mech.GetSkill() && c.isBuffremove() == ParseEnum.BuffRemove.Manual && GetInstid() == c.getSrcInstid())
                        {
                            mech_data[mech].Add(new MechanicLog(c.getTime() - start, mech, this));
                        }
                    } else
                    {

                        if (c.getSkillID() == mech.GetSkill() && c.isBuffremove() == ParseEnum.BuffRemove.None && GetInstid() == c.getDstInstid())
                        {
                            mech_data[mech].Add(new MechanicLog(c.getTime() - start, mech, this));
                            if (mech.GetMechType() == Mechanic.MechType.PlayerOnPlayer)
                            {
                                mech_data[mech].Add(new MechanicLog(c.getTime() - start, mech, log.GetPlayerList().FirstOrDefault(x => x.GetInstid() == c.getSrcInstid())));
                            }
                        }
                    }
                }
            }
            // Hitting enemy
            List<Mechanic> enemyHit = bossMechanics.Where(x => x.GetMechType() == Mechanic.MechType.HitOnEnemy).ToList();
            foreach (Mechanic mech in enemyHit)
            {
                Mechanic.SpecialCondition condition = mech.GetSpecialCondition();
                List<AgentItem> agents = log.GetAgentData().GetAgents((ushort)mech.GetSkill());
                foreach (AgentItem a in agents)
                {
                    foreach (DamageLog dl in GetDamageLogs(0,log,0,log.GetBossData().getAwareDuration()))
                    {
                        if (dl.getDstInstidt() != a.getInstid() || dl.isCondi() > 0 || dl.getTime() < a.getFirstAware() - start || dl.getTime() > a.getLastAware() - start || (condition != null && !condition(dl.getDamage())))
                        {
                            continue;
                        }
                        mech_data[mech].Add(new MechanicLog(dl.getTime(), mech, this));
                    }
                }
            }
        }

        /*protected override void setHealingLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            foreach (CombatItem c in log.getHealingData())
            {
                if (agent.getInstid() == c.getSrcInstid() && c.getTime() > log.getBossData().getFirstAware() && c.getTime() < log.getBossData().getLastAware())//selecting player or minion as caster
                {
                    long time = c.getTime() - time_start;
                    addHealingLog(time, c);
                }
            }
            Dictionary<string, Minions> min_list = getMinions(log);
            foreach (Minions mins in min_list.Values)
            {
                healing_logs.AddRange(mins.getHealingLogs(log, 0, log.getBossData().getAwareDuration()));
            }
            healing_logs.Sort((x, y) => x.getTime() < y.getTime() ? -1 : 1);
        }

        protected override void setHealingReceivedLogs(ParsedLog log)
        {
            long time_start = log.getBossData().getFirstAware();
            foreach (CombatItem c in log.getHealingReceivedData())
            {
                if (agent.getInstid() == c.getDstInstid() && c.getTime() > log.getBossData().getFirstAware() && c.getTime() < log.getBossData().getLastAware())
                {//selecting player as target
                    long time = c.getTime() - time_start;
                    addHealingReceivedLog(time, c);
                }
            }
        }*/
    }
}
