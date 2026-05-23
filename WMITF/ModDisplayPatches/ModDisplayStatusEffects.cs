using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using WMITF.Tools;

namespace WMITF.ModDisplayPatches
{
    [HarmonyPatch]
    public static class ModDisplayStatusEffects
    {
        public static MethodInfo listStatusEffectInfoSOGetItem = AccessTools.Method(typeof(List<StatusEffectInfoSO>), "get_Item");

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseCharacterStatusTooltip))]
        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseEnemyStatusTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_CharacterEnemyTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt(listStatusEffectInfoSOGetItem)))
                return;

            var statusEffectInfoLoc = crs.DeclareLocal<StatusEffectInfoSO>();

            crs.Emit(OpCodes.Ldloca, statusEffectInfoLoc);
            crs.EmitStaticDelegate(DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldloc, statusEffectInfoLoc);
            crs.EmitStaticDelegate(DisplayMod_Combat_AddModName);
        }

        public static StatusEffectInfoSO DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo(StatusEffectInfoSO curr, out StatusEffectInfoSO save)
        {
            return save = curr;
        }

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseInfoStatusTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_InfoTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.EmitStaticDelegate(DisplayMod_Combat_AddModName);
        }

        public static string DisplayMod_Combat_AddModName(string orig, StatusEffectInfoSO info)
        {
            if (ModConfig.ShowModsForStatusEffects != StatusFieldDisplayCondition.On)
                return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetStatusEffectModName(info, out var modName))
                return orig;

            return $"{orig}\n{modName}";
        }

        [HarmonyPatch(typeof(ExtraInformationUIHandler), nameof(ExtraInformationUIHandler.SetGlossaryInformation), typeof(StatusEffectInfoSO))]
        [HarmonyILManipulator]
        public static void DisplayMod_Glossary_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.EmitStaticDelegate(DisplayMod_Glossary_AddModName);
        }

        public static string DisplayMod_Glossary_AddModName(string orig, StatusEffectInfoSO info)
        {
            if (!(ModConfig.ShowModsForStatusEffects is StatusFieldDisplayCondition.On or StatusFieldDisplayCondition.OnlyInGlossary))
                return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetStatusEffectModName(info, out var modName))
                return orig;

            return $"{orig}\n\n{modName}";
        }
    }
}
