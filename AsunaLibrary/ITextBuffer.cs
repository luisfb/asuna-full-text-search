using System;

namespace AsunaLocalSearch
{
    public interface ITextBuffer : IDisposable
    {
        /// <summary>
        /// Returns the original text provided at the creation of the TextBuffer
        /// </summary>
        /// <returns></returns>
        string GetText();

        /// <summary>
        /// Returns a lower-case version of the text contained in the buffer with all accents removed.
        /// </summary>
        /// <returns></returns>
        ReadOnlySpan<char> GetNormalizedText();

        /// <summary>
        /// Returns the original text provided at the creation of the TextBuffer
        /// </summary>
        /// <returns></returns>
        ReadOnlySpan<char> GetOriginalText();
    }
}