using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using GrpcServer.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrpcServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DuplicateController : ControllerBase
    {
        /// <summary>
        /// 日志组件。
        /// </summary>
        private readonly ILogger<DuplicateController> _logger;

        /// <summary>
        /// 内存判重器。
        /// </summary>
        private readonly IDuplicate _memoryDuplicate;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="logger">日志。</param>
        /// <param name="memoryDuplicate">判重器。</param>
        public DuplicateController(ILogger<DuplicateController> logger, IDuplicate memoryDuplicate)
        {
            _logger = logger;
            _memoryDuplicate = memoryDuplicate;
            logger.LogInformation("通过WebApi进入判重控制器。");
        }

        [HttpPost("DuplicateCheck")]
        public ActionResult<WebApiDuplicateCheckResponse> DuplicateCheck([FromBody]WebApiDuplicateCheckRequest duplicateCheckRequest)
        {
            var result = _memoryDuplicate.DuplicateCheck(duplicateCheckRequest.Tag);
            if (result)
                _logger.LogInformation($"通过 WebApi {duplicateCheckRequest.Tag} ---存在---");
            else
                _logger.LogInformation($"通过 WebApi {duplicateCheckRequest.Tag} ---不存在---");

            var response = new WebApiDuplicateCheckResponse { Result = result };

            return response;
        }

        /// <summary>
        /// 进入判重。
        /// </summary>
        /// <param name="entryRequest">判重请求。</param>
        /// <returns>结果。</returns>
        [HttpPost("EntryDuplicate")]
        public ActionResult<WebApiEntryResponse> EntryDuplicate([FromBody]WebApiEntryRequest entryRequest)
        {
            var result = _memoryDuplicate.EntryDuplicate(entryRequest.Tag);
            var msg = string.Empty;
            if (result)
                msg = $"通过 WebApi {entryRequest.Tag} 入判重成功。";
            else
                msg = $"通过 WebApi {entryRequest.Tag} 入判重失败,已有重复的数据";

            _logger.LogInformation(msg);
            var response = new WebApiEntryResponse
            {
                Msg = msg,
                Result = result
            };

            return response;
        }
    }
}