namespace OKRAdminService.EF
{
    public class Paging
    {

        /// <summary>
        /// Paging
        /// </summary>

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        public int PageIndex { get; set; }
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize { get; set; }
        /// <summary>
        /// Gets or sets the sort column.
        /// </summary>
        /// <value>
        /// The sort column.
        /// </value>
        public string SortColumn { get; set; }
        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public string SortOrder { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is sorted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is sorted; otherwise, <c>false</c>.
        /// </value>
        public bool IsSorted { get; set; }
        /// <summary>
        /// Gets or sets the total record.
        /// </summary>
        /// <value>
        /// The total record.
        /// </value>
        public int TotalRecord { get; set; }
    }
}

