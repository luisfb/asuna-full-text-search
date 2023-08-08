using AsunaLibrary.Core;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AsunaLibrary
{
    public unsafe class TextBuffer : ITextBuffer
    {
        private const int _fiveMegaBytes = 1024 * 1024 * 5; //5.242.880 bytes (5Mb)

        private IntPtr _mAllocOriginalTextPtr;
        private int _originalTextLength;
        private int _originalTextSizeInBytes;
        private char* _originalTextPtr;

        private IntPtr _mAllocNormalizedTextPtr;
        private int _normalizedTextLength;
        private int _normalizedTextSizeInBytes;
        private char* _normalizedTextPtr;

        private IntPtr _mAllocWordsPtr;
        private int _wordsLength;
        private int _wordsSizeInBytes;
        private Word* _wordsPtr;
        //private Word* _wordsPtrHead;

        private int TotalAllocatedMemory => _wordsSizeInBytes + _normalizedTextSizeInBytes + _originalTextSizeInBytes;
        private string _text = null;

        private bool _disposed;

        private void MAllocChars(char[] chars)
        {
            _originalTextLength = chars.Length;
            _originalTextSizeInBytes = Marshal.SizeOf<char>() * _originalTextLength;
            _mAllocOriginalTextPtr = Marshal.AllocHGlobal(_originalTextSizeInBytes);
            Marshal.Copy(chars, 0, _mAllocOriginalTextPtr, _originalTextLength);
            _originalTextPtr = (char*)_mAllocOriginalTextPtr;

            char[] normalizedText = Helpers.RemoveAccent(GetText()).ToLowerInvariant().ToCharArray();

            _normalizedTextLength = normalizedText.Length;
            _normalizedTextSizeInBytes = Marshal.SizeOf<char>() * _normalizedTextLength;
            _mAllocNormalizedTextPtr = Marshal.AllocHGlobal(_normalizedTextSizeInBytes);
            Marshal.Copy(normalizedText, 0, _mAllocNormalizedTextPtr, _normalizedTextLength);
            _normalizedTextPtr = (char*)_mAllocNormalizedTextPtr;

            ReadOnlySpan<StringIndexAndLength> wordsPositions = Helpers.GetWordsPositions(_originalTextPtr, _originalTextLength);

            _wordsLength = wordsPositions.Length;
            _wordsSizeInBytes = Marshal.SizeOf<Word>() * _wordsLength;
            _mAllocWordsPtr = Marshal.AllocHGlobal(_wordsSizeInBytes);
            _wordsPtr = (Word*)_mAllocWordsPtr;
            //_wordsPtrHead = _wordsPtr;

            for (int i = 0; i < _wordsLength; i++)
            {
                var w = new Word
                {
                    WordPtr = &_originalTextPtr[wordsPositions[i].Index],
                    Count = i + 1,
                    Index = i,
                    Length = wordsPositions[i].Length,
                    Column = wordsPositions[i].Column,
                    Line = wordsPositions[i].Line
                };
                _wordsPtr[i] = w;
                //_wordsPtr++;
            }
            //_wordsPtr = _wordsPtrHead;

            if (TotalAllocatedMemory > _fiveMegaBytes)
                GC.AddMemoryPressure(TotalAllocatedMemory);
        }

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

        public static TextBuffer FromStream(Stream textStream, Encoding encoding)
        {
            //just in case...
            //textStream.Position = 0;

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

        public static TextBuffer FromFile(string filePath, Encoding encoding)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                return FromStream(fs, encoding);
            //using (var bs = new BufferedStream(fs))
        }

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
            if (_disposed)
                return;

            _disposed = true;

            Marshal.FreeHGlobal(_mAllocOriginalTextPtr);
            Marshal.FreeHGlobal(_mAllocNormalizedTextPtr);
            Marshal.FreeHGlobal(_mAllocWordsPtr);

            _text = null;

            if (TotalAllocatedMemory > _fiveMegaBytes)
                GC.RemoveMemoryPressure(TotalAllocatedMemory);

            GC.SuppressFinalize(this);
        }
    }
}
