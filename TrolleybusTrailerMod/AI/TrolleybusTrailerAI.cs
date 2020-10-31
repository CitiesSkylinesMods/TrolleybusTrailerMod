using ColossalFramework;
using ColossalFramework.Math;
using TrolleybusTrailerMod.Patch;
using UnityEngine;

namespace TrolleybusTrailerMod.AI {
    public class TrolleybusTrailerAI : CarTrailerAI {

        [CustomizableProperty("Poles length")]
        public float m_poleLength = 6.8f;

        [CustomizableProperty("Poles Z Offset")]
        public float m_offsetZ;

        [CustomizableProperty("Poles X Offset")]
        public float m_offsetX = 0.45f;

        [CustomizableProperty("Poles Y Position")]
        public float m_offsetY = 3.5f;

        public void ExtraStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle leaderVehicleData) {
            // base.ExtraSimulationStep(vehicleID, ref vehicleData);
            uint path = leaderVehicleData.m_path;
            byte pathPositionIndex = leaderVehicleData.m_pathPositionIndex;
            Singleton<PathManager>.instance.m_pathUnits.m_buffer[path].GetPosition(pathPositionIndex >> 1, out PathUnit.Position position);
            ushort segment = position.m_segment;
            byte lane = position.m_lane;
            if (segment != 0) {
                ushort num = (ushort)((int)(vehicleData.m_flags2 & (Vehicle.Flags2)(-65536)) >> 16);
                byte b = (byte)((int)(vehicleData.m_flags2 & (Vehicle.Flags2)61440) >> 12);
                CalculatePositionAndRotation(vehicleID, ref vehicleData, out Vector3 position2, out Vector3 swayPosition, out Quaternion rotation);
                float poleLength = m_poleLength;
                Quaternion rhs = Quaternion.Euler(swayPosition.z * 57.29578f, 0f, swayPosition.x * -57.29578f);
                Vector3 polePivot = position2 + rotation * rhs * (Vector3.left * m_offsetX + Vector3.back * m_offsetZ + Vector3.up * m_offsetY);
                Vector3 polePivot2 = position2 + rotation * rhs * (Vector3.right * m_offsetX + Vector3.back * m_offsetZ + Vector3.up * m_offsetY);
                Vector3 poleEnd = position2 + rotation * rhs * (Vector3.left * m_offsetX + Vector3.back * (m_offsetZ + poleLength) + Vector3.up * m_offsetY);
                Vector3 poleEnd2 = position2 + rotation * rhs * (Vector3.right * m_offsetX + Vector3.back * (m_offsetZ + poleLength) + Vector3.up * m_offsetY);
                if (TrolleybusReversePatch.FindSegmentWireConnection(segment, lane, VehicleInfo.VehicleType.TrolleybusLeftPole, polePivot, poleEnd, poleLength, out Vector3 connectionPoint) && TrolleybusReversePatch.FindSegmentWireConnection(segment, lane, VehicleInfo.VehicleType.TrolleybusRightPole, polePivot2, poleEnd2, poleLength, out connectionPoint)) {
                    pathPositionIndex = leaderVehicleData.m_pathPositionIndex;
                    Singleton<PathManager>.instance.m_pathUnits.m_buffer[path].GetNextPosition(pathPositionIndex >> 1, out PathUnit.Position position3);
                    num = position3.m_segment;
                    b = position3.m_lane;
                    CachePathData(vehicleID, ref vehicleData, segment, lane, num, b);
                }
            }
        }
        
