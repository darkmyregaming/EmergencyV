namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class SawEquipment : IFirefighterEquipment
    {
        private const string SawControllerMemoryKey = "SawController";

        string IFirefighterEquipment.DisplayName => "Saw";
        bool IFirefighterEquipment.ShouldUpdateIfEquipped => true;

        bool IFirefighterEquipment.IsEquipped(FirefighterEquipmentController controller)
        {
            SawController c = controller.Memory.GetOrDefault(SawControllerMemoryKey, null) as SawController;

            return c != null && c.IsActive;
        }

        void IFirefighterEquipment.OnGetEquipment(FirefighterEquipmentController controller)
        {
            SawController c = controller.Memory.GetOrDefault(SawControllerMemoryKey, null) as SawController;
            if (c == null)
            {
                c = new SawController(controller);
                controller.Memory.SetOrAdd(SawControllerMemoryKey, c);
            }

            c.IsActive = true;
        }

        void IFirefighterEquipment.OnLeaveEquipment(FirefighterEquipmentController controller)
        {
            SawController c = controller.Memory.GetOrDefault(SawControllerMemoryKey, null) as SawController;
            if (c == null)
            {
                c = new SawController(controller);
                controller.Memory.SetOrAdd(SawControllerMemoryKey, c);
            }

            c.IsActive = false;
        }

        void IFirefighterEquipment.OnEquippedUpdate(FirefighterEquipmentController controller)
        {
            SawController c = controller.Memory.GetOrDefault(SawControllerMemoryKey, null) as SawController;

            c?.OnActiveUpdate();
        }



        private class SawController
        {
            private readonly FirefighterEquipmentController controller;

            private Rage.Object saw;

            private Entity currentEntity;
            private bool isVehicle, isPed, isObject;

            private VehicleDoor[] vehicleDoors;
            private string closestVehicleDoorBoneName;
            private int closestVehicleDoorIndex;
            private float vehicleDoorDetachedPercentage; // maybe save door breaking percentage?

            private PercentageBar breakingPercentageBar;

            private uint gameTimeSincePedStartedAnimation;

            private uint lastParticleStartGameTime;
            private uint gameTimeForNextParticleStart;

            private bool isActive = false;
            public bool IsActive
            {
                get
                {
                    return isActive;
                }
                set
                {
                    if (value == isActive)
                        return;
                    if (value)
                    {
                        saw = new Rage.Object("prop_tool_consaw", Vector3.Zero);
                        saw.AttachTo(controller.Ped, controller.Ped.GetBoneIndex(PedBoneId.RightPhHand), new Vector3(0.095f, 0f, 0f), new Rotator(90f, 170f, 0f));
                        NativeFunction.Natives.SetCurrentPedWeapon(controller.Ped, Game.GetHashKey("WEAPON_UNARMED"), true);
                        controller.Ped.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
                        gameTimeSincePedStartedAnimation = Game.GameTime;
                    }
                    else
                    {
                        if (saw)
                        {
                            saw.Delete();
                        }

                        controller.Ped.Tasks.ClearSecondary();
                    }

                    isActive = value;
                }
            }

            public SawController(FirefighterEquipmentController controller)
            {
                this.controller = controller;
            }

            public void OnActiveUpdate()
            {
                if (controller.IsPlayer)
                {
                    for (int i = 0; i < ControlsToDisable.Length; i++)
                    {
                        NativeFunction.Natives.DisableControlAction(0, (int) ControlsToDisable[i]);
                    }

                    if (NativeFunction.Natives.IsDisabledControlPressed<bool>(0, (int) GameControl.Attack))
                    {
                        // TODO: extract code to method, and allow AI peds to use the saw 
                        Vector3 start = saw.LeftPosition + saw.RightVector * 0.2f;
                        Vector3 end = start - saw.RightVector * 0.25f;

                        HitResult hitResult = World.TraceCapsule(start, end, 0.15f, TraceFlags.IntersectEverything, controller.Ped, saw);

#if DEBUG
                        Util.DrawLine(start, end, System.Drawing.Color.Red);
                        Util.DrawMarker(28, start, Vector3.Zero, Rotator.Zero, new Vector3(0.05f), System.Drawing.Color.Red);
#endif

                        if (hitResult.Hit && hitResult.HitEntity)
                        {
                            if (hitResult.HitEntity != currentEntity)
                            {
                                currentEntity = hitResult.HitEntity;
                                isVehicle = currentEntity is Vehicle;
                                isPed = !isVehicle && currentEntity is Ped;
                                isObject = !isVehicle && !isPed;
                                vehicleDoors = isVehicle ? ((Vehicle) currentEntity).GetDoors() : null;
                                closestVehicleDoorBoneName = null;
                                closestVehicleDoorIndex = -1;
                                vehicleDoorDetachedPercentage = 0.0f;
                                if (breakingPercentageBar != null)
                                {
                                    breakingPercentageBar.Delete();
                                    breakingPercentageBar = null;
                                }
                            }

#if DEBUG
                            Util.DrawMarker(2, currentEntity.AbovePosition + currentEntity.UpVector * 1.25f, Vector3.Zero, new Rotator(180f, 0f, 0f), new Vector3(0.5f), System.Drawing.Color.Red, true);
#endif


                            if ((Game.GameTime - lastParticleStartGameTime) > gameTimeForNextParticleStart)
                            {
                                if (isVehicle)
                                {
                                    Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 25f), 0.75f);
                                    Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 155f), 0.75f);
                                }
                                else if (isPed)
                                {
                                    // "cut_solomon5", "cs_sol5_blood_head_shot"
                                    // "cut_michael2", "liquid_splash_blood"
                                    Util.StartParticleFxNonLoopedOnEntity("cut_solomon5", "cs_sol5_blood_head_shot", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 25f), MathHelper.GetRandomSingle(2.0f, 4.25f));
                                    Util.StartParticleFxNonLoopedOnEntity("cut_solomon5", "cs_sol5_blood_head_shot", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 155f), MathHelper.GetRandomSingle(2.0f, 4.25f));

                                    NativeFunction.Natives.PlayPain(currentEntity, MathHelper.GetRandomInteger(6, 7), 0, 0);
                                }
                                else if (isObject)
                                {
                                    // TODO
                                    Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 25f), 0.75f);
                                    Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 155f), 0.75f);
                                }

                                lastParticleStartGameTime = Game.GameTime;
                                gameTimeForNextParticleStart = (uint) MathHelper.GetRandomInteger(100, 275);

                            }

                            if (isVehicle)
                            {
                                Vehicle v = (Vehicle) currentEntity;

                                string closestBoneName = null;
                                int closestIndex = 0;
                                float closestDistSqr = 999999f * 999999f;

                                Vector3 sawPos = saw.LeftPosition;
                                for (int i = 0; i < vehicleDoors.Length; i++)
                                {
                                    VehicleDoor d = vehicleDoors[i];

                                    if (!d.IsDamaged)
                                    {
                                        string boneName = Util.GetVehicleDoorBoneName(d);
                                        Vector3 pos = v.GetBonePosition(boneName);

                                        float distSqr = Vector3.DistanceSquared(sawPos, pos);
                                        if (distSqr < closestDistSqr)
                                        {
                                            closestDistSqr = distSqr;
                                            closestBoneName = boneName;
                                            closestIndex = d.Index;
                                        }
                                    }
                                }

#if DEBUG
                                Game.DisplayHelp($"Closest:{closestBoneName}~n~DistSqr:{closestDistSqr}~n~Dist:{System.Math.Sqrt(closestDistSqr)}");
#endif

                                if (closestBoneName != null && closestDistSqr < 1.5f * 1.5f)
                                {
                                    if (closestVehicleDoorBoneName != closestBoneName)
                                    {
                                        closestVehicleDoorBoneName = closestBoneName;
                                        closestVehicleDoorIndex = closestIndex;
                                        vehicleDoorDetachedPercentage = 0.0f;
                                    }

                                    if (breakingPercentageBar == null)
                                        breakingPercentageBar = new PercentageBar(closestVehicleDoorBoneName);

                                    vehicleDoorDetachedPercentage += 0.0825f * Game.FrameTime;
                                    breakingPercentageBar.Percentage = vehicleDoorDetachedPercentage;

#if DEBUG
                                    Game.DisplaySubtitle("~b~" + vehicleDoorDetachedPercentage);
#endif
                                    if (vehicleDoorDetachedPercentage >= 1f)
                                    {
                                        v.Doors[closestVehicleDoorIndex].BreakOff();
                                        breakingPercentageBar.Delete();
                                        breakingPercentageBar = null;

                                        API.Functions.OnVehicleDoorRemovedWithSaw(v, closestVehicleDoorIndex, Game.LocalPlayer.Character, true);
                                    }
                                }
                                else
                                {
                                    if (breakingPercentageBar != null)
                                    {
                                        breakingPercentageBar.Delete();
                                        breakingPercentageBar = null;
                                    }
                                }
                            }
                            else if (isPed)
                            {
                                Ped p = (Ped) currentEntity;

                                if (p.IsAlive)
                                {
                                    p.Health -= 3;
                                }
                            }
                            else if (isObject)
                            {
                                // TODO
                                // idea: when cutting a garage door, teleport player to a mp garage; would be cool for extinguishing fires in interiors
                            }
                        }
                        else
                        {
                            CleanCurrentEntity();
                        }
                    }
                    else
                    {
                        CleanCurrentEntity();
                    }

                    if ((Game.GameTime - gameTimeSincePedStartedAnimation) > 2000 && !NativeFunction.Natives.IsEntityPlayingAnim<bool>(controller.Ped, "weapons@heavy@minigun", "idle_2_aim_right_med", 3)) // TODO: change animation
                    {
                        controller.Ped.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
                        gameTimeSincePedStartedAnimation = Game.GameTime;
                    }
                }
            }

            private void CleanCurrentEntity()
            {
                currentEntity = null;
                isVehicle = isPed = isObject = false;
                vehicleDoors = null;
                closestVehicleDoorBoneName = null;
                closestVehicleDoorIndex = -1;
                vehicleDoorDetachedPercentage = 0.0f;
                if (breakingPercentageBar != null)
                {
                    breakingPercentageBar.Delete();
                    breakingPercentageBar = null;
                }
            }

            public void OnCleanUp(bool isTerminating)
            {
                if (saw)
                {
                    saw.Delete();
                }

                controller.Ped.Tasks.ClearSecondary();
            }

            private static readonly GameControl[] ControlsToDisable =
            {
                GameControl.Attack,
                GameControl.Attack2,
                GameControl.CellphoneUp,
                GameControl.CharacterWheel,
                GameControl.SelectWeapon,
                GameControl.Enter,
            };
        }
    }
}


