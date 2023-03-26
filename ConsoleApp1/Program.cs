// See https://aka.ms/new-console-template for more information
using OpenAI;
using PgVectors.NET;

Console.WriteLine("Hello, World!");


var auth = OpenAIAuthentication.LoadFromDirectory();

var openAIClient = new OpenAIClient(auth);

var result = await openAIClient.EmbeddingsEndpoint.CreateEmbeddingAsync("The food was delicious and the waiter...");

//openAIClient.ChatEndpoint.


var pgVectors = result
                    .Data
                    .Select
                        (
                            (e) =>
                            {
                                return
                                    new PgVector
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
                                            );
                            }
                        );


Console.WriteLine(auth.ApiKey);