namespace AniCard.Models.DTOs
{
    public class CharacterFilter
    {
        public string? Name { get; set; }
        public List<string>? Tags { get; set; }
        public int? Sex { get; set; }
        public int? Personality { get; set; }
        public string? UserName { get; set; }
    }

    public enum OrderByField
    {
        Downloads,
        Date
    }

    public enum SortOrder
    {
        Asc,
        Desc
    }

    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }

    public class CharactersQueryParams
    {
        public CharacterFilter Filter { get; set; } = new();
        public OrderByField OrderBy { get; set; } = OrderByField.Downloads;
        public SortOrder Sort { get; set; } = SortOrder.Desc;
        public PaginationParams Pagination { get; set; } = new();
    }
}
