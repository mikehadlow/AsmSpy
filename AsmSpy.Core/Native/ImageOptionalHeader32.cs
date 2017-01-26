using System.Runtime.InteropServices;

namespace AsmSpy.Core.Native
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct ImageOptionalHeader32
    {
        [FieldOffset(0)]
        public ushort Magic;
        [FieldOffset(208)]
        public ImageDataDirectory DataDirectory;
    }
}