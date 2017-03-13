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
        private readonly BlackboardGetter<Entity> entity;
        private readonly Vector3 offset;
        private readonly float speed = 2.0f;
        private readonly BlackboardGetter<float> speedGetter;
        private readonly float stoppingRange;
        private readonly bool persistFollowing;

        /// <param name="entity">Where to get the <see cref="Entity"/> to follow from the blackboard memory.</param>
        public FollowEntity(BlackboardGetter<Entity> entityToFollow, Vector3 offset, float speed, float stoppingRange, bool persistFollowing)
        {
            this.entity = entityToFollow;
            this.offset = offset;
            this.speed = speed;
            this.stoppingRange = stoppingRange;
            this.persistFollowing = persistFollowing;
        }

        /// <param name="entity">Where to get the <see cref="Entity"/> to follow from the blackboard memory.</param>
        /// <param name="speed">Where to get the speed from the blackboard memory.</param>
        public FollowEntity(BlackboardGetter<Entity> entityToFollow, Vector3 offset, BlackboardGetter<float> speed, float stoppingRange, bool persistFollowing)
        {
            this.entity = entityToFollow;
            this.offset = offset;
            this.speedGetter = speed;
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

            Entity entToFollow = entity.Get(context, this);
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

            Entity entToFollow = entity.Get(context, this);
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
            Entity entToFollow = entity.Get(context, this);

            Task followTask = context.Agent.Blackboard.Get<Task>("followTask", context.Tree.Id, this.Id, null);

            float actualSpeed = context.Agent.Blackboard.Get<float>("followTaskActualSpeed", context.Tree.Id, this.Id, -1.0f);

            bool speedChanged = false;
            if (speedGetter != null)
            {
                float speedFromKey = Math.Max(speedGetter.Get(context, this), 1.0f);
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
