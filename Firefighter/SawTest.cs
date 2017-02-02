namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;
    using Object = Rage.Object;

    internal class SawTest
    {
        //"des_fib_floor", "ent_ray_fbi5a_ramp_metal_imp"

        //dict = "weapons@heavy@minigun";
        //name = "idle_2_aim_right_med";

        // "prop_tool_consaw"
        Object saw;
        
        private bool sawActive = false;
        public bool SawActive
        {
            get
            {
                return sawActive;
            }
            set
            {
                if (value == sawActive)
                    return;
                if (value)
                {
                    saw = new Object("prop_tool_consaw", Vector3.Zero);
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
                
                sawActive = value;
            }
        }

        public SawTest()
        {
        }

        Entity currentEntity;
        bool isVehicle, isPed, isObject;

        public void Update()
        {
            if (Game.IsKeyDown(System.Windows.Forms.Keys.I))
            {
                SawActive = !SawActive;
            }
            else if (SawActive) // TODO: add sound
            {
                for (int i = 0; i < controlsToDisable.Length; i++)
                {
                    NativeFunction.Natives.DisableControlAction(0, (int)controlsToDisable[i]);
                }

                if (NativeFunction.Natives.IsDisabledControlPressed<bool>(0, (int)GameControl.Attack))
                {
                    Vector3 start = saw.LeftPosition + saw.RightVector * 0.2f;
                    Vector3 end = start - saw.RightVector * 0.25f;

                    HitResult hitResult = World.TraceCapsule(start, end, 0.05f, TraceFlags.IntersectEverything, Game.LocalPlayer.Character, saw);

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

                            if (MathHelper.GetChance(10))
                            {
                                foreach (VehicleDoor d in v.GetDoors())
                                {
                                    if (!d.IsDamaged)
                                    {
                                        d.BreakOff();
                                    }
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
                            if (MathHelper.GetChance(10))
                            {
                                currentEntity.Delete();
                            }
                        }
                    }
                    else
                    {
                        currentEntity = null;
                        isVehicle = isPed = isObject = false;
                    }
                }

                if ((Game.GameTime - gameTimeSincePlayerStartedAnimation) > 2000 && !NativeFunction.Natives.IsEntityPlayingAnim<bool>(Game.LocalPlayer.Character, "weapons@heavy@minigun", "idle_2_aim_right_med", 3)) // TODO: change animation
                {
                    Game.LocalPlayer.Character.Tasks.PlayAnimation("weapons@heavy@minigun", "idle_2_aim_right_med", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);
                    gameTimeSincePlayerStartedAnimation = Game.GameTime;
                }
            }
        }

        readonly GameControl[] controlsToDisable =
        {
            GameControl.Attack,
            GameControl.Attack2,
            GameControl.CellphoneUp,
            GameControl.CharacterWheel,
            GameControl.SelectWeapon,
        };

        uint gameTimeSincePlayerStartedAnimation;

        uint lastParticleStartGameTime;
        uint gameTimeForNextParticleStart;
    }
}