using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Core
{
    /// <summary>
    /// 判重接口。
    /// </summary>
    public interface IDuplicate
    {
        /// <summary>
        /// 将标签进入判重。
        /// </summary>
        /// <param name="tag">标签。</param>
        /// <returns>保存成功后将返回一个值。</returns>
        bool EntryDuplicate(string tag);

        /// <summary>
        /// 判断标签是否已经存在。
        /// </summary>
        /// <param name="tag">标签。</param>
        /// <returns>如果标签存在则返回true。</returns>
        bool DuplicateCheck(string tag);

        /// <summary>
        /// 删除一条标签。
        /// </summary>
        /// <param name="tag">标签。</param>
        /// <returns>返回结果。</returns>
        bool RemoveItem(string tag);
    }
}
