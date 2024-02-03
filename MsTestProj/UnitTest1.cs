using AsunaLibrary;
using FluentAssertions;

namespace UnitTests
{
    [TestClass]
    public class TextBufferTests
    {
        #region ARRANGE

        private TextBuffer GivenATextBufferFromFile()
        {
            //return TextBuffer.FromFile(FixtureHelper.GetLoremIpsumTextFilePath());
            return new TextBuffer("asdgfasdf asdf asd fas d");
        }

        private TextBuffer GivenATextBuffer()
        {
            //return new TextBuffer(FixtureHelper.GetLoremIpsumTxtContent());
            return new TextBuffer("asdgfasdf asdf asd fas d");
        }

        #endregion

        #region ACT

        private string WhenGetTextIsCalled(ITextBuffer textBuffer)
        {
            return textBuffer.GetText();
        }

        #endregion

        #region ASSERT

        #endregion

        //[Fact(DisplayName = "Should Return The Same String when compared to the original text file.")]
        [TestMethod]
        public void GivenATextBufferFromFile_WhenGetTextIsCalled_ThenShouldReturnAStringEqualsToTheOriginal()
        {
            string originalText = FixtureHelper.GetLoremIpsumTxtContent();
            ITextBuffer tb = GivenATextBufferFromFile();
            
                string textFromBuffer = WhenGetTextIsCalled(tb);

                object.ReferenceEquals(originalText, textFromBuffer).Should().BeFalse();
                (originalText == textFromBuffer).Should().BeTrue();
            
        }

        //[Fact(DisplayName = "Should Always Return The Same String Object.")]
        [TestMethod]
        public void GivenATextBuffer_WhenGetTextIsCalled_ThenShouldReturnAlwaysTheSameStringObject()
        {
            ITextBuffer tb = GivenATextBuffer();
        
                var str1 = WhenGetTextIsCalled(tb);
                var str2 = WhenGetTextIsCalled(tb);
                var str3 = WhenGetTextIsCalled(tb);


                object.ReferenceEquals(str1, str2).Should().BeTrue();
                object.ReferenceEquals(str1, str3).Should().BeTrue();
            
        }
    }
}
