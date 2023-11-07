namespace PS.SharePoint.Core.Attributes
{
    /// <summary>
    /// Sets up correspondence to SP Taxonomy selector column.
    /// The member is assumed to have string or enum type (for enums, see below)
    /// </summary>
    public class SpTaxonomyColumnAttribute : SpColumnAttribute
    {
        public bool Multiselect { get; set; }
        public string TermSet { get; set; }

        public SpTaxonomyColumnAttribute(string name, string termSet, bool multiselect) : base(name)
        {
            TermSet = termSet;
            Multiselect = multiselect;
        }
    }
}
