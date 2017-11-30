using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcpiProbe
{
    public class MSDM : AcpiTable
    {
        public uint Version { get; set; }
        public uint Reserved { get; set; }
        public uint DataType { get; set; }
        public uint DataReserved { get; set; }
        public uint DataLength { get; set; }
        public string Data { get; set; }
    }
}
