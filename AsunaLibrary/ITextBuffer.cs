using System;

namespace AsunaLocalSearch
{
    public interface ITextBuffer : IDisposable
    {
        /// <summary>
        /// Returns the original text provided at the creation of the TextBuffer
        /// </summary>
        /// <returns>The original string provided at the creation of the TextBuffer</returns>
        string Text { get; }

        /// <summary>
        /// Returns a lower-case version of the text contained in the buffer with all diacritics/accents removed.
        /// </summary>
        /// <returns>A ReadOnlySpan of byte containing a read-only reference to the internal normalized version of the original text.</returns>
        /// <exception cref="ObjectDisposedException">The object is already disposed and therefore not available.</exception>
        ReadOnlySpan<byte> GetNormalizedText();
    }
}