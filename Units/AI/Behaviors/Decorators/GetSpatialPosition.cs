namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetSpatialPosition : Service
    {
        private readonly BlackboardGetter<ISpatial> spatial;
        private readonly BlackboardSetter<Vector3> positionSetter;

        /// <param name="spatial">Where to get the <see cref="ISpatial"/> from the blackboard memory.</param>
        /// <param name="positionSetter">Where the position will be saved in the blackboard memory.</param>
        public GetSpatialPosition(BlackboardGetter<ISpatial> spatial, BlackboardSetter<Vector3> positionSetter, int interval, BehaviorTask child) : base(interval, null, child)
        {
            this.spatial = spatial;
            this.positionSetter = positionSetter;

            ServiceMethod = DoService;
        }

        /// <param name="spatial">Where to get the <see cref="ISpatial"/> from the blackboard memory.</param>
        /// <param name="positionSetter">Where the position will be saved in the blackboard memory.</param>
        public GetSpatialPosition(BlackboardGetter<ISpatial> spatial, BlackboardSetter<Vector3> positionSetter, BehaviorTask child) : base(null, child)
        {
            this.spatial = spatial;
            this.positionSetter = positionSetter;

            ServiceMethod = DoService;
        }

        private void DoService(ref BehaviorTreeContext context)
        {
            ISpatial p = spatial.Get(context, this);
            if (p != null)
            {
                positionSetter.Set(context, this, p.Position);
            }
        }
    }
}
