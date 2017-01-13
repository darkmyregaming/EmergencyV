namespace EmergencyV
{
    // System
    using System;
    using System.Runtime.Serialization;

    // RPH
    using Rage;

    using static Settings;

    [DataContract(Name = "FireStation", Namespace = "EmergencyV")]
    internal class FireStationData : BuildingData
    {
        [DataMember]
        public XYZW EngineSpawn;
        [DataMember]
        public XYZW BattalionSpawn;
        [DataMember]
        public XYZW RescueSpawn;
        [DataMember(IsRequired = false)]
        public XYZW[] ParkingSpots;

        public static FireStationData[] GetDefaults()
        {
            return new FireStationData[]
            {
                new FireStationData
                {
                    Name = "Rockford Hills Fire Station",
                    Entrance = new XYZ { X = -634.7078f, Y = -121.6649f, Z = 39.01375f },
                    ActivationRange = 100.0f,
                    EngineSpawn = new XYZW { X = -646.5129f, Y = -105.5248f, Z = 37.959f, W = 123.2762f },
                    BattalionSpawn = new XYZW { X = -624.1754f, Y = -73.23465f, Z = 40.68061f, W = 354.4229f },
                    RescueSpawn = new XYZW { X = -638.9544f, Y = -112.9421f, Z = 37.98552f, W = 85.63601f },
                    ParkingSpots = new[]
                    {
                        new XYZW { X = -633.5306f, Y = -73.38623f, Z = 40.43277f, W = 359.5356f },
                        new XYZW { X = -641.0023f, Y = -73.28398f, Z = 40.10931f, W = 351.6033f },
                    },
                },
                new FireStationData
                {
                    Name = "Davis Fire Station",
                    Entrance = new XYZ { X = 199.9229f, Y = -1634.029f, Z = 29.803f },
                    ActivationRange = 100.0f,
                    EngineSpawn = new XYZW { X = 215.3671f, Y = -1646.172f, Z = 29.80334f, W = 322.4904f },
                    BattalionSpawn = new XYZW { X = 212.203f, Y = -1637.312f, Z = 29.609f, W = 320.1754f },
                    RescueSpawn = new XYZW { X = 212.1281f, Y = -1643.69f, Z = 29.80333f, W = 322.4904f },
                    ParkingSpots = new[] 
                    {
                        new XYZW { X = 231.4567f, Y = -1656.199f, Z = 29.35906f, W = 317.7672f },
                    }
                },
                new FireStationData
                {
                    Name = "El Burro Heights Fire Station",
                    Entrance = new XYZ { X = 1185.74f, Y = -1462.765f, Z = 34.90047f },
                    ActivationRange = 100.0f,
                    EngineSpawn = new XYZW { X = 1200.807f, Y = -1462.141f, Z = 34f, W = 0f },
                    BattalionSpawn = new XYZW { X = 1196.682f, Y = -1458.905f, Z = 34.79833f, W = 0f },
                    RescueSpawn = new XYZW { X = 1196.748f, Y = -1493.069f, Z = 34.69257f, W = 180f },
                    ParkingSpots = new[]
                    {
                        new XYZW { X = 1200.882f, Y = -1493.041f, Z = 34.69253f, W = 180f },
                        new XYZW { X = 1205.027f, Y = -1493.041f, Z = 34.69253f, W = 180f },
                    }
                },
                new FireStationData
                {
                    Name = "Sandy Shores Fire Station",
                    Entrance = new XYZ { X = 1690.509f, Y = 3580.94f, Z = 35.62f },
                    ActivationRange = 100.0f,
                    EngineSpawn = new XYZW { X = 1696.289f, Y = 3586.979f, Z = 35.2f, W = 206.3112f },
                    BattalionSpawn = new XYZW { X = 1714.23f, Y = 3596.945f, Z = 35.32872f, W = 117.7147f },
                    RescueSpawn = new XYZW { X =  1703.577f, Y = 3600.438f, Z = 35.432f, W = 209.5f },
                }
            };
        }
    }
}
