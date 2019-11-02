using Grpc.Core;
using GrpcServer.Core;
using GrpcServer.Protos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GrpcServer.Protos.Duplicater;

namespace GrpcServer.Services
{
    /// <summary>
    /// 判重服务。
    /// </summary>
    public class DuplicateService : DuplicaterBase
    {
        /// <summary>
        /// 日志组件。
        /// </summary>
        private readonly ILogger<DuplicateService> _logger;

        /// <summary>
        /// 内存判重器。
        /// </summary>
        private readonly IDuplicate _memoryDuplicate;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="logger">日志组件。</param>
        /// <param name="memoryDuplicate">判重器。</param>
        public DuplicateService(ILogger<DuplicateService> logger, IDuplicate memoryDuplicate)
        {
            _logger = logger;
            _memoryDuplicate = memoryDuplicate;
            _logger.LogInformation("创建判重服务。");
        }

        /// <summary>
        /// 入判重。
        /// </summary>
        /// <param name="requestStream">请求流。</param>
        /// <param name="responseStream">响应流。</param>
        /// <param name="context">上下文。</param>
        /// <returns></returns>
        public override async Task EntryDuplicate(IAsyncStreamReader<EntryRequest> requestStream, IServerStreamWriter<EntryResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var result = _memoryDuplicate.EntryDuplicate(requestStream.Current.Tag);
                var msg = string.Empty;
                if (result)
                    msg = $"{requestStream.Current.Tag} 入判重成功。";
                else
                    msg = $"{requestStream.Current.Tag} 入判重失败,已有重复的数据";
                _logger.LogInformation(msg);

                await responseStream.WriteAsync(new EntryResponse { Result = result, Msg = msg });
            }

            _logger.LogInformation("本次请求已完成");
        }

        /// <summary>
        /// 判重。
        /// </summary>
        /// <param name="requestStream">请求流。</param>
        /// <param name="responseStream">响应流。</param>
        /// <param name="context">上下文。</param>
        /// <returns></returns>
        public override async Task DuplicateCheck(IAsyncStreamReader<DuplicateCheckRequest> requestStream, IServerStreamWriter<DuplicateCheckResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var result = _memoryDuplicate.DuplicateCheck(requestStream.Current.Tag);
                await responseStream.WriteAsync(new DuplicateCheckResponse { Result = result });
            }

            _logger.LogInformation("本次请求已完成");
        }
    }
}
