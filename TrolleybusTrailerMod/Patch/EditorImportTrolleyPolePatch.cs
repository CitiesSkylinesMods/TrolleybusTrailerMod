using System.Reflection;
using ColossalFramework.UI;
using HarmonyLib;
using TrolleybusTrailerMod.AI;

namespace TrolleybusTrailerMod.Patch {
    [HarmonyPatch]
    public static class EditorImportTrolleybusPolePatch {
        [HarmonyPatch(typeof(AssetImporterAssetImport), "ImportVehicleSubMesh")]
        public static void Postfix(AssetImporterAssetImport __instance, AssetImporterAssetImport.VehicleSubMeshImportCallbackHandler callback) {
            VehicleInfo vehicleInfo = ToolsModifierControl.toolController.m_editPrefabInfo as VehicleInfo;
            if (vehicleInfo) {
                TrolleybusAI trolleybusAI = vehicleInfo.m_vehicleAI as TrolleybusAI;
                TrolleybusTrailerAI trolleybusTrailerAi = vehicleInfo.m_vehicleAI as TrolleybusTrailerAI;
                FieldInfo isPolePanelFieldInfo = typeof(AssetImporterAssetImport).GetField("m_isPolePanel", BindingFlags.Instance | BindingFlags.NonPublic);
                UIPanel panel = (UIPanel) isPolePanelFieldInfo.GetValue(__instance);
                panel.isVisible = trolleybusAI || trolleybusTrailerAi;
            }
        }
    }
}