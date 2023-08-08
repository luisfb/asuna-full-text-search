using System;
using System.Collections.Generic;
using System.Text;

namespace AsunaLibrary.Core
{
    internal static unsafe class UnsafeExactSearch
    {
        //TODO: NEW BENCHMARK:

        internal static List<SearchResult> Search(byte[] word, byte[] text)
        {
            int contentLength = text.Length;
            int wordLength = word.Length;

            int currentLine = 1;
            const byte newLine = 10;
            
            //TODO: initialize this list with a predefined value (Count spaces in text, or do some calc based on text.Length???)
            //TODO Use array pool instead of list
            var results = new List<SearchResult>();

            fixed (byte* palavra = word, content = text)
            {
                int charMatchCount = 0;
                int wordIndex = -1;
                byte* pPtr = palavra;
                byte* cPtr = content;

                for (int i = 0; i <= contentLength; i++)
                {
                    if (charMatchCount >= wordLength)
                    {
                        wordIndex = i - charMatchCount;
                        results.Add(new SearchResult
                        {
                            Index = wordIndex,
                            LineNumber = currentLine
                        });

                        if (i == contentLength)
                            break;

                        charMatchCount = 0;
                        wordIndex = -1;
                        pPtr = palavra;
                        // break;
                    }

                    if (*pPtr == *cPtr)
                    {
                        charMatchCount++;
                        pPtr++;
                    }
                    else
                    {
                        charMatchCount = 0;
                        wordIndex = -1;
                        pPtr = palavra;
                    }

                    if (*cPtr == newLine) //TODO: create a unit test to check if app will crash if theres a match with the last word
                        currentLine++;

                    cPtr++;
                }
                
              /*  if (wordIndex > -1)
                {
                    // Word found
                    a.Add(new SearchResult
                    {
                        Index = wordIndex,
                        LineNumber  = currentLine
                    });
                }*/
            }
            return results;
        }
    }
}
