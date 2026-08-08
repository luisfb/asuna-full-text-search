using AsunaLocalSearch.Core;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Security;

namespace AsunaLocalSearch
{
    public unsafe class TextBuffer : ITextBuffer
    {
        //private const int FIVE_MEGABYTES = 1024 * 1024 * 5; //5.242.880 bytes (5Mb)
        private const int ONE_MEGABYTE = 1024 * 1024; //1.048.576 bytes (1Mb)

        //private static Encoding _latin1_ISO_8859_1 = Encoding.GetEncoding(28591);

        //TODO: use One megabyte, to cover cases where multiple Text Byffers are used
        //private const int ONE_MEGABYTE = 1024 * 1024;

        //TODO nao aplicar limite. Sempre usar AddMemoryPressure independente da alocação. Devo considerar cenarios com multiplos text buffers que permanecerão vivos por um longo periodo de tempo.
        //Desenvolver possibilidade de usar o text buffer com indexes


        /* 
         The AddMemoryPressure and RemoveMemoryPressure methods improve performance only for types that exclusively depend on finalizers to release the unmanaged resources.
        It's not necessary to use these methods in types that follow the dispose pattern, where finalizers are used to clean up unmanaged resources only in the event that a consumer of the type forgets to call Dispose.
         */

        private IntPtr _mAllocNormalizedTextPtr;
        private int _normalizedTextLength;
        private int _normalizedTextSizeInBytes;
        private byte* _normalizedTextPtr;

        private int TotalAllocatedMemory => _normalizedTextSizeInBytes; // + _originalTextSizeInBytes;
        private string _originalText;
        public string Text => _originalText;

        private bool _disposed;


        public TextBuffer(string text)
        {
            _originalText = text;
            MAllocChars();
        }

        private TextBuffer(ReadOnlySpan<char> text)
        {
            _originalText = text.ToString();
            MAllocChars();
        }

        private void MAllocChars()
        {
            string normalized = Helpers.RemoveDiacritics(_originalText).ToLowerInvariant();

            // The whole design relies on a single byte per character (byte index == char index).
            // Characters not representable in Latin1 (ISO-8859-1) would be silently replaced by '?'
            // by the encoder, which would corrupt every search index. Reject them up front so the
            // failure is loud instead of returning wrong positions later.
            for (int i = 0; i < normalized.Length; i++)
            {
                if (normalized[i] > 0xFF)
                    throw new ArgumentException(
                        $"The text contains a character ('{normalized[i]}', U+{(int)normalized[i]:X4}) at position {i} that cannot be represented as a single byte (Latin1/ISO-8859-1) after normalization. Only single-byte-encodable text is supported.",
                        "text");
            }

            byte[] normalizedText = Helpers.Latin1_ISO_8859_1.GetBytes(normalized);

            _normalizedTextLength = normalizedText.Length;
            _normalizedTextSizeInBytes = Marshal.SizeOf<byte>() * _normalizedTextLength;
            _mAllocNormalizedTextPtr = Marshal.AllocHGlobal(_normalizedTextSizeInBytes);
            Marshal.Copy(normalizedText, 0, _mAllocNormalizedTextPtr, _normalizedTextLength);
            _normalizedTextPtr = (byte*)_mAllocNormalizedTextPtr;

            if (TotalAllocatedMemory > ONE_MEGABYTE)
                GC.AddMemoryPressure(TotalAllocatedMemory);
        }

        public static TextBuffer FromStream(Stream textStream, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var buffer = ArrayPool<char>.Shared.Rent(2048);
            try
            {
                using (var sr = new StreamReader(textStream, encoding))
                {
                    int read = 0;
                    int offset = 0;
                    while ((read = sr.ReadBlock(buffer, offset, buffer.Length - offset)) != 0)
                    {
                        offset += read;

                        if (offset == buffer.Length)
                        {
                            //TODO Create a helper or a custom list based on array pool
                            var newBuffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                            Array.Copy(buffer, newBuffer, buffer.Length);
                            ArrayPool<char>.Shared.Return(buffer);
                            buffer = newBuffer;
                        }
                    }
                    if (offset > 0)
                        return new TextBuffer(buffer.AsSpan(0, offset));

                    throw new EndOfStreamException("The provided stream has no length.");
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }


        /// <summary>
        /// Initializes a new instance of the AsunaLocalSearch.TextBuffer with the content of the specified filePath.
        /// </summary>
        /// <param name="filePath">Path to a text file.</param>
        /// <param name="encoding">The file enconding. If not specified, UTF-8 will be used as enconding to read the file.</param>
        /// <returns>An instance of AsunaLocalSearch.TextBuffer with the content of the specified filePath.</returns>
        /// <exception cref="ArgumentNullException">filePath is null</exception>
        /// <exception cref="ArgumentException">filePath is an empty string (""), contains only white space, or contains one or more invalid characters. -or- path refers to a non-file device such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="NotSupportedException">filePath refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="FileNotFoundException">The filePath cannot be found, such as when mode is FileMode.Truncate or FileMode.Open, and the file specified by path does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified filePath is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="PathTooLongException">The specified filePath, file name, or both exceed the system-defined maximum length.</exception>
        public static TextBuffer FromFile(string filePath, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                return FromStream(fs, encoding);
        }

        /// <summary>
        /// Returns a lower-case version of the text contained in the buffer with all diacritics/accents removed.
        /// </summary>
        /// <returns>A ReadOnlySpan of byte containing a read-only reference to the internal normalized version of the original text.</returns>
        /// <exception cref="ObjectDisposedException">The object is already disposed and therefore not available.</exception>
        public ReadOnlySpan<byte> GetNormalizedText()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return new ReadOnlySpan<byte>(_normalizedTextPtr, _normalizedTextLength);
        }

        public override string ToString()
        {
            return Text ?? string.Empty;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
            _originalText = null;

            if (_mAllocNormalizedTextPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_mAllocNormalizedTextPtr);
                _mAllocNormalizedTextPtr = IntPtr.Zero;
                _normalizedTextPtr = null;
            }

            if (TotalAllocatedMemory > ONE_MEGABYTE)
                GC.RemoveMemoryPressure(TotalAllocatedMemory);
        }

        #endregion

        ~TextBuffer()
        {
            Dispose(false);
        }
    }
}
