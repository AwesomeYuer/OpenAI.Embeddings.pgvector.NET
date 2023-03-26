// See https://aka.ms/new-console-template for more information
using Npgsql;
using OpenAI;
using PgVectors.NET;
using PgVectors.Npgsql;
using System.Collections.Generic;
using System.Numerics;

Console.WriteLine("Hello, World!");


var auth = OpenAIAuthentication.LoadFromDirectory();

var openAIClient = new OpenAIClient(auth);

var inputEmbeddings = new[]
{
      "The food was delicious and the waiter..."
    , "The food was terrible and the waiter..."
    , "麻婆豆腐是美味"
    , "coffee is not good"
    , "coke is bad"
    , "apple is very very good"
}
;

var i = 0;
var insertSql = inputEmbeddings
                    .Select
                        (
                            (x) =>
                            {
                                return
                                    $"(${++i},${++i})";
                            }
                        )
                    .Aggregate
                        (
                            (x, y) =>
                            {
                                return
                                    $@"{x},{y}";
                            }
                        );
insertSql = $"INSERT INTO items (content, embedding) VALUES {insertSql}";

var result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync(inputEmbeddings);
i = 0;
var embeddings = result
                    .Data
                    .Select
                        (
                            (e) =>
                            {
                                return
                                    (
                                        inputEmbeddings[i ++]
                                        , new PgVector
                                                    (
                                                        e
                                                            .Embedding
                                                            .Select
                                                                (
                                                                    (ee) =>
                                                                    {
                                                                        return (float)ee;
                                                                    }
                                                                )
                                                            .ToArray()
                                                    )
                                    );
                            }
                        )
                        ;


var connectionString = "Host=localhost;Database=pgvectors;User Id=sa;Password=!@#123QWE";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector();

using var npgsqlDataSource = dataSourceBuilder.Build();
using var connection = npgsqlDataSource.OpenConnection();

await using (var cmd = new NpgsqlCommand(insertSql, connection))
{
    foreach (var (content , pgVector) in embeddings)
    {
        cmd.Parameters.AddWithValue(content);
        cmd.Parameters.AddWithValue(pgVector);
    }
    await cmd.ExecuteNonQueryAsync();
}

var queryEmbeddings = "shit";

result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync(queryEmbeddings);



var selectSql = "SELECT content FROM items ORDER BY embedding <= $1";
selectSql = "SELECT content FROM items ORDER BY cosine_distance(embedding,$1::vector)";
selectSql = "SELECT content FROM items ORDER BY embedding <-> $1::vector";

await using (var cmd = new NpgsqlCommand(selectSql, connection))
{
    var embedding = result
                        .Data[0]
                        .Embedding
                        .Select
                            (
                                (x) => 
                                {
                                    return (float) x;
                                }
                            )
                        .ToArray()
                        ;
    cmd.Parameters.AddWithValue(embedding);

    Console.WriteLine($"{queryEmbeddings}");
    Console.WriteLine();
    Console.WriteLine();

    await using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine((string)reader.GetValue(0));
        }
    }
}
