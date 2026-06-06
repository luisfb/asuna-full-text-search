using System;

namespace AsunaLocalSearch.Core
{
    internal static unsafe class UnsafeLevenshtein
    {
        internal static int Calculate(ReadOnlySpan<byte> source, ReadOnlySpan<byte> target)
        {
            int sourceLength = source.Length;
            int targetLength = target.Length;

            if (sourceLength == 0)
                return targetLength;
            if (targetLength == 0)
                return sourceLength;

            int* distances = stackalloc int[sourceLength + 1];
            int* tempDistances = stackalloc int[sourceLength + 1];

            for (int i = 0; i <= sourceLength; i++)
                distances[i] = i;

            fixed (byte* targetPtr = target, sourcePtr = source)
            {
                byte* sourceChars = sourcePtr;
                byte* targetChars = targetPtr;

                for (int j = 1; j <= targetLength; j++)
                {
                    tempDistances[0] = j;

                    for (int i = 1; i <= sourceLength; i++)
                    {
                        int insertDistance = tempDistances[i - 1] + 1;
                        int deleteDistance = distances[i] + 1;
                        int replaceDistance = distances[i - 1] + ((*sourceChars == *targetChars) ? 0 : 1);

                        tempDistances[i] = Math.Min(Math.Min(insertDistance, deleteDistance), replaceDistance);

                        sourceChars++;
                    }

                    sourceChars = sourcePtr;
                    targetChars++;
                    int* swap = distances;
                    distances = tempDistances;
                    tempDistances = swap;
                }
            }

            return distances[sourceLength];
        }

        internal static int Calculate(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
        {
            int sourceLength = source.Length;
            int targetLength = target.Length;

            if (sourceLength == 0)
                return targetLength;
            if (targetLength == 0)
                return sourceLength;

            int* distances = stackalloc int[sourceLength + 1];
            int* tempDistances = stackalloc int[sourceLength + 1];

            for (int i = 0; i <= sourceLength; i++)
                distances[i] = i;

            fixed (char* targetPtr = target, sourcePtr = source)
            {
                char* sourceChars = sourcePtr;
                char* targetChars = targetPtr;

                for (int j = 1; j <= targetLength; j++)
                {
                    tempDistances[0] = j;

                    for (int i = 1; i <= sourceLength; i++)
                    {
                        int insertDistance = tempDistances[i - 1] + 1;
                        int deleteDistance = distances[i] + 1;
                        int replaceDistance = distances[i - 1] + ((*sourceChars == *targetChars) ? 0 : 1);

                        tempDistances[i] = Math.Min(Math.Min(insertDistance, deleteDistance), replaceDistance);

                        sourceChars++;
                    }

                    sourceChars = sourcePtr;
                    targetChars++;
                    int* swap = distances;
                    distances = tempDistances;
                    tempDistances = swap;
                }
            }

            return distances[sourceLength];
        }
    }
}
