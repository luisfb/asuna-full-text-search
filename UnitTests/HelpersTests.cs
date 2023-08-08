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
        public void asdf()
        {
            var result = Helpers.RemoveAccent("çã§123asdfAA!@#$%¨&*()_+12ab");
            var result2 = Helpers.RemoveAccent("_Ѽ۝۞ਐ");

            var result3 = Helpers.RemoveAccent("aa*bb");
            var result4 = Helpers.StringToByte(result3);

            var sadf = Helpers.StringToByte(result2);


            result2.Should().Be("_Ѽ۝۞ਐ");
            //TODO testar com chars unicode arábico e cirilico pra ver se substitui por espaço ou se substitui por string vazia

            result.Should().Be("ca§123asdfAA!@#$%¨&*()_+12ab");

            

        }
    }
}
