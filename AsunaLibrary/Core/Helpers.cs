using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AsunaLibrary.Core
{
    //TODO Create unit tests for each method of this class
    internal static class Helpers
    {
        private static Encoding _latin1_ISO_8859_1 = Encoding.GetEncoding(28591);

        internal static byte[] StringToByte(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return _latin1_ISO_8859_1.GetBytes(text);
        }

        //TODO: Implement a better way, a more performatic algorithm, to remove diacritics/accents:
        internal static string RemoveDiacritics(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        internal static string ConvertToLatin1(string text)
        {
            var latin1_text = _latin1_ISO_8859_1.GetBytes(text);
            return _latin1_ISO_8859_1.GetString(latin1_text);
        }

        internal static unsafe ReadOnlySpan<StringIndexAndLength> GetWordsPositions(ReadOnlySpan<char> str, int length)
        {
            fixed (char* c = str)
                return GetWordsPositions(c, length);
        }
        internal static unsafe ReadOnlySpan<StringIndexAndLength> GetWordsPositions(char* c, int length)
        {
            const char space = ' ';
            const char newLine = '\n';
            const char carriageReturn = '\r';
            bool containsCarriageReturn = false;

            int currentWordIndex = -1;
            int currentLine = 1;
            int column = 0;

            var arr = ArrayPool<StringIndexAndLength>.Shared.Rent(length);
            try
            {
                int wordCount = 0;
                for (int i = 0; i < length; i++)
                {
                    if (c[i] == carriageReturn)
                    {
                        containsCarriageReturn = true;
                        continue;
                    }

                    column++;

                    if (c[i] == newLine)
                    {
                        if (currentWordIndex > -1)
                        {
                            int wordLength = i - currentWordIndex;

                            if (containsCarriageReturn)
                                wordLength--;

                            arr[wordCount++] = new StringIndexAndLength
                            {
                                Index = currentWordIndex,
                                Length = wordLength,
                                Line = currentLine,
                                Column = column - wordLength
                            };
                            currentWordIndex = -1;
                        }
                        column = 0;
                        currentLine++;
                        continue;
                    }

                    if (c[i] == space)
                    {
                        if (currentWordIndex > -1)
                        {
                            var wordLength = i - currentWordIndex;

                            arr[wordCount++] = new StringIndexAndLength
                            {
                                Index = currentWordIndex,
                                Length = wordLength,
                                Line = currentLine,
                                Column = column - wordLength
                            };
                            currentWordIndex = -1;
                        }
                    }
                    else
                    {
                        if (currentWordIndex == -1)
                            currentWordIndex = i;
                    }
                }

                if (currentWordIndex > -1)
                {
                    var wordLength = length - currentWordIndex;
                    arr[wordCount++] = new StringIndexAndLength
                    {
                        Index = currentWordIndex,
                        Length = wordLength,
                        Line = currentLine,
                        Column = column - wordLength + 1
                    };
                }
                return arr.AsSpan(0, wordCount);
            }
            finally
            {
                ArrayPool<StringIndexAndLength>.Shared.Return(arr);
            }
        }
    }
}
