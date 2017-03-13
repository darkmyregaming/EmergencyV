namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class IsPedInAnyVehicle : Condition
    {
        private readonly BlackboardGetter<Ped> ped;

        /// <param name="ped">Where to get the <see cref="Ped"/> from the blackboard memory.</param>
        public IsPedInAnyVehicle(BlackboardGetter<Ped> ped)
        {
            this.ped = ped;
        }

        protected override bool CheckCondition(ref BehaviorTreeContext context)
        {
            Ped p = ped.Get(context, this);

            if (!p)
            {
                return false;
            }

            return p.IsInAnyVehicle(true);
        }
    }

    internal class IsPedInVehicle : Condition
    {
        private readonly BlackboardGetter<Ped> ped;
        private readonly BlackboardGetter<Vehicle> vehicle;

        /// <param name="ped">Where to get the <see cref="Ped"/> from the blackboard memory.</param>
        /// <param name="vehicle">Where to get the <see cref="Vehicle"/> from the blackboard memory.</param>
        protected IsPedInVehicle(BlackboardGetter<Ped> ped, BlackboardGetter<Vehicle> vehicle)
        {
            this.ped = ped;
            this.vehicle = vehicle;
        }

        protected override bool CheckCondition(ref BehaviorTreeContext context)
        {
            Ped p = ped.Get(context, this);

            if (!p)
            {
                return false;
            }

            Vehicle v = vehicle.Get(context, this);

            if (!v)
            {
                return false;
            }

            return p.IsInVehicle(v, true);
        }
    }

    internal class IsInVehicle : Condition
    {
        private readonly BlackboardGetter<Vehicle> vehicle;

        /// <param name="vehicle">Where to get the <see cref="Vehicle"/> from the blackboard memory.</param>
        public IsInVehicle(BlackboardGetter<Vehicle> vehicle)
        {
            this.vehicle = vehicle;
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

            Vehicle v = vehicle.Get(context, this);

            if (!v)
            {
                return false;
            }

            return ped.IsInVehicle(v, true);
        }
    }
}
