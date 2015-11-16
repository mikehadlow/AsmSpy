namespace AsmSpy.Native
{
    internal struct ImageNtHeaders64
    {
        public uint Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader64 OptionalHeader;
    }
}