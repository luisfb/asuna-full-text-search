using System.Runtime.InteropServices;

namespace AsunaLibrary
{
    //[StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct Word
    {
        internal char* WordPtr { get; set; }
        internal int Index { get; set; }
        internal int Length { get; set; }
        internal int Line { get; set; }
        internal int Column { get; set; }
        internal int Count { get; set; }
    }
}
