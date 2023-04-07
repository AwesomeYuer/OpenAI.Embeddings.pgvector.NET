// See https://aka.ms/new-console-template for more information
using Npgsql;
using OpenAI;
using OpenAI.Models;
using PgVectors.NET;
using PgVectors.Npgsql;
using System.Data;
using System.Data.Common;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Hello, World!");

var auth = OpenAIAuthentication.LoadFromDirectory();

var openAIClient = new OpenAIClient(auth);

var preservedEmbeddings = new[]
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
    , "c#"
    , "php"
    , "Java"
}
;

var i = 0;
var sql = preservedEmbeddings
                    //.Take(2)
                    .Select
                        (
                            (x) =>
                            {
                                return
                                    $"(${++i}, ${++i} ,${++i} ,${++i})";
                            }
                        )
                    .Aggregate
                        (
                            (x, y) =>
                            {
                                return
                                    $"{x}\r\n,{y}";
                            }
                        );

sql = $@"
WITH
T1
as
(
    SELECT
        *
    FROM
        (
            values
                {sql}
        )
    AS T
        (
              ""Content""
            , ""ContentHash""
            , ""Embedding""
            , ""EmbeddingHash""
        )
)
MERGE INTO ""Items"" a
USING
    (
        SELECT
            *
        FROM
            T1
    ) AS aa
ON
    aa.""ContentHash"" = a.""ContentHash""
WHEN
    MATCHED
    AND
    a.""LatestEmbedding"" != aa.""Embedding""
    AND
    a.""LatestEmbeddingHash"" != aa.""EmbeddingHash""
            THEN
                UPDATE
                SET
                      ""LatestEmbedding"" = aa.""Embedding""
                    , ""LatestEmbeddingHash"" = aa.""EmbeddingHash""
                    , ""UpdateTime"" = NOW()
WHEN
    NOT MATCHED
            THEN
                INSERT
                    (
                          ""Content""
                        , ""ContentHash""
                        , ""EarliestEmbeddingHash""
                        , ""LatestEmbeddingHash""
                        , ""EarliestEmbedding""
                        , ""LatestEmbedding""
                    )
                VALUES 
                    (
                          aa.""Content""
                        , aa.""ContentHash""
                        , aa.""EmbeddingHash""
                        , aa.""EmbeddingHash""
                        , aa.""Embedding""
                        , aa.""Embedding""
                    );
";


//return;
var model = await openAIClient
                        .ModelsEndpoint
                        .GetModelDetailsAsync
                                ("text-embedding-ada-002");

//var 
//model = Model.Embedding_Ada_002;

var result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync(preservedEmbeddings, model);
i = 0;
var embeddings = result
                    .Data
                    .Select
                        (
                            (e) =>
                            {
                                return
                                    (
                                        preservedEmbeddings[i ++]
                                        , new PgVector
                                                    (
                                                        e
                                                            .Embedding
                                                            .Select
                                                                (
                                                                    (ee) =>
                                                                    {
                                                                        return (float) ee;
                                                                    }
                                                                )
                                                            .ToArray()
                                                    )
                                    );
                            }
                        )
                        //.Take(2)
                        ;

var connectionString = "Host=localhost;Database=pgvectors;User Id=sa;Password=!@#123QWE";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector();

using var sha256 = SHA256.Create();

using var npgsqlDataSource = dataSourceBuilder.Build();
using var connection = npgsqlDataSource.OpenConnection();

await using (var sqlCommand = new NpgsqlCommand(sql, connection))
{
    foreach (var (content, pgVector) in embeddings)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        bytes = sha256.ComputeHash(bytes);
        string hexString = BitConverter.ToString(bytes).Replace("-", string.Empty);
        var hashCode = pgVector.GetHashCode();
        sqlCommand.Parameters.AddWithValue(content);
        sqlCommand.Parameters.AddWithValue(hexString);
        sqlCommand.Parameters.AddWithValue(pgVector);
        sqlCommand.Parameters.AddWithValue(hashCode);
    }
    await sqlCommand.ExecuteNonQueryAsync();
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
adHocQuery = "JavaScript";


Console.WriteLine($@"{nameof(adHocQuery)}: ""{adHocQuery}"" match similarity:");
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

sql = @"
WITH
T1
as
(
    SELECT
          ""Content""
        , ""EarliestEmbedding"" <-> $1::vector      as ""EarliestDistance""
        , ""LatestEmbedding""   <-> $1::vector      as ""LatestDistance""
        , $1                                        as ""AdHocQueryEmbedding""
        , ""EarliestEmbeddingHash""
        , ""LatestEmbeddingHash""
        , ""UpdateTime""
        , ""CreateTime""
    FROM
        ""Items""
)
SELECT
      a.*
    , (a.""LatestDistance"" - a.""EarliestDistance"") as ""DiffDistance""
FROM
    T1 a
ORDER BY
    3;
--1
"
;

var adHocQueryEmbedding = result
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

await using (var sqlCommand = new NpgsqlCommand(sql, connection))
{
    var adHocQueryPgVector = new PgVector(adHocQueryEmbedding);
    var adHocQueryHashCode = adHocQueryPgVector.GetHashCode();
    sqlCommand.Parameters.AddWithValue(adHocQueryEmbedding);
    var seperator = "\t\t";

    await using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
    {
        
        while (await dataReader.ReadAsync())
        {
            IDataRecord dataRecord = dataReader;
            var earliestDistance = dataReader.GetDouble(dataRecord.GetOrdinal("EarliestDistance"));
            var latestDistance = dataReader.GetDouble(dataRecord.GetOrdinal("LatestDistance"));
            var diffDistance = dataReader.GetDouble(dataRecord.GetOrdinal("DiffDistance"));
            var preservedContent = dataReader.GetString(dataRecord.GetOrdinal("Content"));
            Console
                .WriteLine
                        (
                            $@"{nameof(adHocQuery)}: ""{adHocQuery}({adHocQueryHashCode})"", {nameof(latestDistance)}: [{latestDistance}]{seperator},{nameof(earliestDistance)}: [{earliestDistance}]{seperator},{nameof(diffDistance)}: [{diffDistance}]{seperator},{nameof(preservedContent)}: ""{preservedContent}"""
                        );
            
            //Console
            //    .WriteLine(dataReader.GetFieldValue<PgVector>(3).GetHashCode() == dataReader.GetFieldValue<PgVector>(3).GetHashCode());
        }
        
    }
}
