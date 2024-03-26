using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository(IDbConnectionFactory dbConnectionFactory) : IRatingRepository
{
    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
           INSERT INTO ratings (user_id, movie_id, rating) 
           VALUES (@userId, @movieId, @rating)
           ON CONFLICT (user_id, movie_id) DO UPDATE 
           SET rating = @rating
        """, new {userId, movieId, rating}));

        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<float>(new CommandDefinition(
            """
                       SELECT round(avg(r.rating), 1) from ratings r 
                       WHERE movie_id = @movieId
                       """, 
            new {movieId}, 
            cancellationToken: token
        ));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<(float? Rating, int? UserRating)>(new CommandDefinition(
            """
            SELECT round(avg(r.rating), 1) from ratings r,
                                                (SELECT rating from ratings where movie_id = @movieId AND user_id = @userId LIMIT 1)
            WHERE movie_id = @movieId
            """, 
            new {movieId, userId}, 
            cancellationToken: token
        ));
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                              DELETE FROM ratings
                                                                              WHERE movie_id = @movieId
                                                                              AND user_id = @userId
                                                                         """, new {movieId, userId}));
        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QueryAsync<MovieRating>(new CommandDefinition("""
                                                                              SELECT r.rating, m.slug, m.id as movieId 
                                                                              FROM ratings r 
                                                                              INNER JOIN movies m on m.id = r.movie_id
                                                                              WHERE user_id = @userId 
                                                                              """, new {userId}));
    }
}