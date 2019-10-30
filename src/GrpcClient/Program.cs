using Grpc.Net.Client;
using GrpcServer.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 不需要tls设置。
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5000");

            // 使用Https。
            // var grpcChannel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Duplicater.DuplicaterClient(grpcChannel);
            var entry = client.EntryDuplicate();
            var random = new Random();
            var length = 100000;

            var token = new CancellationToken();
            var response = Task.Run(async () =>
            {
                while (await entry.ResponseStream.MoveNext(token))
                {
                    if (entry.ResponseStream.Current.Result)
                        Console.WriteLine($"{entry.ResponseStream.Current.Msg}");
                    else
                        Console.WriteLine($"{entry.ResponseStream.Current.Msg}入判重失败。");
                }
            });
            for (int i = 0; i < length; i++)
            {
                SpinWait.SpinUntil(() => false, 200);
                var msg = random.Next(0, 2000).ToString();
                await entry.RequestStream.WriteAsync(new EntryRequset { Tag = msg });
            }

            Console.WriteLine("等待释放链接。");
            await entry.RequestStream.CompleteAsync();
            entry.Dispose();
            Console.WriteLine("完成");
            Console.ReadKey();
        }
    }
}
