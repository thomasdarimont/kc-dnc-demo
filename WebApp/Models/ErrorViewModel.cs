using System;

namespace WebApp.Models
{
    public class ErrorViewModel
    {
        
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        
        public string ErrorMessage { get; set; }
        
        public string RequestPath { get; set; }
    }
}