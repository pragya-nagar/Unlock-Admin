using System.Collections.Generic;

namespace OKRAdminService.ViewModels
{
    public class PageResult<T>
    {
        public PageResult() {
            PaggingInfo = new PageInfo();
        }
        public List<T> Records { get; set; }

        public PageInfo PaggingInfo { get; set; }

    }

    public class PageInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
