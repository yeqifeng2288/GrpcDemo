using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    /// <summary>
    /// 响应。
    /// </summary>
    public class WebApiEntryResponse
    {
        /// <summary>
        /// 信息。
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 结果。
        /// </summary>
        public bool Result { get; set; }
    }
}
