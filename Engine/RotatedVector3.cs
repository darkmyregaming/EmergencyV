namespace EmergencyV
{
    // System
    using System;

    // RPH
    using Rage;

    [Serializable]
    public struct RotatedVector3
    {
        public Vector3 Position;
        public Rotator Rotation;
        
        public float Heading { get { return Rotation.Yaw; } }

        public RotatedVector3(Vector3 position, Rotator rotation)
        {
            Position = position;
            Rotation = rotation;
        }


        public override bool Equals(object obj)
        {
            if (!(obj is RotatedVector3))
                return false;

            return this == ((RotatedVector3)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = hash * 11 + Position.GetHashCode();
                hash = hash * 11 + Rotation.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{{ {nameof(Position)}={Position}, {nameof(Rotation)}={Rotation} }}";
        }

        public static bool operator ==(RotatedVector3 left, RotatedVector3 right)
        {
            return left.Position == right.Position &&
                   left.Rotation == right.Rotation;
        }

        public static bool operator !=(RotatedVector3 left, RotatedVector3 right)
        {
            return !(left == right);
        }
    }
}
