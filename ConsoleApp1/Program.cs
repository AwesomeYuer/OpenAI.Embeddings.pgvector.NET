// See https://aka.ms/new-console-template for more information
using Npgsql;
using OpenAI;
using Pgvector;
using Pgvector.Npgsql;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;


Console.WriteLine("Hello, World!");

var auth = OpenAIAuthentication.LoadFromDirectory();

var openAIClient = new OpenAIClient(auth);

var preservedEmbeddingsInputs = new[]
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
    , "性别男"
    , "爱好女"
}
;

var i = 0;
var sql = preservedEmbeddingsInputs
            //.Take(2)
            .Select
                (
                    (x) =>
                    {
                        return
                            $"(${++i}, ${++i} , ${++i}, ${++i})";
                    }
                )
            .Aggregate
                (
                    (x, y) =>
                    {
                        return
                            $"{x}\r\n, {y}";
                    }
                );

// Why OpenAI Embedding return different vectors for the same text input?
// https://community.openai.com/t/why-openai-embedding-return-different-vectors-for-the-same-text-input/144495
// Store all vectors for every inputs
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
    WHERE
        NOT EXISTS
            (
                SELECT
                    1
                FROM
                    ""ContentsEmbeddings"" as aa
                WHERE
                    aa.""ContentHash"" = a.""ContentHash""
                    AND
                    aa.""ID"" < a.""ID""
                    AND
                    EXISTS
                        (
                            SELECT
                                1
                            FROM
                                T1 aaa
                            WHERE
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
                LEFT JOIN
                    T2 bbb
                        ON
                            aaa.""ContentHash"" = bbb.""ContentHash""
    ) AS aa
ON
    aa.""ContentHash"" = a.""ContentHash""
    AND
    aa.""EmbeddingHash"" = a.""EmbeddingHash""
    AND
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
                        .CreateEmbeddingAsync(preservedEmbeddingsInputs, model);
i = 0;
var embeddings = result
                    .Data
                    .Select
                        (
                            (e) =>
                            {
                                return
                                    (
                                        preservedEmbeddingsInputs[i ++]
                                        , new Vector
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
var adHocQueryInput     = "shit";
adHocQueryInput         = "苹果";
adHocQueryInput         = "佛跳墙";
adHocQueryInput         = "尿素";
adHocQueryInput         = "好吃的";
adHocQueryInput         = "C#";
adHocQueryInput         = "php";
adHocQueryInput         = "Java";
adHocQueryInput         = "螃蟹好吃";
adHocQueryInput         = "JavaScript";
adHocQueryInput         = "男";
adHocQueryInput         = "爱好男";


Console.WriteLine($@"{nameof(adHocQueryInput)}: ""{adHocQueryInput}"" match similarity:");
Console.WriteLine();
Console.WriteLine();

result = await openAIClient
                        .EmbeddingsEndpoint
                        .CreateEmbeddingAsync
                                (adHocQueryInput, model);

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
var seperator = "\t\t";
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
        , ""Embedding"" <-> $1::vector                      as ""EuclideanDistance""
        , ""Embedding"" <=> $1::vector                      as ""CosineDistance""
        --, $1                                              as ""AdHocQueryEmbedding""
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
          MAX(a.""Content"")                                as ""Content""
        , a.""ContentHash""
        , COUNT(DISTINCT a.""EmbeddingHash"")               as ""CountOfEmbeddings""
        , AVG(a.""EuclideanDistance"")                      as ""AverageOfEuclideanDistance""
        , MAX(a.""EuclideanDistance"")                      as ""MaxOfEuclideanDistance""
        , MIN(a.""EuclideanDistance"")                      as ""MinOfEuclideanDistance""
        , AVG(a.""CosineDistance"")                         as ""AverageOfCosineDistance""
        , MAX(a.""CosineDistance"")                         as ""MaxOfCosineDistance""
        , MIN(a.""CosineDistance"")                         as ""MinOfCosineDistance""
        , MIN(a.""CreateTime"")                             as ""EarliestEmbeddingTime""
        , MAX(a.""CreateTime"")                             as ""LatestEmbeddingTime""
    FROM
        T1 a
    GROUP BY
          a.""ContentHash""
)
SELECT
      a.*
    , (a.""MaxOfEuclideanDistance"" - a.""MinOfEuclideanDistance"")     as ""DiffOfMaxMinEuclideanDistance""
    , (a.""MaxOfCosineDistance"" - a.""MinOfEuclideanDistance"")        as ""DiffOfMaxMinCosineDistance""
FROM
    T2 a
ORDER BY
    {0}
    {1};
--1
"
;

await using
    (
        var sqlCommand = new NpgsqlCommand
                                (
                                    string.Format(sql, @"a.""AverageOfEuclideanDistance""", string.Empty)
                                    , connection
                                )
    )
{
    var adHocQueryPgVector = new Vector(adHocQueryEmbedding);
    var adHocQueryHashCode = adHocQueryPgVector.GetHashCode();
    sqlCommand.Parameters.AddWithValue(adHocQueryEmbedding);
    

    await using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
    {
        while (await dataReader.ReadAsync())
        {
            IDataRecord dataRecord              = dataReader;
            var averageOfEuclideanDistance      = dataReader.GetDouble(dataRecord.GetOrdinal("AverageOfEuclideanDistance"));
            var countOfEmbeddings               = dataReader.GetInt32(dataRecord.GetOrdinal("CountOfEmbeddings"));
            var maxOfEuclideanDistance          = dataReader.GetDouble(dataRecord.GetOrdinal("MaxOfEuclideanDistance"));
            var minOfEuclideanDistance          = dataReader.GetDouble(dataRecord.GetOrdinal("MinOfEuclideanDistance"));
            var diffOfMaxMinEuclideanDistance   = dataReader.GetDouble(dataRecord.GetOrdinal("DiffOfMaxMinEuclideanDistance"));
            var earliestEmbeddingTime           = dataReader.GetDateTime(dataRecord.GetOrdinal("EarliestEmbeddingTime"));
            var latestEmbeddingTime             = dataReader.GetDateTime(dataRecord.GetOrdinal("LatestEmbeddingTime"));
            var preservedContent                = dataReader.GetString(dataRecord.GetOrdinal("Content"));
            Console
                .WriteLine
                        (
                            $@"{nameof(adHocQueryInput)}: ""{adHocQueryInput}"", {nameof(averageOfEuclideanDistance)}: [{averageOfEuclideanDistance}]{seperator},{nameof(diffOfMaxMinEuclideanDistance)}: [{diffOfMaxMinEuclideanDistance}]{seperator},{nameof(countOfEmbeddings)}: [{countOfEmbeddings}]{seperator},{nameof(preservedContent)}: ""{preservedContent}"""
                        );
            
            //Console
            //    .WriteLine(dataReader.GetFieldValue<PgVector>(3).GetHashCode() == dataReader.GetFieldValue<PgVector>(3).GetHashCode());
        }
    }
}

Console.WriteLine("==================================================================");

await using
    (
        var sqlCommand = new NpgsqlCommand
                                (
                                    string.Format(sql, @"a.""AverageOfCosineDistance""", "")
                                    , connection
                                )
    )
{
    var adHocQueryPgVector = new Vector(adHocQueryEmbedding);
    var adHocQueryHashCode = adHocQueryPgVector.GetHashCode();
    sqlCommand.Parameters.AddWithValue(adHocQueryEmbedding);


    await using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
    {
        while (await dataReader.ReadAsync())
        {
            IDataRecord dataRecord          = dataReader;
            var averageOfCosineDistance     = dataReader.GetDouble(dataRecord.GetOrdinal("AverageOfCosineDistance"));
            var countOfEmbeddings           = dataReader.GetInt32(dataRecord.GetOrdinal("CountOfEmbeddings"));
            var maxOfCosineDistance         = dataReader.GetDouble(dataRecord.GetOrdinal("MaxOfCosineDistance"));
            var minOfCosineDistance         = dataReader.GetDouble(dataRecord.GetOrdinal("MinOfCosineDistance"));
            var diffOfMaxMinCosineDistance  = dataReader.GetDouble(dataRecord.GetOrdinal("DiffOfMaxMinCosineDistance"));
            var earliestEmbeddingTime       = dataReader.GetDateTime(dataRecord.GetOrdinal("EarliestEmbeddingTime"));
            var latestEmbeddingTime         = dataReader.GetDateTime(dataRecord.GetOrdinal("LatestEmbeddingTime"));
            var preservedContent            = dataReader.GetString(dataRecord.GetOrdinal("Content"));
            Console
                .WriteLine
                        (
                            $@"{nameof(adHocQueryInput)}: ""{adHocQueryInput}"", {nameof(averageOfCosineDistance)}: [{averageOfCosineDistance}]{seperator},{nameof(diffOfMaxMinCosineDistance)}: [{diffOfMaxMinCosineDistance}]{seperator},{nameof(countOfEmbeddings)}: [{countOfEmbeddings}]{seperator},{nameof(preservedContent)}: ""{preservedContent}"""
                        );

            //Console
            //    .WriteLine(dataReader.GetFieldValue<PgVector>(3).GetHashCode() == dataReader.GetFieldValue<PgVector>(3).GetHashCode());
        }
    }
}

sql = @"
    SELECT
		  a.""ID""
        , a.""Content""
        , (a.""EarliestEmbedding"" <-> a.""Embedding"")                 as ""EuclideanDistanceWithEarliestEmbedding""
        , cosine_distance(a.""EarliestEmbedding"", a.""Embedding"")		as ""CosineDistanceWithEarliestEmbedding"" 
		, a.""EmbeddingHash""
		, a.""EarliestEmbeddingHash""
        , a.""UpdateTime""
        , a.""CreateTime""
		, a.""Embedding""
		, a.""EarliestEmbedding""
    FROM
        ""ContentsEmbeddings"" a
	order by
		  a.""Content""
		, ""EuclideanDistanceWithEarliestEmbedding""
";

Console.WriteLine("==================================================================");

var results = GetDataAsIEnumerable()
                    .OrderBy
                        (
                            (x) =>
                            {
                                return
                                    x.EuclideanDistanceWithEarliestEmbedding;
                            }
                        );

seperator = "\t\t\t\t";
await foreach (var (preservedContent, pgVectorEarliestEmbedding, pgVectorEmbedding, euclideanDistanceWithEarliestEmbedding, CosineDistanceWithEarliestEmbedding) in results)
{
    var euclideanDistance = pgVectorEmbedding - pgVectorEarliestEmbedding;
    if (euclideanDistanceWithEarliestEmbedding != euclideanDistance)
    {
        //throw new Exception("distanceWithEarliestEmbedding is not Euclidean Distance!");
    }
    Console
        .WriteLine
                (
                    $@"{nameof(preservedContent)}: {preservedContent}{seperator}{nameof(euclideanDistanceWithEarliestEmbedding)}: {euclideanDistanceWithEarliestEmbedding}{seperator}{nameof(euclideanDistance)}: {euclideanDistance}"
                );
}

results = GetDataAsIEnumerable()
                    .OrderByDescending
                        (
                            (x) =>
                            {
                                return
                                    x.CosineDistanceWithEarliestEmbedding;
                            }
                        );

seperator = "\t\t\t\t";
await foreach (var (preservedContent, pgVectorEarliestEmbedding, pgVectorEmbedding, euclideanDistanceWithEarliestEmbedding, CosineDistanceWithEarliestEmbedding) in results)
{
    var euclideanDistance = pgVectorEmbedding - pgVectorEarliestEmbedding;
    if (euclideanDistanceWithEarliestEmbedding != euclideanDistance)
    {
        //throw new Exception("distanceWithEarliestEmbedding is not Euclidean Distance!");
    }
    Console
        .WriteLine
                (
                    $@"{nameof(preservedContent)}: {preservedContent}{seperator}{nameof(euclideanDistanceWithEarliestEmbedding)}: {euclideanDistanceWithEarliestEmbedding}{seperator}{nameof(euclideanDistance)}: {euclideanDistance}"
                );
}


async IAsyncEnumerable
            <
                (
                      string PreservedContent
                    , Vector PgVectorEarliestEmbedding
                    , Vector PgVectorEmbedding
                    , double EuclideanDistanceWithEarliestEmbedding
                    , double CosineDistanceWithEarliestEmbedding
                )
            > GetDataAsIEnumerable()
{

    await using (var sqlCommand = new NpgsqlCommand(sql, connection))
    {
        await using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
        {
            while (await dataReader.ReadAsync())
            {
                IDataRecord dataRecord = dataReader;
                var preservedContent                            = dataReader.GetString(dataRecord.GetOrdinal("Content"));
                var pgVectorEarliestEmbedding                   = dataReader.GetFieldValue<Vector>(dataRecord.GetOrdinal("EarliestEmbedding"));
                var pgVectorEmbedding                           = dataReader.GetFieldValue<Vector>(dataRecord.GetOrdinal("Embedding"));
                var euclideanDistanceWithEarliestEmbedding      = dataReader.GetFieldValue<Double>(dataRecord.GetOrdinal("euclideanDistanceWithEarliestEmbedding"));
                var cosineDistanceWithEarliestEmbedding         = dataReader.GetFieldValue<Double>(dataRecord.GetOrdinal("cosineDistanceWithEarliestEmbedding"));
                yield return
                    (
                          preservedContent
                        , pgVectorEarliestEmbedding
                        , pgVectorEmbedding
                        , euclideanDistanceWithEarliestEmbedding
                        , cosineDistanceWithEarliestEmbedding
                    )
                    ;

            }
        }
    }
}