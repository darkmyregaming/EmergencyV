namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    // RPH
    using Rage;

    [XmlRoot(ElementName = "SETTINGS")]
    internal class Settings
    {
        // members
        public VehiclesSettings VEHICLES { get; set; }
        public PedsSettings PEDS { get; set; }
        public CalloutsSettings CALLOUTS { get; set; }


        // main settings classes
        public class VehiclesSettings
        {
            public string ENGINE_MODEL { get; set; }
            public string RESCUE_MODEL { get; set; }
            public string BATTALION_MODEL { get; set; }

            [XmlElement(IsNullable = true)]
            public ColorData ENGINE_PRIMARY_COLOR { get; set; }
            [XmlElement(IsNullable = true)]
            public ColorData ENGINE_SECONDARY_COLOR { get; set; }

            [XmlElement(IsNullable = true)]
            public ColorData RESCUE_PRIMARY_COLOR { get; set; }
            [XmlElement(IsNullable = true)]
            public ColorData RESCUE_SECONDARY_COLOR { get; set; }

            [XmlElement(IsNullable = true)]
            public ColorData BATTALION_PRIMARY_COLOR { get; set; }
            [XmlElement(IsNullable = true)]
            public ColorData BATTALION_SECONDARY_COLOR { get; set; }
            
            public string AMBULANCE_MODEL { get; set; }

            [XmlElement(IsNullable = true)]
            public ColorData AMBULANCE_PRIMARY_COLOR { get; set; }
            [XmlElement(IsNullable = true)]
            public ColorData AMBULANCE_SECONDARY_COLOR { get; set; }
        }
        
        public class PedsSettings
        {
            public string FIREFIGHTER_MODEL { get; set; }
            
            public bool FIREFIGHTER_FLASHLIGHT_ENABLED { get; set; }
            public PedBoneId FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE { get; set; }
            public XYZ FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET { get; set; }
            public ColorData FIREFIGHTER_FLASHLIGHT_COLOR { get; set; }
            
            public PedComponentVariation[] FIREFIGHTER_FLASHLIGHT_ON_COMPONENTS { get; set; }
            public PedPropVariation[] FIREFIGHTER_FLASHLIGHT_ON_PROPS { get; set; }
            
            public PedComponentVariation[] FIREFIGHTER_FLASHLIGHT_OFF_COMPONENTS { get; set; }
            
            public PedComponentVariation[] FIREFIGHTER_FIRE_GEAR_ENABLED_COMPONENTS { get; set; }
            public PedPropVariation[] FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS { get; set; }
            
            public PedComponentVariation[] FIREFIGHTER_FIRE_GEAR_DISABLED_COMPONENTS { get; set; }
            
            public string EMS_MODEL { get; set; }
        }

        
        public class CalloutsSettings
        {
            [XmlAttribute]
            public double MIN_SECONDS_BETWEEN_CALLOUTS { get; set; }
            [XmlAttribute]
            public double MAX_SECONDS_BETWEEN_CALLOUTS { get; set; }
        }



        // settings utility classes
        public class ColorData
        {
            [XmlAttribute]
            public byte R { get; set; }
            [XmlAttribute]
            public byte G { get; set; }
            [XmlAttribute]
            public byte B { get; set; }

            public System.Drawing.Color ToColor()
            {
                return System.Drawing.Color.FromArgb(R, G, B);
            }
        }
        
        public class XYZ
        {
            [XmlAttribute]
            public float X { get; set; }
            [XmlAttribute]
            public float Y { get; set; }
            [XmlAttribute]
            public float Z { get; set; }

            public Vector3 ToVector3()
            {
                return new Vector3(X, Y, Z);
            }
        }
        
        public class XYZW
        {
            [XmlAttribute]
            public float X { get; set; }
            [XmlAttribute]
            public float Y { get; set; }
            [XmlAttribute]
            public float Z { get; set; }
            [XmlAttribute]
            public float W { get; set; }

            public Vector3 ToVector3()
            {
                return new Vector3(X, Y, Z);
            }

            public Vector4 ToVector4()
            {
                return new Vector4(X, Y, Z, W);
            }
        }
        
        public class PedComponentVariation
        {
            public int ComponentIndex { get; set; }
            public int DrawableIndex { get; set; }
            public int DrawableTextureIndex { get; set; }
        }
        
        public class PedPropVariation
        {
            public int ComponentIndex { get; set; }
            public int DrawableIndex { get; set; }
            public int DrawableTextureIndex { get; set; }
        }



        //
        public static Settings GetDefault()
        {
            return new Settings()
            {
                VEHICLES = new VehiclesSettings()
                {
                    ENGINE_MODEL = "FIRETRUK",
                    ENGINE_PRIMARY_COLOR = null,
                    ENGINE_SECONDARY_COLOR = null,

                    RESCUE_MODEL = "FIRETRUK",
                    RESCUE_PRIMARY_COLOR = null,
                    RESCUE_SECONDARY_COLOR = null,

                    BATTALION_MODEL = "FBI2",
                    BATTALION_PRIMARY_COLOR = new ColorData() { R = 200, G = 0, B = 0 },
                    BATTALION_SECONDARY_COLOR = null,

                    AMBULANCE_MODEL = "AMBULANCE",
                    AMBULANCE_PRIMARY_COLOR = null,
                    AMBULANCE_SECONDARY_COLOR = null,
                },

                PEDS = new PedsSettings()
                {
                    FIREFIGHTER_MODEL = "S_M_Y_FIREMAN_01",

                    FIREFIGHTER_FLASHLIGHT_ENABLED = true,
                    FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE = PedBoneId.Spine2,
                    FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET = new XYZ() { X = -0.1325f, Y = 0.2f, Z = 0.2825f },
                    FIREFIGHTER_FLASHLIGHT_COLOR = new ColorData() { R = 15, G = 15, B = 15 },

                    FIREFIGHTER_FLASHLIGHT_ON_COMPONENTS = new PedComponentVariation[]
                    {
                        new PedComponentVariation() { ComponentIndex = 8, DrawableIndex = 2, DrawableTextureIndex = 0 },
                    },

                    FIREFIGHTER_FLASHLIGHT_ON_PROPS = new PedPropVariation[]
                    {
                    },

                    FIREFIGHTER_FLASHLIGHT_OFF_COMPONENTS = new PedComponentVariation[]
                    {
                        new PedComponentVariation() { ComponentIndex = 8, DrawableIndex = 1, DrawableTextureIndex = 0 },
                    },

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
