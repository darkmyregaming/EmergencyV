namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetEntitySpeed : Service
    {
        private readonly BlackboardGetter<Entity> entity;
        private readonly BlackboardSetter<float> speedSetter;

        /// <param name="entity">Where to get the <see cref="Entity"/> from the blackboard memory.</param>
        /// <param name="speedSetter">Where the speed will be saved in the blackboard memory.</param>
        public GetEntitySpeed(BlackboardGetter<Entity> entity, BlackboardSetter<float> speedSetter, int interval, BehaviorTask child) : base(interval, null, child)
        {
            this.entity = entity;
            this.speedSetter = speedSetter;

            ServiceMethod = DoService;
        }

        /// <param name="entity">Where to get the <see cref="Entity"/> from the blackboard memory.</param>
        /// <param name="speedSetter">Where the speed will be saved in the blackboard memory.</param>
        public GetEntitySpeed(BlackboardGetter<Entity> entity, BlackboardSetter<float> speedSetter, BehaviorTask child) : base(null, child)
        {
            this.entity = entity;
            this.speedSetter = speedSetter;

            ServiceMethod = DoService;
        }

        private void DoService(ref BehaviorTreeContext context)
        {
            Entity e = entity.Get(context, this);
            speedSetter.Set(context, this, e.Speed);
        }
    }
}
