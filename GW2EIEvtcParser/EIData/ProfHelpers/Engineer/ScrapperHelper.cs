﻿using System.Collections.Generic;
using GW2EIEvtcParser.EIData.Buffs;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;
using static GW2EIEvtcParser.EIData.DamageModifier;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EIData
{
    internal static class ScrapperHelper
    {
        internal static readonly List<InstantCastFinder> InstantCastFinder = new List<InstantCastFinder>()
        {
            new EffectCastFinder(BulwarkGyro, EffectGUIDs.ScrapperBulwarkGyro).UsingChecker((evt, combatData, agentData, skillData) => evt.Src.Spec == Spec.Scrapper),
            new EffectCastFinder(PurgeGyro, EffectGUIDs.ScrapperPurgeGyro).UsingChecker((evt, combatData, agentData, skillData) => evt.Src.Spec == Spec.Scrapper),
            new EffectCastFinder(DefenseField, EffectGUIDs.ScrapperDefenseField).UsingChecker((evt, combatData, agentData, skillDatag) => evt.Src.Spec == Spec.Scrapper),
        };

        internal static readonly List<DamageModifier> DamageMods = new List<DamageModifier>
        {
            new BuffDamageModifier(new long[] { Swiftness, Superspeed, Stability }, "Object in Motion", "5% under swiftness/superspeed/stability, accumulative", DamageSource.NoPets, 5.0, DamageType.Strike, DamageType.All, Source.Scrapper, ByMultiPresence, BuffImages.ObjectInMotion, DamageModifierMode.All).WithBuilds(GW2Builds.July2019Balance)
        };

        internal static readonly List<Buff> Buffs = new List<Buff>
        {
            new Buff("Watchful Eye", WatchfulEye, Source.Scrapper, BuffClassification.Defensive, BuffImages.BulwarkGyro),
            new Buff("Watchful Eye PvP", WatchfulEyePvP, Source.Scrapper, BuffClassification.Defensive, BuffImages.BulwarkGyro),
        };

        private static HashSet<long> Minions = new HashSet<long>()
        {
            (int)MinionID.BlastGyro,
            (int)MinionID.BulwarkGyro,
            (int)MinionID.FunctionGyro,
            (int)MinionID.MedicGyro,
            (int)MinionID.ShredderGyro,
            (int)MinionID.SneakGyro,
            (int)MinionID.PurgeGyro,
        };

        internal static bool IsKnownMinionID(long id)
        {
            return Minions.Contains(id);
        }
    }
}
