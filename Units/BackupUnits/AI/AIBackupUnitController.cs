namespace EmergencyV
{
    // System
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class AIBackupUnitController
    {
        public delegate void AIBackupUnitControllerTaskEventHandler(AIBackupUnitTask task);

        public BackupUnit Unit { get; }

        public event AIBackupUnitControllerTaskEventHandler TaskStarted;
        public event AIBackupUnitControllerTaskEventHandler TaskFinished;

        public bool IsDoingAnyTask { get { return currentTask != null; } }
        public bool HasQueuedTasks { get { return tasksQueue != null && tasksQueue.Count > 0; } }
        public AIBackupUnitTaskPriority CurrentTaskPriority { get { return IsDoingAnyTask ? currentTask.Priority : AIBackupUnitTaskPriority.None; } }

        private Queue<AIBackupUnitTask> tasksQueue;
        private AIBackupUnitTask currentTask;

        public AIBackupUnitController(BackupUnit unit)
        {
            Unit = unit;
            tasksQueue = new Queue<AIBackupUnitTask>();
        }

        internal void AbortAllTasks()
        {
            if (currentTask != null && !currentTask.IsFinished)
                currentTask.Abort();

            foreach (AIBackupUnitTask t in tasksQueue)
            {
                t.Abort();
            }
            tasksQueue.Clear();
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

        private void SetCurrentTask(AIBackupUnitTask task)
        {
            currentTask = task;
            currentTask.Finished += OnCurrentTaskFinished;
            currentTask.Start();
            TaskStarted?.Invoke(currentTask);
        }

        private void OnCurrentTaskFinished(AIBackupUnitTask task, bool aborted)
        {
            if (currentTask != null)
            {
                currentTask.Finished -= OnCurrentTaskFinished;
                TaskFinished?.Invoke(currentTask);
                currentTask = null;
            }
        }

        public AIBackupUnitTask DriveToPosition(Vector3 position, bool sirenOn, float speed, float acceptedDistance, VehicleDrivingFlags flags, bool considerTaskPriority = true) => GiveTask<AIBackupUnitDriveToPositionTask>(considerTaskPriority, Unit, position, sirenOn, speed, acceptedDistance, flags);
        public AIBackupUnitTask DriveToPositionAndPark(RotatedVector3 parkingLocation, bool considerTaskPriority = true) => GiveTask<AIBackupUnitDriveToParkingLocationAndParkTask>(considerTaskPriority, Unit, parkingLocation);
        public AIBackupUnitTask ChillAround(bool considerTaskPriority = true) => GiveTask<AIBackupUnitChillAroundTask>(considerTaskPriority, Unit);
        public AIBackupUnitTask ExtinguishFireInArea(Vector3 position, float range, bool considerTaskPriority = true) => GiveTask<AIBackupUnitExtinguishFireInAreaTask>(considerTaskPriority, Unit, position, range);

        // if considerTaskPriority is true and tasks priority is greater than current task, current task is aborted and the queue is cleared
        protected AIBackupUnitTask GiveTask<TTask>(bool considerTaskPriority = true, params object[] args) where TTask : AIBackupUnitTask
        {
            AIBackupUnitTask t = (AIBackupUnitTask)Activator.CreateInstance(typeof(TTask), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, args, null);
            Game.LogTrivial($"[{this.GetType().Name}.GiveTask] ({typeof(TTask).Name}, ConsiderTaskPriority:{considerTaskPriority})");
            if (!IsDoingAnyTask)
            {
                Game.LogTrivial($"[{this.GetType().Name}.GiveTask]      No task running, setting as current task...");
                SetCurrentTask(t);
            }
            else
            {
                if (considerTaskPriority)
                {
                    if (t.Priority > CurrentTaskPriority)
                    {
                        Game.LogTrivial($"[{this.GetType().Name}.GiveTask]      Priority greater than current task, aborting current task, clearing queue and setting as current task...");
                        tasksQueue.Clear();
                        currentTask.Abort();
                        SetCurrentTask(t);
                    }
                    else
                    {
                        Game.LogTrivial($"[{this.GetType().Name}.GiveTask]      Priority less than current task, enqueuing task...");
                        tasksQueue.Enqueue(t);
                    }
                }
                else
                {
                    Game.LogTrivial($"[{this.GetType().Name}.GiveTask]      Enqueuing task...");
                    tasksQueue.Enqueue(t);
                }
            }

            return t;
        }
    }
}
