using Dapper;

namespace Movies.Application.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFactory)
{
    public async Task InitializeAsync()
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS movies (
                                          id UUID PRIMARY KEY,
                                          slug TEXT NOT NULL,
                                          title TEXT NOT NULL,
                                          year_of_release INTEGER NOT NULL);
                                      """);

        await connection.ExecuteAsync("""
                                      CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_idx
                                      ON MOVIES
                                      USING BTREE(slug);
                                      """);

        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS genres(
                                          movie_id UUID references movies (id),
                                          name TEXT NOT NULL
                                      );
                                      """);
    }
}