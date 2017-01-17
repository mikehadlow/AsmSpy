namespace AsmSpy.Native
{
    internal struct ImageNtHeaders64
    {
        public ImageNtHeaders64(uint signature, ImageFileHeader fileHeader, ImageOptionalHeader64 optionalHeader)
        {
            Signature = signature;
            FileHeader = fileHeader;
            OptionalHeader = optionalHeader;
        }

        public uint Signature { get; }
        public ImageFileHeader FileHeader { get; }
        public ImageOptionalHeader64 OptionalHeader { get; }
    }
}