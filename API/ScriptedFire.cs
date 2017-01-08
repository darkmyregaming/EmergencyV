namespace EmergencyV.API
{
    // System
    using System;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    public class ScriptedFire
    {
        public uint NativeHandle { get; }
        public Fire Fire { get; }
        internal LoopedParticle AttachedParticle { get; }
        internal float ParticleMaxScale { get; } = MathHelper.GetRandomSingle(1.0f, 2.5f);

        internal ScriptedFire(uint nativeHandle, Fire fire, bool isBigFire = false)
        {
            NativeHandle = nativeHandle;
            Fire = fire;

            if (isBigFire)
            {
                dynamic p = MathHelper.Choose(FireParticles);

                AttachedParticle = new LoopedParticle(p.Asset, p.Name, Fire.Position + Vector3.WorldUp * 0.5f, new Rotator(0f, 0f, MathHelper.GetRandomSingle(0f, 360f)), ParticleMaxScale);
            }

            RegisterScriptedFire(this);
        }

        private void Remove()
        {
            if (AttachedParticle)
            {
                AttachedParticle.Stop();
            }
            NativeFunction.Natives.RemoveScriptFire(NativeHandle);
        }


        internal static List<ScriptedFire> CurrentScriptedFires = new List<ScriptedFire>();
        internal static GameFiber ScriptedFiresUpdateFiber;

        private static void RegisterScriptedFire(ScriptedFire f)
        {
            CurrentScriptedFires.Add(f);
            if (ScriptedFiresUpdateFiber == null)
            {
                ScriptedFiresUpdateFiber = GameFiber.StartNew(ScriptedFiresUpdateLoop, "ScriptedFires Update Loop");
            }
        }

        private static void ScriptedFiresUpdateLoop()
        {
            while (true)
            {
                for (int i = CurrentScriptedFires.Count - 1; i >= 0; i--)
                {
                    ScriptedFire f = CurrentScriptedFires[i];

                    if (!f.Fire)
                    {
                        f.Remove();
                        CurrentScriptedFires.RemoveAt(i);
                    }
                }

                GameFiber.Sleep(250);
            }
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

// FIRE
// "scr_exile1", "cs_ex1_cargo_fire"
// "scr_exile2", "scr_ex2_jeep_engine_fire"
// "core", "fire_wrecked_car"
// "core", "fire_wrecked_heli"
// "core", "fire_wrecked_plane_cockpit"
// "core", "fire_wrecked_heli_cockpit"
// "core", "fire_wrecked_tank_cockpit"
// "core", "fire_wrecked_bus"
// "core", "fire_wrecked_truck"
// "core", "fire_wrecked_plane"
// "core", "fire_wrecked_boat"
// "core", "fire_wrecked_bike"
// "core", "fire_wrecked_tank"
// "core", "fire_wrecked_boat"

// SMALL FIRE
// "core", "fire_wrecked_rc"


// LIGHT SMOKE
// "scr_sell", "scr_vehicle_damage_smoke"
