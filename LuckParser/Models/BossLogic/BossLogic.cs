﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class BossLogic
    {

        public enum ParseMode { Raid, Fractal, Golem, Unknown };

        private CombatReplayMap _map;
        public readonly List<Mechanic> MechanicList = new List<Mechanic> {
            new Mechanic(-2, "Dead", Mechanic.MechType.PlayerStatus, ParseEnum.BossIDS.Unknown, "symbol:'x',color:'rgb(0,0,0)',", "Dead",0),
            new Mechanic(-3, "Downed", Mechanic.MechType.PlayerStatus, ParseEnum.BossIDS.Unknown, "symbol:'cross',color:'rgb(255,0,0)',", "Downed",0),
            new Mechanic(SkillItem.ResurrectId, "Resurrect", Mechanic.MechType.PlayerStatus, ParseEnum.BossIDS.Unknown, "symbol:'cross-open',color:'rgb(0,255,255)',", "Res",0)}; //Resurrects (start), Resurrect
        public ParseMode Mode { get; protected set; } = ParseMode.Unknown;
        public bool CanCombatReplay { get; set; } = false;
        public string Extension { get; protected set; } = "boss";
        public string IconUrl { get; protected set; } = "https://wiki.guildwars2.com/images/d/d2/Guild_emblem_004.png";
        public List<Mob> TrashMobs { get; } = new List<Mob>();
        private ushort _triggerID;

        public BossLogic(ushort triggerID)
        {
            _triggerID = triggerID;
        }

        protected virtual CombatReplayMap GetCombatMapInternal()
        {
            return null;
        }


        public CombatReplayMap GetCombatMap()
        {
            if (_map == null)
            {
                _map = GetCombatMapInternal();
            }
            return _map;
        }

        public virtual List<ushort> GetFightTargetsIDs()
        {
            return new List<ushort>
            {
                _triggerID
            };
        }

        protected List<PhaseData> GetInitialPhase(ParsedLog log)
        {
            List<PhaseData> phases = new List<PhaseData>();
            long fightDuration = log.FightData.FightDuration;
            phases.Add(new PhaseData(0, fightDuration));
            phases[0].Name = "Full Fight";
            return phases;
        }

        public virtual List<PhaseData> GetPhases(ParsedLog log, bool requirePhases)
        {
            List<PhaseData> phases = GetInitialPhase(log);
            return phases;
        }

        public virtual void ComputeAdditionalBossData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
        }

        protected virtual List<ParseEnum.TrashIDS> GetTrashMobsIDS()
        {
            return new List<ParseEnum.TrashIDS>();
        }

        public virtual int IsCM(ParsedLog log)
        {
            return -1;
        }

        public virtual void ComputeAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
        }

        public void ComputeTrashMobsData(ParsedLog log, int pollingRate)
        {
            List<ParseEnum.TrashIDS> ids = GetTrashMobsIDS();
            List<AgentItem> aList = log.AgentData.NPCAgentList.Where(x => ids.Contains(ParseEnum.GetTrashIDS(x.ID))).ToList();
            foreach (AgentItem a in aList)
            {
                Mob mob = new Mob(a);
                mob.InitCombatReplay(log, pollingRate, true, false);
                TrashMobs.Add(mob);
            }
        }

        protected void SetSuccessByDeath(ParsedLog log)
        {
            CombatItem killed = log.CombatData.GetStatesData(ParseEnum.StateChange.ChangeDead).LastOrDefault(x => x.SrcInstid == log.Boss.InstID);
            if (killed != null)
            {
                log.LogData.Success = true;
                log.FightData.FightEnd = killed.Time;
            }
        }

        public virtual void SetSuccess(ParsedLog log)
        {
            SetSuccessByDeath(log);
        }

        public virtual string GetReplayIcon()
        {
            return "";
        }
        //
        protected static List<CombatItem> GetFilteredList(ParsedLog log, long skillID, ushort instid)
        {
            bool needStart = true;
            List<CombatItem> main = log.GetBoonData(skillID).Where(x => ((x.DstInstid == instid && x.IsBuffRemove == ParseEnum.BuffRemove.None) || (x.SrcInstid == instid && x.IsBuffRemove != ParseEnum.BuffRemove.None))).ToList();
            List<CombatItem> filtered = new List<CombatItem>();
            for (int i = 0; i < main.Count; i++)
            {
                CombatItem c = main[i];
                if (needStart && c.IsBuffRemove == ParseEnum.BuffRemove.None)
                {
                    needStart = false;
                    filtered.Add(c);
                }
                else if (!needStart && c.IsBuffRemove != ParseEnum.BuffRemove.None)
                {
                    // consider only last remove event before another application
                    if ((i == main.Count - 1) || (i < main.Count - 1 && main[i + 1].IsBuffRemove == ParseEnum.BuffRemove.None))
                    {
                        needStart = true;
                        filtered.Add(c);
                    }
                }
            }
            return filtered;
        }
    }
}
