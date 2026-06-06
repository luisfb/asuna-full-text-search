using AsunaLocalSearch.Indexes;

namespace UnitTests
{
    public class HashsetIndexTest
    {
        public HashsetIndexTest()
        {
            
        }


        [Fact(DisplayName = "Should index the files at given directory.")]
        public void ShouldIndexFiles()
        {
            var asd = HashsetIndex.CreateIndexOfFileNamesAsync("C:\\Git\\asuna-full-text-search\\UnitTests", "*.cs").Result;

        }

    }
}
