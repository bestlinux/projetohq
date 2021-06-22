namespace ProjetoHQApi.Application.Parameters
{
    public class PagingParameter
    {
        private const int maxPageSize = 50000;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 100000;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}