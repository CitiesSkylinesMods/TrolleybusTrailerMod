using CitiesHarmony.API;
using ICities;
using UnityEngine;

namespace TrolleybusTrailerMod {
    public class TrolleybusTrailerMod : IUserMod {
        public string Name { get; } = "Trolleybus Trailer AI";
        public string Description { get; } = "Support Trolleybus trailers with poles at any trailer";

        public void OnEnabled() {
                HarmonyHelper.DoOnHarmonyReady(() => {
                    Debug.Log("[Trolleybus Trailer AI] Try Patch...");
                    Patcher.PatchAll();
                });
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) {
                Debug.Log("[Trolleybus Trailer AI] Try UnPatch...");
                Patcher.UnPatchAll();
            }
        }
    }
}