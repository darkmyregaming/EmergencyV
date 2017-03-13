using System.Drawing;

namespace EmergencyV
{
    // System
    using System.Linq;
    using System.Diagnostics;

    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Composites;
    using RPH.Utilities.AI.Decorators;
    using RPH.Utilities.AI.Leafs;

    // EmergencyV
    using EmergencyV.Units.AI.Behaviors.Decorators;
    using EmergencyV.Units.AI.Behaviors.Leafs;

    internal class FirefighterPartner
    {
        public Firefighter Firefighter { get; }

        public Blip Blip { get; }

        public bool ExecuteBehaviorTree { get; } = true;
        public BehaviorAgent BehaviorAgent { get; }


        private FirefighterPartner(Vector3 position, float heading)
        {
            Firefighter = new Firefighter(position, heading);
            Firefighter.Equipment.SetEquipped<FireGearEquipment>(false);
            Firefighter.AI.IsEnabled = false;
            Firefighter.Deleted += OnPedDeleted;

            Blip = new Blip(Firefighter.Ped);
            Blip.Sprite = BlipSprite.Friend;
            Blip.Scale = 0.65f;
            Blip.Color = Color.FromArgb(180, 0, 0);
            Blip.Name = "Firefighter Partner";

            BehaviorAgent = new BehaviorAgent(Firefighter.Ped);
            BehaviorAgent.Blackboard.Set<int>("partnerSeatIndex", GetAllPartners()?.Length ?? 0);
            BehaviorAgent.Blackboard.Set<Firefighter>("partnerFirefighterInstance", Firefighter);
            
            RegisterFirefighterPartner(this);
        }

        private void OnPedDeleted(AdvancedPed sender)
        {
            if(Blip)
                Blip.Delete();
        }

        private void Update()
        {
            if (ExecuteBehaviorTree)
            {
                IdleTree.ExecuteOn(BehaviorAgent);
            }
        }

        public static FirefighterPartner CreatePartner(Vector3 position, float heading)
        {
            FirefighterPartner p = new FirefighterPartner(position, heading);
            return p;
        }

        public static FirefighterPartner[] GetAllPartners()
        {
            return UpdateInstancesFibersManager.Instance.GetAllInstancesOfType<FirefighterPartner>();
        }

        protected static void RegisterFirefighterPartner(FirefighterPartner partner)
        {
            if (!UpdateInstancesFibersManager.Instance.IsUpdateDataSetForType<FirefighterPartner>())
            {
                UpdateInstancesFibersManager.Instance.SetUpdateDataForType<FirefighterPartner>(
                    canDoUpdateCallback: (p) => p.Firefighter.Ped && !p.Firefighter.Ped.IsDead,
                    onInstanceUpdateCallback: (p) => p.Update(),
                    onInstanceUnregisteredCallback: null);
            }

            UpdateInstancesFibersManager.Instance.RegisterInstance(partner);
        }

        private static BehaviorTree idleTree;
        private static BehaviorTree IdleTree
        {
            get
            {
                if (idleTree == null)
                {
                    idleTree = new BehaviorTree(
                                /* MAIN SERVICES */
                                new GetPlayerPed(new BlackboardSetter<Ped>("playerPed", BlackboardMemoryScope.Tree), 5000,
                                new Service(1000, (ref BehaviorTreeContext context) =>
                                    {
                                        Firefighter f = context.Agent.Blackboard.Get<Firefighter>("partnerFirefighterInstance");
                                        bool playerHasFireGear = PlayerFireEquipmentController.Instance.IsEquipped<FireGearEquipment>();
                                        if (f.Equipment.IsEquipped<FireGearEquipment>() != playerHasFireGear)
                                        {
                                            f.Equipment.SetEquipped<FireGearEquipment>(playerHasFireGear);
                                        }
                                    },
                                //new Service(1000, (ref BehaviorTreeContext context) =>
                                //    {
                                //        Firefighter f = context.Agent.Blackboard.Get<Firefighter>("partnerFirefighterInstance");
                                //        bool isPlayerFlashlightOn = PlayerFireEquipmentController.Instance.IsFlashlightOn;
                                //        if (f.Equipment.IsFlashlightOn != isPlayerFlashlightOn)
                                //        {
                                //            f.Equipment.IsFlashlightOn = isPlayerFlashlightOn;
                                //        }
                                //    },

                                /* MAIN BEHAVIORS SELECTOR */
                                new Selector(

                                    /* ENTER PLAYER VEHICLE */
                                    new Sequence(
                                        new IsPedInAnyVehicle(new BlackboardGetter<Ped>("playerPed", BlackboardMemoryScope.Tree)),
                                        new GetPedCurrentVehicle(new BlackboardGetter<Ped>("playerPed", BlackboardMemoryScope.Tree), new BlackboardSetter<Vehicle>("playerPedCurrentVehicle", BlackboardMemoryScope.Tree), 100,
                                            new Succeeder(
                                                new Sequence(
                                                    new EntityExists(new BlackboardGetter<Entity>("playerPedCurrentVehicle", BlackboardMemoryScope.Tree)),
                                                    new Inverter(new IsInVehicle(new BlackboardGetter<Vehicle>("playerPedCurrentVehicle", BlackboardMemoryScope.Tree))),
                                                    new EnterVehicle(new BlackboardGetter<Vehicle>("playerPedCurrentVehicle", BlackboardMemoryScope.Tree), new BlackboardGetter<int>("partnerSeatIndex", BlackboardMemoryScope.Global), 5.0f, EnterVehicleFlags.None)
                                                    )
                                                )
                                            )
                                        ),

                                    /* EXTINGUISH FIRE */
                                    new Sequence(
                                        new IsPlayerExtinguishingFire(),
                                        new GetSpatialPosition(new BlackboardGetter<ISpatial>("playerPed", BlackboardMemoryScope.Tree), new BlackboardSetter<Vector3>("extinguishFirePosition", BlackboardMemoryScope.Tree), 1000,
                                            new ExtinguishFire(new BlackboardGetter<Vector3>("extinguishFirePosition", BlackboardMemoryScope.Tree), 25.0f)
                                            )
                                        ),

                                    /* FOLLOW PLAYER */
                                    new GetEntitySpeed(new BlackboardGetter<Entity>("playerPed", BlackboardMemoryScope.Tree), new BlackboardSetter<float>("playerPedSpeed", BlackboardMemoryScope.Tree), 1000,
                                        new FollowEntity(new BlackboardGetter<Entity>("playerPed", BlackboardMemoryScope.Tree), new Vector3(0f, -2.5f, 0f), new BlackboardGetter<float>("playerPedSpeed", BlackboardMemoryScope.Tree), 5f, true)
                                        )
                                    )
                                    )
                                    )
                                    //)
                                );
                }

                return idleTree;
            }
        }
        
    }
}
