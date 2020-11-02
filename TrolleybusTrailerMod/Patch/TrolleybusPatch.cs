using HarmonyLib;
using TrolleybusTrailerMod.AI;

namespace TrolleybusTrailerMod.Patch {
    [HarmonyPatch]
    public static class TrolleybusPatch {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TrolleybusAI), "ExtraSimulationStep")]
        public static void ExtraSimulationStep(ushort vehicleID, ref Vehicle vehicleData) {
            ushort trailingVehicle = vehicleData.m_trailingVehicle;
            ref var vehicles= ref VehicleManager.instance.m_vehicles.m_buffer;
            while (trailingVehicle != 0) {
                ref Vehicle data = ref vehicles[trailingVehicle];
                if (data.Info && data.Info.m_subMeshes != null && data.Info.m_subMeshes.Length > 0) {
                    (data.Info.m_vehicleAI as TrolleybusTrailerAI)?.ExtraStep(trailingVehicle, ref data, ref vehicleData);
                }

                trailingVehicle = data.m_trailingVehicle;
            }
        }
    }
}