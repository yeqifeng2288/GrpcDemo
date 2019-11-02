using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    /// <summary>
    /// 判重请求实体。
    /// </summary>
    public class WebApiDuplicateCheckRequest
    {
        /// <summary>
        /// 请求。
        /// </summary>
        public string Tag { get; set; }
    }
}
