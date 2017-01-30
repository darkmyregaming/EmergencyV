namespace EmergencyV
{
    // System
    using System;

    // RPH
    using Rage;

    internal struct RotatedVector3
    {
        public readonly Vector3 Position;
        public readonly Rotator Rotation;
        public readonly float Heading;

        public RotatedVector3(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
            Rotation = new Rotator(0.0f, 0.0f, heading);
        }

        public RotatedVector3(Vector3 position, Rotator rotation)
        {
            Position = position;
            Heading = rotation.Yaw;
            Rotation = rotation;
        }


        public static RotatedVector3 Zero
        {
            get { return new RotatedVector3(Vector3.Zero, 0f); }
        }


        public static bool operator ==(RotatedVector3 left, RotatedVector3 right)
        {
            return left.Position == right.Position && left.Rotation == right.Rotation;
        }

        public static bool operator !=(RotatedVector3 left, RotatedVector3 right)
        {
            return left.Position != right.Position || left.Rotation != right.Rotation;
        }

        public static RotatedVector3 operator +(RotatedVector3 left, RotatedVector3 right)
        {
            return new RotatedVector3(left.Position + right.Position, left.Rotation + right.Rotation);
        }

        public static RotatedVector3 operator -(RotatedVector3 left, RotatedVector3 right)
        {
            return new RotatedVector3(left.Position - right.Position, left.Rotation - right.Rotation);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(RotatedVector3))
                throw new System.InvalidCastException();

            return Equals((RotatedVector3)obj);
        }

        public bool Equals(RotatedVector3 spawnPoint)
        {
            return this.Position == spawnPoint.Position && this.Rotation == spawnPoint.Rotation;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + this.Position.GetHashCode();
            hash = (hash * 7) + this.Rotation.GetHashCode();
            return hash;
        }
    }
}
