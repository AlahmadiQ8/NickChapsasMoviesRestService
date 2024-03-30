using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         INSERT INTO movies (id, slug, title, year_of_release)
                                                                         values (@Id, @Slug, @Title, @YearOfRelease)
                                                                         """, movie, cancellationToken: token));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                                                                    INSERT INTO genres (movie_id, name)
                                                                    values (@MovieId, @Name)
                                                                    """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
            }
        }
        
        transaction.Commit();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userRating  
                                  FROM movies m 
                                  LEFT JOIN ratings r ON m.id = r.movie_id
                                  LEFT JOIN ratings myr ON m.id = myr.movie_id AND myr.user_id = @userId
                                  WHERE id = @Id
                                  GROUP BY id, userRating
                                  """
            , new { id, userId }, cancellationToken: token)
        );

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  SELECT name FROM genres WHERE movie_id = @id
                                  """, new { id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userRating  
                                  FROM movies m 
                                  LEFT JOIN ratings r ON m.id = r.movie_id
                                  LEFT JOIN ratings myr ON m.id = myr.movie_id AND myr.user_id = @userId
                                  WHERE slug = @slug
                                  GROUP BY id, userRating
                                  """
                , new { slug, userId }, cancellationToken: token)
        );

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  SELECT name FROM genres WHERE movie_id = @id
                                  """, new { id = movie.Id }, cancellationToken: token));
        
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);

        var orderClause = string.Empty;
        if (options.SortField is not null)
        {
            orderClause = $"""
                           , m.{options.SortField}
                           ORDER BY m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                           """;
        }
        
        var result = await connection.QueryAsync(new CommandDefinition($"""
                                                                       SELECT m.*, 
                                                                       string_agg(distinct g.name, ',') as genres,
                                                                       round(avg(r.rating), 1) as rating, 
                                                                       myr.rating as userRating
                                                                       FROM movies m 
                                                                       LEFT JOIN genres g ON m.id = g.movie_id
                                                                       LEFT JOIN ratings r ON m.id = r.movie_id
                                                                       LEFT JOIN ratings myr ON m.id = myr.movie_id 
                                                                                                    AND myr.user_id = @userId
                                                                       WHERE (@title IS NULL OR m.title LIKE ('%' || @title || '%'))
                                                                       AND (@yearOfRelease IS NULL OR m.year_of_release = @yearOfRelease)
                                                                       GROUP BY id, myr.rating
                                                                       {orderClause}
                                                                       LIMIT @pageSize
                                                                       OFFSET @pageOffset
                                                                       """, 
            new { 
                userId = options.UserId, 
                title = options.Title, 
                yearOfRelease = options.YearOfRelease,
                pageSize = options.PageSize,
                pageOffset = (options.Page - 1) * options.PageSize
            }, cancellationToken: token));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.year_of_release,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userRating,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new CommandDefinition("""
                                                            DELETE FROM genres WHERE movie_id = @id
                                                            """, new { id = movie.Id}, cancellationToken: token));
        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                                                                INSERT INTO genres (movie_id, name)
                                                                VALUES (@MovieId, @Name)
                                                                """, new {MovieId = movie.Id, Name = genre}, cancellationToken: token));
        }
        
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         UPDATE movies SET slug = @Slug, title = @Title, year_of_release = @YearOfRelease
                                                                         WHERE id = @Id
                                                                         """, movie, cancellationToken: token));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync(new CommandDefinition("""
                                                            DELETE FROM genres WHERE movie_id = @id
                                                            """, new {id}, cancellationToken: token));
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                            DELETE FROM movies WHERE id = @id
                                                            """, new {id}, cancellationToken: token));
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                                                                         SELECT COUNT(1) FROM movies WHERE id = @id
                                                                         """, new {id}, cancellationToken: token));
    } 

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleAsync<int>(
            new CommandDefinition("""
                                  SELECT COUNT(id) FROM MOVIES
                                  WHERE (@title IS NULL OR title LIKE ('%' || @title || '%'))
                                  AND (@yearOfRelease IS NULL OR year_of_release = @yearOfRelease)
                                  """,
                new { title, yearOfRelease }, cancellationToken: token));
    }
}