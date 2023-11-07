namespace PS.SharePoint.Core.Entities
{
    public class SharePointConfiguration
    {
        public SharePointConfiguration(string sharePointUrl)
        {
            this.SharePointUrl = sharePointUrl;
        }

        public string SharePointUrl { get; set; }
    }
}
