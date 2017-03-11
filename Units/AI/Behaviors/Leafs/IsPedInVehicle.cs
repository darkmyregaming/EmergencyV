namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class IsPedInAnyVehicle : Condition
    {
        private readonly string pedKey;

        /// <param name="pedKey">The key where the ped is saved in the blackboard's tree memory.</param>
        public IsPedInAnyVehicle(string pedKey)
        {
            this.pedKey = pedKey;
        }

        protected override bool CheckCondition(ref BehaviorTreeContext context)
        {
            Ped ped = context.Agent.Blackboard.Get<Ped>(pedKey, context.Tree.Id);

            if (!ped)
            {
                return false;
            }

            return ped.IsInAnyVehicle(true);
        }
    }

    internal class IsPedInVehicle : Condition
    {
        private readonly string pedKey;
        private readonly string vehicleKey;

        /// <param name="pedKey">The key where the ped is saved in the blackboard's tree memory.</param>
        /// <param name="vehicleKey">The key where the vehicle to enter is saved in the blackboard's tree memory.</param>
        protected IsPedInVehicle(string pedKey, string vehicleKey)
        {
            this.pedKey = pedKey;
            this.vehicleKey = vehicleKey;
        }

        protected override bool CheckCondition(ref BehaviorTreeContext context)
        {
            Ped ped = context.Agent.Blackboard.Get<Ped>(pedKey, context.Tree.Id);

            if (!ped)
            {
                return false;
            }

            Vehicle vehicle = context.Agent.Blackboard.Get<Vehicle>(vehicleKey, context.Tree.Id);

            if (!vehicle)
            {
                return false;
            }

            return ped.IsInVehicle(vehicle, true);
        }
    }

    internal class IsInVehicle : Condition
    {
        private readonly string vehicleKey;

        /// <param name="vehicleKey">The key where the vehicle to enter is saved in the blackboard's tree memory.</param>
        public IsInVehicle(string vehicleKey)
        {
            this.vehicleKey = vehicleKey;
        }

        protected override bool CheckCondition(ref BehaviorTreeContext context)
        {
            if (!(context.Agent.Target is Ped))
            {
                throw new System.InvalidOperationException($"The behavior condition {nameof(IsInVehicle)} can't be used with {context.Agent.Target.GetType().Name}, it can only be used with {nameof(Ped)}s");
            }

            Ped ped = ((Ped)context.Agent.Target);

            if (!ped)
            {
                return false;
            }

            Vehicle vehicle = context.Agent.Blackboard.Get<Vehicle>(vehicleKey, context.Tree.Id);

            if (!vehicle)
            {
                return false;
            }

            return ped.IsInVehicle(vehicle, true);
        }
    }
}