        private void CalculatePositionAndRotation(ushort vehicleID, ref Vehicle vehicleData, out Vector3 position, out Vector3 swayPosition, out Quaternion rotation)
        {
            uint targetFrame = vehicleData.GetTargetFrame(m_info, vehicleID);
            Vehicle.Frame frameData = vehicleData.GetFrameData(targetFrame - 32);
            Vehicle.Frame frameData2 = vehicleData.GetFrameData(targetFrame - 16);
            float t = ((float)(targetFrame & 0xF) + Singleton<SimulationManager>.instance.m_referenceTimer) * 0.0625f;
            Bezier3 bezier = default(Bezier3);
            bezier.a = frameData.m_position;
            bezier.b = frameData.m_position + frameData.m_velocity * 0.333f;
            bezier.c = frameData2.m_position - frameData2.m_velocity * 0.333f;
            bezier.d = frameData2.m_position;
            position = bezier.Position(t);
            Bezier3 bezier2 = default(Bezier3);
            bezier2.a = frameData.m_swayPosition;
            bezier2.b = frameData.m_swayPosition + frameData.m_swayVelocity * 0.333f;
            bezier2.c = frameData2.m_swayPosition - frameData2.m_swayVelocity * 0.333f;
            bezier2.d = frameData2.m_swayPosition;
            swayPosition = bezier2.Position(t);
            swayPosition.x *= m_info.m_leanMultiplier / Mathf.Max(1f, m_info.m_generatedInfo.m_wheelGauge);
            swayPosition.z *= m_info.m_nodMultiplier / Mathf.Max(1f, m_info.m_generatedInfo.m_wheelBase);
            rotation = Quaternion.Lerp(frameData.m_rotation, frameData2.m_rotation, t);
        }

