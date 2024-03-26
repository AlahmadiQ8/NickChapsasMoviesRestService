using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

// ReSharper disable once UnusedType.Global
public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
    }
}