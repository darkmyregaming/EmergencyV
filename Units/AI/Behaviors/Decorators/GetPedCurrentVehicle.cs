namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetPedCurrentVehicle : Service
    {
        private readonly BlackboardGetter<Ped> ped;
        private readonly BlackboardSetter<Vehicle> vehicleSetter;

        /// <param name="ped">Where to get the <see cref="Ped"/> from the blackboard memory.</param>
        /// <param name="vehicleSetter">Where the <see cref="Vehicle"/> will be saved in the blackboard memory.</param>
        public GetPedCurrentVehicle(BlackboardGetter<Ped> ped, BlackboardSetter<Vehicle> vehicleSetter, int interval, BehaviorTask child) : base(interval, null, child)
        {
            this.ped = ped;
            this.vehicleSetter = vehicleSetter;

            ServiceMethod = DoService;
        }

        /// <param name="ped">Where to get the <see cref="Ped"/> from the blackboard memory.</param>
        /// <param name="vehicleSetter">Where the <see cref="Vehicle"/> will be saved in the blackboard memory.</param>
        public GetPedCurrentVehicle(BlackboardGetter<Ped> ped, BlackboardSetter<Vehicle> vehicleSetter, BehaviorTask child) : base(null, child)
        {
            this.ped = ped;
            this.vehicleSetter = vehicleSetter;

            ServiceMethod = DoService;
        }

        private void DoService(ref BehaviorTreeContext context)
        {
            Ped p = ped.Get(context, this);

            if (!p)
            {
                return;
            }

            Vehicle v = p.CurrentVehicle;
            vehicleSetter.Set(context, this, v);
        }
    }
}
