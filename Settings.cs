namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Xml;
    using System.Runtime.Serialization;

    // RPH
    using Rage;

    [DataContract(Name = "UserSettings", Namespace = "EmergencyV")]
    internal class Settings
    {
        // members
        [DataMember]
        public VehiclesSettings VEHICLES;
        [DataMember]
        public PedsSettings PEDS;
        [DataMember]
        public CalloutsSettings CALLOUTS;


        // main settings classes
        [DataContract(Name = "VEHICLES", Namespace = "EmergencyV")]
        public class VehiclesSettings
        {
            [DataMember]
            public string ENGINE_MODEL;
            [DataMember]
            public string RESCUE_MODEL;
            [DataMember]
            public string BATTALION_MODEL;

            [DataMember(IsRequired = false)]
            public ColorData ENGINE_PRIMARY_COLOR;
            [DataMember(IsRequired = false)]
            public ColorData ENGINE_SECONDARY_COLOR;

            [DataMember(IsRequired = false)]
            public ColorData RESCUE_PRIMARY_COLOR;
            [DataMember(IsRequired = false)]
            public ColorData RESCUE_SECONDARY_COLOR;

            [DataMember(IsRequired = false)]
            public ColorData BATTALION_PRIMARY_COLOR;
            [DataMember(IsRequired = false)]
            public ColorData BATTALION_SECONDARY_COLOR;
        }

        [DataContract(Name = "PEDS", Namespace = "EmergencyV")]
        public class PedsSettings
        {
            [DataMember(Order = 0)]
            public string FIREFIGHTER_MODEL;

            [DataMember(Order = 1)]
            public bool FIREFIGHTER_FLASHLIGHT_ENABLED;
            [DataMember(Order = 2)]
            public PedBoneId FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE;
            [DataMember(Order = 3)]
            public XYZ FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET;
            [DataMember(Order = 4)]
            public ColorData FIREFIGHTER_FLASHLIGHT_COLOR;

            [DataMember(Order = 5)]
            public PedComponentVariation[] FIREFIGHTER_FIRE_GEAR_ENABLED_COMPONENTS;
            [DataMember(Order = 6)]
            public PedPropVariation[] FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS;

            [DataMember(Order = 7)]
            public PedComponentVariation[] FIREFIGHTER_FIRE_GEAR_DISABLED_COMPONENTS;

            [DataMember(Order = 8)]
            public string EMS_MODEL;
        }


        [DataContract(Name = "CALLOUTS", Namespace = "EmergencyV")]
        public class CalloutsSettings
        {
            [DataMember(Order = 0)]
            public double MIN_SECONDS_BETWEEN_CALLOUTS;
            [DataMember(Order = 1)]
            public double MAX_SECONDS_BETWEEN_CALLOUTS;
        }



        // settings utility classes
        [DataContract(Name = "Color", Namespace = "EmergencyV")]
        public class ColorData
        {
            [DataMember(Order = 0)]
            public byte R;
            [DataMember(Order = 1)]
            public byte G;
            [DataMember(Order = 2)]
            public byte B;

            public System.Drawing.Color ToColor()
            {
                return System.Drawing.Color.FromArgb(R, G, B);
            }
        }

        [DataContract(Name = "Vector3", Namespace = "EmergencyV")]
        public class XYZ
        {
            [DataMember(Order = 0)]
            public float X;
            [DataMember(Order = 1)]
            public float Y;
            [DataMember(Order = 2)]
            public float Z;

            public Vector3 ToVector3()
            {
                return new Vector3(X, Y, Z);
            }
        }

        [DataContract(Name = "Vector4", Namespace = "EmergencyV")]
        public class XYZW
        {
            [DataMember(Order = 0)]
            public float X;
            [DataMember(Order = 1)]
            public float Y;
            [DataMember(Order = 2)]
            public float Z;
            [DataMember(Order = 3)]
            public float W;

            public Vector3 ToVector3()
            {
                return new Vector3(X, Y, Z);
            }

            public Vector4 ToVector4()
            {
                return new Vector4(X, Y, Z, W);
            }
        }

        [DataContract(Name = "PedComponentVariation", Namespace = "EmergencyV")]
        public class PedComponentVariation
        {
            [DataMember(Order = 0)]
            public int ComponentIndex;
            [DataMember(Order = 1)]
            public int DrawableIndex;
            [DataMember(Order = 2)]
            public int DrawableTextureIndex;
        }

        [DataContract(Name = "PedPropVariation", Namespace = "EmergencyV")]
        public class PedPropVariation
        {
            [DataMember(Order = 0)]
            public int ComponentIndex;
            [DataMember(Order = 1)]
            public int DrawableIndex;
            [DataMember(Order = 2)]
            public int DrawableTextureIndex;
        }



        //
        public static Settings GetDefault()
        {
            return new Settings()
            {
                VEHICLES = new VehiclesSettings()
                {
                    ENGINE_MODEL = "FIRETRUK",
                    RESCUE_MODEL = "FIRETRUK",
                    BATTALION_MODEL = "FBI2",
                    BATTALION_PRIMARY_COLOR = new ColorData() { R = 200, G = 0, B = 0 },
                },

                PEDS = new PedsSettings()
                {
                    FIREFIGHTER_MODEL = "S_M_Y_FIREMAN_01",

                    FIREFIGHTER_FLASHLIGHT_ENABLED = true,
                    FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE = PedBoneId.Spine2,
                    FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET = new XYZ() { X = -0.1325f, Y = 0.2f, Z = 0.2825f },
                    FIREFIGHTER_FLASHLIGHT_COLOR = new ColorData() { R = 15, G = 15, B = 15 },

                    FIREFIGHTER_FIRE_GEAR_ENABLED_COMPONENTS = new PedComponentVariation[]
                    {
                        new PedComponentVariation() {ComponentIndex = 8, DrawableIndex = 1, DrawableTextureIndex = 0 },
                    },
                    FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS = new PedPropVariation[]
                    {
                        new PedPropVariation() {ComponentIndex = 1, DrawableIndex = 0, DrawableTextureIndex = 0 },
                    },

                    FIREFIGHTER_FIRE_GEAR_DISABLED_COMPONENTS = new PedComponentVariation[]
                    {
                        new PedComponentVariation() {ComponentIndex = 8, DrawableIndex = 0, DrawableTextureIndex = 0 },
                    },

                    EMS_MODEL = "S_M_M_PARAMEDIC_01",
                },

                CALLOUTS = new CalloutsSettings()
                {
                    MIN_SECONDS_BETWEEN_CALLOUTS = 10.0,
                    MAX_SECONDS_BETWEEN_CALLOUTS = 180.0,
                },
            };
        }
    }
}
