﻿using System.Collections.Generic;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;
using static GW2EIEvtcParser.EIData.DamageModifier;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EIData
{
    internal static class BladeswornHelper
    {
        /////////////////////
        internal static readonly List<InstantCastFinder> InstantCastFinder = new List<InstantCastFinder>()
        {
            new BuffLossCastFinder(62861, GunsaberMode, EIData.InstantCastFinder.DefaultICD, GW2Builds.EODBeta2, GW2Builds.EndOfLife), // Gunsaber sheath
            new BuffGainCastFinder(62745, GunsaberMode, EIData.InstantCastFinder.DefaultICD, GW2Builds.EODBeta2, GW2Builds.EndOfLife), // Gunsaber         
            new DamageCastFinder(62847, 62847, EIData.InstantCastFinder.DefaultICD, GW2Builds.EODBeta2, GW2Builds.EndOfLife), // Unseen Sword
        };

        private static readonly HashSet<long> _gunsaberForm = new HashSet<long>
        {
            62861, 62745,
        };

        public static bool IsGunsaberForm(long id)
        {
            return _gunsaberForm.Contains(id);
        }

        internal static readonly List<DamageModifier> DamageMods = new List<DamageModifier>
        {
            new BuffDamageModifier(FierceAsFire, "Fierce as Fire", "1%", DamageSource.NoPets, 1.0, DamageType.Strike, DamageType.All, Source.Bladesworn, ByStack, "https://wiki.guildwars2.com/images/8/8e/Fierce_as_Fire.png", GW2Builds.EODBeta4, GW2Builds.EndOfLife, DamageModifierMode.All),
        };

        internal static readonly List<Buff> Buffs = new List<Buff>
        {
            new Buff("Gunsaber Mode", GunsaberMode, Source.Bladesworn, BuffClassification.Other,"https://wiki.guildwars2.com/images/f/f0/Unsheathe_Gunsaber.png"),
            new Buff("Dragon Trigger", DragonTrigger, Source.Bladesworn, BuffClassification.Other,"https://wiki.guildwars2.com/images/b/b1/Dragon_Trigger.png"),
            new Buff("Positive Flow", PositiveFlow, Source.Bladesworn, BuffStackType.StackingConditionalLoss, 25, BuffClassification.Other, "https://wiki.guildwars2.com/images/f/f9/Attribute_bonus.png"),
            new Buff("Fierce as Fire", FierceAsFire, Source.Bladesworn, BuffStackType.Stacking, 10, BuffClassification.Other, "https://wiki.guildwars2.com/images/8/8e/Fierce_as_Fire.png"),
            new Buff("Stim State", StimState, Source.Bladesworn, BuffClassification.Other,"https://wiki.guildwars2.com/images/a/ad/Combat_Stimulant.png"),
            new Buff("Guns and Glory", GunsAndGlory, Source.Bladesworn, BuffStackType.Queue, 9, BuffClassification.Other,"https://wiki.guildwars2.com/images/7/72/Guns_and_Glory.png"),
        };


    }
}
