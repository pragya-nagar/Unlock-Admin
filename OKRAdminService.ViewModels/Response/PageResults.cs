using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class PageResults<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int HeaderCode { get; set; }
        public List<T> Records { get; set; }
        public IEnumerable<T> Results { get; set; }
    }
}
