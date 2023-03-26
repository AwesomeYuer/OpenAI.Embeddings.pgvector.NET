// See https://aka.ms/new-console-template for more information
using Npgsql;
using OpenAI;
using PgVectors.NET;
using PgVectors.Npgsql;

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
var sql = inputEmbeddings
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

sql = $"INSERT INTO items (content, embedding) VALUES {sql}";

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

await using (var cmd = new NpgsqlCommand(sql, connection))
{
    foreach (var (content , pgVector) in embeddings)
    {
        cmd.Parameters.AddWithValue(content);
        cmd.Parameters.AddWithValue(pgVector);
    }
    await cmd.ExecuteNonQueryAsync();
}

var queryEmbeddings = "shit";

Console.WriteLine($"{nameof(queryEmbeddings)}: {queryEmbeddings}");
Console.WriteLine();
Console.WriteLine();

result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync(queryEmbeddings);


// Query order by ascending the distance between the vector of ad-hoc query key words's embedding and the vectors of preserved contents of embeddings in database
// The distance means similarity
sql = "SELECT content FROM items ORDER BY embedding <= $1";
sql = "SELECT content FROM items ORDER BY cosine_distance(embedding,$1::vector)";
sql = "SELECT content FROM items ORDER BY embedding <-> $1::vector";

await using (var cmd = new NpgsqlCommand(sql, connection))
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

    await using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine((string)reader.GetValue(0));
        }
    }
}
