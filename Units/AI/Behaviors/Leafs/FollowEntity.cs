namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // System
    using System;

    // RPH
    using Rage;
    using Rage.Native;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class FollowEntity : RPH.Utilities.AI.Leafs.Action
    {
        string entityToFollowKey;
        Vector3 offset;
        float speed = 2.0f;
        string speedKey;
        float stoppingRange;
        bool persistFollowing;

        /// <param name="entityToFollowKey">The key where the entity to follow is saved in the blackboard's tree memory.</param>
        public FollowEntity(string entityToFollowKey, Vector3 offset, float speed, float stoppingRange, bool persistFollowing)
        {
            this.entityToFollowKey = entityToFollowKey;
            this.offset = offset;
            this.speed = speed;
            this.stoppingRange = stoppingRange;
            this.persistFollowing = persistFollowing;
        }

        /// <param name="entityToFollowKey">The key where the entity to follow is saved in the blackboard's tree memory.</param>
        /// <param name="speedKey">The key where the speed to follow is saved in the blackboard's tree memory.</param>
        public FollowEntity(string entityToFollowKey, Vector3 offset, string speedKey, float stoppingRange, bool persistFollowing)
        {
            this.entityToFollowKey = entityToFollowKey;
            this.offset = offset;
            this.speedKey = speedKey;
            this.stoppingRange = stoppingRange;
            this.persistFollowing = persistFollowing;
        }

        protected override void OnOpen(ref BehaviorTreeContext context)
        {
            if (!(context.Agent.Target is Ped))
            {
                throw new InvalidOperationException($"The behavior action {nameof(FollowEntity)} can't be used with {context.Agent.Target.GetType().Name}, it can only be used with {nameof(Ped)}s");
            }

            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return;
            }

            Entity entToFollow = context.Agent.Blackboard.Get<Entity>(entityToFollowKey, context.Tree.Id);
            if (!entToFollow)
            {
                return;
            }

            GiveTasks(ref context);
        }

        protected override BehaviorStatus OnBehave(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return BehaviorStatus.Failure;
            }

            Entity entToFollow = context.Agent.Blackboard.Get<Entity>(entityToFollowKey, context.Tree.Id);
            if (!entToFollow)
            {
                return BehaviorStatus.Failure;
            }

            GiveTasks(ref context);

            Task followTask = context.Agent.Blackboard.Get<Task>("followTask", context.Tree.Id, this.Id, null);

            if (followTask != null && followTask.IsActive)
            {
                return BehaviorStatus.Running;
            }
            else
            {
                if (persistFollowing)
                {
                    return BehaviorStatus.Failure;
                }
                else
                {
                    return BehaviorStatus.Success;
                }
            }
        }

        protected override void OnClose(ref BehaviorTreeContext context)
        {
            context.Agent.Blackboard.Set<Task>("followTask", null, context.Tree.Id, this.Id);
        }

        private void GiveTasks(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);
            Entity entToFollow = context.Agent.Blackboard.Get<Entity>(entityToFollowKey, context.Tree.Id);

            Task followTask = context.Agent.Blackboard.Get<Task>("followTask", context.Tree.Id, this.Id, null);

            float actualSpeed = context.Agent.Blackboard.Get<float>("followTaskActualSpeed", context.Tree.Id, this.Id, -1.0f);

            bool speedChanged = false;
            if (speedKey != null)
            {
                float speedFromKey = Math.Max(context.Agent.Blackboard.Get<float>(speedKey, context.Tree.Id), 1.0f);
                if (actualSpeed != speedFromKey)
                {
                    actualSpeed = speedFromKey;
                    context.Agent.Blackboard.Set<float>("followTaskActualSpeed", actualSpeed, context.Tree.Id, this.Id);
                    speedChanged = true;
                }
            }
            else if (actualSpeed <= 0.0f)
            {
                actualSpeed = speed;
                context.Agent.Blackboard.Set<float>("followTaskActualSpeed", actualSpeed, context.Tree.Id, this.Id);
                speedChanged = true;
            }

            if (speedChanged || followTask == null || (!followTask.IsActive && persistFollowing))
            {
                NativeFunction.Natives.TaskFollowToOffsetOfEntity(ped, entToFollow, offset.X, offset.Y, offset.Z, actualSpeed, -1, stoppingRange, persistFollowing);
                followTask = Task.GetTask(ped, "TASK_FOLLOW_TO_OFFSET_OF_ENTITY");

                context.Agent.Blackboard.Set<Task>("followTask", followTask, context.Tree.Id, this.Id);
            }
        }
    }
}
