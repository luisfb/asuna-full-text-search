namespace AsunaLocalSearch
{
    public struct FullTextSearchResult
    {
        public readonly bool IsExactMatch => Ranking == 0;
        public string Match { get; internal set; }
        public int Index { get; internal set; }
        public int Length { get; internal set; }
        public int Ranking { get; internal set; }

        //For multi-line text:
        public int LineNumber { get; internal set; }
        public int ColumnNumber { get; internal set; }
    }
}
