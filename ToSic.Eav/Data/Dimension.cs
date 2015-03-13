namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Dimension Assignment
    /// </summary>
    internal class Dimension : IDimension, ILanguage
    {
        public int DimensionId { get; set; }
        public bool ReadOnly { get; set; }
        public string Key { get; set; }
    }
}