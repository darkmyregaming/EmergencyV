namespace EmergencyV
{
    // System
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    // RPH
    using Rage;
    using Rage.Native;
    
    internal class FirefightersUnit
    {
        public FireStation DesignatedFireStation { get; }
        public FireStation.ParkingSpot DesignatedParkingSpot { get; }

        public Vehicle Vehicle { get; }
        public Firefighter Driver { get; private set; }

        public Firefighter[] Firefighters { get; } // includes Driver

        public Blip VehicleBlip { get; }
        public Blip RespondingVehicleBlip { get; private set; }

        public UnitAIController AI { get; }
        
        public bool IsParkedAtFireStation
        {
            get
            {
                return AI.CurrentTaskPriority <= UnitTask.UnitTaskPriority.Low && Vector3.DistanceSquared(Vehicle.Position, DesignatedParkingSpot.Location.Position) < 10.0f * 10.0f && Vehicle.Speed < 0.1f;
            }
        }

        public bool IsAnyFirefighterInVehicle
        {
            get
            {
                return Firefighters.Any(f => f.Ped.IsInVehicle(Vehicle, false));
            }
        }

        public bool AreAllFirefightersInVehicle
        {
            get
            {
                return Firefighters.All(f => f.Ped.IsInVehicle(Vehicle, false));
            }
        }
        
        public FirefightersUnit(FireStation fireStation)
        {
            DesignatedFireStation = fireStation;
            DesignatedParkingSpot = fireStation.GetFreeParkingSpot();
            Vehicle = new Vehicle(Plugin.UserSettings.VEHICLES.ENGINE_MODEL, DesignatedParkingSpot.Location.Position, DesignatedParkingSpot.Location.Heading);
            VehicleBlip = new Blip(Vehicle);
            VehicleBlip.Sprite = BlipSprite.ArmoredVan;
            VehicleBlip.Scale = 0.45f;
            VehicleBlip.Color = System.Drawing.Color.FromArgb(180, 0, 0);
            VehicleBlip.Name = "Firefighters Unit";
            NativeFunction.Natives.SetBlipAsShortRange(VehicleBlip, true);
            int seats = Math.Min(Vehicle.PassengerCapacity + 1, 4);
            Firefighters = new Firefighter[seats];
            for (int i = 0; i < seats; i++)
            {
                Firefighter f = new Firefighter(Vector3.Zero, 0.0f);
                f.PreferedVehicleSeatIndex = i - 1;
                f.Ped.WarpIntoVehicle(Vehicle, i - 1);
                f.Equipment.HasFireGear = false;

                if (i == 0)
                    Driver = f;
                Firefighters[i] = f;
            }
            AI = new UnitAIController(this);

            RegisterFirefightersUnit(this);
        }

        public void DeleteEntities()
        {
            if (VehicleBlip) VehicleBlip.Delete();
            if (RespondingVehicleBlip) RespondingVehicleBlip.Delete();
            if (Vehicle) Vehicle.Delete();
            foreach (Firefighter f in Firefighters)
                if (f.Ped) f.Ped.Delete();
        }

        public void DismissEntities()
        {
            if (VehicleBlip) VehicleBlip.Delete();
            if (RespondingVehicleBlip) RespondingVehicleBlip.Delete();
            if (Vehicle) Vehicle.Dismiss();
            foreach (Firefighter f in Firefighters)
                if (f.Ped) f.Ped.Dismiss();
        }

        private void SetResponding(bool value)
        {
            if (value)
            {
                if (!RespondingVehicleBlip)
                {
                    Game.LogTrivial("creating responding blip");
                    RespondingVehicleBlip = new Blip(Vehicle);
                    RespondingVehicleBlip.Sprite = BlipSprite.PoliceChase;
                    RespondingVehicleBlip.Scale = 0.475f;
                    RespondingVehicleBlip.Name = "Firefighters Unit - Responding";
                    RespondingVehicleBlip.Order = 1;
                    NativeFunction.Natives.SetBlipAsShortRange(RespondingVehicleBlip, true);
                }
            }
            else
            {
                Game.LogTrivial("deleting responding blip");
                if (RespondingVehicleBlip) RespondingVehicleBlip.Delete();
            }
        }

        private void Update()
        {
            AI.Update();

            if (DesignatedFireStation.IsCreated && IsParkedAtFireStation)
            {
                if (IsAnyFirefighterInVehicle)
                {
                    foreach (Firefighter f in Firefighters)
                    {
                        if (!f.AI.IsPerformingTaskOfType<AITaskLeaveVehicle>())
                            f.AI.LeaveVehicle(LeaveVehicleFlags.None);
                    }
                }
                else if (!AI.IsDoingAnyTask)
                {
                    AI.ChillAround();
                }
            }

            bool sirenOn = Vehicle.IsSirenOn;
            if (sirenOn && !RespondingVehicleBlip)
                SetResponding(true);
            else if (!sirenOn && RespondingVehicleBlip)
                SetResponding(false);
        }


        internal static List<FirefightersUnit> CurrentFirefightersUnits = new List<FirefightersUnit>();
        internal static GameFiber FirefightersUnitsUpdateFiber;

        private static void RegisterFirefightersUnit(FirefightersUnit u)
        {
            CurrentFirefightersUnits.Add(u);
            if (FirefightersUnitsUpdateFiber == null)
            {
                FirefightersUnitsUpdateFiber = GameFiber.StartNew(FirefightersUnitsUpdateLoop, "Firefighters Units Update Loop");
            }
        }

        private static void FirefightersUnitsUpdateLoop()
        {
            while (true)
            {
                for (int i = CurrentFirefightersUnits.Count - 1; i >= 0; i--)
                {
                    FirefightersUnit u = CurrentFirefightersUnits[i];
                    u.Update();
                }

                GameFiber.Yield();
            }
        }



        public class UnitAIController
        {
            public FirefightersUnit Unit { get; }

            public bool IsDoingAnyTask { get { return currentTask != null; } }
            public bool HasQueuedTasks { get { return tasksQueue != null && tasksQueue.Count > 0; } }
            public UnitTask.UnitTaskPriority CurrentTaskPriority { get { return IsDoingAnyTask ? currentTask.Priority : UnitTask.UnitTaskPriority.None; } }

            private Queue<UnitTask> tasksQueue;
            private UnitTask currentTask;

            public UnitAIController(FirefightersUnit unit)
            {
                Unit = unit;
                tasksQueue = new Queue<UnitTask>();
            }

            internal void Update()
            {
                if (IsDoingAnyTask)
                {
                    if (!currentTask.IsFinished)
                        currentTask.Update();
                }
                else if (HasQueuedTasks)
                {
                    SetCurrentTask(tasksQueue.Dequeue());
                }
            }

            private void SetCurrentTask(UnitTask task)
            {
                currentTask = task;
                currentTask.Finished += OnCurrentTaskFinished;
                currentTask.Start();
            }

            private void OnCurrentTaskFinished(UnitTask task)
            {
                if (currentTask != null)
                {
                    currentTask.Finished -= OnCurrentTaskFinished;
                    currentTask = null;
                }
            }

            public UnitTask DriveToPosition(Vector3 position, bool sirenOn, float speed, float acceptedDistance, VehicleDrivingFlags flags, bool considerTaskPriority = true) => GiveTask<DriveToPositionTask>(considerTaskPriority, Unit, position, sirenOn, speed, acceptedDistance, flags);
            public UnitTask DriveToStationAndPark(bool considerTaskPriority = true) => GiveTask<DriveToStationAndParkTask>(considerTaskPriority, Unit);
            public UnitTask ChillAround(bool considerTaskPriority = true) => GiveTask<ChillAroundTask>(considerTaskPriority, Unit);
            public UnitTask ExtinguishFireInArea(Vector3 position, float range, bool considerTaskPriority = true) => GiveTask<ExtinguishFireInAreaTask>(considerTaskPriority, Unit, position, range);

            // if considerTaskPriority is true and tasks priority is greater than current task, current task is aborted and the queue is cleared
            protected UnitTask GiveTask<TTask>(bool considerTaskPriority = true, params object[] args) where TTask : UnitTask
            {
                UnitTask t = (UnitTask)Activator.CreateInstance(typeof(TTask), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, args, null);
                Game.LogTrivial($"[FirefightersUnit.UnitAIController.GiveTask] ({typeof(TTask).Name}, ConsiderTaskPriority:{considerTaskPriority})");
                if (!IsDoingAnyTask)
                {
                    Game.LogTrivial($"[FirefightersUnit.UnitAIController.GiveTask]      No task running, setting as current task...");
                    SetCurrentTask(t);
                }
                else
                {
                    if (considerTaskPriority)
                    {
                        if (t.Priority > CurrentTaskPriority)
                        {
                            Game.LogTrivial($"[FirefightersUnit.UnitAIController.GiveTask]      Priority greater than current task, aborting current task, clearing queue and setting as current task...");
                            tasksQueue.Clear();
                            currentTask.Abort();
                            SetCurrentTask(t);
                        }
                        else
                        {
                            Game.LogTrivial($"[FirefightersUnit.UnitAIController.GiveTask]      Priority less than current task, enqueuing task...");
                            tasksQueue.Enqueue(t);
                        }
                    }
                    else
                    {
                        Game.LogTrivial($"[FirefightersUnit.UnitAIController.GiveTask]      Enqueuing task...");
                        tasksQueue.Enqueue(t);
                    }
                }

                return t;
            }
        }

        public abstract class UnitTask
        {
            public enum UnitTaskPriority
            {
                None = 0,
                Low = 1,
                Medium = 2,
                High = 3,
            }

            public abstract UnitTaskPriority Priority { get; }

            public delegate void UnitTaskFinishedEventHandler(UnitTask task);

            public FirefightersUnit Unit { get; }
            public event UnitTaskFinishedEventHandler Finished;
            private bool isFinished;
            public bool IsFinished
            {
                get { return isFinished; }
                protected set
                {
                    if (value == isFinished)
                        return;
                    isFinished = value;
                    if (isFinished)
                    {
                        OnFinished();
                        Finished?.Invoke(this);
                    }
                }
            }
            public bool IsAborted { get; private set; }

            public bool HasStarted { get; private set; }

            protected UnitTask(FirefightersUnit unit)
            {
                Unit = unit;
            }

            internal void Start()
            {
                if (HasStarted)
                    throw new InvalidOperationException();

                StartInternal();

                HasStarted = true;
            }

            internal void Update()
            {
                if (HasStarted && !IsAborted && !IsFinished)
                    UpdateInternal();
            }

            internal void Abort()
            {
                if(IsAborted)
                    throw new InvalidOperationException();
                
                IsAborted = true;
                IsFinished = true;
            }

            protected abstract void StartInternal();
            protected abstract void UpdateInternal();
            protected abstract void OnFinished();
        }

        public class ExtinguishFireInAreaTask : UnitTask
        {
            public override UnitTaskPriority Priority { get { return UnitTaskPriority.High; } }

            Vector3 position;
            float range;
            List<AITask> extinguishFiresTasks;

            protected ExtinguishFireInAreaTask(FirefightersUnit unit, Vector3 position, float range) : base(unit)
            {
                this.position = position;
                this.range = range;
            }

            protected override void StartInternal()
            {
                if (extinguishFiresTasks == null)
                    extinguishFiresTasks = new List<AITask>();

                foreach (Firefighter f in Unit.Firefighters)
                {
                    f.Ped.Tasks.Clear();
                    f.Equipment.HasFireExtinguisher = true;
                    f.Equipment.HasFireGear = true;
                    f.Equipment.IsFlashlightOn = true;
                    extinguishFiresTasks.Add(f.AI.ExtinguishFireInArea(position, range, false));
                }
            }

            protected override void UpdateInternal()
            {
                if (extinguishFiresTasks.All(t => t.IsFinished || !t.Ped || t.Ped.IsDead))
                    IsFinished = true;
            }

            protected override void OnFinished()
            {
                foreach (Firefighter f in Unit.Firefighters)
                {
                    if (f.Ped)
                    {
                        f.Ped.Tasks.Clear();

                        f.Equipment.HasFireExtinguisher = false;
                        f.Equipment.HasFireGear = false;
                        f.Equipment.IsFlashlightOn = false;
                    }
                }
            }
        }

        public class DriveToPositionTask : UnitTask
        {
            public override UnitTaskPriority Priority { get { return UnitTaskPriority.High; } }
            
            Vector3 position;
            bool sirenOn;
            float speed;
            float acceptedDistance;
            VehicleDrivingFlags flags;

            List<AITask> enterVehicleTasks;
            AITask drivingTask;

            protected DriveToPositionTask(FirefightersUnit unit, Vector3 position, bool sirenOn, float speed, float acceptedDistance, VehicleDrivingFlags flags) : base(unit)
            {
                this.position = position;
                this.sirenOn = sirenOn;
                this.speed = speed;
                this.acceptedDistance = acceptedDistance;
                this.flags = flags;
            }

            protected override void StartInternal()
            {
                foreach (Firefighter f in Unit.Firefighters)
                {
                    f.Ped.Tasks.Clear();
                    if (!f.Ped.IsInVehicle(Unit.Vehicle, false))
                    {
                        if (enterVehicleTasks == null)
                            enterVehicleTasks = new List<AITask>();

                        enterVehicleTasks.Add(f.AI.EnterVehicle(Unit.Vehicle, f.PreferedVehicleSeatIndex));
                    }
                }
            }

            protected override void UpdateInternal()
            {
                if (drivingTask == null && (enterVehicleTasks == null || enterVehicleTasks.All(t => t.IsFinished)))
                {
                    Unit.Vehicle.IsSirenOn = sirenOn;
                    drivingTask = Unit.Driver.AI.DriveTo(position, speed, acceptedDistance, flags);
                }
                else if (drivingTask != null && drivingTask.IsFinished)
                {
                    IsFinished = true;
                }
            }

            protected override void OnFinished()
            {
                foreach (Firefighter f in Unit.Firefighters)
                {
                    if (f.Ped) f.Ped.Tasks.Clear();
                }
            }
        }

        public class DriveToStationAndParkTask : UnitTask
        {
            public override UnitTaskPriority Priority { get { return UnitTaskPriority.Medium; } }

            List<AITask> enterVehicleTasks;
            AITask drivingTask;
            Task parkTask;

            protected DriveToStationAndParkTask(FirefightersUnit unit) : base(unit)
            {
            }

            protected override void StartInternal()
            {
                foreach (Firefighter f in Unit.Firefighters)
                {
                    f.Ped.Tasks.Clear();
                    if (!f.Ped.IsInVehicle(Unit.Vehicle, false))
                    {
                        if (enterVehicleTasks == null)
                            enterVehicleTasks = new List<AITask>();

                        enterVehicleTasks.Add(f.AI.EnterVehicle(Unit.Vehicle, f.PreferedVehicleSeatIndex));
                    }
                }
            }

            protected override void UpdateInternal()
            {
                if (drivingTask == null && (enterVehicleTasks == null || enterVehicleTasks.All(t => t.IsFinished)))
                {
                    Unit.Vehicle.IsSirenOn = false;
                    Vector3 parkPos = Unit.DesignatedParkingSpot.Location.Position;
                    Vector3 position;
                    NativeFunction.Natives.GetClosestVehicleNode(parkPos.X, parkPos.Y, parkPos.Z, out position, 1, 3.0f, 0.0f);
                    drivingTask = Unit.Driver.AI.DriveTo(position, 13.5f, 22.5f, VehicleDrivingFlags.YieldToCrossingPedestrians | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.DriveAroundPeds | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.AllowWrongWay);
                }
                else if (parkTask == null && (drivingTask != null && drivingTask.IsFinished))
                {
                    Vector3 parkPos = Unit.DesignatedParkingSpot.Location.Position;
                    float parkHeading = Unit.DesignatedParkingSpot.Location.Heading;
                    NativeFunction.Natives.TaskVehiclePark(Unit.Driver.Ped, Unit.Vehicle, parkPos.X, parkPos.Y, parkPos.Z, parkHeading, 1, 20f, false);
                    parkTask = Task.GetTask(Unit.Driver.Ped, "TASK_VEHICLE_PARK");
                }
                else if (parkTask != null && !parkTask.IsActive)
                {
                    IsFinished = true;
                }
            }

            protected override void OnFinished()
            {
                foreach (Firefighter f in Unit.Firefighters)
                {
                    if (f.Ped) f.Ped.Tasks.Clear();
                }
            }
        }

        public class ChillAroundTask : UnitTask
        {
            public override UnitTaskPriority Priority { get { return UnitTaskPriority.Low; } }

            uint lastChangeGameTime, nextChangeGameTime;

            protected ChillAroundTask(FirefightersUnit unit) : base(unit)
            {
            }

            protected override void StartInternal()
            {
                GiveTasks(true);

                lastChangeGameTime = Game.GameTime;
                nextChangeGameTime = unchecked(lastChangeGameTime + (uint)MathHelper.GetRandomInteger(8500, 30000));
            }

            protected override void UpdateInternal()
            {
                if (Game.GameTime > nextChangeGameTime)
                {
                    GiveTasks(false);

                    lastChangeGameTime = Game.GameTime;
                    nextChangeGameTime = unchecked(lastChangeGameTime + (uint)MathHelper.GetRandomInteger(8500, 30000));
                }
            }

            protected override void OnFinished()
            {
                foreach (Firefighter f in Unit.Firefighters)
                {
                    if (f.Ped) f.Ped.Tasks.Clear();
                }
            }

            private void GiveTasks(bool allShouldReceiveTask)
            {
                Firefighter chattingFirefighter1 = null, chattingFirefighter2 = null;
                if (Plugin.Random.Next(100) < 70)
                {
                    chattingFirefighter1 = MathHelper.Choose(Unit.Firefighters);
                    chattingFirefighter2 = MathHelper.Choose(Unit.Firefighters.Where(f => f != chattingFirefighter1).ToArray());
                    chattingFirefighter1.Ped.Tasks.Clear();
                    chattingFirefighter2.Ped.Tasks.Clear();

                    Vector3 targetPos = chattingFirefighter1.Ped.GetOffsetPositionFront(1.5f);
                    chattingFirefighter2.AI.WalkTo(targetPos, targetPos.GetHeadingTowards(chattingFirefighter1.Ped), 1.0f).Finished += (t, aborted) =>
                    {
                        NativeFunction.Natives.TaskChatToPed(chattingFirefighter1.Ped, chattingFirefighter2.Ped, 16, 0f, 0f, 0f, 0f, 0f);
                        NativeFunction.Natives.TaskChatToPed(chattingFirefighter2.Ped, chattingFirefighter1.Ped, 16, 0f, 0f, 0f, 0f, 0f);
                    };
                }

                foreach (Firefighter f in Unit.Firefighters)
                {
                    if (f != chattingFirefighter1 && f != chattingFirefighter2 && (allShouldReceiveTask || Plugin.Random.Next(101) < Plugin.Random.Next(10, 50)))
                    {
                        f.AI.WalkTo((MathHelper.Choose(Unit.Vehicle.FrontPosition, Unit.Vehicle.RearPosition, Unit.Vehicle.RightPosition, Unit.Vehicle.LeftPosition)).Around2D(2.5f, 8f), MathHelper.GetRandomSingle(0f, 360f), 1.0f).Finished += (t, aborted) =>
                        {
                            GameFiber.Sleep(200);
                            switch (Plugin.Random.Next(4))
                            {
                                default:
                                case 0: NativeFunction.Natives.TaskUseMobilePhone(t.Ped, 1, MathHelper.Choose(0, 1, 2)); break;
                                case 1: NativeFunction.Natives.TaskStartScenarioInPlace(t.Ped, "WORLD_HUMAN_SMOKING", 0, true); break;
                                case 2: NativeFunction.Natives.TaskStartScenarioInPlace(t.Ped, "WORLD_HUMAN_HANG_OUT_STREET", 0, true); break;
                                case 3: NativeFunction.Natives.TaskStartScenarioInPlace(t.Ped, "WORLD_HUMAN_STAND_IMPATIENT", 0, true); break;
                            }

                        };
                    }
                }
            }
        }
    }
}
