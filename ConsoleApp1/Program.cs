// See https://aka.ms/new-console-template for more information
using Npgsql;
using OpenAI;
using PgVectors.NET;
using PgVectors.Npgsql;
using System.Data;
using System.Data.Common;
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
    , "我爱祖国"
    , "狼吃肉狗吃屎"
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
,
T2
as
(
    SELECT
        *
    FROM
        ""ContentsEmbeddings"" as a
    where
        not exists
            (
                select
                    1
                from
                    ""ContentsEmbeddings"" as aa
                where
                    aa.""ContentHash"" = a.""ContentHash""
                    and
                    aa.""ID"" < a.""ID""
                    and
                    exists
                        (
                            select
                                1
                            from
                                T1 aaa
                            where
                                aaa.""ContentHash"" = a.""ContentHash""
                        ) 
            )

)
MERGE INTO ""ContentsEmbeddings"" a
USING
    (
        SELECT  
              aaa.""Content""
            , aaa.""ContentHash""
            , bbb.""EmbeddingHash""     as ""EarliestEmbeddingHash""
            , bbb.""Embedding""         as ""EarliestEmbedding""
            , bbb.""CreateTime""        as ""EarliestEmbeddingCreateTime""
            , aaa.""EmbeddingHash""
            , aaa.""Embedding""
        FROM
            T1 as aaa
                left join
                    T2 bbb
                        on
                            aaa.""ContentHash"" = bbb.""ContentHash""
    ) AS aa
ON
    aa.""ContentHash"" = a.""ContentHash""
    and
    aa.""EmbeddingHash"" = a.""EmbeddingHash""
    and
    aa.""Embedding"" = a.""Embedding""
WHEN
    MATCHED
        THEN
            UPDATE
            SET
                  ""EmbeddingCount"" = ""EmbeddingCount"" + 1
                , ""UpdateTime"" = NOW()
WHEN
    NOT MATCHED
        THEN
            INSERT
                (
                      ""Content""
                    , ""ContentHash""
                    , ""EarliestEmbeddingHash""
                    , ""EarliestEmbedding""
                    , ""EarliestEmbeddingCreateTime""
                    , ""EmbeddingHash""
                    , ""Embedding""
                )
            VALUES
                (
                      aa.""Content""
                    , aa.""ContentHash""
                    , coalesce(aa.""EarliestEmbeddingHash""         , aa.""EmbeddingHash""  )
                    , coalesce(aa.""EarliestEmbedding""             , aa.""Embedding""      )
                    , coalesce(aa.""EarliestEmbeddingCreateTime""   , NOW()                 )
                    , aa.""EmbeddingHash""
                    , aa.""Embedding""
                )
";

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
        // 1
        sqlCommand.Parameters.AddWithValue(content);

        // 2
        var bytes = Encoding.UTF8.GetBytes(content);
        bytes = sha256.ComputeHash(bytes);
        string hexString = BitConverter.ToString(bytes).Replace("-", string.Empty);
        sqlCommand.Parameters.AddWithValue(hexString);
        
        // 3
        sqlCommand.Parameters.AddWithValue(pgVector);

        // 4
        bytes = Encoding.UTF8.GetBytes(pgVector.ToString());
        bytes = sha256.ComputeHash(bytes);
        hexString = BitConverter.ToString(bytes).Replace("-", string.Empty);
        sqlCommand.Parameters.AddWithValue(hexString);
    }
    await sqlCommand.ExecuteNonQueryAsync();
}

//return;
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
        , ""ContentHash""
        , ""EmbeddingHash""
        , ""Embedding"" <-> $1::vector      as ""Distance""
        --, $1                              as ""AdHocQueryEmbedding""
        , ""UpdateTime""
        , ""CreateTime""
    FROM
        ""ContentsEmbeddings""
)
,
T2
as
(
    SELECT
          MAX(a.""Content"")                    as ""Content""
        , a.""ContentHash""
        , COUNT(DISTINCT a.""EmbeddingHash"")   as ""CountOfEmbeddings""
        , AVG(a.""Distance"")                   as ""AverageOfDistance""
        , MAX(a.""Distance"")                   as ""MaxOfDistance""
        , MIN(a.""Distance"")                   as ""MinOfDistance""
        , MIN(a.""CreateTime"")                 as ""EarliestEmbeddingTime""
        , MAX(a.""CreateTime"")                 as ""LatestEmbeddingTime""
    FROM
        T1 a
    GROUP BY
          a.""ContentHash""
)
SELECT
      a.*
    , (a.""MaxOfDistance"" - a.""MinOfDistance"") as ""DiffOfMaxMinDistance""
FROM
    T2 a
ORDER BY
    a.""AverageOfDistance"";
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
            IDataRecord dataRecord      = dataReader;
            var averageOfDistance       = dataReader.GetDouble(dataRecord.GetOrdinal("AverageOfDistance"));
            var countOfEmbeddings       = dataReader.GetInt32(dataRecord.GetOrdinal("CountOfEmbeddings"));
            var maxOfDistance           = dataReader.GetDouble(dataRecord.GetOrdinal("MaxOfDistance"));
            var minOfDistance           = dataReader.GetDouble(dataRecord.GetOrdinal("MinOfDistance"));
            var diffOfMaxMinDistance    = dataReader.GetDouble(dataRecord.GetOrdinal("DiffOfMaxMinDistance"));
            var earliestEmbeddingTime   = dataReader.GetDateTime(dataRecord.GetOrdinal("EarliestEmbeddingTime"));
            var latestEmbeddingTime     = dataReader.GetDateTime(dataRecord.GetOrdinal("LatestEmbeddingTime"));
            var preservedContent        = dataReader.GetString(dataRecord.GetOrdinal("Content"));
            Console
                .WriteLine
                        (
                            $@"{nameof(adHocQuery)}: ""{adHocQuery}"", {nameof(averageOfDistance)}: [{averageOfDistance}]{seperator},{nameof(diffOfMaxMinDistance)}: [{diffOfMaxMinDistance}]{seperator},{nameof(countOfEmbeddings)}: [{countOfEmbeddings}]{seperator},{nameof(preservedContent)}: ""{preservedContent}"""
                        );
            
            //Console
            //    .WriteLine(dataReader.GetFieldValue<PgVector>(3).GetHashCode() == dataReader.GetFieldValue<PgVector>(3).GetHashCode());
        }
    }
}
