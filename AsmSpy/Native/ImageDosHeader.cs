using System.Runtime.InteropServices;

namespace AsmSpy.Native
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct ImageDosHeader
    {
        [FieldOffset(60)]
        public int FileAddressOfNewExeHeader;
    }
}