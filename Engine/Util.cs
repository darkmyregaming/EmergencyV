namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;
    using System.IO;
    using System.Xml;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

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

        public static void DrawSpotlightWithShadow(Vector3 origin, Vector3 direction, Color color, float distance, float brightness, float roundness, float radius, float fallout)
        {
            NativeFunction.Natives.x5BCA583A583194DB(origin.X, origin.Y, origin.Z,
                                                     direction.X, direction.Y, direction.Z,
                                                     color.R, color.G, color.B,
                                                     distance, brightness, roundness,
                                                     radius, fallout, 0.0f); // _DRAW_SPOT_LIGHT_WITH_SHADOW
        }

        public static API.ScriptedFire[] CreateFires(Vector3[] positions, int maxChildren, bool isGasFire, bool onGround = true)
        {
            uint[] handle = new uint[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 p = positions[i];
                if (onGround)
                {
                    float? z = World.GetGroundZ(p, false, true);
                    if (z.HasValue)
                    {
                        p.Z = z.Value;
                        positions[i].Z = z.Value;
                    }
                }

                handle[i] = NativeFunction.Natives.StartScriptFire<uint>(p.X, p.Y, p.Z, maxChildren, isGasFire);
            }

            API.ScriptedFire[] fires = World.GetAllFires().Where(f => positions.Contains(f.Position)).Select(f => new API.ScriptedFire(handle[Array.IndexOf(positions, f.Position)], f)).ToArray();
            return fires;
        }

        public static Rage.Object[] CreateConesAtVehicleRightSide(Vehicle vehicle, float distanceFromVehicle, bool freezeConesPosition = true, bool createSideCones = true, bool createFrontCones = true, bool createRearCones = true)
        {
            return CreateConesAtVehicleSide(vehicle, distanceFromVehicle, freezeConesPosition, createSideCones, createFrontCones, createRearCones, vehicle.RightPosition, vehicle.RightVector);
        }

        public static Rage.Object[] CreateConesAtVehicleLeftSide(Vehicle vehicle, float distanceFromVehicle, bool freezeConesPosition = true, bool createSideCones = true, bool createFrontCones = true, bool createRearCones = true)
        {
            return CreateConesAtVehicleSide(vehicle, distanceFromVehicle, freezeConesPosition, createSideCones, createFrontCones, createRearCones, vehicle.LeftPosition, -vehicle.RightVector);
        }

        private static Rage.Object[] CreateConesAtVehicleSide(Vehicle vehicle, float distanceFromVehicle, bool freezeConesPosition, bool createSideCones, bool createFrontCones, bool createRearCones, Vector3 sidePosition, Vector3 sideVector)
        {
            // TODO: CreateConesAtVehicleSide(): make the vehicles go around the cones instead of ramming into them
            System.Collections.Generic.List<Rage.Object> cones = new System.Collections.Generic.List<Rage.Object>();
            Action<Vector3> createCone = (pos) =>
            {
                Rage.Object o = new Rage.Object($"prop_mp_cone_02", pos);
                float? z = World.GetGroundZ(o.Position, false, true);
                if (z.HasValue)
                {
                    o.SetPositionZ(z.Value);
                }
                o.IsPositionFrozen = freezeConesPosition;
                cones.Add(o);
            };


            float length = vehicle.Length;
            Vector3 forwardVector = vehicle.ForwardVector;

            const float separation = 2.0f;

            if (createSideCones)
            {
                // create cones at the side of the vehicle
                createCone(sidePosition + (sideVector * distanceFromVehicle)); // middle cone
                for (float i = separation, j = -separation; i < (length / 2); i += separation, j -= separation)
                {
                    createCone(sidePosition + (sideVector * distanceFromVehicle) + (forwardVector * i)); // from middle to front cones

                    createCone(sidePosition + (sideVector * distanceFromVehicle) + (forwardVector * j)); // from middle to rear cones
                }
            }

            float width = vehicle.Width;
            Vector3 rearPosition = vehicle.RearPosition;
            Vector3 frontPosition = vehicle.FrontPosition;

            float rearFrontConesdistanceFromVehicle = distanceFromVehicle * 2 + length / 2;
            // create cones at the front and rear, going from one side to the other
            for (float i = -(width / 2) - distanceFromVehicle; i < (width / 2); i += separation / 2)
            {
                if (createRearCones)
                    createCone(rearPosition + (forwardVector * -(rearFrontConesdistanceFromVehicle + (i * separation))) + (sideVector * -i)); // rear cones

                if (createFrontCones)
                    createCone(frontPosition + (forwardVector * (rearFrontConesdistanceFromVehicle + (i * separation))) + (sideVector * -i)); // front cones
            }

            return cones.ToArray();
        }


        // Data contract (de)serialization
        public static void Serialize<T>(string fileName, T graph)
        {
            using (XmlWriter writer = XmlWriter.Create(fileName, new XmlWriterSettings() { Indent = true }))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(writer, graph);
            }
        }

        public static T Deserialize<T>(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("File to deserialize not found", fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(T));
                
                T obj = (T)ser.ReadObject(reader, true);
                return obj;
            }
        }


        // easing functions
        public static class Easing
        {
            // http://gizma.com/easing/
            public static float OutQuart(float currentTime, float startValue, float changeInValue, float duration)
            {
                currentTime /= duration;
                currentTime--;
                return -changeInValue * (currentTime * currentTime * currentTime * currentTime - 1) + startValue;
            }
            
            public static float OutSine(float currentTime, float startValue, float changeInValue, float duration)
            {
                return changeInValue * (float)Math.Sin(currentTime / duration * (Math.PI / 2)) + startValue;
            }

            public static float OutQuint(float currentTime, float startValue, float changeInValue, float duration)
            {
                currentTime /= duration;
                currentTime--;
                return changeInValue * (currentTime * currentTime * currentTime * currentTime * currentTime + 1) + startValue;
            }
        }

        public static string EnumNameToDelimitedString(Type type, object obj, char delimiter)
        {
            string name = Enum.GetName(type, obj);
            StringBuilder final = new StringBuilder();

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (!char.IsLetter(c))
                    continue;
                if (i != 0 && char.IsUpper(c))
                    final.Append(delimiter);

                final.Append(char.ToLower(c));
            }

            return final.ToString();
        }

        public static string FirstCharToUpper(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("string cannot be null or empty");
            return text.First().ToString().ToUpper() + text.Substring(1);
        }

        public static float GetHeadingTowards(this Vector3 v, Vector3 pos)
        {
            Vector3 d = pos - v;
            return MathHelper.ConvertDirectionToHeading(d.ToNormalized());
        }

        public static float GetHeadingTowards(this Vector3 v, ISpatial s)
        {
            Vector3 d = s.Position - v;
            return MathHelper.ConvertDirectionToHeading(d.ToNormalized());
        }

        public static float GetHeadingAbsDifference(float heading1, float heading2)
        {
            float h = MathHelper.NormalizeHeading(MathHelper.NormalizeHeading(heading1) - MathHelper.NormalizeHeading(heading2));
            return Math.Min(Math.Abs(360 - h), h);
        }

        internal static Ped GetClosestDeadPed(this Vector3 v, float range)
        {
            Ped victim = null;
            float closestDist = float.MaxValue;
            foreach (Ped p in World.EnumeratePeds())
            {
                if (!p || p.IsPlayer || !p.IsHuman || p.IsAlive)
                    continue;
                float dist = Vector3.DistanceSquared(v, p.Position);
                if (dist > range * range)
                    continue;
                if (dist < closestDist)
                {
                    victim = p;
                    closestDist = dist;
                }
            }
            return victim;
        }
    }
}
