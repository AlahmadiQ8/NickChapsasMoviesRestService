using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieRepository movieRepository) : ControllerBase
{
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await movieRepository.CreateAsync(movie);
        return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var movie = await movieRepository.GetByIdAsync(id);
        if (movie == null)
        {
            return NotFound();
        }

        return Ok(movie.MapToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await movieRepository.GetAllAsync();
        var moviesResponse = movies.MapToResponse();
        return Ok(moviesResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute]Guid id, [FromBody]UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var updated = await movieRepository.UpdateAsync(movie);
        if (!updated)
            return NotFound();
        return Ok(movie.MapToResponse());
    }
    
    
}