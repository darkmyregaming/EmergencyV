namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class Firefighter
    {
        public Ped Ped { get; }
        public FirefighterEquipmentController Equipment { get; }
        public AIController AI { get; }

        public Firefighter(Vector3 position, float heading)
        {
            Ped = new Ped(Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL, position, heading);
            Ped.BlockPermanentEvents = true;

            Equipment = new FirefighterEquipmentController(Ped);
            AI = new AIController(this);
        }

        public void Update()
        {
            if (Ped)
            {
                Equipment.Update();
                AI.Update();
            }
        }


        public class AIController
        {
            public bool IsEnabled { get; set; } = true;
            public Firefighter Firefighter { get; }
            public Ped Ped { get; }


            State state;
            public State CurrentState
            {
                get { return state; }
                set
                {
                    State prev = state;
                    state = value;
                    OnStateChanged(state, prev);
                }
            }

            Task goToTask;

            Vector3 extinguishFirePos;
            float extinguishFireRange;
            float extinguishFireRangeSq;
            List<Fire> firesToExtinguish;
            Fire targetFire;
            Task fireWeaponAtTargetFireTask;

            public AIController(Firefighter firefighter)
            {
                Firefighter = firefighter;
                Ped = firefighter.Ped;
            }

            public void Update()
            {
                if (IsEnabled && !Ped.IsDead)
                {
                    switch (state)
                    {
                        default:
                        case State.Idle:
                            break;
                            
                        case State.GoToPosition:
                            {
                                if (goToTask == null || !goToTask.IsActive)
                                {
                                    CurrentState = State.Idle;
                                }
                            }
                            break;

                        case State.ExtinguishingFireInArea: // TODO: add hose to AI controlled peds
                            {
                                if (Vector3.DistanceSquared(Ped.Position, extinguishFirePos) < extinguishFireRangeSq)
                                {
                                    if (!Firefighter.Equipment.HasFireExtinguisher)
                                        Firefighter.Equipment.HasFireExtinguisher = true;

                                    if (firesToExtinguish.Count == 0)
                                    {
                                        foreach (Fire f in World.GetAllFires())
                                        {
                                            if (Vector3.DistanceSquared(f.Position, extinguishFirePos) < extinguishFireRangeSq)
                                                firesToExtinguish.Add(f);
                                        }

                                        if (firesToExtinguish.Count == 0) // if no fires are found, this task is finished
                                        {
                                            CurrentState = State.Idle;
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
                                                targetFire = firesToExtinguish.OrderBy(f => Vector3.DistanceSquared(f.Position, Ped.Position)).FirstOrDefault();
                                            }
                                            else return;
                                        }
                                        else
                                        {
                                            if (Vector3.DistanceSquared(Ped.Position, targetFire) > 3.75f * 3.75f)
                                            {
                                                if (goToTask == null || !goToTask.IsActive)
                                                {
                                                    goToTask = Ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, Ped.Position.GetHeadingTowards(targetFire), 2.0f, 3.0f);
                                                }
                                            }
                                            else
                                            {
                                                if (fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive)
                                                {
                                                    fireWeaponAtTargetFireTask = Ped.Tasks.FireWeaponAt(targetFire.Position, 7500, FiringPattern.FullAutomatic);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if(goToTask == null || !goToTask.IsActive)
                                {
                                    goToTask = Ped.Tasks.FollowNavigationMeshToPosition(extinguishFirePos.Around2D(extinguishFireRange), Ped.Position.GetHeadingTowards(extinguishFirePos), 2.0f, 5.0f);
                                }
                            }
                            break;

                        case State.PerformingCPR:
                            break;

                        case State.EnteringVehicle:
                            break;

                        case State.Driving:
                            break;
                    }
                }
            }

            public void WalkTo(Vector3 position, float targetHeading, float distanceThreshold)
            {
                Log(nameof(WalkTo) + " called");
                goToTask = Ped.Tasks.FollowNavigationMeshToPosition(position, targetHeading, 1.0f, distanceThreshold);
                state = State.GoToPosition;
            }

            public void WalkStraightTo(Vector3 position, float targetHeading, float distanceToSlideAt)
            {
                Log(nameof(WalkStraightTo) + " called");
                goToTask = Ped.Tasks.GoStraightToPosition(position, 1.0f, targetHeading, distanceToSlideAt, -1);
                state = State.GoToPosition;
            }

            public void RunTo(Vector3 position, float targetHeading, float distanceThreshold)
            {
                Log(nameof(RunTo) + " called");
                goToTask = Ped.Tasks.FollowNavigationMeshToPosition(position, targetHeading, 2.0f, distanceThreshold);
                state = State.GoToPosition;
            }

            public void RunStraightTo(Vector3 position, float targetHeading, float distanceToSlideAt)
            {
                Log(nameof(RunStraightTo) + " called");
                goToTask = Ped.Tasks.GoStraightToPosition(position, 2.0f, targetHeading, distanceToSlideAt, -1);
                state = State.GoToPosition;
            }

            public void ExtinguishFireInArea(Vector3 position, float range)
            {
                Log(nameof(ExtinguishFireInArea) + " called");
                extinguishFirePos = position;
                extinguishFireRange = range;
                extinguishFireRangeSq = range * range;
                firesToExtinguish = new List<Fire>();
                state = State.ExtinguishingFireInArea;
            }

            //public void PerformCPR(Ped ped)
            //{
            //    throw new NotImplementedException(); // TODO: implement CPR for AI controlled peds
            //}

            private void OnStateChanged(State current, State previous)
            {
                switch (previous)
                {
                    case State.Idle:
                        break;

                    case State.GoToPosition:
                        Ped.Tasks.Clear();
                        goToTask = null;
                        break;

                    case State.ExtinguishingFireInArea:
                        Ped.Tasks.Clear();
                        firesToExtinguish = null;
                        targetFire = null;
                        fireWeaponAtTargetFireTask = null;
                        break;

                    case State.PerformingCPR:
                        break;

                    case State.EnteringVehicle:
                        break;

                    case State.Driving:
                        break;
                }
            }

            private void Log(object o)
            {
                Game.LogTrivial($"[Firefighter AI - State:{state}] {o}");
            }

            public enum State
            {
                Idle,
                GoToPosition,
                ExtinguishingFireInArea,
                PerformingCPR,
                EnteringVehicle,
                Driving,
            }
        }
    }
}
