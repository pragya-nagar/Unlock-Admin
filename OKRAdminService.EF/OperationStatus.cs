using System;
using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public class OperationStatus : IOperationStatus
    {
        public bool Success { get; set; }
        public int RecordsAffected { get; set; }
        public string Message { get; set; }
        public dynamic Entity { get; set; }
        public dynamic OperationId { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public string InnerMessage { get; set; }
        public string InnerInnerMessage { get; set; }
        public string InnerStackTrace { get; set; }
        public string ValidationErrors { get; set; }
        public List<KeyValuePair<string, string>> BulkErrors { get; set; }
        public IOperationStatus CreateFromException( Exception ex )
        {
            OperationStatus opStatus = new OperationStatus
            {
                Success = false,
                OperationId = null
            };

            if ( ex != null )
            {
                opStatus.Message = ex.Message;
                opStatus.ExceptionMessage = ex.Message;
                opStatus.StackTrace = ex.StackTrace;
                opStatus.InnerMessage = ex.InnerException?.Message;
                opStatus.InnerInnerMessage = ex.InnerException?.InnerException?.Message;
                opStatus.InnerStackTrace = ex.InnerException?.StackTrace;
            }
            return opStatus;
        }
    }

}