//private class SawTool
//{
//    Rage.Object saw;

//    private bool isActive = false;
//    public bool IsActive
//    {
//        get
//        {
//            return isActive;
//        }
//        set
//        {
//            if (value == isActive)
//                return;
//            if (value)
//            {
//                saw = new Rage.Object("prop_tool_consaw", Vector3.Zero);
//                saw.AttachTo(Game.LocalPlayer.Character, Game.LocalPlayer.Character.GetBoneIndex(PedBoneId.RightPhHand), new Vector3(0.095f, 0f, 0f), new Rotator(90f, 170f, 0f));
//                NativeFunction.Natives.SetCurrentPedWeapon(Game.LocalPlayer.Character, Game.GetHashKey("WEAPON_UNARMED"), true);
//                Game.LocalPlayer.Character.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
//                gameTimeSincePlayerStartedAnimation = Game.GameTime;
//            }
//            else
//            {
//                if (saw)
//                {
//                    saw.Delete();
//                }

//                Game.LocalPlayer.Character.Tasks.ClearSecondary();
//            }

//            isActive = value;
//        }
//    }

//    Entity currentEntity;
//    bool isVehicle, isPed, isObject;

//    VehicleDoor[] vehicleDoors;
//    string closestVehicleDoorBoneName;
//    int closestVehicleDoorIndex;
//    float vehicleDoorDetachedPercentage; // maybe save door breaking percentage?

