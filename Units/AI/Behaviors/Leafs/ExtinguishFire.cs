namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class ExtinguishFire : RPH.Utilities.AI.Leafs.Action
    {
        string positionKey;
        float range;
        float rangeSq;

        /// <param name="positionKey">The key where the center position is saved in the blackboard's tree memory.</param>
        public ExtinguishFire(string positionKey, float range)
        {
            this.positionKey = positionKey;
            this.range = range;
            this.rangeSq = range * range;
        }

        protected override void OnOpen(ref BehaviorTreeContext context)
        {
            if (!(context.Agent.Target is Ped))
            {
                throw new InvalidOperationException($"The behavior action {nameof(ExtinguishFire)} can't be used with {context.Agent.Target.GetType().Name}, it can only be used with {nameof(Ped)}s");
            }

            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return;
            }

            if (!ped.Inventory.Weapons.Contains(WeaponHash.FireExtinguisher))
                ped.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, -1, true);

            GiveTasks(ref context);
        }

        protected override BehaviorStatus OnBehave(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return BehaviorStatus.Failure;
            }

            GiveTasks(ref context);
            
            List<Fire> firesToExtinguish = context.Agent.Blackboard.Get<List<Fire>>("firesToExtinguish", context.Tree.Id, this.Id, null);

            if (firesToExtinguish.Count == 0) // if no fires are found, this task is finished
            {
                return BehaviorStatus.Success;
            }
            else
            {
                return BehaviorStatus.Running;
            }
        }

        protected override void OnClose(ref BehaviorTreeContext context)
        {
            context.Agent.Blackboard.Set<List<Fire>>("firesToExtinguish", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Fire>("closestFire", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Fire>("furthestFire", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Fire>("targetFire", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Task>("fireWeaponAtTargetFireTask", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Task>("goToTask", null, context.Tree.Id, this.Id);
        }

        private void GiveTasks(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);
            Vector3 position = context.Agent.Blackboard.Get<Vector3>(positionKey, context.Tree.Id);
            List<Fire> firesToExtinguish = context.Agent.Blackboard.Get<List<Fire>>("firesToExtinguish", context.Tree.Id, this.Id, null);
            Fire closestFire = context.Agent.Blackboard.Get<Fire>("closestFire", context.Tree.Id, this.Id, null);
            Fire furthestFire = context.Agent.Blackboard.Get<Fire>("furthestFire", context.Tree.Id, this.Id, null);
            Fire targetFire = context.Agent.Blackboard.Get<Fire>("targetFire", context.Tree.Id, this.Id, null);
            Task fireWeaponAtTargetFireTask = context.Agent.Blackboard.Get<Task>("fireWeaponAtTargetFireTask", context.Tree.Id, this.Id, null);
            Task goToTask = context.Agent.Blackboard.Get<Task>("goToTask", context.Tree.Id, this.Id, null);

            if (firesToExtinguish == null)
            {
                firesToExtinguish = new List<Fire>();
                context.Agent.Blackboard.Set<List<Fire>>("firesToExtinguish", firesToExtinguish, context.Tree.Id, this.Id);
            }

            if (Vector3.DistanceSquared(ped.Position, position) < rangeSq)
            {
                if (firesToExtinguish.Count == 0)
                {
                    foreach (Fire f in World.GetAllFires())
                    {
                        if (Vector3.DistanceSquared(f.Position, position) < rangeSq)
                            firesToExtinguish.Add(f);
                    }

                    if (firesToExtinguish.Count == 0)
                        return;
                }
                else
                {
                    if (!closestFire || !furthestFire)
                    {
                        if (fireWeaponAtTargetFireTask != null && fireWeaponAtTargetFireTask.IsActive)
                        {
                            ped.Tasks.Clear();
                            fireWeaponAtTargetFireTask = null;
                            context.Agent.Blackboard.Set<List<Fire>>("fireWeaponAtTargetFireTask", null, context.Tree.Id, this.Id);
                        }

                        firesToExtinguish.RemoveAll(f => !f.Exists());

                        if (firesToExtinguish.Count >= 1)
                        {
                            IOrderedEnumerable<Fire> orderedFires = firesToExtinguish.OrderBy(f => Vector3.DistanceSquared(f.Position, ped.Position));
                            closestFire = orderedFires.FirstOrDefault();
                            furthestFire = orderedFires.LastOrDefault();

                            context.Agent.Blackboard.Set<Fire>("closestFire", closestFire, context.Tree.Id, this.Id);
                            context.Agent.Blackboard.Set<Fire>("furthestFire", furthestFire, context.Tree.Id, this.Id);

                            targetFire = closestFire;
                            context.Agent.Blackboard.Set<Fire>("targetFire", targetFire, context.Tree.Id, this.Id);
                        }
                    }
                    else
                    {
                        if (Vector3.DistanceSquared(ped.Position, closestFire) > 4.0f * 4.0f)
                        {
                            if (goToTask == null || !goToTask.IsActive)
                            {
                                goToTask = ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, ped.Position.GetHeadingTowards(targetFire), 2.0f, 2.75f);
                                context.Agent.Blackboard.Set<Task>("goToTask", goToTask, context.Tree.Id, this.Id);
                            }
                        }
                        else
                        {
                            if (fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive)
                            {
                                if ((goToTask != null && goToTask.IsActive))
                                {
                                    ped.Tasks.Clear();
                                    ped.Tasks.ClearSecondary();
                                }

                                fireWeaponAtTargetFireTask = ped.Tasks.FireWeaponAt(targetFire.Position, 7500, FiringPattern.FullAutomatic);
                                context.Agent.Blackboard.Set<Task>("fireWeaponAtTargetFireTask", fireWeaponAtTargetFireTask, context.Tree.Id, this.Id);
                            }
                        }
                    }
                }
            }
            else
            {
                if ((goToTask == null || !goToTask.IsActive))
                {
                    goToTask = ped.Tasks.FollowNavigationMeshToPosition(position.Around2D(range), ped.Position.GetHeadingTowards(position), 2.0f, 5.0f);
                    context.Agent.Blackboard.Set<Task>("goToTask", goToTask, context.Tree.Id, this.Id);
                }
            }
        }
    }
}
    //    // System
//    using System.Linq;
//    using System.Collections.Generic;

//    // RPH
//    using Rage;
//    using Rage.Native;

//    internal class AITaskExtinguishFireInArea : AITask
//{
//    Firefighter firefighter;

//    Vector3 position;
//    float range;
//    float rangeSq;
//    List<Fire> firesToExtinguish;
//    Fire closestFire;
//    Fire furthestFire;
//    Fire targetFire;
//    Task goToTask;
//    Task fireWeaponAtTargetFireTask;
//    Task performDrivingManeuverTask;

//    bool useVehicleCannon;

//    protected AITaskExtinguishFireInArea(AIController controller, Vector3 position, float range, bool shouldUseVehicleWaterCannon = true) : base(controller)
//    {
//        firefighter = controller.Owner as Firefighter;
//        if (firefighter == null)
//            throw new System.ArgumentException($"The AIController.Owner instance isn't a Firefighter instance. The {nameof(AITaskExtinguishFireInArea)} requires a Firefighter instance.", nameof(controller));

//        this.position = position;
//        this.range = range;
//        firesToExtinguish = new List<Fire>();

//        if (shouldUseVehicleWaterCannon && Ped.IsInAnyVehicle(false) && Ped.SeatIndex == -1)
//        {
//            const uint waterCannonVehWeaponHash = 1422046295;
//            if (NativeFunction.Natives.SetCurrentPedVehicleWeapon<bool>(Ped, waterCannonVehWeaponHash)) // SetCurrentPedVehicleWeapon returns true if the vehicle has the specified weapon
//            {
//                this.range = range + 10.0f; // increase range to allow more maneuverability with the truck
//                useVehicleCannon = true;
//            }
//        }


//        rangeSq = this.range * this.range;
//    }

//    internal override void Update()
//    {
//        if (!Ped || Ped.IsDead)
//        {
//            Abort();
//            return;
//        }

//#if DEBUG
//        if (closestFire)
//        {
//            Util.DrawMarker(0, closestFire.Position, Vector3.Zero, Rotator.Zero, new Vector3(1.25f), System.Drawing.Color.Green);
//        }
//        if (furthestFire)
//        {
//            Util.DrawMarker(0, furthestFire.Position, Vector3.Zero, Rotator.Zero, new Vector3(1.25f), System.Drawing.Color.DarkGreen);
//        }

//        Util.DrawMarker(28, position, Vector3.Zero, Rotator.Zero, new Vector3(range), System.Drawing.Color.FromArgb(30, 255, 0, 0));
//        Game.DisplaySubtitle(Game.LocalPlayer.Character.DistanceTo(position).ToString());
//#endif
//        if (useVehicleCannon && (!Ped.IsInAnyVehicle(false) || (Ped.IsInAnyVehicle(false) && Ped.SeatIndex != -1)))
//        {
//            useVehicleCannon = false;
//        }


//        if (Vector3.DistanceSquared(Ped.Position, position) < rangeSq)
//        {
//            if (!firefighter.Equipment.HasFireExtinguisher)
//                firefighter.Equipment.HasFireExtinguisher = true;

//            if (firesToExtinguish.Count == 0)
//            {
//                foreach (Fire f in World.GetAllFires())
//                {
//                    if (Vector3.DistanceSquared(f.Position, position) < rangeSq)
//                        firesToExtinguish.Add(f);
//                }

//                if (firesToExtinguish.Count == 0) // if no fires are found, this task is finished
//                {
//                    IsFinished = true;
//                    return;
//                }
//            }
//            else
//            {
//                if (!closestFire || !furthestFire)
//                {
//                    if (fireWeaponAtTargetFireTask != null && fireWeaponAtTargetFireTask.IsActive)
//                    {
//                        Ped.Tasks.Clear();
//                        fireWeaponAtTargetFireTask = null;
//                    }

//                    firesToExtinguish.RemoveAll(f => !f.Exists());

//                    if (firesToExtinguish.Count >= 1)
//                    {
//                        IOrderedEnumerable<Fire> orderedFires = firesToExtinguish.OrderBy(f => Vector3.DistanceSquared(f.Position, Ped.Position));
//                        closestFire = orderedFires.FirstOrDefault();
//                        furthestFire = orderedFires.LastOrDefault();

//                        targetFire = useVehicleCannon ?
//                                        (Vector3.DistanceSquared(Ped.Position, furthestFire) > 35f ? closestFire : furthestFire) :
//                                        closestFire;
//                    }
//                    else return;
//                }
//                else
//                {
//                    if (!useVehicleCannon && Vector3.DistanceSquared(Ped.Position, closestFire) > 4.0f * 4.0f)
//                    {
//                        if (goToTask == null || !goToTask.IsActive)
//                        {
//                            goToTask = Ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, Ped.Position.GetHeadingTowards(targetFire), 2.0f, 2.75f);
//                        }
//                    }
//                    else if (useVehicleCannon &&
//                            (Vector3.DistanceSquared(Ped.CurrentVehicle.FrontPosition, targetFire) < 10f * 10f && Util.GetHeadingAbsDifference(Ped.CurrentVehicle.Heading, Ped.CurrentVehicle.Position.GetHeadingTowards(targetFire.Position)) < 30.0f))
//                    {
//                        if ((performDrivingManeuverTask == null || !performDrivingManeuverTask.IsActive))
//                        {
//                            GameFiber.StartNew(() =>
//                            {
//                                performDrivingManeuverTask = Ped.Tasks.PerformDrivingManeuver(VehicleManeuver.ReverseStraight50);
//                                GameFiber.Sleep(1250);
//                                performDrivingManeuverTask = Ped.Tasks.PerformDrivingManeuver(MathHelper.Choose(VehicleManeuver.ReverseLeft, VehicleManeuver.ReverseRight));
//                                GameFiber.Sleep(1250);
//                                performDrivingManeuverTask = Ped.Tasks.PerformDrivingManeuver(VehicleManeuver.HandBrakeDrivingDirection);
//                                GameFiber.Sleep(2000);
//                                Ped.Tasks.Clear();
//                                Ped.Tasks.ClearSecondary();
//                                performDrivingManeuverTask = null;
//                            });
//                        }
//                    }
//                    else
//                    {
//                        if ((fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive) && performDrivingManeuverTask == null)
//                        {
//                            if ((goToTask != null && goToTask.IsActive) || (performDrivingManeuverTask != null && performDrivingManeuverTask.IsActive))
//                            {
//                                Ped.Tasks.Clear();
//                                Ped.Tasks.ClearSecondary();
//                            }

//                            if (useVehicleCannon)
//                            {
//                                //NativeFunction.Natives.GiveWeaponToPed(Ped, Game.GetHashKey("VEHICLE_WEAPON_WATER_CANNON"), true, true);
//                                NativeFunction.Natives.TaskVehicleShootAtCoord(Ped, targetFire.Position.X, targetFire.Position.Y, targetFire.Position.Z, 250.0f);
//                                fireWeaponAtTargetFireTask = Task.GetTask(Ped, "TASK_VEHICLE_SHOOT_AT_COORD");
//                            }
//                            else
//                            {
//                                fireWeaponAtTargetFireTask = Ped.Tasks.FireWeaponAt(targetFire.Position, 7500, FiringPattern.FullAutomatic);
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        else
//        {
//            if ((goToTask == null || !goToTask.IsActive) && (performDrivingManeuverTask == null || !performDrivingManeuverTask.IsActive))
//            {
//                if (useVehicleCannon)
//                {
//                    goToTask = Ped.Tasks.DriveToPosition(position, 3.0f, VehicleDrivingFlags.DriveBySight, range - 5.0f);
//                }
//                else
//                {
//                    goToTask = Ped.Tasks.FollowNavigationMeshToPosition(position.Around2D(range), Ped.Position.GetHeadingTowards(position), 2.0f, 5.0f);
//                }
//            }
//        }
//    }

//    protected override void OnFinished(bool isAborted)
//    {
//        Ped.Tasks.Clear();
//        firesToExtinguish = null;
//        closestFire = null;
//        furthestFire = null;
//        targetFire = null;
//        fireWeaponAtTargetFireTask = null;
//        goToTask = null;
//    }
//}