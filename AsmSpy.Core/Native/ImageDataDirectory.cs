namespace AsmSpy.Core.Native
{
    internal struct ImageDataDirectory
    {
        public ImageDataDirectory(uint size, uint virtualAddress)
        {
            Size = size;
            VirtualAddress = virtualAddress;
        }

        public uint VirtualAddress { get; }
        public uint Size { get; }
    }
}