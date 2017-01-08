namespace EmergencyV
{
    // System
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class AIFirefighterTaskExtinguishFireInArea : AIFirefighterTask
    {
        Vector3 position;
        float range;
        float rangeSq;
        List<Fire> firesToExtinguish;
        Fire closestFire;
        Fire furthestFire;
        Fire targetFire;
        Task goToTask;
        Task fireWeaponAtTargetFireTask;
        Task performDrivingManeuverTask;

        bool useVehicleCannon;
        
        protected AIFirefighterTaskExtinguishFireInArea(Firefighter firefighter, Vector3 position, float range, bool shouldUseVehicleWaterCannon = true) : base(firefighter)
        {
            this.position = position;
            this.range = range;
            firesToExtinguish = new List<Fire>();
            
            if (shouldUseVehicleWaterCannon && Ped.IsInAnyVehicle(false) && Ped.SeatIndex == -1)
            {
                const uint waterCannonVehWeaponHash = 1422046295;
                if (NativeFunction.Natives.SetCurrentPedVehicleWeapon<bool>(Ped, waterCannonVehWeaponHash)) // SetCurrentPedVehicleWeapon returns true if the vehicle has the specified weapon
                {
                    this.range = range + 10.0f; // increase range to allow more maneuverability with the truck
                    useVehicleCannon = true;
                }
            }


            rangeSq = this.range * this.range;
        }
        
        internal override void Update()
        {
#if DEBUG
            if (closestFire)
            {
                Util.DrawMarker(0, closestFire.Position, Vector3.Zero, Rotator.Zero, new Vector3(1.25f), System.Drawing.Color.Green);
            }
            if (furthestFire)
            {
                Util.DrawMarker(0, furthestFire.Position, Vector3.Zero, Rotator.Zero, new Vector3(1.25f), System.Drawing.Color.DarkGreen);
            }

            Util.DrawMarker(28, position, Vector3.Zero, Rotator.Zero, new Vector3(range), System.Drawing.Color.FromArgb(30, 255, 0, 0));
            Game.DisplaySubtitle(Game.LocalPlayer.Character.DistanceTo(position).ToString());
#endif
            if (useVehicleCannon && (!Ped.IsInAnyVehicle(false) || (Ped.IsInAnyVehicle(false) && Ped.SeatIndex != -1)))
            {
                useVehicleCannon = false;
            }


            if (Vector3.DistanceSquared(Ped.Position, position) < rangeSq)
            {
                if (!Firefighter.Equipment.HasFireExtinguisher)
                    Firefighter.Equipment.HasFireExtinguisher = true;

                if (firesToExtinguish.Count == 0)
                {
                    foreach (Fire f in World.GetAllFires())
                    {
                        if (Vector3.DistanceSquared(f.Position, position) < rangeSq)
                            firesToExtinguish.Add(f);
                    }

                    if (firesToExtinguish.Count == 0) // if no fires are found, this task is finished
                    {
                        IsFinished = true;
                        return;
                    }
                }
                else
                {
                    if (!closestFire || !furthestFire)
                    {
                        if (fireWeaponAtTargetFireTask != null && fireWeaponAtTargetFireTask.IsActive)
                        {
                            Ped.Tasks.Clear();
                            fireWeaponAtTargetFireTask = null;
                        }

                        firesToExtinguish.RemoveAll(f => !f.Exists());

                        if (firesToExtinguish.Count >= 1)
                        {
                            IOrderedEnumerable<Fire> orderedFires = firesToExtinguish.OrderBy(f => Vector3.DistanceSquared(f.Position, Ped.Position));
                            closestFire = orderedFires.FirstOrDefault();
                            furthestFire = orderedFires.LastOrDefault();

                            targetFire = useVehicleCannon ?
                                            (Vector3.DistanceSquared(Ped.Position, furthestFire) > 35f ? closestFire : furthestFire) :
                                            closestFire;
                        }
                        else return;
                    }
                    else
                    {
                        if (!useVehicleCannon && Vector3.DistanceSquared(Ped.Position, closestFire) > 4.0f * 4.0f)
                        {
                            if (goToTask == null || !goToTask.IsActive)
                            {
                                goToTask = Ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, Ped.Position.GetHeadingTowards(targetFire), 2.0f, 2.75f);
                            }
                        }
                        else if (useVehicleCannon && 
                                (Vector3.DistanceSquared(Ped.CurrentVehicle.FrontPosition, targetFire) < 10f * 10f && Util.GetHeadingAbsDifference(Ped.CurrentVehicle.Heading, Ped.CurrentVehicle.Position.GetHeadingTowards(targetFire.Position)) < 30.0f))
                        {
                            if ((performDrivingManeuverTask == null || !performDrivingManeuverTask.IsActive))
                            {
                                GameFiber.StartNew(() =>
                                { 
                                    performDrivingManeuverTask = Ped.Tasks.PerformDrivingManeuver(VehicleManeuver.ReverseStraight50);
                                    GameFiber.Sleep(1250);
                                    performDrivingManeuverTask = Ped.Tasks.PerformDrivingManeuver(MathHelper.Choose(VehicleManeuver.ReverseLeft, VehicleManeuver.ReverseRight));
                                    GameFiber.Sleep(1250);
                                    performDrivingManeuverTask = Ped.Tasks.PerformDrivingManeuver(VehicleManeuver.HandBrakeDrivingDirection);
                                    GameFiber.Sleep(2000);
                                    Ped.Tasks.Clear();
                                    Ped.Tasks.ClearSecondary();
                                    performDrivingManeuverTask = null;
                                });
                            }
                        }
                        else
                        {
                            if ((fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive) && performDrivingManeuverTask == null)
                            {
                                if ((goToTask != null && goToTask.IsActive) || (performDrivingManeuverTask != null && performDrivingManeuverTask.IsActive))
                                {
                                    Ped.Tasks.Clear();
                                    Ped.Tasks.ClearSecondary();
                                }

                                if (useVehicleCannon)
                                {
                                    //NativeFunction.Natives.GiveWeaponToPed(Ped, Game.GetHashKey("VEHICLE_WEAPON_WATER_CANNON"), true, true);
                                    NativeFunction.Natives.TaskVehicleShootAtCoord(Ped, targetFire.Position.X, targetFire.Position.Y, targetFire.Position.Z, 250.0f);
                                    fireWeaponAtTargetFireTask = Task.GetTask(Ped, "TASK_VEHICLE_SHOOT_AT_COORD");
                                }
                                else
                                {
                                    fireWeaponAtTargetFireTask = Ped.Tasks.FireWeaponAt(targetFire.Position, 7500, FiringPattern.FullAutomatic);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if ((goToTask == null || !goToTask.IsActive) && (performDrivingManeuverTask == null || !performDrivingManeuverTask.IsActive))
                {
                    if (useVehicleCannon)
                    {
                        goToTask = Ped.Tasks.DriveToPosition(position, 3.0f, VehicleDrivingFlags.DriveBySight, range - 5.0f);
                    }
                    else
                    {
                        goToTask = Ped.Tasks.FollowNavigationMeshToPosition(position.Around2D(range), Ped.Position.GetHeadingTowards(position), 2.0f, 5.0f);
                    }
                }
            }
        }

        protected override void OnFinished()
        {
            Ped.Tasks.Clear();
            firesToExtinguish = null;
            closestFire = null;
            furthestFire = null;
            targetFire = null;
            fireWeaponAtTargetFireTask = null;
            goToTask = null;
        }
    }
}
