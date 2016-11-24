namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class Util
    {
        public static void DrawMarker(int type, Vector3 position, Vector3 direction, Rotator rotation, Vector3 scale, Color color, bool bobUpAndDown = false, bool faceCamera = false, bool rotate = false)
        {
            NativeFunction.Natives.DrawMarker(type,
                                              position.X, position.Y, position.Z,
                                              direction.X, direction.Y, direction.Z,
                                              rotation.Pitch, rotation.Roll, rotation.Yaw,
                                              scale.X, scale.Y, scale.Z, 
                                              color.R, color.G, color.B, color.A,
                                              bobUpAndDown, faceCamera,
                                              2, rotate,
                                              0, 0,
                                              false);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            NativeFunction.Natives.DrawLine(start.X, start.Y, start.Z,
                                            end.X, end.Y, end.Z,
                                            color.R, color.G, color.B, color.A);
        }
    }
}