//    PercentageBar breakingPercentageBar;

//    public void OnActiveUpdate()
//    {
//        for (int i = 0; i < controlsToDisable.Length; i++)
//        {
//            NativeFunction.Natives.DisableControlAction(0, (int)controlsToDisable[i]);
//        }

//        if (NativeFunction.Natives.IsDisabledControlPressed<bool>(0, (int)GameControl.Attack))
//        {
//            Vector3 start = saw.LeftPosition + saw.RightVector * 0.2f;
//            Vector3 end = start - saw.RightVector * 0.25f;

//            HitResult hitResult = World.TraceCapsule(start, end, 0.15f, TraceFlags.IntersectEverything, Game.LocalPlayer.Character, saw);

//#if DEBUG
//            Util.DrawLine(start, end, System.Drawing.Color.Red);
//            Util.DrawMarker(28, start, Vector3.Zero, Rotator.Zero, new Vector3(0.05f), System.Drawing.Color.Red);
//#endif

//            if (hitResult.Hit && hitResult.HitEntity)
//            {
//                if (hitResult.HitEntity != currentEntity)
//                {
//                    currentEntity = hitResult.HitEntity;
//                    isVehicle = currentEntity is Vehicle;
//                    isPed = !isVehicle && currentEntity is Ped;
//                    isObject = !isVehicle && !isPed;
//                    vehicleDoors = isVehicle ? ((Vehicle)currentEntity).GetDoors() : null;
//                    closestVehicleDoorBoneName = null;
//                    closestVehicleDoorIndex = -1;
//                    vehicleDoorDetachedPercentage = 0.0f;
//                    if (breakingPercentageBar != null)
//                    {
//                        breakingPercentageBar.Delete();
//                        breakingPercentageBar = null;
//                    }
//                }

