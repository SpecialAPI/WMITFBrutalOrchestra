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
    public static class ModDisplayAchievements
    {
        [HarmonyPatch(typeof(ExtraInformationUIHandler), nameof(ExtraInformationUIHandler.SetAchievementInformation))]
        [HarmonyILManipulator]
        public static void DisplayMod_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpBeforeNext(x => x.MatchCallOrCallvirt<Info_AchievementLayout>(nameof(Info_AchievementLayout.SetInformation))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.EmitStaticDelegate(DisplayMod_AddModName);
        }

        public static string DisplayMod_AddModName(string orig, AchievementBase_t ach)
        {
            if (!ModConfig.ShowModsForAchievements)
                return orig;

            if (orig == null || ach is not ModdedAchievement_t mAch)
                return orig;

            if (!PluginFinder.TryGetAchievementModName(mAch, out var modName))
                return orig;

            return $"{orig}\n\n{modName}";
        }
    }
}
