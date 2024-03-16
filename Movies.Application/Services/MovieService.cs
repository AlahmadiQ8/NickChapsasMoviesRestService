using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;

namespace Movies.Application.Services;

public class MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator) : IMovieService
{
    public async Task<bool> CreateAsync(Movie movie)
    {
        await movieValidator.ValidateAndThrowAsync(movie); 
        return await movieRepository.CreateAsync(movie);
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        return movieRepository.GetByIdAsync(id);
    }

    public Task<Movie?> GetBySlugAsync(string slug)
    {
        return movieRepository.GetBySlugAsync(slug);
    }

    public Task<IEnumerable<Movie>> GetAllAsync()
    {
        return movieRepository.GetAllAsync();
    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        await movieValidator.ValidateAndThrowAsync(movie);
        var movieExists = await movieRepository.ExistsByIdAsync(movie.Id);
        if (!movieExists)
        {
            return null;
        }

        await movieRepository.UpdateAsync(movie);
        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        return movieRepository.DeleteByIdAsync(id);
    }
}