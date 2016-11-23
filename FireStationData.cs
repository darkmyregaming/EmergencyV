namespace EmergencyV
{
    // System
    using System;

    // RPH
    using Rage;

    [Serializable]
    public struct FireStationData
    {
        public string Name;
        public Vector3 EntrancePosition;
        public RotatedVector3 FiretruckLocation;
    }


    internal static class FireStationDataPlaceHolder
    {
        public static FireStationData Get()
        {
            return new FireStationData()
            {
                Name = "[PLACEHOLDER] Rockford Hills Fire Station",
                EntrancePosition = new Vector3(-634.7078f, -121.6649f, 39.01375f),
                FiretruckLocation = new RotatedVector3(new Vector3(-646.5129f, -105.5248f, 37.959f), new Rotator(0.0f, 0.0f, 123.2762f)),
            };
        }
    }
}
