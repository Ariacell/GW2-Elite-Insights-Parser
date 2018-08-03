﻿using LuckParser.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LuckParser.Models.ParseModels
{
    public class BossData
    {
        // Fields
        private ulong agent = 0;
        private ushort instid = 0;
        private long first_aware = 0;
        private long last_aware = long.MaxValue;
        private ushort id;
        private String name = "UNKNOWN";
        private int health = -1;
        private int toughness = -1;
        private List<Point> healthOverTime = new List<Point>();
        private bool isCM = false;
        // Constructors
        public BossData(ushort id)
        {
            this.id = id;
        }

        public String[] toStringArray()
        {
            String[] array = new String[7];
            array[0] = string.Format("{0:X}", agent); ;
            array[1] =instid.ToString();
            array[2] = first_aware.ToString();
            array[3] = last_aware.ToString();
            array[4] =id.ToString();
            array[5] = name;
            array[6] = health.ToString();
            return array;
        }

        // Getters
        public ulong getAgent()
        {
            return agent;
        }

        public ushort getInstid()
        {
            return instid;
        }

        public long getFirstAware()
        {
            return first_aware;
        }

        public long getLastAware()
        {
            return last_aware;
        }

        public long getAwareDuration()
        {
            return last_aware - first_aware;
        }

        public ushort getID()
        {
            return id;
        }

        public String getName()
        {
          
            return name;
        }

        public int getHealth()
        {
            return health;
        }
        public int getTough()
        {
            return toughness;
        }

        public List<Point> getHealthOverTime() {
            return healthOverTime;
        }
        // Setters
        public void setAgent(ulong agent)
        {
            this.agent = agent;
        }

        public void setInstid(ushort instid)
        {
            this.instid = instid;
        }

        public void setFirstAware(long first_aware)
        {
            this.first_aware = first_aware;
        }

        public void setLastAware(long last_aware)
        {
            this.last_aware = last_aware;
        }

        public void setName(String name)
        {
            name = name.Replace("\0", "");
            this.name = name;
        }

        public void setHealth(int health)
        {
            this.health = health;
        }
        public void setTough(int tough)
        {
            this.toughness = tough;
        }
        public void setHealthOverTime(List<Point> hot) {
            this.healthOverTime = hot;
        }
        public bool getCM()
        {
            return isCM;
        }
        public void setCM(List<CombatItem> clist)
        {
            isCM = false;
            switch(id)
            {
                // Cairn
                case 17194:
                    isCM = clist.Exists(x => x.getSkillID() == 38098);
                    break;
                // MO
                case 17172:
                    isCM = (health > 25e6);
                    break;
                // Samarog
                case 17188:
                    isCM = (health > 30e6);
                    break;
                // Deimos
                case 17154:
                    isCM = (health > 40e6);
                    break;
                // SH
                case 0x4D37:
                    List<CombatItem> necrosis = clist.Where(x => x.getSkillID() == 47414 && x.isBuffremove() == ParseEnum.BuffRemove.None).ToList();
                    // split necrosis
                    Dictionary<ushort, List<CombatItem>> splitNecrosis = new Dictionary<ushort, List<CombatItem>>();
                    foreach (CombatItem c in necrosis)
                    {
                        ushort inst = c.getDstInstid();
                        if (!splitNecrosis.ContainsKey(inst))
                        {
                            splitNecrosis.Add(inst, new List<CombatItem>());
                        }
                        splitNecrosis[inst].Add(c);
                    }
                    List<CombatItem> longestNecrosis = splitNecrosis.Values.First(l => l.Count == splitNecrosis.Values.Max(x => x.Count));
                    long minDiff = long.MaxValue;
                    for (int i = 0; i < longestNecrosis.Count - 1; i++)
                    {
                        CombatItem cur = longestNecrosis[i];
                        CombatItem next = longestNecrosis[i + 1];
                        long timeDiff = next.getTime() - cur.getTime();
                        if (timeDiff > 1000 && minDiff > timeDiff)
                        {
                            minDiff = timeDiff;
                        }
                    }
                    isCM = minDiff < 11000;
                    break;
                // Dhuum
                case 0x4BFA:
                    isCM = (health > 35e6);
                    break;
                // Skorvald
                case 0x44E0:
                    isCM = (health == 5551340);
                    break;
            }
            if (isCM)
            {
                name += " CM";
            }
        }
    }
}