namespace EmergencyV.Units
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

    internal class PlayerPartnerTest // placeholder just for testing
    {
        static int seatIndex;

        public Firefighter Firefighter { get; }

        public bool ExecuteBehaviorTree { get; } = true;

        protected BehaviorAgent Agent { get; }


        private PlayerPartnerTest(Vector3 position, float heading)
        {
            Firefighter = new Firefighter(position, heading);
            Firefighter.Equipment.HasFireExtinguisher = false;

            Agent = new BehaviorAgent(Firefighter.Ped);

            int s = seatIndex++;
            Agent.Blackboard.Set<int>("partnerSeatIndex", s, IdleTree.Id);


            Agent.Blackboard.Set<Firefighter>("partnerFirefighterInstance", Firefighter);
        }

        static System.Collections.Generic.List<PlayerPartnerTest> partners = new System.Collections.Generic.List<PlayerPartnerTest>();
        static GameFiber fiber;
        [Rage.Attributes.ConsoleCommand]
        private static void CreatePartner()
        {
            PlayerPartnerTest p = new PlayerPartnerTest(Game.LocalPlayer.Character.Position, 0f);
            partners.Add(p);


            if (fiber == null)
            {
                fiber = GameFiber.StartNew(() =>
                {
                    while (true)
                    {
                        GameFiber.Sleep(300);

                        Stopwatch sw = Stopwatch.StartNew();

                        for (int i = 0; i < partners.Count; i++)
                        {
                            PlayerPartnerTest partner = partners[i];
                            if (partner.ExecuteBehaviorTree)
                            {
                                IdleTree.ExecuteOn(partner.Agent);
                            }
                        }

                        sw.Stop();
                        Game.LogTrivial($"{sw.ElapsedMilliseconds} | {sw.ElapsedMilliseconds} | {sw.ElapsedTicks}");
                    }
                }, "partners fiber");
            }
        }

        [Rage.Attributes.ConsoleCommand]
        private static void DeletePartners()
        {
            foreach (PlayerPartnerTest p in partners)
            {
                p.Firefighter.Ped.Delete();
            }
            partners.Clear();
            seatIndex = 0;
        }

        private static BehaviorTree idleTree;
        private static BehaviorTree IdleTree
        {
            get
            {
                if (idleTree == null)
                {
                    idleTree = new BehaviorTree(
                                new GetPlayerPed("playerPed", 5000,
                                    new Selector(
                                        new Sequence(
                                            new IsPedInAnyVehicle("playerPed"),
                                            new GetPedCurrentVehicle("playerPed", "playerPedCurrentVehicle", 100,
                                                new Succeeder(
                                                    new Sequence(
                                                        new EntityExists("playerPedCurrentVehicle"),
                                                        new Inverter(new IsInVehicle("playerPedCurrentVehicle")),
                                                        new EnterVehicle("playerPedCurrentVehicle", "partnerSeatIndex", 5.0f, EnterVehicleFlags.None)
                                                        )
                                                    )
                                                )
                                            ),

                                        new Failer(
                                            new Service(1000, (ref BehaviorTreeContext context) => context.Agent.Blackboard.Set<bool>("playerHasFireGear", PlayerFireEquipmentController.Instance.HasFireGear, context.Tree.Id),
                                                new DelegatedAction((ref BehaviorTreeContext context) => // TODO: make ped go to closest firetruck to take fire gear
                                                    {
                                                        Ped p = ((Ped)context.Agent.Target);
                                                        Firefighter f = context.Agent.Blackboard.Get<Firefighter>("partnerFirefighterInstance");
                                                        bool shouldHaveFireGear = context.Agent.Blackboard.Get<bool>("playerHasFireGear", context.Tree.Id);
                                                        if (f.Equipment.HasFireGear != shouldHaveFireGear)
                                                        {
                                                            f.Equipment.HasFireGear = shouldHaveFireGear;
                                                        }
                                                    })
                                                )
                                            ),

                                        new Failer( 
                                            new Service(1000, (ref BehaviorTreeContext context) => context.Agent.Blackboard.Set<bool>("playerIsFlashlightOn", PlayerFireEquipmentController.Instance.IsFlashlightOn, context.Tree.Id),
                                                new DelegatedAction((ref BehaviorTreeContext context) =>
                                                    {
                                                        Ped p = ((Ped)context.Agent.Target);
                                                        Firefighter f = context.Agent.Blackboard.Get<Firefighter>("partnerFirefighterInstance");
                                                        bool shouldFlashlightBeOn = context.Agent.Blackboard.Get<bool>("playerIsFlashlightOn", context.Tree.Id);
                                                        if (f.Equipment.IsFlashlightOn != shouldFlashlightBeOn)
                                                        {
                                                            f.Equipment.IsFlashlightOn = shouldFlashlightBeOn;
                                                        }
                                                    })
                                                )
                                            ),

                                        new Sequence(
                                            new IsPlayerExtinguishingFire(),
                                            new GetSpatialPosition("playerPed", "extinguishFirePosition", 1000,
                                                new ExtinguishFire("extinguishFirePosition", 25.0f)
                                                )
                                            ),
                                        
                                        new GetEntitySpeed("playerPed", "playerPedSpeed", 1000,
                                            new FollowEntity("playerPed", new Vector3(0f, -2.5f, 0f), "playerPedSpeed", 5f, true)
                                            )
                                        )
                                    )
                                );
                }

                return idleTree;
            }
        }
        
    }
}
