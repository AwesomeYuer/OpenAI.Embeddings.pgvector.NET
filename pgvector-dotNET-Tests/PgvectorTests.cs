using Npgsql;
using Pgvector.Npgsql;
using Pgvector;

namespace PgVectors.NET;

public class Example
{
    //[Fact]
    public async Task Main()
    {
        var connString = "Host=localhost;Database=pgvectors;User ID=sa;Password=password01!";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        dataSourceBuilder.UseVector();
        await using var dataSource = dataSourceBuilder.Build();

        var conn = dataSource.OpenConnection();

        await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        conn.ReloadTypes();

        await using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS items", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("CREATE TABLE items (embedding vector(3))", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }




        await using (var cmd = new NpgsqlCommand("INSERT INTO items (embedding) VALUES ($1), ($2), ($3)", conn))
        {
            var embedding1 = new Vector(new float[] { 1, 1, 1 });
            var embedding2 = new Vector(new float[] { 2, 2, 2 });
            var embedding3 = new Vector(new float[] { 1, 1, 2 });
            cmd.Parameters.AddWithValue(embedding1);
            cmd.Parameters.AddWithValue(embedding2);
            cmd.Parameters.AddWithValue(embedding3);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new NpgsqlCommand("SELECT * FROM items ORDER BY embedding <-> $1 LIMIT 5", conn))
        {
            var embedding = new Vector(new float[] { 1, 1, 1 });
            cmd.Parameters.AddWithValue(embedding);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    Console.WriteLine((Vector) reader.GetValue(0));
            }
        }

        await using (var cmd = new NpgsqlCommand("CREATE INDEX my_index ON items USING ivfflat (embedding vector_l2_ops)", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
