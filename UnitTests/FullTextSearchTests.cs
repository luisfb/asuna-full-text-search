using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsunaLocalSearch;
using FluentAssertions;
using System.Diagnostics;


namespace UnitTests
{
    public class FullTextSearchTests
    {
        #region ARRANGE

        private string GivenATextFile()
        {
            return FixtureHelper.GetLoremIpsumTxtContent(Encoding.UTF8);
        }

        #endregion

        #region ACT

        private List<FullTextSearchResult> WhenExecuteAFullTextSearch(string wordToSearch, string targetText)
        {
            return Asuna.FullTextSearch(wordToSearch, targetText).ToList();
        }

        #endregion

        #region ASSERT

        [Fact(DisplayName = "Should return the right column and line numbers for all results of a full-text search.")]
        public void GivenATextFile_WhenExecuteAFullTextSearch_ThenShouldReturnTheRightColumnAndLineForEachResult()
        {
            string fileContent = GivenATextFile();
            var results = WhenExecuteAFullTextSearch("asuna", fileContent);

            var word1 = results.Where(x => x.LineNumber == 11 && x.ColumnNumber == 121).FirstOrDefault();
            word1.Match.Should().Be("tincidunt");

            var word2 = results.Where(x => x.LineNumber == 3 && x.ColumnNumber == 105).FirstOrDefault();
            word2.Match.Should().Be("asuna");

            var word3 = results.Where(x => x.LineNumber == 8 && x.ColumnNumber == 1).FirstOrDefault();
            word3.Match.Should().Be("nunc");

            var word4 = results.Where(x => x.LineNumber == 22 && x.ColumnNumber == 5).FirstOrDefault();
            word4.Match.Should().Be("line");

            var word5 = results.Where(x => x.LineNumber == 30 && x.ColumnNumber == 138).FirstOrDefault();
            word5.Match.Should().Be("convallis");

            var word6 = results.Where(x => x.LineNumber == 30 && x.ColumnNumber == 148).FirstOrDefault();
            word6.Match.Should().Be("a.");

            results.Where(x => x.Match == "asuna").Count().Should().Be(4);
        }

        #endregion
    }
}
