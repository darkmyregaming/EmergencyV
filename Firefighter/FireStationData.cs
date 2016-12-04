namespace EmergencyV
{
    // System
    using System;

    // RPH
    using Rage;
    
    internal struct FireStationData
    {
        public string Name;
        public Vector3 EntrancePosition;
        public RotatedVector3 EngineLocation;
        public RotatedVector3 BattalionLocation;
        public RotatedVector3 RescueLocation;
    }


    internal static class FireStationDataPlaceHolder
    {
        public static FireStationData Get()
        {
            return new FireStationData()
            {
                Name = "[PLACEHOLDER] Rockford Hills Fire Station",
                EntrancePosition = new Vector3(-634.7078f, -121.6649f, 39.01375f),
                EngineLocation = new RotatedVector3(new Vector3(-646.5129f, -105.5248f, 37.959f), new Rotator(0.0f, 0.0f, 123.2762f)),
                BattalionLocation = new RotatedVector3(new Vector3(-624.1754f, -73.23465f, 40.68061f), new Rotator(0.0f, 0.0f, 354.4229f)),
                RescueLocation = new RotatedVector3(new Vector3(-638.9544f, -112.9421f, 37.98552f), new Rotator(0.0f, 0.0f, 85.63601f)),
            };
        }
    }
}
