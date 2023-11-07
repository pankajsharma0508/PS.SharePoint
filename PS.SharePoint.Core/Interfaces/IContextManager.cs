using Microsoft.SharePoint.Client;
using PS.SharePoint.Core.Entities;
using System;

namespace PS.SharePoint.Core.Interfaces
{
    public interface IContextManager
    {
        SharePointConfiguration Configuration { get; set; }
        void ExecuteQuery(ClientContext ctx, string message);
        void ExecuteQuery(string message, Action<ClientContext> action);
        void CheckInDocument(ClientContext clientContext, File file);
    }
}
