using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCSharp
{
    class FundamentalStatistics
    {
        /// <summary>
        /// 平均値
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Mean(IEnumerable<double> data)
        {
            // 平均を計算
            return (double)data.Sum() / data.Count();
        }


        /// <summary>
        /// 標本分散
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Variance(IEnumerable<double> data)
        {
            if (data.Count() < 2)
            {
                throw new Exception("サンプルサイズが不足しています");
            }

            // 平均を計算
            double averageValue = FundamentalStatistics.Mean(data);

            // 分散を計算
            return data.Select(num => Math.Pow(averageValue - num, 2)).Sum() / (data.Count() - 1);
        }
    }
}
