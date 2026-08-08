using AsunaLocalSearch;
using FluentAssertions;
using System.Text;

namespace UnitTests
{
    public class TextBufferTests
    {
        #region ARRANGE

        private TextBuffer GivenATextBufferFromFile()
        {
            return TextBuffer.FromFile(FixtureHelper.GetLoremIpsumTextFilePath());
        }

        private TextBuffer GivenATextBuffer()
        {
            return new TextBuffer(FixtureHelper.GetLoremIpsumTxtContent());
        }

        private static string GivenATempFileWith(string content, Encoding encoding)
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllText(path, content, encoding);
            return path;
        }

        #endregion

        #region ACT

        private string WhenGetTextIsCalled(ITextBuffer textBuffer)
        {
            return textBuffer.Text;
        }

        #endregion

        #region ASSERT

        #endregion

        [Fact(DisplayName = "Should Return The Same String when compared to the original text file.")]
        public void GivenATextBufferFromFile_WhenGetTextIsCalled_ThenShouldReturnAStringEqualsToTheOriginal()
        {
            string originalText = FixtureHelper.GetLoremIpsumTxtContent();
            ITextBuffer tb = GivenATextBufferFromFile();
            string textFromBuffer = WhenGetTextIsCalled(tb);

            //Different reference:
            object.ReferenceEquals(originalText, textFromBuffer).Should().BeFalse();
            //Same text:
            (originalText == textFromBuffer).Should().BeTrue();

        }

        [Fact(DisplayName = "Should Always Return The Same String Object.")]
        public void GivenATextBuffer_WhenGetTextIsCalled_ThenShouldReturnAlwaysTheSameStringObject()
        {
            ITextBuffer tb = GivenATextBuffer();

            var str1 = WhenGetTextIsCalled(tb);
            var str2 = WhenGetTextIsCalled(tb);
            var str3 = WhenGetTextIsCalled(tb);

            object.ReferenceEquals(str1, str2).Should().BeTrue();
            object.ReferenceEquals(str1, str3).Should().BeTrue();

        }

        #region FromFile / FromStream encoding

        [Fact(DisplayName = "FromFile should honor the provided encoding and round-trip UTF-8 accented text.")]
        public void GivenAnUtf8FileWithAccents_WhenLoadedWithUtf8Encoding_ThenTextMatchesTheOriginal()
        {
            const string original = "Ola, acao e coracao! Cafe com pao. Acentuacao removida: ção é ã.";
            string path = GivenATempFileWith(original, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            try
            {
                using ITextBuffer tb = TextBuffer.FromFile(path, Encoding.UTF8);
                tb.Text.Should().Be(original);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact(DisplayName = "FromFile should honor a non-default encoding: reading UTF-8 bytes as Latin1 mis-decodes the text.")]
        public void GivenAnUtf8FileWithAccents_WhenLoadedWithLatin1Encoding_ThenTextDiffersFromOriginal()
        {
            // Each accented char is multi-byte in UTF-8; decoding those bytes as Latin1 (one byte
            // per char) produces mojibake. This proves the encoding argument is actually applied.
            const string original = "acao";
            string path = GivenATempFileWith("ação", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            //string path = GivenATempFileWith("ação", Encoding.UTF8);
            try
            {
                using ITextBuffer tb = TextBuffer.FromFile(path, Encoding.GetEncoding(28591));
                tb.Text.Should().NotBe("ação");
                tb.Text.Should().NotBe(original);
            }
            finally
            {
                File.Delete(path);
            }
        }

        #endregion

        #region Single-byte invariant validation

        [Theory(DisplayName = "Constructor should reject text that is not single-byte-encodable after normalization.")]
        [InlineData("Hello 日本語")]
        [InlineData("price is 10€")]
        [InlineData("emoji 😀 here")]
        [InlineData("Cyrillic Привет")]
        public void GivenTextWithNonLatin1Characters_WhenConstructingTextBuffer_ThenShouldThrowArgumentException(string text)
        {
            Action act = () => new TextBuffer(text);
            act.Should().Throw<ArgumentException>();
        }

        [Theory(DisplayName = "Constructor should accept text that is single-byte-encodable after normalization.")]
        [InlineData("Plain ASCII text")]
        [InlineData("Acentuação é removida pela normalização")]
        [InlineData("Latin1 punctuation: copyright ©, pound £, section §")]
        public void GivenSingleByteEncodableText_WhenConstructingTextBuffer_ThenShouldNotThrow(string text)
        {
            Action act = () =>
            {
                using var tb = new TextBuffer(text);
            };
            act.Should().NotThrow();
        }

        #endregion

        #region Dispose

        [Fact(DisplayName = "GetNormalizedText should throw ObjectDisposedException after dispose.")]
        public void GivenADisposedTextBuffer_WhenGetNormalizedTextIsCalled_ThenShouldThrowObjectDisposedException()
        {
            var tb = GivenATextBuffer();
            tb.Dispose();

            Action act = () => tb.GetNormalizedText();

            act.Should().Throw<ObjectDisposedException>();
        }

        [Fact(DisplayName = "Dispose should be idempotent (safe to call multiple times, no double-free).")]
        public void GivenATextBuffer_WhenDisposedMultipleTimes_ThenShouldNotThrow()
        {
            var tb = GivenATextBuffer();

            Action act = () =>
            {
                tb.Dispose();
                tb.Dispose();
                tb.Dispose();
            };

            act.Should().NotThrow();
        }

        #endregion
    }
}
