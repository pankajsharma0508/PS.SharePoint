using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Entities
{
    public class SpException : Exception
    {
        public SpException(string message, string sharePointCorrelationId, string sharepointUrl, Exception ex) : base(message, ex)
        {
            Data.Add("SharepointUrl", sharepointUrl);
            Data.Add("SharePointCorrelationId", sharePointCorrelationId);
        }
    }
}
