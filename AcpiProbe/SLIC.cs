using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcpiProbe
{
    public class SLIC : AcpiTable
    {
        public OemPublicKeyStructure PublicKey { get; set; }
        public SlicMarkerStructure Marker { get; set; }

        public class OemPublicKeyStructure
        {
            public uint Type { get; set; }
            public uint Length { get; set; } 
            public uint KeyType { get; set; }
            public uint Version { get; set; }
            public uint Reserved { get; set; }
            public uint Algorithm { get; set; }
            public string Magic { get; set; }
            public uint BitLength { get; set; }
            public uint Exponent { get; set; }
            public List<byte> Modulus { get; set; }
        }
        public class SlicMarkerStructure
        {
            public uint Type { get; set; }
            public uint Length { get; set; }
            public uint Version { get; set; }
            public string OemId { get; set; }
            public string OemTableId { get; set; }
            public string WindowsFlag { get; set; }
            public uint SlicVer { get; set; }
            //public UInt64 Reserved { get; set; }
            public List<Byte> Reserved { get; set; }
            public List<Byte> Signature { get; set; }
        }
    }
}
