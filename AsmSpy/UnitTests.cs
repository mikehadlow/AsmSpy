using System.IO;

namespace AsmSpy
{
    public class UnitTests
    {
        public void AnalyseAssemblies_should_output_correct_info_only_conflicts()
        {
            const string path = @"D:\Source\sutekishop\Suteki.Shop\Suteki.Shop.CreateDb\bin\Debug";
            Program.AnalyseAssemblies(new DirectoryInfo(path), true);
        }

        public void AnalyseAssemblies_should_output_correct_info()
        {
            const string path = @"D:\Source\sutekishop\Suteki.Shop\Suteki.Shop.CreateDb\bin\Debug";
            Program.AnalyseAssemblies(new DirectoryInfo(path), false);
        }
    }
}