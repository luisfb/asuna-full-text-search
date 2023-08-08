using AsunaLibrary;
using FluentAssertions;

namespace UnitTests
{
    public class TextBufferTests
    {
        [Fact(DisplayName = "Teste de referencia ToString")]
        public void aaa()
        {
            using (ITextBuffer tb = new TextBuffer("String Teste para refêrencia em memória."))
            {

                var str1 = tb.GetText();
                var str2 = tb.GetText();
                var str3 = tb.GetText();

             //  var aa = new String()

                object.ReferenceEquals(str1, str2).Should().BeTrue();
                object.ReferenceEquals(str1, str3).Should().BeTrue();

               // Asuna.FullTextSearch("asuna", tb);

            }
        }
    }
}
