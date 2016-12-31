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
        Fire targetFire;
        Task goToTask;
        Task fireWeaponAtTargetFireTask;

        bool useVehicleCannon;

        protected AIFirefighterTaskExtinguishFireInArea(Firefighter firefighter, Vector3 position, float range, bool shouldUseVehicleWaterCannon = true) : base(firefighter)
        {
            this.position = position;
            this.range = range;
            rangeSq = range * range;
            firesToExtinguish = new List<Fire>();

            if (shouldUseVehicleWaterCannon && Ped.IsInAnyVehicle(false) && Ped.SeatIndex == -1)
            {
                useVehicleCannon = true;
                //uint h;   // TODO: add some way to check if the vehicle has a water cannon
                //const uint waterCannonVehWeaponHash = 1422046295;
                //if (NativeFunction.Natives.GetCurrentPedVehicleWeapon<bool>(Game.LocalPlayer.Character, out h) && h == waterCannonVehWeaponHash)
                //{
                //    useVehicleCannon = true;
                //}
            }
        }

        public override void Update()
        {
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
                    if (!targetFire)
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
                            targetFire = useVehicleCannon ? orderedFires.LastOrDefault() : orderedFires.FirstOrDefault();
                        }
                        else return;
                    }
                    else
                    {
                        if (!useVehicleCannon && Vector3.DistanceSquared(Ped.Position, targetFire) > 4.0f * 4.0f)
                        {
                            if (goToTask == null || !goToTask.IsActive)
                            {
                                goToTask = Ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, Ped.Position.GetHeadingTowards(targetFire), 2.0f, 2.75f);
                            }
                        }
                        else
                        {
                            if (fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive)
                            {
                                if (useVehicleCannon)
                                {
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
                if ((goToTask == null || !goToTask.IsActive) && !useVehicleCannon)
                    goToTask = Ped.Tasks.FollowNavigationMeshToPosition(position.Around2D(range), Ped.Position.GetHeadingTowards(position), 2.0f, 5.0f);
            }
        }

        public override void OnFinished()
        {
            firesToExtinguish = null;
            targetFire = null;
            fireWeaponAtTargetFireTask = null;
            goToTask = null;
        }
    }
}
