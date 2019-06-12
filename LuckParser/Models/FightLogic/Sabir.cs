﻿using LuckParser.Parser;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static LuckParser.Parser.ParseEnum.TrashIDS;
using System.Drawing;

namespace LuckParser.Models.Logic
{
    public class Sabir : RaidLogic
    {
        public Sabir(ushort triggerID, AgentData agentData) : base(triggerID, agentData)
        {
            MechanicList.AddRange(new List<Mechanic>()
            {
            });
            Extension = "sabir";
            IconUrl = "";
        }

        protected override List<ParseEnum.TrashIDS> GetTrashMobsIDS()
        {
            return new List<ParseEnum.TrashIDS>()
            {
                ParalyzingWisp,
                VoltaicWisp,
                SmallKillerTornado,
                SmallJumpyTornado,
                BigKillerTornado
            };
        }

        protected override CombatReplayMap GetCombatMapInternal()
        {
            return null;
        }

        public override int IsCM(CombatData combatData, AgentData agentData, FightData fightData)
        {
            return 0;
        }
    }
}
