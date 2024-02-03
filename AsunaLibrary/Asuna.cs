using AsunaLibrary.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsunaLibrary
{
    public static class Asuna
    {
        public static IEnumerable<SearchResult> Search(string queryString, string text, SearchOptions options = SearchOptions.IgnoreCase)
        {
            byte[] queryStringBytes;
            byte[] textBytes;
            if (options == SearchOptions.IgnoreCase)
            {
                queryStringBytes = Helpers.StringToByte(Helpers.RemoveAccent(queryString).ToLowerInvariant());
                textBytes = Helpers.StringToByte(Helpers.RemoveAccent(text).ToLowerInvariant());
            }
            else
            {
                queryStringBytes = Helpers.StringToByte(Helpers.RemoveAccent(queryString));
                textBytes = Helpers.StringToByte(Helpers.RemoveAccent(text));
            }

            //TODO
            //if text.length < x Usar .Contains

            //return null;
            return UnsafeExactSearch.Search(queryStringBytes, textBytes);
        }

        public static ICollection<FullTextSearchResult> FullTextSearch(string queryString, string text)
        {
            string normalizedText = Helpers.RemoveAccent(text).ToLower();
            //string[] strArr = normalizedText.Split(' ', StringSplitOptions.None);

            ReadOnlySpan<char> textSpan = normalizedText.AsSpan();

            //var le = textSpan.Length;

            //ATENÇÃO: APLICAR O AND OU O OR AQUI
            // LEMBRAR QUE NA queryString PODE TER ESPAÇO, OU SEJA, MAIS DE UMA PALAVRA
            ReadOnlySpan<char> querySpan = Helpers.RemoveAccent(queryString).ToLower().AsSpan();

            var wordsPositions = Helpers.GetWordsPositions(textSpan, textSpan.Length);
            //var wordsPositions = Helpers.GetWordsPositions_chatGptVersion(normalizedText);

            var results = new FullTextSearchResult[wordsPositions.Length];
            int i = 0;
            foreach (var item in wordsPositions)
            {
                var distance = UnsafeLevenshtein.Calculate(querySpan, textSpan.Slice(item.Index, item.Length));

                var searchResult = new FullTextSearchResult
                {
                    Index = item.Index,
                    Length = item.Length,
                    Ranking = distance,
                    Match = textSpan.Slice(item.Index, item.Length).ToString(), //Fazer isso sob demanda. Gerar a string para todas as palavras vai reduzir a performance
                    LineNumber = item.Line,
                    ColumnNumber = item.Column
                };
                results[i++] = searchResult;
                //var p = textSpan.Slice(item.Index, item.Length);

            }
            return results.OrderBy(x => x.Ranking).ThenBy(x => x.Index).ToList();
            //return results;

            //SearchResult[] sss = strArr.Select(x => new SearchResult {  })




            //            byte[] textBytes = Helpers.StringToByte(normalizedText);
            /*
            unchecked
            {
                for (int i = 0; i < strArr.Length; i++)
                {
                    var result = UnsafeLevenshtein.Calculate(queryString, strArr[i]);
                }
            }
            return null;*/
        }

        public static IEnumerable<SearchResult> Search(string queryString, ITextBuffer buffer)
        {
            return null;
        }

        public static ICollection<FullTextSearchResult> FullTextSearch(string queryString, ITextBuffer buffer)
        {
            return null;
            //TODO buffer.GetText() must not be used here
            buffer.GetOriginalText
        }
    }

}
