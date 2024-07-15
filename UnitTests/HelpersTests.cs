using AsunaLibrary.Core;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class HelpersTests
    {

        [Fact]
        public void Should_Remove_Diacritics()
        {
            var result = Helpers.RemoveDiacritics("çã§123asdfAA!@#$%¨&*()_+12ab");

            var result2 = Helpers.RemoveDiacritics("_Ѽ۝۞ਐ");

            var result3 = Helpers.RemoveDiacritics("aa*bb");
            var result4 = Helpers.StringToByte(result3);

            var sadf = Helpers.StringToByte(result2);

            result2.Should().Be("_Ѽ۝۞ਐ");
            //TODO testar com chars unicode arábico e cirilico pra ver se substitui por espaço ou se substitui por string vazia

            result.Should().Be("ca§123asdfAA!@#$%¨&*()_+12ab");

        }

        [Fact]
        public void Should_Convert_UTF8_Multi_Byte_String_To_Single_Byte_String()
        {
            string oneBytePerChar = "asdfg1234";
            string twoBytesPerChar = "éçñâöô";
            string threeBytesPerChar = "अ中∑";
            string fourBytesPerChar = "𐍈𤭢";

            var utf8_oneBytePerChar = Encoding.UTF8.GetBytes(oneBytePerChar);
            utf8_oneBytePerChar.Count().Should().Be(9);

            var utf8_twoBytesPerChar = Encoding.UTF8.GetBytes(twoBytesPerChar);
            utf8_twoBytesPerChar.Count().Should().Be(12);

            var utf8_threeBytesPerChar = Encoding.UTF8.GetBytes(threeBytesPerChar);
            utf8_threeBytesPerChar.Count().Should().Be(9);

            var utf8_fourBytesPerChar = Encoding.UTF8.GetBytes(fourBytesPerChar);
            utf8_fourBytesPerChar.Count().Should().Be(8);

            var stringOne = Helpers.ConvertToLatin1(oneBytePerChar);

            var stringTwo = Helpers.ConvertToLatin1(twoBytesPerChar);

            var stringThree = Helpers.ConvertToLatin1(threeBytesPerChar);

            var stringFour = Helpers.ConvertToLatin1(fourBytesPerChar);

            stringOne.Should().Be("asdfg1234");
            stringTwo.Should().Be("éçñâöô");
            stringThree.Should().Be("???");
            stringFour.Should().Be("????");
        }

    }
}
