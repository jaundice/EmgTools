using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace EmgTools.IO.OlimexShield
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct EkgEmgShieldEvent
    {
        [FieldOffset(0)]
        public readonly byte Sync0;
        [FieldOffset(1)]
        public readonly byte Sync1;
        [FieldOffset(2)]
        public readonly byte Version;
        [FieldOffset(3)]
        public readonly byte Count;
        [FieldOffset(4)]
        private fixed ushort RawData[6];
        [FieldOffset(16)]
        public readonly byte Switches;

        public ushort this[int index]
        {
            get { return ReadBigEndian(index); }
        }

        private ushort ReadBigEndian(int index)
        {
            if (index > 5 || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            fixed (ushort* data = RawData)
            {
                var raw = data[index];
                if (BitConverter.IsLittleEndian)
                {
                    var bytes = BitConverter.GetBytes(raw);
                    Array.Reverse(bytes);
                    return BitConverter.ToUInt16(bytes, 0);
                }
                return raw;
            }
        }
    }
}