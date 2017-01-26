namespace AsmSpy.Core.Native
{
    internal struct ImageFileHeader
    {
        public ImageFileHeader(ushort machine, ushort numberOfSections, uint timeDateStamp, uint pointerToSymbolTable, uint numberOfSymbols, ushort sizeOfOptionalHeader, ushort characteristics)
        {
            Machine = machine;
            NumberOfSections = numberOfSections;
            TimeDateStamp = timeDateStamp;
            PointerToSymbolTable = pointerToSymbolTable;
            NumberOfSymbols = numberOfSymbols;
            SizeOfOptionalHeader = sizeOfOptionalHeader;
            Characteristics = characteristics;
        }

        public ushort Machine { get; }
        public ushort NumberOfSections { get; }
        public uint TimeDateStamp { get; }
        public uint PointerToSymbolTable { get; }
        public uint NumberOfSymbols { get; }
        public ushort SizeOfOptionalHeader { get; }
        public ushort Characteristics { get; }
    }
}