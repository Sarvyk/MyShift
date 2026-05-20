using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Extensions
{
    internal static class EnumerableExtension
    {
        /// <summary>
        /// Собрать последовательность для пагинации.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <param name="batchNumber"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (batchSize < 0 || batchNumber < 0)
                throw new ArgumentOutOfRangeException("Размер последовательности или позиция не может быть меньше нуля");
            return source.Skip(batchSize * batchNumber).Take(batchSize);
        }
    }
}