//#if DEBUG
//                Util.DrawMarker(2, currentEntity.AbovePosition + currentEntity.UpVector * 1.25f, Vector3.Zero, new Rotator(180f, 0f, 0f), new Vector3(0.5f), System.Drawing.Color.Red, true);
//#endif


//                if ((Game.GameTime - lastParticleStartGameTime) > gameTimeForNextParticleStart)
//                {
//                    if (isVehicle)
//                    {
//                        Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 25f), 0.75f);
//                        Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 155f), 0.75f);
//                    }
//                    else if (isPed)
//                    {
//                        // "cut_solomon5", "cs_sol5_blood_head_shot"
//                        // "cut_michael2", "liquid_splash_blood"
//                        Util.StartParticleFxNonLoopedOnEntity("cut_solomon5", "cs_sol5_blood_head_shot", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 25f), MathHelper.GetRandomSingle(2.0f, 4.25f));
//                        Util.StartParticleFxNonLoopedOnEntity("cut_solomon5", "cs_sol5_blood_head_shot", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 155f), MathHelper.GetRandomSingle(2.0f, 4.25f));

//                        NativeFunction.Natives.PlayPain(currentEntity, MathHelper.GetRandomInteger(6, 7), 0, 0);
//                    }
//                    else if (isObject)
//                    {
//                        // TODO
//                        Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 25f), 0.75f);
//                        Util.StartParticleFxNonLoopedOnEntity("des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp", saw, new Vector3(-0.715f, 0.005f, 0f), new Rotator(0f, 0f, 155f), 0.75f);
//                    }

//                    lastParticleStartGameTime = Game.GameTime;
//                    gameTimeForNextParticleStart = (uint)MathHelper.GetRandomInteger(100, 275);

//                }

//                if (isVehicle)
//                {
//                    Vehicle v = (Vehicle)currentEntity;

//                    string closestBoneName = null;
//                    int closestIndex = 0;
//                    float closestDistSqr = 999999f * 999999f;

//                    Vector3 sawPos = saw.LeftPosition;
//                    for (int i = 0; i < vehicleDoors.Length; i++)
//                    {
//                        VehicleDoor d = vehicleDoors[i];

//                        if (!d.IsDamaged)
//                        {
//                            string boneName = Util.GetVehicleDoorBoneName(d);
//                            Vector3 pos = v.GetBonePosition(boneName);

