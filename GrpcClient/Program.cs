using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;
using Test;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                
                var client = new Tester.TesterClient(channel);
                #region Client&Server possible communication
                await UnaryStreamingCall(client);

                //AsyncServerStreamingCall<HelloReply> call = await ServerStreamingCall(client);

                //AsyncClientStreamingCall<HelloRequest, HelloReply> call = await ClientStreamingCall(client);

                //AsyncDuplexStreamingCall<HelloRequest, HelloReply> call = await BidirectionalStreamingCall(client);
                #endregion

                #region gRPC headers and trailers
                //await UnaryStreamingCallWithHeadersAndTrailersAndDeadline(client);
                #endregion

            }
            Console.ReadLine();
        }

        private static async Task<AsyncDuplexStreamingCall<HelloRequest, HelloReply>> BidirectionalStreamingCall(Tester.TesterClient client)
        {
            var call = client.SayHelloBidirectionalStreaming();

            Console.WriteLine("Starting background task to receive messages");
            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(response.Message);
                    // Echo messages sent to the service
                }
            });

            Console.WriteLine("Starting to send messages");
            Console.WriteLine("Type a message to echo then press enter.");
            while (true)
            {
                var result = Console.ReadLine();
                if (string.IsNullOrEmpty(result))
                {
                    break;
                }

                await call.RequestStream.WriteAsync(new HelloRequest { Name = result });
            }

            Console.WriteLine("Disconnecting");
            await call.RequestStream.CompleteAsync();
            await readTask;
            return call;
        }

        private static async Task<AsyncClientStreamingCall<HelloRequest, HelloReply>> ClientStreamingCall(Tester.TesterClient client)
        {
            var call = client.SayHelloClientStreaming();
            //rpc SayHelloClientStreaming (stream HelloRequest) returns (HelloReply)

            for (var i = 1; i < 4; i++)
            {
                await call.RequestStream.WriteAsync(new HelloRequest { Name = Console.ReadLine() });
            }
            await call.RequestStream.CompleteAsync();

            var response = await call;
            Console.WriteLine($"Count: {response.Message}");
            // Count: 3
            return call;
        }

        private static async Task<AsyncServerStreamingCall<HelloReply>> ServerStreamingCall(Tester.TesterClient client)
        {
            var call = client.SayHelloServerStreaming(new HelloRequest { Name = "World" });
            #region old code
            //while (await call.ResponseStream.MoveNext())
            //{
            //    Console.WriteLine("Greeting: " + call.ResponseStream.Current.Message);
            //    // "Greeting: Hello World" is written multiple times
            //} 
            #endregion
            ////IAsyncStreamReader<T>.ReadAllAsync() extension method reads all messages from the response stream c#8
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine("Greeting: " + response.Message);
                // "Greeting: Hello World" is written multiple times
            }
            return call;
        }

        private static async Task UnaryStreamingCall(Tester.TesterClient client)
        {
            try
            {
                var response = await client.SayHelloUnaryAsync(new HelloRequest { Name = "World" });
                Console.WriteLine("Greeting: " + response.Message);

            }catch(RpcException ex)
            {
                var msg = ex.Message;
            }


        }

        private static async Task UnaryStreamingCallWithHeadersAndTrailersAndDeadline(Tester.TesterClient client)
        {
            try
            {
                using var call = client.SayHelloUnaryAsync(new HelloRequest { Name = "World" }, deadline: DateTime.UtcNow.AddSeconds(50));
                
                var headers = await call.ResponseHeadersAsync;
                var myValue = headers.GetValue("my-trailer-name");
               
                var response = await call.ResponseAsync;

                var trailers = call.GetTrailers();
                var myValue2 = trailers.GetValue("my-trailer-name");

                Console.WriteLine("Greeting: " + response.Message);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine("Greeting timeout.");
            }
        }
    }
}
