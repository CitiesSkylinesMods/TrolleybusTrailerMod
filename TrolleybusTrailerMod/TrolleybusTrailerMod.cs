using System;
using System.Reflection;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using UnityEngine;

namespace TrolleybusTrailerMod {
    public class TrolleybusTrailerMod: IUserMod {
        public string Name { get; } = "Trolleybus Trailer AI";
        public string Description { get; } = "Support Trolleybus trailers with poles at any trailer";

        public readonly string HARMONY_ID = "krzychu124.trolleybus.trailer.ai";
        private Harmony _harmony;

        public void OnEnabled() {
            if (CitiesHarmony.API.HarmonyHelper.IsHarmonyInstalled) {
                CitiesHarmony.API.HarmonyHelper.DoOnHarmonyReady(() => {
                    Debug.Log("[Trolleybus Trailer AI] Applying patches...");
                    _harmony = new Harmony(HARMONY_ID);
                    try {
                        _harmony.PatchAll(Assembly.GetExecutingAssembly());
                        Debug.Log("[Trolleybus Trailer AI] Patches applied correctly");
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
                });
            }
        }

        public void OnDisabled() {
            if (_harmony != null) {
                Debug.Log("[Trolleybus Trailer AI] UnPatching...");
                _harmony.UnpatchAll(HARMONY_ID);
            }
        }
    }
}