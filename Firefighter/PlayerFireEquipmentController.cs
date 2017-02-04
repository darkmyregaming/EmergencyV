namespace EmergencyV
{
    // System
    using System;
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    internal class PlayerFireEquipmentController : FirefighterEquipmentController
    {
        private static PlayerFireEquipmentController instance;
        public static PlayerFireEquipmentController Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerFireEquipmentController();
                return instance;
            }
        }

        public override bool HasFireGear
        {
            get
            {
                return base.HasFireGear;
            }

            set
            {
                bool prevValue = base.HasFireGear;
                base.HasFireGear = value;
                if (base.HasFireGear != prevValue && Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
                {
                    if (value)
                    {
                        CreateToggleFlashlightItem();
                    }
                    else
                    {
                        RemoveToggleFlashlightItem();
                    }
                }
            }
        }

        SawTool saw = new SawTool();
        public bool HasSaw
        {
            get { return saw.IsActive; }
            set { saw.IsActive = value; }
        }

        private PlayerFireEquipmentController() : base(Game.LocalPlayer.Character)
        {
        }

        private bool isNearFiretruck = false;
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;

        internal override void Update()
        {
            Ped = Game.LocalPlayer.Character;

            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 3.25)
            {
                bool nearFiretruckNow = IsFiretruckNearbyPlayer();

                if (isNearFiretruck != nearFiretruckNow)
                {
                    if (nearFiretruckNow)
                    {
                        CreateVehicleEquipmentMenu();
                    }
                    else
                    {
                        RemoveVehicleEquipmentMenu();
                    }
                }

                isNearFiretruck = nearFiretruckNow;
            }

            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
            {
                if (IsFlashlightOn)
                {
                    Vector3 flashlightPos = Game.LocalPlayer.Character.GetOffsetPosition(Game.LocalPlayer.Character.GetPositionOffset(Game.LocalPlayer.Character.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

                    Util.DrawSpotlightWithShadow(flashlightPos, Game.LocalPlayer.Character.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
                }
            }

            if (saw.IsActive)
            {
                saw.OnActiveUpdate();
            }
        }

        internal void CleanUp(bool isTerminating)
        {
            saw.OnCleanUp(isTerminating);
        }

        private bool IsFiretruckNearbyPlayer()
        {
            if (!Game.LocalPlayer.Character)
                return false;

            bool isNearAnyFiretruck = false;

            Vehicle[] nearbyFiretrucks = Game.LocalPlayer.Character.GetNearbyVehicles(4).Where(v => v.Model == new Model("firetruk")).ToArray();
            if (nearbyFiretrucks.Length >= 1)
            {
                for (int i = 0; i < nearbyFiretrucks.Length; i++)
                {
                    Vehicle v = nearbyFiretrucks[0];
                    if (v && Vector3.DistanceSquared(v.RearPosition, Game.LocalPlayer.Character.Position) < 2.5f * 2.5f)
                    {
                        isNearAnyFiretruck = true;
                    }
                }
            }

            return isNearAnyFiretruck;
        }

        private void CreateVehicleEquipmentMenu()
        {
            PluginMenu.Instance.AddMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.AddItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM", "MAIN_MENU", "Equipment", null, null, "VEHICLE_EQUIPMENT_SUBMENU");


            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_FIRE_GEAR_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasFireGear ? "Leave Fire Gear" : "Get Fire Gear", () =>
            {
                HasFireGear = !HasFireGear;
                PluginMenu.Instance.UpdateItem("VEHICLE_EQUIPMENT_FIRE_GEAR_ITEM", HasFireGear ? "Leave Fire Gear" : "Get Fire Gear");
            });

            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_FIRE_EXTINGUISHER_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasFireExtinguisher ? "Leave Fire Extinguisher" : "Get Fire Extinguisher", () =>
            {
                HasSaw = false;
                HasFireExtinguisher = !HasFireExtinguisher;
                PluginMenu.Instance.UpdateItem("VEHICLE_EQUIPMENT_FIRE_EXTINGUISHER_ITEM", HasFireExtinguisher ? "Leave Fire Extinguisher" : "Get Fire Extinguisher");
                PluginMenu.Instance.UpdateItem("VEHICLE_EQUIPMENT_SAW_ITEM", HasSaw ? "Leave Saw" : "Get Saw");
            });

            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_SAW_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasSaw ? "Leave Saw" : "Get Saw", () =>
            {
                HasSaw = !HasSaw;
                PluginMenu.Instance.UpdateItem("VEHICLE_EQUIPMENT_SAW_ITEM", HasSaw ? "Leave Saw" : "Get Saw");
            });
        }

        private void RemoveVehicleEquipmentMenu()
        {
            PluginMenu.Instance.RemoveMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.RemoveItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM");
        }

        private void CreateToggleFlashlightItem()
        {
            PluginMenu.Instance.AddItem("TOGGLE_FLASHLIGHT_ITEM", "ACTIONS_SUBMENU", "Toggle Flashlight", () => { IsFlashlightOn = !IsFlashlightOn; }, Plugin.Controls["TOGGLE_FLASHLIGHT"]);
        }

        private void RemoveToggleFlashlightItem()
        {
            PluginMenu.Instance.RemoveItem("TOGGLE_FLASHLIGHT_ITEM");
        }





        private class SawTool
        {
            Rage.Object saw;

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
                        saw.AttachTo(Game.LocalPlayer.Character, Game.LocalPlayer.Character.GetBoneIndex(PedBoneId.RightPhHand), new Vector3(0.095f, 0f, 0f), new Rotator(90f, 170f, 0f));
                        NativeFunction.Natives.SetCurrentPedWeapon(Game.LocalPlayer.Character, Game.GetHashKey("WEAPON_UNARMED"), true);
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
                        gameTimeSincePlayerStartedAnimation = Game.GameTime;
                    }
                    else
                    {
                        if (saw)
                        {
                            saw.Delete();
                        }

                        Game.LocalPlayer.Character.Tasks.ClearSecondary();
                    }

                    isActive = value;
                }
            }

            Entity currentEntity;
            bool isVehicle, isPed, isObject;

            VehicleDoor[] vehicleDoors;
            string closestVehicleDoorBoneName;
            int closestVehicleDoorIndex;
            float vehicleDoorDetachedPercentage; // maybe save door breaking percentage?

            PercentageBar breakingPercentageBar;

            public void OnActiveUpdate()
            {
                for (int i = 0; i < controlsToDisable.Length; i++)
                {
                    NativeFunction.Natives.DisableControlAction(0, (int)controlsToDisable[i]);
                }

                if (NativeFunction.Natives.IsDisabledControlPressed<bool>(0, (int)GameControl.Attack))
                {
                    Vector3 start = saw.LeftPosition + saw.RightVector * 0.2f;
                    Vector3 end = start - saw.RightVector * 0.25f;

                    HitResult hitResult = World.TraceCapsule(start, end, 0.15f, TraceFlags.IntersectEverything, Game.LocalPlayer.Character, saw);

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
                            vehicleDoors = isVehicle ? ((Vehicle)currentEntity).GetDoors() : null;
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
                            gameTimeForNextParticleStart = (uint)MathHelper.GetRandomInteger(100, 275);

                        }

                        if (isVehicle)
                        {
                            Vehicle v = (Vehicle)currentEntity;

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
                            Ped p = (Ped)currentEntity;

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

                if ((Game.GameTime - gameTimeSincePlayerStartedAnimation) > 2000 && !NativeFunction.Natives.IsEntityPlayingAnim<bool>(Game.LocalPlayer.Character, "weapons@heavy@minigun", "idle_2_aim_right_med", 3)) // TODO: change animation
                {
                    Game.LocalPlayer.Character.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
                    gameTimeSincePlayerStartedAnimation = Game.GameTime;
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

                Game.LocalPlayer.Character.Tasks.ClearSecondary();
            }

            uint gameTimeSincePlayerStartedAnimation;

            uint lastParticleStartGameTime;
            uint gameTimeForNextParticleStart;

            readonly GameControl[] controlsToDisable =
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
