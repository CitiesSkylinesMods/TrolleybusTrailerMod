using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TrolleybusTrailerMod.AI;
using UnityEngine;

namespace TrolleybusTrailerMod.Patch {
    [HarmonyPatch]
    public static class EditorMaskDropdownPatch {
        [HarmonyPatch(typeof(EditorUserFlagAttribute), MethodType.Constructor, new []{typeof(string), typeof(Type[])})]
        public static void Postfix(EditorUserFlagAttribute __instance, string displayName, params Type[] allowedTypes) {
            if (displayName.Equals("Right Pole") || displayName.Equals("Left Pole")) {
                FieldInfo allowedTypesFieldInfo = typeof(EditorUserFlagAttribute).GetField("m_AllowedTypes", BindingFlags.Instance | BindingFlags.NonPublic);
                Type[] value = (Type[])allowedTypesFieldInfo.GetValue(__instance);
                value = value.AddItem(typeof(TrolleybusTrailerAI)).ToArray();
                allowedTypesFieldInfo.SetValue(__instance, value);
            }
        }
    }
}