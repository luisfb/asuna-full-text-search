using System.Text;

namespace UnitTests
{
    internal static class FixtureHelper
    {
        internal static string GetLoremIpsumTxtContent(Encoding encoding = null)
        {
            if(encoding == null)
                encoding = Encoding.UTF8;

            string textFileContent;
            var filePath = GetLoremIpsumTextFilePath();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fs, encoding))
                textFileContent = sr.ReadToEnd();

            return textFileContent;
        }

        internal static string GetLoremIpsumTextFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Fixture", "Lorem ipsum.txt");
        }
    }
}
