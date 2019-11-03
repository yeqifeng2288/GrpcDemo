using Entities.Models;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer.Protos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        private static int _randomMax = 10000;

        static void Main(string[] args)
        {
            // 不需要tls设置 但如果同时使用webapi回变得很麻烦。
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            Task.Run(() =>
            {
                WebApiTest();
            });

            Task.Run(() =>
            {
                GrpcTest();
            });

            Console.ReadKey();
        }

        #region WebApi

        /// <summary>
        /// 测试WebApi。
        /// </summary>
        private static void WebApiTest()
        {
            var collect = new ServiceCollection();
            collect.AddHttpClient("webapi");
            var provider = collect.BuildServiceProvider();
            var factory = provider.GetService<IHttpClientFactory>();
            var client = factory.CreateClient("webapi");
            var task1 = WebApiEnterTest(client);
            var task2 = WebApiDuplicateCheckTest(client);
            Task.WaitAll(task1, task2);
        }

        /// <summary>
        /// WebApi进入判重。
        /// </summary>
        /// <param name="client">http客户端。</param>
        /// <returns>返回值。</returns>
        private static async Task WebApiEnterTest(HttpClient client)
        {
            var randomBuider = new RandomBulider(0, 2000, _randomMax);
            foreach (var tag in randomBuider.Next())
            {
                HttpRequestMessage hrm = BuildStringHttpRequestMessage(new WebApiEntryRequest { Tag = tag.ToString() });
                var response = await client.SendAsync(hrm);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var entryReponse = JsonSerializer.Deserialize<WebApiEntryResponse>(json);
                    if (entryReponse.Result)
                        Console.WriteLine($"{entryReponse.Msg}");
                    else
                        Console.WriteLine($"{entryReponse.Msg}入判重失败。");
                }

                SpinWait.SpinUntil(() => false, 100);
            }
        }

        /// <summary>
        /// WebApi判重。
        /// </summary>
        /// <param name="client">Http客户端。。</param>
        /// <returns>返回值。</returns>
        private static async Task WebApiDuplicateCheckTest(HttpClient client)
        {
            var randomBuider = new RandomBulider(0, 2000, _randomMax);
            foreach (var tag in randomBuider.Next())
            {
                var hrm = BuildStringHttpRequestMessage(new WebApiDuplicateCheckRequest { Tag = tag.ToString() });
                var response = await client.SendAsync(hrm);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var entryReponse = JsonSerializer.Deserialize<WebApiDuplicateCheckResponse>(json);
                    if (entryReponse.Result)
                        Console.WriteLine($"通过WebApi:已存在，忽略。");
                    else
                        Console.WriteLine("通过WebApi:不存在，执行操作任务。");
                }

                SpinWait.SpinUntil(() => false, 100);
            }
        }

        /// <summary>
        /// 构造http请求方法。
        /// </summary>
        /// <param name="entity">需要json化的实体。</param>
        /// <returns>返回包含json化信息的http请求信息。</returns>
        private static HttpRequestMessage BuildStringHttpRequestMessage<TValue>(TValue entity)
        {
            var msg = JsonSerializer.Serialize(entity);
            var hrm = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5000/Duplicate/EntryDuplicate")
            {
                Version = new Version(2, 0),
                Content = new StringContent(msg, Encoding.UTF8, "application/json")
            };
            return hrm;
        }
        #endregion

        #region Grpc
        /// <summary>
        /// 测试GRPC。
        /// </summary>
        private static void GrpcTest()
        {
            var grpcChannel = GrpcChannel.ForAddress("http://localhost:5000");

            // 使用Https。
            //var grpcChannel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new Duplicater.DuplicaterClient(grpcChannel);
            var task1 = EnterTest(client);

            var task2 = DuplicateCheckTest(client);
            Task.WaitAll(task1, task2);
        }

        /// <summary>
        /// 判断是否已存在。
        /// </summary>
        /// <param name="client">客户端。</param>
        /// <returns>返回结果。</returns>
        private static async Task DuplicateCheckTest(Duplicater.DuplicaterClient client)
        {
            var dulicate = client.DuplicateCheck();
            var token = new CancellationToken();

            var response = Task.Run(async () =>
            {
                while (await dulicate.ResponseStream.MoveNext(token))
                {
                    if (dulicate.ResponseStream.Current.Result)
                        Console.WriteLine($"已存在，忽略。");
                    else
                        Console.WriteLine("不存在，执行操作任务。");
                }

                Console.WriteLine("响应完成。");
            });

            var request = Task.Run(async () =>
            {
                var randomBuider = new RandomBulider(0, 2000, _randomMax);

                foreach (var tag in randomBuider.Next())
                {
                    SpinWait.SpinUntil(() => false, 100);
                    await dulicate.RequestStream.WriteAsync(new DuplicateCheckRequest { Tag = tag.ToString() });
                }
            });

            await request;
            Console.WriteLine("是否判重。");
            await dulicate.RequestStream.CompleteAsync();
            await response;
            dulicate.Dispose();
            Console.WriteLine("测试查判重完成。");
        }

        /// <summary>
        /// 入判重测试。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static async Task EnterTest(Duplicater.DuplicaterClient client)
        {
            var entry = client.EntryDuplicate();
            var randomBuilder = new RandomBulider(0, 2000, _randomMax);

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

                Console.WriteLine("响应完成。");
            });

            var request = Task.Run(async () =>
            {
                foreach (var tag in randomBuilder.Next())
                {
                    SpinWait.SpinUntil(() => false, 100);
                    var msg = tag.ToString();
                    await entry.RequestStream.WriteAsync(new EntryRequest { Tag = msg });
                }
            });

            await request;
            Console.WriteLine("等待释放链接。");
            await entry.RequestStream.CompleteAsync();
            await response;
            entry.Dispose();
            Console.WriteLine("测试入判重完成");
        }
        #endregion
    }
}
