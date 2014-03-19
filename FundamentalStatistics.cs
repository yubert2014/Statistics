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
        /// 調和平均
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double HarmonicMean(IEnumerable<double> data)
        {
            // 一時変数
            double temp = 0.0;

            // 配列型に変更
            var dataArray = data.ToArray();

            // 配列分ループしながら、逆数を加算
            for (int i = 0; i < dataArray.Length; i++)
            {
                temp += 1.0 / dataArray[i];
            }

            // 長さで割る
            temp = temp / dataArray.Length;

            // 調和平均を返却
            return 1.0 / temp;
        }


        /// <summary>
        /// 幾何平均
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double GeometricMean(IEnumerable<double> data)
        {
            // 配列型に変更
            var dataArray = data.ToArray();

            double temp = dataArray[0];

            // 配列分ループ
            for (int i = 1; i < dataArray.Length; i++)
            {

                // すべての配列を掛け算
                temp *= dataArray[i];
            }

            // 根を取る
            return Math.Pow(temp, 1.0 / dataArray.Length);

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

        // ジニ係数

        /// <summary>
        /// ジニ係数の計算
        /// </summary>
        public static double GiniCoefficient(IEnumerable<double> data)
        {
            // 配列に変更
            var x = data.ToArray();

            // 長さ
            int n = x.Length;

            // ソート
            Array.Sort(x);

            // 累積度数を取る
            double temp = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                temp += x[i];
                x[i] = temp;
            }

            // 累積相対度数(先頭に0を加える)
            double[] xx = new double[x.Length + 1];
            for (int i = 0; i < x.Length; i++)
            {
                xx[i + 1] = x[i] / x[n - 1];
            }

            // 0 ～ 1 を等間隔に区切ったベクトルを作る
            double divSize = 1.0 / x.Length;
            double[] y = new double[x.Length + 1];
            for (int i = 0; i <= x.Length; i++)
            {
                y[i] = i * divSize;
            }

            // xx - y
            double[] z = new double[xx.Length];
            for (int i = 0; i < xx.Length; i++)
            {
                z[i] = y[i] - xx[i];
            }

            // ジニ係数の計算
            double GiniValue = 2 * z.Sum() / n;

            return GiniValue;
        }

    }
}
