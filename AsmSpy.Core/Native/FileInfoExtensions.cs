using System.IO;

namespace AsmSpy.Core.Native
{
    internal static class FileInfoExtensions
    {
        private const int PageSize = 4096;

        internal static bool IsAssembly(this FileInfo fileInfo)
        {
            // Symbolic links always have a length of 0, which is the length of the symbolic link file (not the target file).
            // After we read the first page of the file we check we actually got a page, so we can skip the check here safely.
            if (fileInfo.Length < PageSize && !fileInfo.IsSymbolicLink())
            {
                return false;
            }

            var data = new byte[PageSize];
            using (var fs = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var iRead = fs.Read(data, 0, PageSize);
                if (iRead != PageSize)
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

        internal static bool IsSymbolicLink(this FileInfo fileInfo)
        {
            return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }
    }
}
