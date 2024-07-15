using AsunaLibrary.Core;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace AsunaLibrary
{
    public unsafe class TextBuffer : ITextBuffer
    {
        private const int FIVE_MEGABYTES = 1024 * 1024 * 5; //5.242.880 bytes (5Mb)

        //TODO remove
        private IntPtr _mAllocOriginalTextPtr;
        private int _originalTextLength;
        private int _originalTextSizeInBytes;
        private char* _originalTextPtr;

        private IntPtr _mAllocNormalizedTextPtr;
        private int _normalizedTextLength;
        private int _normalizedTextSizeInBytes;
        private char* _normalizedTextPtr;

        private int TotalAllocatedMemory => _normalizedTextSizeInBytes + _originalTextSizeInBytes;
        private string _text = null;

        private bool _disposed;

        public TextBuffer(char[] text)
        {
            MAllocChars(text);
        }

        public TextBuffer(ReadOnlySpan<char> text)
        {
            MAllocChars(text.ToArray());
        }

        public TextBuffer(string text)
        {
            MAllocChars(text.ToCharArray());
        }

        private void MAllocChars(char[] chars)
        {
            var latin1_ISO_8859_1 = Encoding.GetEncoding(28591);

            var bytes = latin1_ISO_8859_1.GetBytes(chars);

            _originalTextLength = chars.Length;
            _originalTextSizeInBytes = sizeof(char) * _originalTextLength;
            //Several days were lost here trying to figure out why the app was crashing here.
            //The reason: Marshal.SizeOf<char>() is returning 1, which is wrong. Should return 2.
            //Instead, I am now using sizeof(char)

            _mAllocOriginalTextPtr = Marshal.AllocHGlobal(_originalTextSizeInBytes);
            Marshal.Copy(chars, 0, _mAllocOriginalTextPtr, _originalTextLength);
            _originalTextPtr = (char*)_mAllocOriginalTextPtr;

            //TODO: change the way I store the original text. I must preserve the original encoding. GetText() should not be called.
            char[] normalizedText = Helpers.RemoveDiacritics(GetText()).ToLowerInvariant().ToCharArray();

            _normalizedTextLength = normalizedText.Length;
            _normalizedTextSizeInBytes = sizeof(char) * _normalizedTextLength;
            _mAllocNormalizedTextPtr = Marshal.AllocHGlobal(_normalizedTextSizeInBytes);
            Marshal.Copy(normalizedText, 0, _mAllocNormalizedTextPtr, _normalizedTextLength);
            _normalizedTextPtr = (char*)_mAllocNormalizedTextPtr;

            if (TotalAllocatedMemory > FIVE_MEGABYTES)
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

                    //throw exception
                    return null;
                    
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        public static TextBuffer FromFile(string filePath, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                return FromStream(fs, encoding);
        }

        [Obsolete]
        public string GetText()
        {
            return _text ??= new string(_originalTextPtr, 0, _originalTextLength);
        }

        public ReadOnlySpan<char> GetNormalizedText()
        {
            return new ReadOnlySpan<char>(_normalizedTextPtr, _normalizedTextLength);
        }

        public ReadOnlySpan<char> GetOriginalText()
        {
            return new ReadOnlySpan<char>(_originalTextPtr, _originalTextLength);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
            _text = null;

            if (_mAllocOriginalTextPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(_mAllocOriginalTextPtr);

            if (_mAllocNormalizedTextPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(_mAllocNormalizedTextPtr);

            if (TotalAllocatedMemory > FIVE_MEGABYTES)
                GC.RemoveMemoryPressure(TotalAllocatedMemory);

            if(disposing)
                GC.SuppressFinalize(this);
        }

        ~TextBuffer()
        {
            Dispose(false);
        }
    }
}
