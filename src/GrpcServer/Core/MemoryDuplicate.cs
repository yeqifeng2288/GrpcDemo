using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcServer.Core
{
    /// <summary>
    /// 内存判重。
    /// </summary>
    public class MemoryDuplicate : IDuplicate
    {
        /// <summary>
        /// 判重数据容器。
        /// </summary>
        private readonly HashSet<string> _tagContainer;

        /// <summary>
        /// 日志组件。
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 自旋锁。
        /// </summary>
        private SpinLock _spinLock;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="logger">日志组件。</param>
        public MemoryDuplicate(ILogger<IDuplicate> logger)
        {
            _tagContainer = new HashSet<string>();
            _spinLock = new SpinLock();
            _logger = logger;
        }

        /// <summary>
        /// 判断标签是否已经存在。
        /// </summary>
        /// <param name="tag">标签。</param>
        /// <returns>如果标签存在则返回true。</returns>
        public bool DuplicateCheck(string tag)
        {
            return _tagContainer.Contains(tag);
        }


        /// <summary>
        /// 删除一条标签。
        /// </summary>
        /// <param name="tag">标签。</param>
        /// <returns>返回结果。</returns>
        public bool RemoveItem(string tag)
        {
            return Locker(() =>
            {
                return _tagContainer.Remove(tag);
            });
        }

        /// <summary>
        /// 将标签进入判重。
        /// </summary>
        /// <param name="tag">标签。</param>
        /// <returns>保存成功后将返回一个值。</returns>
        public bool EntryDuplicate(string tag)
        {
            var result = Locker(() =>
            {
                return _tagContainer.Add(tag);
            });

            return result;
        }

        /// <summary>
        /// 锁线程,执行线程中的方法。。
        /// </summary>
        /// <param name="expression">表达式。</param>
        private bool Locker(Func<bool> expression)
        {
            try
            {
                var locker = false;
                _spinLock.Enter(ref locker);
                return expression();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
            finally
            {
                _spinLock.Exit();
            }
        }
    }
}
