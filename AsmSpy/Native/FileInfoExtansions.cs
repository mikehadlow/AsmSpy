using System.IO;

namespace AsmSpy.Native
{
    internal static class FileInfoExtansions
    {
        internal static bool IsAssembly(this FileInfo fileInfo)
        {
            if (fileInfo.Length < 4096)
            {
                return false;
            }
            var data = new byte[4096];
            using (var fs = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                var iRead = fs.Read(data, 0, 4096);
                if (iRead != 4096)
                {
                    return false;
                }
            }
            unsafe
            {
                fixed (byte* pData = data)
                {
                    var idh = (ImageDosHeader*)pData;
                    var inhs = (ImageNtHeaders32*)(idh->FileAddressOfNewExeHeader + pData);
                    var machineType = (MachineType)inhs->FileHeader.Machine;
                    if (machineType == MachineType.X64 &&
                      inhs->OptionalHeader.Magic == 0x20b)
                    {
                        var dataDir =
                          ((ImageNtHeaders64*)inhs)->OptionalHeader.DataDirectory;
                        if (dataDir.Size <= 0)
                        {
                            return false;
                        }
                    }
                    else if (inhs->OptionalHeader.DataDirectory.Size <= 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
