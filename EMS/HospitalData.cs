namespace EmergencyV
{
    using System.Runtime.Serialization;

    using static Settings;

    [DataContract(Name = "Hospital", Namespace = "EmergencyV")]
    internal class HospitalData : BuildingData
    {
        [DataMember]
        public XYZW AmbulanceSpawn;
        [DataMember]
        public XYZ DropOffLocation;
        
        public static HospitalData[] GetDefaults()
        {
            return new HospitalData[]
            {
                new HospitalData
                {
                    Name = "Central Los Santos Medical Center",
                    Entrance = new XYZ { X = 342.98f, Y = -1398.16f, Z = 32.51f },
                    ActivationRange = 64.0f,
                    AmbulanceSpawn = new XYZW { X = 405.43f, Y = -1431.11f, Z = 29.20f, W = 326.25f },
                    DropOffLocation = new XYZ { X = 295.8f, Y = -1439.98f, Z = 29.31f },
                },
                new HospitalData
                {
                    Name = "Sandy Shores Medical Center",
                    Entrance = new XYZ { X = 1840.07f, Y = 3673.39f, Z = 34.28f },
                    ActivationRange = 64.0f,
                    AmbulanceSpawn = new XYZW { X = 1820.06f, Y = 3654.43f, Z = 33.9f, W = 210.78f },
                    DropOffLocation = new XYZ { X = 1827.95f, Y = 3694.44f, Z = 33.99f },
                }
            };
        }
    }
}
