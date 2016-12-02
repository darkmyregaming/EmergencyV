﻿namespace EmergencyV
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
                    hose.AttachTo(Plugin.LocalPlayerCharacter, Plugin.LocalPlayerCharacter.GetBoneIndex(PedBoneId.RightPhHand), new Vector3(-2.3f, 0.05f, -0.2f), new Rotator(-35f, 4.5f, 182.04f)); // new Vector3(0f, -2f, -2.1f), new Rotator(-45f, 98f, 0f)
                    nozzle = new Object("hei_prop_hei_hose_nozzle", Vector3.Zero);
                    nozzle.AttachTo(hose, hose.GetBoneIndex("Prop_Heist_Hose_01_b1"), Vector3.Zero, Rotator.Zero);
                    cannonJetLoopedParticle = new LoopedParticle("core", "water_cannon_jet", nozzle, new Vector3(0f, 0.05f, 0f), new Rotator(0f, 0f, 90f), 2.0f);
                    Plugin.LocalPlayerCharacter.Inventory.GiveNewWeapon(WeaponHash.Minigun, 0, true);
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
                    Plugin.LocalPlayerCharacter.Inventory.Weapons.Remove(WeaponHash.Minigun);
                }

                hoseActive = value;
            }
        }

        public HoseTest()
        {
        }
        
        DateTime lastFiresCheck = DateTime.UtcNow;
        Vector3 hitPosition, hitNormal;
        Entity hitEntity;
        List<Fire> nearbyFires = new List<Fire>();
        public void Update()
        {
            if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                HoseActive = !HoseActive;

            if (HoseActive)
            {
                Game.DisableControlAction(0, GameControl.Attack, true);
                Game.DisableControlAction(0, GameControl.Attack2, true);
                Game.DisableControlAction(0, GameControl.SelectWeapon, true);

                if (Plugin.LocalPlayerCharacter.Inventory.EquippedWeaponObject)
                {
                    if (Plugin.LocalPlayerCharacter.Inventory.EquippedWeapon.Hash == WeaponHash.Minigun)
                    {
                        Plugin.LocalPlayerCharacter.Inventory.EquippedWeaponObject.IsVisible = false;
                    }
                }

                if (Plugin.LocalPlayerCharacter.IsAiming)
                {
                    if (!cannonJetLoopedParticle.Exists())
                        cannonJetLoopedParticle = new LoopedParticle("core", "water_cannon_jet", nozzle, new Vector3(0f, 0.05f, 0f), new Rotator(0f, 0f, 90f), 2.0f);

                    if ((DateTime.UtcNow - lastFiresCheck).TotalSeconds > 1.0)
                    {
                        Vector3 rayStart, rayEnd;
                        HitResult hitResult = DoRaycasts(out rayStart, out rayEnd);

                        if (hitResult.Hit)
                        {
                            RaycastHitProcess(hitResult, rayStart, rayEnd);
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
                        if (nearbyFires[i] && MathHelper.GetChance(35))
                        {
                            nearbyFires[i].DesiredBurnDuration -= 0.315f;
                            //nearbyFires[i].Delete();
                        }
                        else
                        {
                            nearbyFires.RemoveAt(i);
                            continue;
                        }
                    }
                }
                else
                {
                    if (cannonJetLoopedParticle.Exists())
                        cannonJetLoopedParticle.Stop();
                    if (cannonSprayLoopedParticle.Exists())
                        cannonSprayLoopedParticle.Stop();
                }

#if DEBUG
                // draw raycasts lines
                Vector3 start_ = nozzle.Position;
                Vector3 end_ = start_ - nozzle.RightVector * 15f + Vector3.WorldDown * 0.915f;
                Util.DrawLine(start_, end_, System.Drawing.Color.Red);

                Vector3 dir_ = (end_ - start_).ToNormalized();
                start_ = end_;
                end_ = start_ + dir_ * 7.5f + Vector3.WorldDown * 0.0235f;
                Util.DrawLine(start_, end_, System.Drawing.Color.Purple);

                dir_ = (end_ - start_).ToNormalized();
                start_ = end_;
                end_ = start_ + dir_ * 8f + Vector3.WorldDown * 0.04f;
                Util.DrawLine(start_, end_, System.Drawing.Color.SeaGreen);

                dir_ = (end_ - start_).ToNormalized();
                start_ = end_;
                end_ = start_ + dir_ * 6.5f + Vector3.WorldDown * 0.125f;
                Util.DrawLine(start_, end_, System.Drawing.Color.Brown);

                // hit position marker
                Util.DrawMarker(28, hitPosition, Vector3.Zero, Rotator.Zero, new Vector3(0.4f), System.Drawing.Color.Blue);

                // draw hit position normal line
                start_ = hitPosition;
                end_ = start_ + hitNormal * 6.5f;
                Util.DrawLine(start_, end_, System.Drawing.Color.LightBlue);

                // draw fires markers
                for (int i = 0; i < nearbyFires.Count; i++)
                {
                    if (nearbyFires[i])
                    {
                        Util.DrawMarker(28, nearbyFires[i].Position, Vector3.Zero, Rotator.Zero, new Vector3(0.15f), System.Drawing.Color.Green);
                    }
                }
#endif
            }
        }

        private HitResult DoRaycasts(out Vector3 hitRayStart, out Vector3 hitRayEnd)
        {
            Vector3 start = nozzle.Position;
            Vector3 end = start - nozzle.RightVector * 15f + Vector3.WorldDown * 0.915f;

            HitResult hitResult = World.TraceCapsule(start, end, 0.05f, TraceFlags.IntersectEverything, Plugin.LocalPlayerCharacter, nozzle, hose);

            if (!hitResult.Hit)
            {
                Vector3 dir = (end - start).ToNormalized();
                start = end;
                end = start + dir * 7.5f + Vector3.WorldDown * 0.0235f;

                hitResult = World.TraceCapsule(start, end, 0.075f, TraceFlags.IntersectEverything, Plugin.LocalPlayerCharacter, nozzle, hose);

                if (!hitResult.Hit)
                {
                    dir = (end - start).ToNormalized();
                    start = end;
                    end = start + dir * 8f + Vector3.WorldDown * 0.04f;

                    hitResult = World.TraceCapsule(start, end, 0.08f, TraceFlags.IntersectEverything, Plugin.LocalPlayerCharacter, nozzle, hose);

                    if (!hitResult.Hit)
                    {
                        dir = (end - start).ToNormalized();
                        start = end;
                        end = start + dir * 6.5f + Vector3.WorldDown * 0.125f;

                        hitResult = World.TraceCapsule(start, end, 0.09f, TraceFlags.IntersectEverything, Plugin.LocalPlayerCharacter, nozzle, hose);
                    }
                }
            }

            hitRayStart = start;
            hitRayEnd = end;
            return hitResult;
        }

        private void RaycastHitProcess(HitResult hitResult, Vector3 rayStart, Vector3 rayEnd)
        {
            hitPosition = hitResult.HitPosition;
            hitNormal = hitResult.HitNormal;
            hitEntity = hitResult.HitEntity;

            Fire[] fires = World.GetAllFires();
            nearbyFires.AddRange(fires.Where(f => !nearbyFires.Contains(f) && Vector3.DistanceSquared(f.Position, hitPosition) < 3.25f * 3.25f));

            if (!cannonSprayLoopedParticle.Exists())
            {
                cannonSprayLoopedParticle = new LoopedParticle("core", "water_cannon_spray", hitPosition, hitNormal.ToRotator(), 0.835f);
            }
            else
            {
                cannonSprayLoopedParticle.SetOffsets(hitPosition, hitNormal.ToRotator());
            }

            if (hitEntity)
            {
                float force = 50f;
                int entType = NativeFunction.Natives.GetEntityType<int>(hitEntity);

                if (entType == 1) // if entity is ped
                {
                    force = 65f;
                    Ped p = (Ped)hitEntity;
                    NativeFunction.Natives.SetPedToRagdoll(p, 3500, 3500, 0, false, false, false);
                    p.SetWetnessHeight(1.0f);
                }
                else if (entType == 2) // ... vehicle
                {
                    force = 420f;
                }

                Vector3 forceVector = (hitPosition - rayStart).ToNormalized() * force;


                NativeFunction.Natives.ApplyForceToEntity(hitEntity, 1,
                                                          forceVector.X, forceVector.Y, forceVector.Z,
                                                          0f, 0f, 0f,
                                                          -1, false, false, false,
                                                          false, false);
            }
        }
    }
}