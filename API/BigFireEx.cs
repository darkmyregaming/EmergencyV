namespace EmergencyV.API
{
    // System
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class BigFireEx : FireEx
    {
        internal LoopedParticle AttachedParticle { get; }
        internal float ParticleMaxScale { get; } = MathHelper.GetRandomSingle(0.85f, 2.75f);

        internal BigFireEx(uint nativeHandle, Fire fire) : base(nativeHandle, fire)
        {
            dynamic p = MathHelper.Choose(FireParticles);

            AttachedParticle = new LoopedParticle(p.Asset, p.Name, Fire.Position + Vector3.WorldUp * 0.2f, new Rotator(0f, 0f, MathHelper.GetRandomSingle(0f, 360f)), ParticleMaxScale);
        }

        protected override void Remove()
        {
            if (AttachedParticle)
            {
                AttachedParticle.Stop();
            }
            base.Remove();
        }

        internal static dynamic[] FireParticles =
        {
            new { Asset = "scr_exile1", Name = "cs_ex1_cargo_fire" },
            new { Asset = "scr_exile2", Name = "scr_ex2_jeep_engine_fire" },
            new { Asset = "core", Name = "fire_wrecked_car" },
            new { Asset = "core", Name = "fire_wrecked_heli" },
            new { Asset = "core", Name = "fire_wrecked_plane_cockpit" },
            new { Asset = "core", Name = "fire_wrecked_heli_cockpit" },
            new { Asset = "core", Name = "fire_wrecked_tank_cockpit" },
            new { Asset = "core", Name = "fire_wrecked_bus" },
            new { Asset = "core",Name = "fire_wrecked_truck" },
            new { Asset = "core", Name = "fire_wrecked_plane" },
            new { Asset = "core", Name = "fire_wrecked_boat" },
            new { Asset = "core", Name = "fire_wrecked_bike" },
            new { Asset = "core", Name = "fire_wrecked_tank" },
            new { Asset = "core", Name = "fire_wrecked_boat" },
        };
    }
}
