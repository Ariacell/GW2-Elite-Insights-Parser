﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData.Buffs;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using GW2EIEvtcParser.ParserHelpers;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EIData.Buff;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EIData
{
    internal static class ScourgeHelper
    {
        internal static readonly List<InstantCastFinder> InstantCastFinder = new List<InstantCastFinder>()
        {
            new BuffGainCastFinder(TrailOfAnguish, TrailOfAnguishBuff),
            // Trail of Anguish? Unique effect?
            // new EffectCastFinder(TrailOfAnguish, EffectGUIDs.ScourgeTrailOfAnguish).UsingSrcSpecChecker(Spec.Scourge).UsingICD(6100),
            new DamageCastFinder(NefariousFavorSkill, NefariousFavorShadeHit),
            new DamageCastFinder(GarishPillarSkill, GarishPillarHit),
            new BuffGainCastFinder(DesertShroud, DesertShroudBuff).UsingDurationChecker(6000),
            new BuffGainCastFinder(SandstormShroudSkill, DesertShroudBuff).UsingDurationChecker(3500),
            // new EXTBarrierCastFinder(DesertShroud, DesertShroud),
            new EXTBarrierCastFinder(SandCascadeSkill, SandCascadeBarrier),
            new BuffGainCastFinder(SadisticSearing, SadisticSearing).UsingOrigin(EIData.InstantCastFinder.InstantCastOrigin.Trait),
            new BuffLossCastFinder(SadisticSearingActivation, SadisticSearing).UsingChecker((blcf, combatData, agentData, skillData) => 
            {
                long sadisticSearingDuration = 10000 - blcf.RemovedDuration;
                if (combatData.GetDamageData(ManifestSandShadeShadeHit).Any(x => x.CreditedFrom == blcf.To && x.Time >= blcf.Time - sadisticSearingDuration && x.Time <= blcf.Time)) 
                {
                    return true;
                }
                return false;
            }).UsingOrigin(EIData.InstantCastFinder.InstantCastOrigin.Trait),
        };

        internal static readonly List<DamageModifier> DamageMods = new List<DamageModifier>
        {
        };

        internal static readonly List<Buff> Buffs = new List<Buff>
        {
            new Buff("Sadistic Searing", SadisticSearing, Source.Scourge, BuffClassification.Other, BuffImages.SadisticSearing),
            new Buff("Path Uses", PathUses, Source.Scourge, BuffStackType.Stacking, 25, BuffClassification.Other, BuffImages.SandSwell),
        };

        internal static void ComputeProfessionCombatReplayActors(AbstractPlayer player, ParsedEvtcLog log, CombatReplay replay)
        {
            Color color = Colors.Necromancer;

            // Sand Swell portal locations
            if (log.CombatData.TryGetGroupedEffectEventsBySrcWithGUID(player.AgentItem, EffectGUIDs.ScourgeSandSwellPortal, out IReadOnlyList<IReadOnlyList<EffectEvent>> sandswellPortals))
            {
                foreach (IReadOnlyList<EffectEvent> group in sandswellPortals)
                {
                    GenericAttachedDecoration first = null;
                    foreach (EffectEvent effect in group)
                    {
                        (int, int) lifespan = ProfHelper.ComputeEffectLifespan(log, effect, 8000, player.AgentItem, PathUses);
                        var connector = new PositionConnector(effect.Position);
                        replay.Decorations.Add(new CircleDecoration(true, 0, 90, lifespan, color.WithAlpha(0.5f).ToString(), connector).UsingSkillMode(player, false));
                        GenericAttachedDecoration icon = new IconDecoration(ParserIcons.PortalSandswell, CombatReplaySkillDefaultSizeInPixel, 90, 0.7f, lifespan, connector).UsingSkillMode(player, false);
                        if (first == null)
                        {
                            first = icon;
                        }
                        else
                        {
                            replay.Decorations.Add(first.LineTo(icon, 0, color.WithAlpha(0.5f).ToString()).UsingSkillMode(player, false));
                        }
                        replay.Decorations.Add(icon);
                    }
                }

            }
            // Shade
            foreach (EffectEvent effect in log.CombatData.GetEffectEventsBySrcWithGUID(player.AgentItem, EffectGUIDs.ScourgeShade))
            {
                (int, int) lifespan = ProfHelper.ComputeEffectLifespan(log, effect, log.LogData.GW2Build >= GW2Builds.July2023BalanceAndSilentSurfCM ? 8000 : 20000);
                var connector = new PositionConnector(effect.Position);
                replay.Decorations.Add(new CircleDecoration(false, 0, 180, lifespan, color.WithAlpha(0.5f).ToString(), connector).UsingSkillMode(player));
                replay.Decorations.Add(new IconDecoration(ParserIcons.EffectShade, 128, 0.5f, lifespan, connector).UsingSkillMode(player));
            }
        }
    }
}
