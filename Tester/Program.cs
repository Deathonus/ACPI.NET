﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var probe = new AcpiProbe.Probe();
            var msdm = probe.GetMicrosoftDigitalManagementTable();
            var slic = probe.GetSoftwareLicensingTable();
            Console.ReadLine();
        }
    }
}
