namespace YTU_test.Models.Requests
{
    public class WeatherForecastQueryParameters
    {
        // Sayfalama (Pagination)
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        // Sıralama (Sorting)
        public string? SortBy { get; set; } 
        public string? SortOrder { get; set; } = "asc";

        // Filtreleme (Dynamic Filtering)
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int? MinTemperatureC { get; set; }
        public int? MaxTemperatureC { get; set; }
        public string? SummaryContains { get; set; }
    }
}