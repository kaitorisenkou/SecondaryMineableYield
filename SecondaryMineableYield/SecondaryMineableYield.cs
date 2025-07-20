using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static RimWorld.ColonistBar;

namespace SecondaryMineableYield {
    [StaticConstructorOnStartup]
    public class SecondaryMineableYield {
        static SecondaryMineableYield() {
            Log.Message("[SecondaryMineableYield] Now active");
            var harmony = new Harmony("kaitorisenkou.SecondaryMineableYield");
            harmony.Patch(
#if v15 || v16
                AccessTools.Method(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) },null),
#else
                AccessTools.Method(typeof(Mineable), "TrySpawnYield", null, null),
#endif
                null,
                new HarmonyMethod(typeof(SecondaryMineableYield), nameof(Patch_TrySpawnYield), null),
                null,
                null
                );
            harmony.Patch(
                AccessTools.Method(typeof(Mineable), "PreApplyDamage", null, null),
                null,
                new HarmonyMethod(typeof(SecondaryMineableYield), nameof(Patch_PreApplyDamage), null),
                null,
                null
                );
            Log.Message("[SecondaryMineableYield] Harmony patch complete!");
        }
#if v15 || v16
        public static void Patch_TrySpawnYield(Mineable __instance, float ___yieldPct, Map map, Pawn pawn) {
#else
        public static void Patch_TrySpawnYield(Mineable __instance, float ___yieldPct, Map map, float yieldChance, Pawn pawn) {
#endif
            var ext = __instance.def.GetModExtension<ModExtension_SecondaryMineableYield>();
            if (ext == null)
                return;
            if (Rand.Value > ext.mineableDropChance) {
                return;
            }
            float weights = ext.GetWeightSum;
            float rand = Rand.Value * weights;
            SecondaryYieldEntry entry = null;
            foreach (var i in ext.entries) {
                weights -= i.randomWeight;
                if (weights < rand) {
                    entry = i;
                    break;
                }
            }
            if (entry == null)
                return;
            int stackCount = Mathf.Max(1, entry.EffectiveMineableYield);
            if (entry.mineableYieldWasteable) {
                stackCount = Mathf.Max(1, GenMath.RoundRandom((float)stackCount * ___yieldPct));
            }
            Thing thing = ThingMaker.MakeThing(entry.mineableThing, null);
            thing.stackCount = stackCount;
            GenPlace.TryPlaceThing(
                thing, __instance.Position, map, ThingPlaceMode.Near,
                (Thing t, int i) => {
                    if (pawn == null || pawn.Faction == Faction.OfPlayer || !t.def.EverHaulable || t.def.designateHaulable)
                        return;
                    t.SetForbidden(true, false);
                }
                );
        }

        public static void Patch_PreApplyDamage(Mineable __instance, DamageInfo dinfo, bool absorbed) {
            if (absorbed || 
                __instance.def.building.mineableThing!=null||
                dinfo.Def != DamageDefOf.Mining ||
                dinfo.Instigator == null ||
                !(dinfo.Instigator is Pawn)) {
                return;
            }

            var ext = __instance.def.GetModExtension<ModExtension_SecondaryMineableYield>();
            if (ext != null && !ext.entries.NullOrEmpty()) {
                __instance.Notify_TookMiningDamage(GenMath.RoundRandom(dinfo.Amount), (Pawn)dinfo.Instigator);
            }
        }
    }
}
