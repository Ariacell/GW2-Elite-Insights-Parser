﻿using LuckParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.ParseModels
{
    public class BoonSourceFinder05032019 : BoonSourceFinder
    {
        public override ushort TryFindSrc(AbstractActor a, List<CastLog> castsToCheck, long time, long extension, ParsedLog log)
        {
            HashSet<long> idsToCheck = new HashSet<long>();
            switch (extension)
            {
                // Treated True Nature, Sand Squall, SoI
                case 3000:
                    idsToCheck.Add(51696);
                    idsToCheck.Add(10236);
                    idsToCheck.Add(29453);
                    break;
                // True Nature, Soulbeast trait
                case 2000:
                    if (a.Prof == "Soulbeast")
                    {
                        if (log.PlayerListBySpec.ContainsKey("Herald"))
                        {
                            return 0;
                        }
                        // if not herald  in squad then can only be the trait
                        return a.InstID;
                    }
                    idsToCheck.Add(51696);
                    break;

            }
            List<CastLog> cls = castsToCheck.Where(x => idsToCheck.Contains(x.SkillId) && x.Time <= time && time <= x.Time + x.ActualDuration + 10 && x.EndActivation.NoInterruptEndCasting()).ToList();
            if (cls.Count == 1)
            {
                CastLog item = cls.First();
                if (extension == 3000 && log.PlayerListBySpec.TryGetValue("Tempest", out List<Player> tempests))
                {
                    List<CombatItem> magAuraApplications = log.CombatData.GetBoonData(5684).Where(x => x.IsBuffRemove == ParseEnum.BuffRemove.None && x.DstInstid != item.SrcInstId).ToList();
                    foreach (Player tempest in tempests)
                    {
                        if (magAuraApplications.FirstOrDefault(x => x.SrcInstid == tempest.InstID && Math.Abs(x.Time - time) < 50) != null)
                        {
                            return 0;
                        }
                    }
                }
                return item.SrcInstId;
            }
            return 0;
        }
    }
}
