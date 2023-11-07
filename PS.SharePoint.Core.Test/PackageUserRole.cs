using PS.SharePoint.Core.Attributes;
using PS.SharePoint.Core.Entities;

namespace PS.SharePoint.Core.Test
{
    [SpList("Users")]
    public class PackageUserRole : BaseListItem
    {
        [SpColumn("Title")] // Single Line text
        public string Title { get; set; }

        [SpColumn("Description")] // Single Line text
        public string Description { get; set; }

        [SpColumn("Age")] // Number
        public int Age { get; set; }

        [SpColumn("IsEnrolled")] // Yes/No 
        public bool IsEnrolled { get; set; }

        //[SpColumnUser("Name")] // User/Person 
        public SpUser Name { get; set; }

        [SpColumn("BirthDate")] // Date  
        public DateTime BirthDate { get; set; }

        //[SpTaxonomyColumn("State", "State", false)] // Date  
        //public string State { get; set; }

        public override string ToString()
        {
            return $" Record {Id} | {Title} | {Age} | {IsEnrolled} | {Name} | {BirthDate} | {Description}";
        }
    }
}
