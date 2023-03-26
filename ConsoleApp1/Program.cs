// See https://aka.ms/new-console-template for more information
using Npgsql;
using OpenAI;
using PgVectors.NET;
using PgVectors.Npgsql;
using System.Data.Common;

Console.WriteLine("Hello, World!");

var auth = OpenAIAuthentication.LoadFromDirectory();

var openAIClient = new OpenAIClient(auth);

var inputEmbeddings = new[]
{
      "The food was delicious and the waiter..."
    , "The food was terrible and the waiter..."
    , "麻婆豆腐是美味"
    , "coffee is not good"
    , "tea is good"
    , "coke is bad"
    , "apple is very very good"
    , "臭豆腐"
    , "屎"
    , "尿"
    , "屁"
    , "龙虾"
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

var model = await openAIClient
                        .ModelsEndpoint
                        .GetModelDetailsAsync
                                ("text-embedding-ada-002");

var result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync(inputEmbeddings, model);
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

// Preserve the vectors of contents of embeddings for match
await using (var npgsqlCommand = new NpgsqlCommand(sql, connection))
{
    foreach (var (content , pgVector) in embeddings)
    {
        npgsqlCommand.Parameters.AddWithValue(content);
        npgsqlCommand.Parameters.AddWithValue(pgVector);
    }
    await npgsqlCommand.ExecuteNonQueryAsync();
}

var adHocQuery = "shit";
adHocQuery = "苹果";
adHocQuery = "佛跳墙";
adHocQuery = "尿素";
adHocQuery = "好吃的";
adHocQuery = "C#";
adHocQuery = "php";
adHocQuery = "Java";
adHocQuery = "螃蟹好吃";

Console.WriteLine($"{nameof(adHocQuery)}: {adHocQuery} match similarity:");
Console.WriteLine();
Console.WriteLine();

result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync
                                (adHocQuery, model);

// Query match similarity
// Query order by ascending the distance between the vector of ad-hoc query key words's embedding and the vectors of preserved contents of embeddings in database
// The distance means similarity
sql = "SELECT content FROM items ORDER BY embedding <= $1";
sql = "SELECT content FROM items ORDER BY cosine_distance(embedding,$1::vector)";
sql = "SELECT content FROM items ORDER BY embedding <-> $1::vector";

sql = "SELECT content, avg(embedding <-> $1::vector) as AverageDistance FROM items GROUP BY content ORDER BY 2" ;

var adHocQueryEmbedding = result
                                .Data[0]
                                .Embedding
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return (float)x;
                                        }
                                    )
                                .ToArray()
                                ;

await using (var npgsqlCommand = new NpgsqlCommand(sql, connection))
{
    npgsqlCommand.Parameters.AddWithValue(adHocQueryEmbedding);

    await using (DbDataReader dataReader = await npgsqlCommand.ExecuteReaderAsync())
    {
        while (await dataReader.ReadAsync())
        {
            Console
                .WriteLine
                        (
                            $"{nameof(adHocQuery)}: [{adHocQuery}], AverageDistance: [{(double) dataReader["AverageDistance"]}]\t\t\t\t, content: [{(string) dataReader["content"]}]"
                        );
        }
    }
}
