using System;
using System.Collections.Generic;
using System.Text;

namespace GrpcClient
{
    /// <summary>
    /// 随机数构造者。
    /// </summary>
    internal class RandomBulider
    {
        /// <summary>
        /// 随机数生成器。
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// 生成次数。
        /// </summary>
        private readonly int _count;

        /// <summary>
        /// 最小值。
        /// </summary>
        private readonly int _minValue;

        /// <summary>
        /// 最大值。
        /// </summary>
        private readonly int _maxValue;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="minValue">最小值。</param>
        /// <param name="maxValue">最大值。</param>
        /// <param name="count">生成次数。</param>
        /// <param name="seed">种子,随机数的起始值。如果指定了一个负数，则使用该数的绝对值。</param>
        public RandomBulider(int minValue, int maxValue, int count, int seed = 0)
        {
            _random = new Random(seed);
            _minValue = minValue;
            _maxValue = maxValue;
            _count = count;
        }

        /// <summary>
        /// 获取一个随机数。。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> Next()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _random.Next(_minValue, _maxValue);
            }
        }
    }
}
