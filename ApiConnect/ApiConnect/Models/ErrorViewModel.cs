using System;
using Microsoft.Office365.OutlookServices;

namespace ApiConnect.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);


    }
}