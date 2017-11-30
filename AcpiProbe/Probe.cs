using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AcpiProbe
{
    public class Probe
    {

        [DllImport("kernel32.dll")]
        private static extern uint EnumSystemFirmwareTables(uint FirmwareTableProviderSignature, IntPtr pFirmwareTableBuffer, uint BufferSize);

        [DllImport("kernel32.dll")]
        private static extern uint GetSystemFirmwareTable(uint FirmwareTableProviderSignature, uint FirmwareTableID, IntPtr pFirmwareTableBuffer, uint BufferSize);

        private uint provider = ACPI;

        //Static provider values, obtainable with StringToUint(Provider_Name, true)
        private const uint ACPI = 1094930505;
        private const uint FIRM = 1179210317;
        private const uint RSMB = 1381190978;

        public Probe()
        {
            //this.provider = StringToUint("ACPI", true); //This would set the provider to ACPI(1094930505)
        }

        private uint StringToUint(string input, Boolean reverse = false)
        {
            if (input.Length != 4)
            {
                throw new ArgumentException("String must be exactly 4 characters long.");
            }

            IEnumerable<char> chars;
            if (reverse)
            {
                chars = input.Reverse();
            }
            else
            {
                chars = input.AsEnumerable();
            }
            
            return BitConverter.ToUInt32(
                new[] //Creates an array from the backwards string
                {
                    Convert.ToByte(chars.ElementAt(0)),
                    Convert.ToByte(chars.ElementAt(1)),
                    Convert.ToByte(chars.ElementAt(2)),
                    Convert.ToByte(chars.ElementAt(3))
                }, 0);
        }

        public List<string> GetFirmwareTableHeaders()
        {
            List<string> returnList = new List<string>();
            IntPtr tablePtr = new IntPtr();
            uint bufferSize = 0;
            uint numberOfBytesReturned;

            bufferSize = EnumSystemFirmwareTables(provider, IntPtr.Zero, bufferSize);
            if (bufferSize % 4 != 0)
            {
                throw new OverflowException("Firmware Table headers buffer was attempted to be set to a number not perfectly divisible by 4 (the size of a header).");
            }

            tablePtr = Marshal.AllocHGlobal(Convert.ToInt32(bufferSize));
            numberOfBytesReturned = EnumSystemFirmwareTables(provider, tablePtr, bufferSize);

            for (int i = 0; i < numberOfBytesReturned; i += 4)
            {
                returnList.Add(
                    Convert.ToString(Marshal.ReadByte(tablePtr, i)) +
                    Convert.ToString(Marshal.ReadByte(tablePtr, i + 1)) +
                    Convert.ToString(Marshal.ReadByte(tablePtr, i + 2)) +
                    Convert.ToString(Marshal.ReadByte(tablePtr, i + 3))
                    );
            }
            return returnList;
        }

        private string ConvertBytesToString(byte[] bytes, uint startIndex, uint length)
        {
            if (length < 1) throw new ArgumentException("Can not have a length of less than 1 in ConvertBytesToString.");

            var builder = new StringBuilder();
            var endIndex = startIndex + length - 1;
            for (uint i = startIndex; i <= endIndex; i++)
            {
                builder.Append(Convert.ToChar(bytes[i]));
            }
            return builder.ToString();
        }

        private List<byte> ConvertByteArrayToByteList(byte[] bytes, uint startIndex, uint length)
        {
            if (length < 1) throw new ArgumentException("Can not have a length of less than 1 in ConvertByteArrayToByteList.");

            var returnList = new List<byte>();
            var endIndex = startIndex + length - 1;
            for (uint i = startIndex;i <= endIndex;i++)
            {
                returnList.Add(bytes[i]);
            }
            return returnList;
        }

        private T GetAcpiTableHeader<T>(byte[] tableArray) where T : AcpiTable, new()
        {
            return new T()
            {
                Signature = ConvertBytesToString(tableArray, 0, 4),
                Length = BitConverter.ToUInt32(tableArray, 4),
                Revision = tableArray[8],
                Checksum = tableArray[9],
                OemId = ConvertBytesToString(tableArray, 10, 6),
                OemTableId = ConvertBytesToString(tableArray, 16, 8),
                OemRevision = BitConverter.ToUInt32(tableArray, 24),
                CreatorId = ConvertBytesToString(tableArray, 28, 4),
                CreatorRevision = BitConverter.ToUInt32(tableArray, 32)
            };
        }

        private byte[] GetSystemFirmwareTable(string signature)
        {
            if (signature.Length != 4) throw new ArgumentException("Signature must be exactly 4 characters.");

            IntPtr tablePtr = new IntPtr();
            uint bufferSize = 0;
            uint numberOfBytesReturned;

            bufferSize = GetSystemFirmwareTable(provider, StringToUint(signature), IntPtr.Zero, bufferSize);
            tablePtr = Marshal.AllocHGlobal(Convert.ToInt32(bufferSize));
            numberOfBytesReturned = GetSystemFirmwareTable(provider, StringToUint(signature), tablePtr, bufferSize);

            var returnArray = new byte[numberOfBytesReturned];
            Marshal.Copy(tablePtr, returnArray, 0, Convert.ToInt32(numberOfBytesReturned));
            return returnArray;
        }

        public MSDM GetMicrosoftDigitalManagementTable()
        {
            var tableArray = GetSystemFirmwareTable("MSDM");

            var dataLength = BitConverter.ToUInt32(tableArray, 52);
            var msdm = GetAcpiTableHeader<MSDM>(tableArray);

            msdm.Version = BitConverter.ToUInt32(tableArray, 36);
            msdm.Reserved = BitConverter.ToUInt32(tableArray, 40);
            msdm.DataType = BitConverter.ToUInt32(tableArray, 44);
            msdm.DataReserved = BitConverter.ToUInt32(tableArray, 48);
            msdm.DataLength = dataLength;
            msdm.Data = ConvertBytesToString(tableArray, 56, dataLength);

            return msdm;
        }

        public SLIC GetSoftwareLicensingTable()
        {
            var tableArray = GetSystemFirmwareTable("SLIC");
            var slic = GetAcpiTableHeader<SLIC>(tableArray);

            var publicKeyLength = BitConverter.ToUInt32(tableArray, 40);
            slic.PublicKey = new SLIC.OemPublicKeyStructure()
            {
                Type = BitConverter.ToUInt32(tableArray, 36),
                Length = publicKeyLength,
                KeyType = tableArray[44],
                Version = tableArray[45],
                Reserved = BitConverter.ToUInt16(tableArray, 46),
                Algorithm = BitConverter.ToUInt32(tableArray, 48),
                Magic = ConvertBytesToString(tableArray, 52, 4),
                BitLength = BitConverter.ToUInt32(tableArray, 56),
                Exponent = BitConverter.ToUInt32(tableArray, 60),
                Modulus = ConvertByteArrayToByteList(tableArray, 64, publicKeyLength - 28)
                //The length = # of bytes in whole structure, so we subtract the static values that preceed the Modulus
                //Type(4) + Length(4) + KeyType(1) + Version(1) + Reserved(2) + Alg(4) + Magic(4) + BitLen(4) + Exponent(4) = 28
            };
            var markerLength = BitConverter.ToUInt32(tableArray, 196);
            slic.Marker = new SLIC.SlicMarkerStructure()
            {
                Type = BitConverter.ToUInt32(tableArray, 192),
                Length = markerLength,
                Version = BitConverter.ToUInt32(tableArray, 200),
                OemId = ConvertBytesToString(tableArray, 204, 6),
                OemTableId = ConvertBytesToString(tableArray, 210, 8),
                WindowsFlag = ConvertBytesToString(tableArray,218, 8),
                SlicVer = BitConverter.ToUInt32(tableArray, 226),
                Reserved = ConvertByteArrayToByteList(tableArray, 230, 16),
                Signature = ConvertByteArrayToByteList(tableArray, 246, markerLength - 54)
                //The length = # of bytes in whole structure, so we subtract the static values that preceed the Signature
                //Type(4) + Length(4) + Version(4) + OemId(6) + OemTable(8) + Flag(8) + SlicVer(4) + Reserved(16) = 54
            };
            return slic;
        }

    }
}
