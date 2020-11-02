using HarmonyLib;
using UnityEngine;

namespace TrolleybusTrailerMod.Patch {
    [HarmonyPatch]
    public static class TrolleybusReversePatch {

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TrolleybusAI), "FindWireConnection")]
        public static bool FindWireConnection(ushort vehicleID, ref Vehicle vehicleData, VehicleInfo.VehicleType wireType, float poleLength, Vector3 polePivot, Vector3 poleEnd, out Vector3 wireAttachmentPosition) {
            Debug.LogError("[TrolleybusReversePatch]:FindWireConnection() Redirection failed!");
            wireAttachmentPosition = Vector3.zero;
            return false;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TrolleybusAI), "FindSegmentWireConnection")]
        public static bool FindSegmentWireConnection(ushort segmentID, int laneIndex, VehicleInfo.VehicleType wireType, Vector3 polePivot, Vector3 poleEnd, float poleLength, out Vector3 connectionPoint) {
            Debug.LogError("[TrolleybusReversePatch]:FindSegmentWireConnection() Redirection failed!");
            connectionPoint = Vector3.zero;
            return false;
        }
        
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TrolleybusAI), "CachePathData")]
        public static void CachePathData(object instance, ushort vehicleID, ref Vehicle vehicleData, ushort lastSegment, byte lastLane, ushort nextSegment, byte nextLane) {
            Debug.LogError("[TrolleybusReversePatch]:FindWireConnection() Redirection failed!");
        }
    }
}