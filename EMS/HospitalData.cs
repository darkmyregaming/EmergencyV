namespace EmergencyV
{
    using System.Runtime.Serialization;

    using static Settings;

    [DataContract(Name = "Hospital", Namespace = "EmergencyV")]
    internal class HospitalData : BuildingData
    {
        [DataMember]
        public XYZW AmbulanceSpawn;
        
        public static HospitalData[] GetDefaults()
        {
            return new HospitalData[]
            {
                new HospitalData
                {
                    Name = "Central Los Santos Medical Center",
                    Entrance = new XYZ { X = 342.98f, Y = -1398.16f, Z = 32.51f },
                    ActivationRange = 64.0f,
                    AmbulanceSpawn = new XYZW { X = 405.43f, Y = -1431.11f, Z = 29.20f, W = 326.25f }
                }
            };
        }
    }
}
