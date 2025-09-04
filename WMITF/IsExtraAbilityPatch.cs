using System;
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
    public static class IsExtraAbilityPatch
    {
        public static MethodInfo siea_s = AccessTools.Method(typeof(IsExtraAbilityPatch), nameof(SetIsExtraAbility_Set));

        public static FieldInfo isExtraAbilityField = AccessTools.Field(typeof(CombatAbility), "WMITF_isExtraAbility");

        [HarmonyPatch(typeof(CharacterCombat), nameof(CharacterCombat.AddExtraAbility))]
        [HarmonyPatch(typeof(EnemyCombat), nameof(EnemyCombat.AddExtraAbility))]
        [HarmonyILManipulator]
        public static void SetIsExtraAbility_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchStloc(0)))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Call, siea_s);
        }

        public static void SetIsExtraAbility_Set(CombatAbility ab)
        {
            if(isExtraAbilityField == null)
            {
                Debug.LogError("WMITF_isExtraAbility field is null (SetIsExtraAbility_Set). This should not be happening.");
                return;
            }

            isExtraAbilityField.SetValue(ab, true);
        }

        public static bool IsFromExtraAbility(this CombatAbility ab)
        {
            if (isExtraAbilityField == null)
            {
                Debug.LogError("WMITF_isExtraAbility field is null (IsFromExtraAbility). This should not be happening.");
                return false;
            }

            return (bool)isExtraAbilityField.GetValue(ab);
        }
    }
}
