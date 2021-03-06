﻿using Harmony;
using System.Reflection;
using System.Collections.Generic;
using Verse;
using RimWorld;


namespace BoneMod
{

    [StaticConstructorOnStartup]
    static class BoneMod
    {

        static BoneMod()
        {

            var harmony = HarmonyInstance.Create("com.rimworld.bonemod");


            harmony.PatchAll(Assembly.GetExecutingAssembly());
            UnityEngine.Debug.Log("BoneMod called");
        }
    }

    [HarmonyPatch(typeof(Verse.Corpse))]
    [HarmonyPatch("SpecialDisplayStats", PropertyMethod.Getter)]
    class CorpseDisplay
    {
        static void Postfix(Verse.Corpse __instance, ref IEnumerable<StatDrawEntry> __result)
        {
            // Create a modifyable list
            List<StatDrawEntry> NewList = new List<StatDrawEntry>();

            // copy vanilla entries into the new list
            foreach (StatDrawEntry entry in __result)
            {
                // it's possible to discard entries here if needed
                NewList.Add(entry);
            }

            // custom code to modify list contents
            // add vanilla leather line just to verify that output is modified
            StatDef BoneAmount = DefDatabase<StatDef>.GetNamed("BoneAmount", true);
            float pawnBoneCount = __instance.InnerPawn.GetStatValue(BoneAmount, true) * BoneModSettings.boneFactor;
            NewList.Add(new StatDrawEntry(BoneAmount.category, BoneAmount, pawnBoneCount, StatRequest.For(__instance.InnerPawn), ToStringNumberSense.Undefined));

            // convert list to IEnumerable to match the caller's expectations
            IEnumerable<StatDrawEntry> output = NewList;
            
            // make caller use the list
            __result = output;
        }
    }

    [HarmonyPatch(typeof(Verse.Pawn))]
    [HarmonyPatch("ButcherProducts")]
    class ButcherProducts
    {
        static void Postfix(Verse.Pawn __instance, ref IEnumerable<Thing> __result, float efficiency)
        {
            int boneCount = GenMath.RoundRandom(__instance.GetStatValue(DefDatabase<StatDef>.GetNamed("BoneAmount", true), true) * BoneModSettings.boneFactor * efficiency);
            if (boneCount > 0)
            {

                List<Thing> NewList = new List<Thing>();
                foreach (Thing entry in __result)
                {
                    NewList.Add(entry);
                }
                Thing bones = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("BoneItem"), null);
                bones.stackCount = boneCount;
                NewList.Add(bones);


                IEnumerable<Thing> output = NewList;
                __result = output;
            }
        }
    }
}
