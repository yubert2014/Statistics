using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCSharp
{
    /// <summary>
    /// Levene検定
    /// </summary>
    class Levene
    {

        /// <summary>
        /// Levene検定
        /// </summary>
        /// <param name="dataList">データリスト</param>
        /// <param name="type">0 : メディアン, 1 : 平均</param>
        public static void LeveneTest(List<double[]> dataList, int type = 0)
        {
            // 変換後のリスト
            List<double[]> afterList = new List<double[]>();

            // データ分ループ
            double average = 0.0;
            double median = 0.0;
            double[] afterData;

            // ここで平均、中央値で分岐
            switch (type)
            {
                case 0:
                    // メディアン
                    foreach (double[] data in dataList)
                    {
                        // メディアン
                        median = MathNet.Numerics.Statistics.Statistics.Median(data);
                        // データ数ループして、メディアンを引いていく
                        afterData = new double[data.Length];
                        for (int i = 0; i < data.Length; i++)
                        {
                            afterData[i] = Math.Abs(data[i] - median);
                        }
                        // リストに詰める
                        afterList.Add(afterData);
                    }
                    break;
                case 1:
                    // 平均値

                    foreach (double[] data in dataList)
                    {
                        // 平均値を計算 
                        average = data.Average();
                        // データ数ループして、平均値を引いていく
                        afterData = new double[data.Length];
                        for (int i = 0; i < data.Length; i++)
                        {
                            afterData[i] = Math.Abs(data[i] - average);
                        }
                        // リストに詰める
                        afterList.Add(afterData);
                    }
                    break;
            }

            // 全体のデータ個数
            int n = dataList.Count * dataList[0].Length;
            // 各群のデータ個数
            int ni = dataList[0].Length;
            // 群の数
            int k = dataList.Count;

            // それぞれの群の分散を計算していく
            List<double> varianceList = new List<double>();
            foreach (double[] data in afterList)
            {
                varianceList.Add(MathNet.Numerics.Statistics.Statistics.Variance(data));
            }

            // 群内変動
            double sw = 0.0;
            foreach (double data in varianceList)
            {
                sw += (ni - 1) * data;
            }

            // 群内変動の自由度
            int dfw = n - k;
            // 群間変動の自由度
            int dfb = k - 1;

            // afterListを連結
            List<double> linkAfterList = new List<double>();
            foreach (double[] data in afterList)
            {
                linkAfterList.AddRange(data);
            }
            double[] afterArray = linkAfterList.ToArray();

            // 検定統計量
            double f = ((MathNet.Numerics.Statistics.Statistics.Variance(afterArray) * (n - 1) - sw) / dfb) / (sw / dfw);

            // P-Value
            var fDist = new MathNet.Numerics.Distributions.FisherSnedecor(dfb, dfw);
            double pValue = 1.0 - fDist.CumulativeDistribution(f);

            // 結果
            Console.WriteLine("検定統計量 : " + f);
            Console.WriteLine("有意確率 : " + pValue);
        }
    }
}
