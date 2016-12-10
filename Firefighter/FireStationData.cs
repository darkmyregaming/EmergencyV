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
    }


    internal static class FireStationDataPlaceHolder
    {
        public static FireStationData Get()
        {
            return new FireStationData()
            {
                Name = "[PLACEHOLDER] Rockford Hills Fire Station",
                Entrance = new XYZ() { X = -634.7078f, Y = -121.6649f, Z = 39.01375f },
                ActivationRange = 100.0f,
                EngineSpawn = new XYZW() { X = -646.5129f, Y = -105.5248f, Z = 37.959f, W = 123.2762f },
                BattalionSpawn = new XYZW() { X = -624.1754f, Y = -73.23465f, Z = 40.68061f, W = 354.4229f },
                RescueSpawn = new XYZW() { X = -638.9544f, Y = -112.9421f, Z = 37.98552f, W = 85.63601f },
            };
        }
    }
}
