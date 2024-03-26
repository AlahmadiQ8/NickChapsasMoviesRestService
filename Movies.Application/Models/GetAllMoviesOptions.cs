namespace Movies.Application.Models;

public class GetAllMoviesOptions
{
    public string? Title { get; init; }
    public int? YearOfRelease { get; init; }
    public Guid? UserId { get; set; }
}