using System;

namespace OKRAdminService.WebCore.Filters
{
    [Serializable]
    public sealed class DataNotFoundException : Exception
    {
        public DataNotFoundException(string message) : base(message)
        {
        }
    }
}
