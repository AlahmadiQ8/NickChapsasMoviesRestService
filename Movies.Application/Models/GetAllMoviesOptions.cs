namespace Movies.Application.Models;

public class GetAllMoviesOptions
{
    public string? Title { get; init; }
    public int? YearOfRelease { get; init; }
    public Guid? UserId { get; set; }
    public string? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}