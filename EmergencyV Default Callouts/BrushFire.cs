namespace EmergencyVDefaultCallouts
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // EmergencyV
    using EmergencyV;
    using EmergencyV.API;

    // RPH
    using Rage;

    [FireCalloutInfo("Default.BrushFire.Engine", FirefighterRole.Engine, CalloutProbability.Medium)]
    [FireCalloutInfo("Default.BrushFire.Battalion", FirefighterRole.Battalion, CalloutProbability.Medium)]
    [FireCalloutInfo("Default.BrushFire.Rescue", FirefighterRole.Rescue, CalloutProbability.Medium)]
    internal class BrushFire : Callout
    {
        BrushFireLocationData locationData;

        Blip blip;
        FireEx[] fires;
        List<Vehicle> firetrucks;
        List<Firefighter> firefighters;
        List<Rage.Object> cones;

        List<AITask> extinguishFireTasks = new List<AITask>();
        List<AITask> enterVehicleTasks = new List<AITask>();

        bool createdFiretrucks = false;
        bool createdFires = false;
        bool givenExtinguishFireTasks = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            locationData = BrushFireLocationData.GetRandom();
            DisplayName = "Brush Fire";
            DisplayExtraInfo = $"Location: {World.GetStreetName(locationData.FireStartPositions[0])}\r\nUnits on scene";

            ShowCalloutAreaBlipBeforeAccepting(locationData.GoToPosition, 65.0f);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            blip = new Blip(locationData.GoToPosition);
            blip.IsRouteEnabled = true;


            return base.OnCalloutAccepted();
        }

        public override void Update()
        {
            if (!createdFiretrucks)
            {
                if (Vector3.DistanceSquared(Game.LocalPlayer.Character.Position, locationData.GoToPosition) < 400.0f * 400.0f)
                {
                    CreateFiretrucks();
                    createdFiretrucks = true;
                }
            }

            if (!createdFires)
            {
                if (Vector3.DistanceSquared(Game.LocalPlayer.Character.Position, locationData.GoToPosition) < 50f * 50f)
                {
                    CreateFires();
                    createdFires = true;
                }
            }

            if (!givenExtinguishFireTasks)
            {
                if (Vector3.DistanceSquared(Game.LocalPlayer.Character.Position, locationData.GoToPosition) < 40f * 40f)
                {
                    GiveTasks();

                    givenExtinguishFireTasks = true;
                }
            }

            base.Update();
        }

        private void GiveTasks()
        {
            foreach (Firefighter f in firefighters)
            {
                f.AI.ExtinguishFireInArea(locationData.FireStartPositions[MathHelper.GetRandomInteger(locationData.FireStartPositions.Length)], 125.0f, true)
                    .Finished += (t, aborted) =>
                {
                    Game.LogTrivial("Finished ExtinguishFireInArea task");
                    Vehicle v = t.Ped.Metadata.Firetruck;
                    if (v)
                    {
                        t.Controller.EnterVehicle(v, t.Controller.Owner.PreferedVehicleSeatIndex).Finished += (t2, aborted2) => { Game.LogTrivial("Finished EnterVehicle task"); };
                    }
                };
                GameFiber.Sleep(5);
            }
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
        }

        protected override void OnFinished()
        {
            if (blip) blip.Delete();

            if (fires != null)
                foreach (FireEx fire in fires)
                    if(fire.Fire) fire.Fire.Delete();

            if (firetrucks != null)
                foreach (Vehicle v in firetrucks)
                    if (v) v.Dismiss();

            if (firefighters != null)
                foreach (Firefighter f in firefighters)
                    if (f.Ped) f.Ped.Dismiss();

            if (cones != null)
                foreach (Rage.Object o in cones)
                    if (o) o.Dismiss();

            base.OnFinished();
        }

        private void CreateFires()
        {
            int eachPosFireCount = locationData.FireCount / locationData.FireStartPositions.Length;

            Vector3[] firePos = new Vector3[eachPosFireCount * locationData.FireStartPositions.Length];
            int i = 0;
            foreach (Vector3 startPos in locationData.FireStartPositions)
            {
                for (int j = 0; j < eachPosFireCount; j++, i++)
                {
                    firePos[i] = startPos.Around2D(0.5f, locationData.FireStartPositionRadius);
                }
            }

            fires = Functions.CreateFires(firePos, 20, false, true);

            foreach (FireEx f in fires)
            {
                f.Fire.DesiredBurnDuration = 30.0f;
                f.Fire.SpreadRadius = 1.0f;
            }
        }

        private void CreateFiretrucks()
        {
            cones = new List<Rage.Object>();
            firetrucks = new List<Vehicle>();
            firefighters = new List<Firefighter>();
            if (locationData.FiretrucksSpawns != null)
            {
                foreach (BrushFireLocationData.FiretruckSpawnData sp in locationData.FiretrucksSpawns)
                {
                    Vector3 p = sp.Position;
                    float? z = World.GetGroundZ(p, false, true);
                    if (z.HasValue) p.Z = z.Value;
                    Vehicle v = new Vehicle("firetruk", p, sp.Heading);
                    v.IsSirenOn = true;
                    if (sp.SideCones || sp.FrontCones || sp.RearCones)
                    {
                        switch (sp.ConesPosition)
                        {
                            case BrushFireLocationData.FiretruckSpawnData.ConesPositionType.Left: cones.AddRange(Functions.CreateConesAtVehicleLeftSide(v, 3.25f, true, sp.SideCones, sp.FrontCones, sp.RearCones)); break;
                            case BrushFireLocationData.FiretruckSpawnData.ConesPositionType.Right: cones.AddRange(Functions.CreateConesAtVehicleRightSide(v, 3.25f, true, sp.SideCones, sp.FrontCones, sp.RearCones)); break;
                        }
                        
                    }
                    firetrucks.Add(v);

                    int seats = Math.Min(4, v.PassengerCapacity + 1);
                    for (int i = 0; i < seats; i++)
                    {
                        Firefighter f = new Firefighter(Vector3.Zero, 0.0f);
                        f.PreferedVehicleSeatIndex = i - 1;
                        f.Equipment.HasFireExtinguisher = true;
                        f.Equipment.HasFireGear = true;
                        f.Equipment.IsFlashlightOn = true;
                        f.Ped.WarpIntoVehicle(v, i - 1);
                        f.Ped.Metadata.Firetruck = v;
                        firefighters.Add(f);
                    }
                }
            }
        }

        private struct BrushFireLocationData
        {
            public Vector3 GoToPosition;

            public int FireCount;
            public Vector3[] FireStartPositions;
            public float FireStartPositionRadius;

            public FiretruckSpawnData[] FiretrucksSpawns;

            public static BrushFireLocationData GetRandom()
            {
                BrushFireLocationData[] d =
                {
                    new BrushFireLocationData()
                    {
                        GoToPosition = new Vector3(1656.273f, -2499.838f, 79.39397f),

                        FireCount = 40,
                        FireStartPositions = new[]
                        {
                            new Vector3(1672.691f, -2457.34f, 85.60426f),
                            new Vector3(1677.922f, -2477.648f, 83.2f),
                        },
                        FireStartPositionRadius = 15f,

                        FiretrucksSpawns = new[]
                        {
                            new FiretruckSpawnData { Position = new Vector3(1659.66f, -2519.537f, 76.70167f), Heading = 336.858f, ConesPosition = FiretruckSpawnData.ConesPositionType.Left, SideCones = true, RearCones = true },
                            new FiretruckSpawnData { Position = new Vector3(1649.691f, -2457.34f, 85.60426f), Heading = 202.6975f, ConesPosition = FiretruckSpawnData.ConesPositionType.Right, SideCones = true, RearCones = true },
                        },
                    },

                    new BrushFireLocationData()
                    {
                        GoToPosition = new Vector3(2470.365f, 5648.774f, 45.02168f),

                        FireCount = 40,
                        FireStartPositions = new[]
                        {
                            new Vector3(2480.403f, 5683.278f, 46.25755f),
                            new Vector3(2489.392f, 5666.227f, 46.21795f),
                            new Vector3(2499.385f, 5640.371f, 46.75524f),
                        },
                        FireStartPositionRadius = 15f,

                        FiretrucksSpawns = new[]
                        {
                            new FiretruckSpawnData { Position = new Vector3(2498.291f, 5600.729f, 44.90869f), Heading = 22.04346f, ConesPosition = FiretruckSpawnData.ConesPositionType.Left, SideCones = true, RearCones = true },
                            new FiretruckSpawnData { Position = new Vector3(2461.981f, 5685.726f, 45.15458f), Heading = 23.93354f, ConesPosition = FiretruckSpawnData.ConesPositionType.Left, SideCones = true, FrontCones = true },
                        },
                    },

                    new BrushFireLocationData()
                    {
                        GoToPosition = new Vector3(-673.8821f, -34.87631f, 38.50744f),

                        FireCount = 30,
                        FireStartPositions = new[]
                        {
                            new Vector3(-646.0173f, -32.27942f, 40.50384f),
                            new Vector3(-655.9035f, -31.7934f, 39.73713f),
                            new Vector3(-628.6351f, -31.15221f, 41.77246f),
                        },
                        FireStartPositionRadius = 12.5f,

                        FiretrucksSpawns = new[]
                        {
                            new FiretruckSpawnData { Position = new Vector3(-691.0282f, -44.12651f, 37.89026f), Heading = 26.47132f, ConesPosition = FiretruckSpawnData.ConesPositionType.Left, SideCones = true, RearCones = true },
                            new FiretruckSpawnData { Position = new Vector3(-645.9019f, -46.45034f, 40.62471f), Heading = 92.37659f, ConesPosition = FiretruckSpawnData.ConesPositionType.Left, SideCones = true, RearCones = true },
                        },
                    },
                };

                return d[MathHelper.GetRandomInteger(d.Length)];
            }


            public struct FiretruckSpawnData
            {
                public Vector3 Position;
                public float Heading;
                public ConesPositionType ConesPosition;
                public bool SideCones, FrontCones, RearCones;

                public enum ConesPositionType { Left, Right }
            }
        }
    }
}
