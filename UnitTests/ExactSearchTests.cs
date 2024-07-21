using AsunaLocalSearch;
using FluentAssertions;
using System.Diagnostics;
using System.Text;

namespace UnitTests
{
    public class ExactSearchTests
    {
        #region ARRANGE

        private string GivenATextFile()
        {
            return FixtureHelper.GetLoremIpsumTxtContent(Encoding.UTF8);
        }

        private string GivenAString()
        {
            return "this is myString";
        }

        #endregion

        #region ACT

        private List<SearchResult> WhenSearchForAnExactWordCaseInsensitive(string wordToSearch, string targetText)
        {
            return Asuna.Search(wordToSearch, targetText).ToList();
        }

        private List<SearchResult> WhenSearchForAnExactWordCaseSensitive(string wordToSearch, string targetText)
        {
            return Asuna.Search(wordToSearch, targetText, SearchOptions.MatchCase).ToList();
        }

        #endregion

        #region ASSERT

        [Fact(DisplayName = "Should return the right index and line number for the result of an exact search.")]
        public void GivenAString_WhenSearchForAnExactWordCaseInsensitive_ThenShouldReturnAMatch()
        {
            var targetText = GivenAString();
            var result = WhenSearchForAnExactWordCaseInsensitive("myString", targetText);

            var expected = new SearchResult
            {
                Index = 8,
                LineNumber = 1,
            };

            result.Count().Should().Be(1);
            result.First().Index.Should().Be(8);
            result.First().LineNumber.Should().Be(1);
            result.Should().Contain(expected);
        }

        [Fact]
        public void teste()
        {
            var dir1 = Directory.GetCurrentDirectory();
            //var dir2 = AppDomain.CurrentDomain.BaseDirectory;

            var filePath = System.IO.Path.Combine(dir1, "Fixture", "Lorem ipsum.txt");
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var bs = new BufferedStream(fs))
                {
                    using (var sr = new StreamReader(bs, Encoding.UTF8))
                    {
                        string result = sr.ReadToEnd();

                        var results = Asuna.FullTextSearch("asuna", result);

                        int limit = 0;
                        foreach (var item in results.OrderBy(x => x.LineNumber).ThenBy(x => x.ColumnNumber))
                        {
                            limit++;
                            Debug.Print(item.Match + "   Linha: " + item.LineNumber.ToString() +
                                "  Coluna: " + item.ColumnNumber.ToString() +
                                "  Ranking: " + item.Ranking.ToString());
                            if (limit > 700) break;
                        }

                        var a = 9 * 9;
                    }
                }

            }

        }

        

        [Fact(DisplayName = "Should return the right indexes for all occurrences of the found word.")]
        public void GivenATextFile_WhenSearchForAnExactWordCaseInsensitive_ThenShouldReturnTheRightIndexesForEachResult()
        {
            string fileContent = GivenATextFile();
            var results = WhenSearchForAnExactWordCaseInsensitive("asuna", fileContent);

            var length = "asuna".Length;

            results.Count().Should().Be(6);

            fileContent.Substring(results[0].Index, length).Should().Be("asuna");
            fileContent.Substring(results[1].Index, length).Should().Be("asuna");
            fileContent.Substring(results[2].Index, length).Should().Be("asuna");
            fileContent.Substring(results[3].Index, length).Should().Be("asuna");
            fileContent.Substring(results[4].Index, length).Should().Be("asuna");
            fileContent.Substring(results[5].Index, length).Should().Be("asuna");

            results[0].LineNumber.Should().Be(3);
            results[1].LineNumber.Should().Be(9);
            results[2].LineNumber.Should().Be(11);
            results[3].LineNumber.Should().Be(18);
            results[4].LineNumber.Should().Be(26);
            results[5].LineNumber.Should().Be(28);
        }

        [Fact(DisplayName = "Should find a word no matter if it is a substring of another word.")]
        public void GivenAText_WhenSearchForAnExactWordCaseInsensitive_ThenShouldReturnMatchEvenIfTheQueryTextIsASubstringOfAWord()
        {
            var results = Asuna.Search("asuna", "asdfggfdsasunaasdfg");

            WhenSearchForAnExactWordCaseInsensitive("asuna", "asdfggfdsasunaasdfg");

            results.Count().Should().Be(1);
            results.First().Index.Should().Be(9);
        }

        #endregion

    }
}