        public override void RenderExtraStuff(ushort vehicleID, ref Vehicle vehicleData, RenderManager.CameraInfo cameraInfo, InstanceID id, Vector3 position, Quaternion rotation, Vector4 tyrePosition, Vector4 lightState, Vector3 scale, Vector3 swayPosition, bool underground, bool overground)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vehicle.Flags flags = instance.m_vehicles.m_buffer[vehicleID].m_flags;
            if (m_info != null && m_info.m_vehicleAI != null && m_info.m_subMeshes != null)
            {
                Matrix4x4 bodyMatrix = m_info.m_vehicleAI.CalculateBodyMatrix(flags, ref position, ref rotation, ref scale, ref swayPosition);
                Matrix4x4 value = m_info.m_vehicleAI.CalculateTyreMatrix(flags, ref position, ref rotation, ref scale, ref bodyMatrix);
                if ((flags & Vehicle.Flags.Inverted) != 0)
                {
                    tyrePosition.x = 0f - tyrePosition.x;
                    tyrePosition.y = 0f - tyrePosition.y;
                }
                MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                materialBlock.Clear();
                materialBlock.SetMatrix(instance.ID_TyreMatrix, value);
                materialBlock.SetVector(instance.ID_TyrePosition, tyrePosition);
                materialBlock.SetVector(instance.ID_LightState, lightState);
                for (int i = 0; i < m_info.m_subMeshes.Length; i++)
                {
                    VehicleInfo.MeshInfo meshInfo = m_info.m_subMeshes[i];
                    RenderPoleMesh(vehicleID, ref vehicleData, position, rotation, swayPosition, underground, overground, instance, meshInfo, materialBlock);
                }
            }
            base.RenderExtraStuff(vehicleID, ref vehicleData, cameraInfo, id, position, rotation, tyrePosition, lightState, scale, swayPosition, underground, overground);
        }
        
        private void RenderPoleMesh(ushort vehicleID, ref Vehicle vehicleData, Vector3 position, Quaternion rotation, Vector3 swayPosition, bool underground, bool overground, VehicleManager vehicleManager, VehicleInfo.MeshInfo meshInfo, MaterialPropertyBlock materialBlock)
        {
            VehicleInfoBase subInfo = meshInfo.m_subInfo;
            bool flag = Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor;
            if ((flag && !BuildingDecoration.IsSubMeshRendered(subInfo)) || (meshInfo.m_vehicleFlagsRequired & (Vehicle.Flags.TakingOff | Vehicle.Flags.Landing)) == 0 || !(subInfo != null))
            {
                return;
            }
            Matrix4x4 matrix = CalculatePoleMatrix(vehicleID, ref vehicleData, position, rotation, swayPosition, meshInfo, subInfo, flag);
            vehicleManager.m_drawCallData.m_defaultCalls++;
            if (underground)
            {
                if (subInfo.m_undergroundMaterial == null && subInfo.m_material != null)
                {
                    VehicleProperties properties = vehicleManager.m_properties;
                    if (properties != null)
                    {
                        subInfo.m_undergroundMaterial = new Material(properties.m_undergroundShader);
                        subInfo.m_undergroundMaterial.CopyPropertiesFromMaterial(subInfo.m_material);
                    }
                }
                Graphics.DrawMesh(subInfo.m_mesh, matrix, subInfo.m_undergroundMaterial, vehicleManager.m_undergroundLayer, null, 0, materialBlock);
            }
            if (overground)
            {
                Graphics.DrawMesh(subInfo.m_mesh, matrix, subInfo.m_material, m_info.m_prefabDataLayer, null, 0, materialBlock);
            }
        }
        private Matrix4x4 CalculatePoleMatrix(ushort vehicleID, ref Vehicle vehicleData, Vector3 position, Quaternion rotation, Vector3 swayPosition, VehicleInfo.MeshInfo meshInfo, VehicleInfoBase subInfo, bool assetEditor)
        {
            Vehicle.Flags flags = meshInfo.m_vehicleFlagsRequired & (Vehicle.Flags.TakingOff | Vehicle.Flags.Landing);
            VehicleInfo.VehicleType wireType = GetWireType(vehicleData.m_flags, flags);
            float poleLength = m_poleLength;
            Quaternion rhs = Quaternion.Euler(swayPosition.z * 57.29578f, 0f, swayPosition.x * -57.29578f);
            Vector3 a = (flags != Vehicle.Flags.TakingOff) ? Vector3.left : Vector3.right;
            Vector3 vector = position + rotation * rhs * (a * m_offsetX + Vector3.back * m_offsetZ + Vector3.up * m_offsetY);
            Vector3 poleEnd = position + rotation * rhs * (a * m_offsetX + Vector3.back * (m_offsetZ + poleLength) + Vector3.up * m_offsetY);
            Vector3 wireAttachmentPosition;
            if (!assetEditor)
            {
                TrolleybusReversePatch.FindWireConnection(vehicleID, ref vehicleData, wireType, poleLength, vector, poleEnd, out wireAttachmentPosition);
            }
            else
            {
                wireAttachmentPosition = position + rotation * rhs * (a * m_offsetX + Vector3.back * (m_offsetZ + poleLength) + Vector3.up * 4.55f);
            }
            Matrix4x4 result = default(Matrix4x4);
            result.SetTRS(vector, Quaternion.LookRotation(vector - wireAttachmentPosition, Vector3.up), Vector3.one);
            return result;
        }
        
        private void CachePathData(ushort vehicleID, ref Vehicle vehicleData, ushort lastSegment, byte lastLane, ushort nextSegment, byte nextLane)
        	{
        		if (lastLane > 15)
        		{
        			lastSegment = 0;
        			lastLane = 0;
        		}
        		if (nextLane > 15)
        		{
        			nextSegment = 0;
        			nextLane = 0;
        		}
        		vehicleData.m_custom = lastSegment;
        		vehicleData.m_flags2 &= (Vehicle.Flags2)(-3841);
        		vehicleData.m_flags2 |= (Vehicle.Flags2)(lastLane << 8);
        		vehicleData.m_flags2 &= (Vehicle.Flags2)65535;
        		vehicleData.m_flags2 |= (Vehicle.Flags2)(nextSegment << 16);
        		vehicleData.m_flags2 &= (Vehicle.Flags2)(-61441);
        		vehicleData.m_flags2 |= (Vehicle.Flags2)(nextLane << 12);
        	}
        
        private static VehicleInfo.VehicleType GetWireType(Vehicle.Flags vehicleFlags, Vehicle.Flags subMeshFlags)
        {
            return (subMeshFlags == Vehicle.Flags.TakingOff) ? (((vehicleFlags & Vehicle.Flags.LeftHandDrive) != 0) ? VehicleInfo.VehicleType.TrolleybusLeftPole : VehicleInfo.VehicleType.TrolleybusRightPole) : (((vehicleFlags & Vehicle.Flags.LeftHandDrive) != 0) ? VehicleInfo.VehicleType.TrolleybusRightPole : VehicleInfo.VehicleType.TrolleybusLeftPole);
        }
    }
}