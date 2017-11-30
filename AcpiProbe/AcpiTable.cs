using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcpiProbe
{
    public abstract class AcpiTable
    {
        public string Signature { get; set; }
        public uint Length { get; set; }
        public uint Revision { get; set; }
        public uint Checksum { get; set; }
        public string OemId { get; set; }
        public string OemTableId { get; set; }
        public uint OemRevision { get; set; }
        public string CreatorId { get; set; }
        public uint CreatorRevision { get; set; }
    }
}
