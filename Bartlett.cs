using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCSharp
{
    /// <summary>
    /// バートレット検定
    /// </summary>
    class Bartlett
    {
        /// <summary>
        /// 検定
        /// </summary>
        /// <param name="dataList"></param>
        public static void BartlettTest(List<double[]> dataList)
        {

            // 全体の長さ
            int k = dataList.Count;

            int nTotal = 0;
            nTotal = dataList.Select(num => num.Length - 1).Sum();

            // 検定統計量の計算
            double vTotal = 0.0;
            for (int i = 0; i < dataList.Count; i++)
            {
                vTotal += ((dataList[i].Length - 1) * MathNet.Numerics.Statistics.Statistics.Variance(dataList[i]));
            }
            vTotal /= nTotal;

            // sum(n * log(v))
            double sumNLogV = 0.0;

            // sum(1.0 / n)
            double sumOneN = 0.0;
            for (int i = 0; i < dataList.Count; i++)
            {
                sumNLogV += (dataList[i].Length - 1) * Math.Log(MathNet.Numerics.Statistics.Statistics.Variance(dataList[i]));
                sumOneN += 1.0 / (dataList[i].Length - 1.0);
            }

            // 検定統計量
            double STATISTIC = ((nTotal * Math.Log(vTotal) - sumNLogV) / (1 + (sumOneN - 1.0 / nTotal) / (3.0 * (k - 1))));


            /**
             * 有意確率まで計算
             * */
            // 自由度
            double PARAMETER = k - 1;

            // 有意確率
            var pChiSq = new MathNet.Numerics.Distributions.ChiSquare(PARAMETER);
            double PVAL = 1.0 - pChiSq.CumulativeDistribution(STATISTIC);

            // 結果
            Console.WriteLine(String.Format("検定統計量 : {0}", STATISTIC));
            Console.WriteLine(String.Format("有意確率 : {0}", PVAL));
        }
    }
}
