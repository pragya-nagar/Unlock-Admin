using System;
using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public interface IOperationStatus
    {
        bool Success { get; set; }
        int RecordsAffected { get; set; }
        string Message { get; set; }
        dynamic Entity { get; set; }
        dynamic OperationId { get; set; }
        string ExceptionMessage { get; set; }
        string StackTrace { get; set; }
        string InnerStackTrace { get; set; }
        string InnerMessage { get; set; }
        string InnerInnerMessage { get; set; }
        string ValidationErrors { get; set; }
        List<KeyValuePair<string, string>> BulkErrors { get; set; }
        IOperationStatus CreateFromException( Exception ex );
    }
}
