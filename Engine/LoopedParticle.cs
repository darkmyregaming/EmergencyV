namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    internal class LoopedParticle : IHandleable
    {
        public string AssetName { get; }
        public string ParticleName { get; }

        public PoolHandle Handle { get; }

        public LoopedParticle(string assetName, string particleName, Ped ped, PedBoneId bone, Vector3 offset, Rotator rotation, float scale)
        {
            AssetName = assetName;
            ParticleName = particleName;
            LoadAsset();
            Handle = NativeFunction.Natives.StartParticleFxLoopedOnPedBone<uint>(particleName, 
                                                                                 ped,
                                                                                 offset.X, offset.Y, offset.Z,
                                                                                 rotation.Pitch, rotation.Roll, rotation.Yaw,
                                                                                 ped.GetBoneIndex(bone),
                                                                                 scale,
                                                                                 false, false , false);
        }

        public LoopedParticle(string assetName, string particleName, Entity entity, Vector3 offset, Rotator rotation, float scale)
        {
            AssetName = assetName;
            ParticleName = particleName;
            LoadAsset();
            Handle = NativeFunction.Natives.StartParticleFxLoopedOnEntity<uint>(particleName,
                                                                                entity,
                                                                                offset.X, offset.Y, offset.Z,
                                                                                rotation.Pitch, rotation.Roll, rotation.Yaw,
                                                                                scale,
                                                                                false, false, false);
        }

        private void LoadAsset()
        {
            NativeFunction.Natives.RequestNamedPtfxAsset(AssetName);
            int waitCounter = 10;
            while (!NativeFunction.Natives.HasNamedPtfxAssetLoaded<bool>(AssetName) && waitCounter > 0)
            {
                GameFiber.Sleep(10);
                waitCounter--;
            }

            NativeFunction.Natives.x6C38AF3693A69A91(AssetName); // _SET_PTFX_ASSET_NEXT_CALL
        }

        public void SetOffsets(Vector3 offset, Rotator rotation)
        {
            NativeFunction.Natives.SetParticleFxLoopedOffsets(Handle.Value, 
                                                              offset.X, offset.Y, offset.Z,
                                                              rotation.Pitch, rotation.Roll, rotation.Yaw);
        }

        public void SetColor(Color color)
        {
            NativeFunction.Natives.SetParticleFxLoopedColour(Handle.Value, color.R, color.G, color.B, false);
            NativeFunction.Natives.SetParticleFxLoopedAlpha(Handle.Value, color.A);
        }

        public void SetScale(float scale)
        {
            NativeFunction.Natives.SetParticleFxLoopedScale(Handle.Value, scale);
        }

        public bool IsValid()
        {
            return NativeFunction.Natives.DoesParticleFxLoopedExist<bool>(Handle.Value);
        }

        public void Stop()
        {
            NativeFunction.Natives.StopParticleFxLooped(Handle.Value, false);
        }

        public bool Equals(IHandleable other)
        {
            if (other == null || !(other is LoopedParticle))
                return false;

            return this.Handle == ((LoopedParticle)other).Handle;
        }
    }
}
