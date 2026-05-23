using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace WMITF
{
    [HarmonyPatch]
    public static class ModDisplayItems
    {
        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseInfoPortraitTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_CombatTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringTrioData>(nameof(StringTrioData.description))))
                return;

            crs.Emit(OpCodes.Ldloc_1);
            crs.EmitStaticDelegate(DisplayMod_CombatTooltip_AddModName);
        }

        public static string DisplayMod_CombatTooltip_AddModName(string orig, BaseWearableSO w)
        {
            if (!ModConfig.ShowModsForItems)
                return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetWearableModName(w, out var modName))
                return orig;

            return $"{orig}\n{modName}";
        }

        [HarmonyPatch(typeof(ExtraInformationUIHandler), nameof(ExtraInformationUIHandler.SetItemInformation))]
        [HarmonyILManipulator]
        public static void DisplayMod_ItemInfo_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringTrioData>(nameof(StringTrioData.description))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.EmitStaticDelegate(DisplayMod_Overworld_AddModName);
        }

        public static string DisplayMod_Overworld_AddModName(string orig, BaseWearableSO w)
        {
            if (!ModConfig.ShowModsForItems)
                return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetWearableModName(w, out var modName))
                return orig;

            return $"{orig}\n\n{modName}";
        }

        [HarmonyPatch(typeof(OverworldManagerBG), nameof(OverworldManagerBG.TryOpenPrizeChest))]
        [HarmonyILManipulator]
        public static void DisplayMod_PrizeDescription_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringTrioData>(nameof(StringTrioData.description))))
                return;

            crs.Emit(OpCodes.Ldloc_1);
            crs.EmitStaticDelegate(DisplayMod_PrizeDescription_AddModName);
        }

        public static string DisplayMod_PrizeDescription_AddModName(string orig, PrizeContentData dat)
        {
            return DisplayMod_Overworld_AddModName(orig, dat.prize);
        }

        [HarmonyPatch(typeof(OverworldManagerBG), nameof(OverworldManagerBG.ProcessBronzoPresent), MethodType.Enumerator)]
        [HarmonyILManipulator]
        public static void DisplayMod_BronzoDescription_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringTrioData>(nameof(StringTrioData.description))))
                return;

            crs.Emit(OpCodes.Ldarg_0);
            crs.EmitStaticDelegate(DisplayMod_BronzoDescription_AddModName);
        }

        public static string DisplayMod_BronzoDescription_AddModName(string orig, IEnumerator rat)
        {
            return DisplayMod_Overworld_AddModName(orig, rat.EnumeratorGetField<BaseWearableSO>("item"));
        }
    }
}
