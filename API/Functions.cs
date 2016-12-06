namespace EmergencyV.API
{
    // RPH
    using Rage;

    public static class Functions
    {
        public static Fire[] CreateFires(Vector3[] positions, int maxChildren, bool isGasFire, bool onGround = true)
        {
            return Util.CreateFires(positions, maxChildren, isGasFire, onGround);
        }
    }
}
