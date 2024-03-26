using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

// ReSharper disable once UnusedType.Global
public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly HashSet<string> AcceptableSortFields = new()
    {
        "title",
        "year"
    };
    
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage(x => $"You can only sort by 'title' or 'year'. but not {x.SortField}");
    }
}