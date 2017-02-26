namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // System
    using System;

    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class EnterVehicle : RPH.Utilities.AI.Leafs.Action
    {
        string vehicleKey;
        string seatIndexKey;
        float speed;
        EnterVehicleFlags flags;

        /// <param name="key">The key where the vehicle to enter is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the seat index is saved in the blackboard's tree memory.</param>
        public EnterVehicle(string vehicleKey, string seatIndexKey, float speed, EnterVehicleFlags flags)
        {
            this.vehicleKey = vehicleKey;
            this.seatIndexKey = seatIndexKey;
            this.speed = speed;
            this.flags = flags;
        }

        protected override void OnOpen(ref BehaviorTreeContext context)
        {
            if (!(context.Agent.Target is Ped))
            {
                throw new InvalidOperationException($"The behavior action {nameof(EnterVehicle)} can't be used with {context.Agent.Target.GetType().Name}, it can only be used with {nameof(Ped)}s");
            }

            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return;
            }

            Vehicle veh = context.Agent.Blackboard.Get<Vehicle>(vehicleKey, context.Tree.Id);

            if (!veh || veh.IsDead)
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

            Vehicle veh = context.Agent.Blackboard.Get<Vehicle>(vehicleKey, context.Tree.Id);

            if (!veh || veh.IsDead)
            {
                ped.Tasks.Clear();
                return BehaviorStatus.Failure;
            }

            GiveTasks(ref context);

            Task goToTask = context.Agent.Blackboard.Get<Task>("goToTask", context.Tree.Id, this.Id, null);
            Task enterTask = context.Agent.Blackboard.Get<Task>("enterTask", context.Tree.Id, this.Id, null);

            if ((goToTask != null && goToTask.IsActive) || (enterTask != null && enterTask.IsActive))
            {
                return BehaviorStatus.Running;
            }
            else
            {
                if (ped.IsInVehicle(veh, true))
                {
                    return BehaviorStatus.Success;
                }
                else
                {
                    return BehaviorStatus.Failure;
                }
            }
        }

        protected override void OnClose(ref BehaviorTreeContext context)
        {
            context.Agent.Blackboard.Set<Task>("goToPosTask", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Task>("enterTask", null, context.Tree.Id, this.Id);
        }

        private void GiveTasks(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);
            Vehicle veh = context.Agent.Blackboard.Get<Vehicle>(vehicleKey, context.Tree.Id);

            if (Vector3.DistanceSquared(ped.Position, veh) > 6.5f * 6.5f)
            {
                Task goToTask = context.Agent.Blackboard.Get<Task>("goToTask", context.Tree.Id, this.Id, null);
                context.Agent.Blackboard.Set<Task>("enterTask", null, context.Tree.Id, this.Id);
                if ((goToTask == null || !goToTask.IsActive))
                {
                    goToTask = ped.Tasks.FollowNavigationMeshToPosition(veh.Position, ped.Position.GetHeadingTowards(veh), 2.0f, 5.0f);
                    context.Agent.Blackboard.Set<Task>("goToTask", goToTask, context.Tree.Id, this.Id);
                }
            }
            else if (!ped.IsInVehicle(veh, true))
            {
                Task enterTask = context.Agent.Blackboard.Get<Task>("enterTask", context.Tree.Id, this.Id, null);
                context.Agent.Blackboard.Set<Task>("goToTask", null, context.Tree.Id, this.Id);
                if (enterTask == null || !enterTask.IsActive)
                {
                    int seatIndex = context.Agent.Blackboard.Get<int>(seatIndexKey, context.Tree.Id, null, -2);

                    enterTask = ped.Tasks.EnterVehicle(veh, -1, seatIndex, speed, flags);
                    context.Agent.Blackboard.Set<Task>("enterTask", enterTask, context.Tree.Id, this.Id);
                }
            }
        }
    }
}