//                            float distSqr = Vector3.DistanceSquared(sawPos, pos);
//                            if (distSqr < closestDistSqr)
//                            {
//                                closestDistSqr = distSqr;
//                                closestBoneName = boneName;
//                                closestIndex = d.Index;
//                            }
//                        }
//                    }

//#if DEBUG
//                    Game.DisplayHelp($"Closest:{closestBoneName}~n~DistSqr:{closestDistSqr}~n~Dist:{System.Math.Sqrt(closestDistSqr)}");
//#endif

//                    if (closestBoneName != null && closestDistSqr < 1.5f * 1.5f)
//                    {
//                        if (closestVehicleDoorBoneName != closestBoneName)
//                        {
//                            closestVehicleDoorBoneName = closestBoneName;
//                            closestVehicleDoorIndex = closestIndex;
//                            vehicleDoorDetachedPercentage = 0.0f;
//                        }

//                        if (breakingPercentageBar == null)
//                            breakingPercentageBar = new PercentageBar(closestVehicleDoorBoneName);

//                        vehicleDoorDetachedPercentage += 0.0825f * Game.FrameTime;
//                        breakingPercentageBar.Percentage = vehicleDoorDetachedPercentage;

//#if DEBUG
//                        Game.DisplaySubtitle("~b~" + vehicleDoorDetachedPercentage);
//#endif
//                        if (vehicleDoorDetachedPercentage >= 1f)
//                        {
//                            v.Doors[closestVehicleDoorIndex].BreakOff();
//                            breakingPercentageBar.Delete();
//                            breakingPercentageBar = null;

//                            API.Functions.OnVehicleDoorRemovedWithSaw(v, closestVehicleDoorIndex, Game.LocalPlayer.Character, true);
//                        }
//                    }
//                    else
//                    {
//                        if (breakingPercentageBar != null)
//                        {
//                            breakingPercentageBar.Delete();
//                            breakingPercentageBar = null;
//                        }
//                    }
//                }
//                else if (isPed)
//                {
//                    Ped p = (Ped)currentEntity;

//                    if (p.IsAlive)
//                    {
//                        p.Health -= 3;
//                    }
//                }
//                else if (isObject)
//                {
//                    // TODO
//                    // idea: when cutting a garage door, teleport player to a mp garage; would be cool for extinguishing fires in interiors
//                }
//            }
//            else
//            {
//                CleanCurrentEntity();
//            }
//        }
//        else
//        {
//            CleanCurrentEntity();
//        }

//        if ((Game.GameTime - gameTimeSincePlayerStartedAnimation) > 2000 && !NativeFunction.Natives.IsEntityPlayingAnim<bool>(Game.LocalPlayer.Character, "weapons@heavy@minigun", "idle_2_aim_right_med", 3)) // TODO: change animation
//        {
//            Game.LocalPlayer.Character.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
//            gameTimeSincePlayerStartedAnimation = Game.GameTime;
//        }
//    }

//    private void CleanCurrentEntity()
//    {
//        currentEntity = null;
//        isVehicle = isPed = isObject = false;
//        vehicleDoors = null;
//        closestVehicleDoorBoneName = null;
//        closestVehicleDoorIndex = -1;
//        vehicleDoorDetachedPercentage = 0.0f;
//        if (breakingPercentageBar != null)
//        {
//            breakingPercentageBar.Delete();
//            breakingPercentageBar = null;
//        }
//    }

//    public void OnCleanUp(bool isTerminating)
//    {
//        if (saw)
//        {
//            saw.Delete();
//        }

//        Game.LocalPlayer.Character.Tasks.ClearSecondary();
//    }

//    uint gameTimeSincePlayerStartedAnimation;

//    uint lastParticleStartGameTime;
//    uint gameTimeForNextParticleStart;

//    readonly GameControl[] controlsToDisable =
//    {
//                GameControl.Attack,
//                GameControl.Attack2,
//                GameControl.CellphoneUp,
//                GameControl.CharacterWheel,
//                GameControl.SelectWeapon,
//                GameControl.Enter,
//            };
//}