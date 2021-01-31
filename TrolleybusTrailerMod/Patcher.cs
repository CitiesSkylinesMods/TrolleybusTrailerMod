using System;
using System.Reflection;
using ColossalFramework.UI;
using HarmonyLib;
using UnityEngine;

namespace TrolleybusTrailerMod {
    public static class Patcher {       
        private const string HarmonyId  = "krzychu124.trolleybus.trailer.ai";
        private static bool patched;
        public static void PatchAll() {
            try {
                Harmony harmony = new Harmony(HarmonyId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Debug.Log("[Trolleybus Trailer AI] Patches applied correctly");
                patched = true;
            } catch (Exception e) {
                Debug.LogError("[Trolleybus Trailer AI] Exception while patching game:\n" + e);
                UIView.library
                    .ShowModal<ExceptionPanel>(
                        "ExceptionPanel")
                    .SetMessage(
                        "Trolleybus Trailer AI ",
                        "Something went wrong while patching. :(\nMod will not work correctly. Contact author for support.",
                        true);
            }
        }
        
        public static void UnPatchAll() {
            if (!patched) return;
            Harmony harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId );
            patched = false;
            Debug.Log("[Trolleybus Trailer AI] UnPatched successfully");
        }
    }
}