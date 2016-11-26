namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;
    using Object = Rage.Object;

    internal class HoseTest
    {
        LoopedParticle cannonJetLoopedParticle;
        LoopedParticle cannonSprayLoopedParticle;
        Object nozzle;
        Object hose;

        // "hei_prop_hei_hose_nozzle", "hei_prop_heist_hose_01"

        private bool hoseActive = false;
        public bool HoseActive
        {
            get
            {
                return hoseActive;
            }
            set
            {
                if (value == hoseActive)
                    return;
                if (value)
                {
                    hose = new Object("hei_prop_heist_hose_01", Vector3.Zero);
                    hose.AttachTo(Plugin.LocalPlayerCharacter, Plugin.LocalPlayerCharacter.GetBoneIndex(PedBoneId.RightPhHand), new Vector3(0f, -2f, -2.1f), new Rotator(-45f, 98f, 0f));
                    nozzle = new Object("hei_prop_hei_hose_nozzle", Vector3.Zero);
                    nozzle.AttachTo(hose, hose.GetBoneIndex("Prop_Heist_Hose_01_b1"), Vector3.Zero, Rotator.Zero);
                    cannonJetLoopedParticle = new LoopedParticle("core", "water_cannon_jet", nozzle, new Vector3(0f, 0.05f, 0f), new Rotator(0f, 0f, 90f), 2.0f);
                }
                else
                {
                    if (cannonJetLoopedParticle.Exists())
                       cannonJetLoopedParticle.Stop();
                    if (cannonSprayLoopedParticle.Exists())
                        cannonSprayLoopedParticle.Stop();
                    if (nozzle.Exists())
                        nozzle.Delete();
                    if (hose.Exists())
                        hose.Delete();
                    cannonJetLoopedParticle = null;
                    cannonSprayLoopedParticle = null;
                    nozzle = null;
                    hose = null;
                }

                hoseActive = value;
            }
        }

        public HoseTest()
        {
        }

        DateTime lastFiresCheck = DateTime.UtcNow;
        Vector3 hitPosition, hitNormal;
        List<Fire> nearbyFires = new List<Fire>();
        public void Update()
        {
            if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                HoseActive = !HoseActive;

            if (HoseActive)
            {
                if((DateTime.UtcNow - lastFiresCheck).TotalSeconds > 1.0)
                {
                    Vector3 start = nozzle.Position;
                    Vector3 end = start - nozzle.RightVector * 15f + Vector3.WorldDown * 0.915f;

                    HitResult hitResult = World.TraceCapsule(start, end, 0.1f, TraceFlags.IntersectEverything, Plugin.LocalPlayerCharacter, nozzle, hose);

                    if (hitResult.Hit)
                    {
                        hitPosition = hitResult.HitPosition;
                        hitNormal = hitResult.HitNormal;
                        Fire[] fires = World.GetAllFires();
                        nearbyFires.AddRange(fires.Where(f => !nearbyFires.Contains(f) && Vector3.DistanceSquared(f.Position, hitPosition) < 3.25f * 3.25f));
                        if (!cannonSprayLoopedParticle.Exists())
                        {
                            cannonSprayLoopedParticle = new LoopedParticle("core", "water_cannon_spray", hitPosition, hitNormal.ToRotator(), 0.835f);
                        }
                    }
                    else
                    {
                        hitPosition = Vector3.Zero;
                        hitNormal = Vector3.Zero;
                        if (nearbyFires.Count >= 1)
                            nearbyFires.Clear();
                        if (cannonSprayLoopedParticle.Exists())
                        {
                            cannonSprayLoopedParticle.Stop();
                            cannonSprayLoopedParticle = null;
                        }
                    }
                }

                for (int i = 0; i < nearbyFires.Count; i++)
                {
                    if (nearbyFires[i] && MathHelper.GetChance(20))
                    {
                        //nearbyFires[i].DesiredBurnDuration -= 0.15f;
                        nearbyFires[i].Delete();
                    }
                    else
                    {
                        nearbyFires.RemoveAt(i);
                        continue;
                    }
                }

#if DEBUG
                Vector3 start_ = nozzle.Position;
                Vector3 end_ = start_ - nozzle.RightVector * 15f + Vector3.WorldDown * 0.915f;
                Util.DrawLine(start_, end_, System.Drawing.Color.Red);

                Util.DrawMarker(28, hitPosition, Vector3.Zero, Rotator.Zero, new Vector3(0.5f), System.Drawing.Color.Blue);

                start_ = hitPosition;
                end_ = start_ + hitNormal * 6.5f;
                Util.DrawLine(start_, end_, System.Drawing.Color.LightBlue);

                for (int i = 0; i < nearbyFires.Count; i++)
                {
                    if (nearbyFires[i])
                    {
                        Util.DrawMarker(28, nearbyFires[i].Position, Vector3.Zero, Rotator.Zero, new Vector3(0.225f), System.Drawing.Color.Green);
                    }
                }
#endif
            }


        }
    }
}