namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class AITaskFollow : AITask
    {
        Task task;

        protected AITaskFollow(AIController controller, Entity entityToFollow, Vector3 offset, float stoppingRange, float speed, bool persistFollowing) : base(controller)
        {
            NativeFunction.Natives.TaskFollowToOffsetOfEntity(Ped, entityToFollow, offset.X, offset.Y, offset.Z, speed, -1, stoppingRange, persistFollowing);
            task = Task.GetTask(Ped, "TASK_FOLLOW_TO_OFFSET_OF_ENTITY");
        }

        internal override void Update()
        {
            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        protected override void OnFinished(bool aborted)
        {
            if (aborted)
                Ped.Tasks.Clear();
            task = null;
        }
    }
}
