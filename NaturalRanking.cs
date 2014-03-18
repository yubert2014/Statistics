using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCSharp
{
    /// <summary>
    /// JavaのNaturalRankingを参考にした
    /// </summary>
    public class NaturalRanking
    {
        /// <summary>
        /// Tie（タイ）の扱い
        /// </summary>
        public enum TiesStrategy
        {

            /** Ties assigned sequential ranks in order of occurrence */
            SEQUENTIAL,

            /** Ties get the minimum applicable rank */
            MINIMUM,

            /** Ties get the maximum applicable rank */
            MAXIMUM,

            /** Ties get the average of applicable ranks */
            AVERAGE,

        }

        // TiesStrategy
        TiesStrategy tiesStrategy;


        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public NaturalRanking()
        {
            tiesStrategy = TiesStrategy.AVERAGE;
        }

        /// <summary>
        /// ランク付け
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public double[] rank(double[] data)
        {

            // 初期位置の記録
            IntDoublePair[] ranks = new IntDoublePair[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ranks[i] = new IntDoublePair(data[i], i);
            }

            // ソート
            Array.Sort(ranks);


            // Walk the sorted array, filling output array using sorted positions,
            // resolving ties as we go
            double[] outValue = new double[ranks.Length];
            // arrayをソートする際の位置
            int pos = 1;
            outValue[ranks[0].getPosition()] = pos;
            List<int> tiesTrace = new List<int>();
            tiesTrace.Add(ranks[0].getPosition());
            for (int i = 1; i < ranks.Length; i++)
            {
                //if (Double.compare(ranks[i].getValue(), ranks[i - 1].getValue()) > 0)
                if (ranks[i].getValue() > ranks[i - 1].getValue())
                {
                    // tie sequence has ended (or had length 1)
                    pos = i + 1;
                    if (tiesTrace.Count > 1)
                    {  // if seq is nontrivial, resolve
                        resolveTie(outValue, tiesTrace);
                    }
                    tiesTrace = new List<int>();
                    tiesTrace.Add(ranks[i].getPosition());
                }
                else
                {
                    // tie sequence continues
                    tiesTrace.Add(ranks[i].getPosition());
                }
                outValue[ranks[i].getPosition()] = pos;
            }
            if (tiesTrace.Count > 1)
            {  // handle tie sequence at end
                resolveTie(outValue, tiesTrace);
            }

            return outValue;
        }

        /// <summary>
        /// Tieの解決
        /// </summary>
        /// <param name="ranks"></param>
        /// <param name="tiesTrace"></param>
        private void resolveTie(double[] ranks, List<int> tiesTrace)
        {

            // constant value of ranks over tiesTrace
            double c = ranks[tiesTrace[0]];

            // length of sequence of tied ranks
            int length = tiesTrace.Count;

            switch (tiesStrategy)
            {
                case TiesStrategy.AVERAGE:
                    // 平均値を採用
                    fill(ranks, tiesTrace, (2 * c + length - 1) / 2d);
                    break;
                case TiesStrategy.MAXIMUM:
                    // 最大値を採用
                    fill(ranks, tiesTrace, c + length - 1);
                    break;
                case TiesStrategy.MINIMUM:
                    // 最小値を採用
                    fill(ranks, tiesTrace, c);
                    break;
                case TiesStrategy.SEQUENTIAL:
                    // Fill sequentially from c to c + length - 1 walk and fill
                    long f = (long)Math.Round(c);
                    int i = 0;
                    foreach (int intValue in tiesTrace)
                    {
                        ranks[intValue] = f + i++;
                    }
                    break;
                default:
                    // エラーを投げる
                    throw new Exception("MathInternalError()");
            }
        }

        /// <summary>
        /// 値の埋め込み
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tiesTrace"></param>
        /// <param name="value"></param>
        private void fill(double[] data, List<int> tiesTrace, double value)
        {
            // リスト分ループ
            foreach (int intValue in tiesTrace)
            {
                data[intValue] = value;
            }
        }

        /// <summary>
        /// IntDoublePair
        /// </summary>
        private class IntDoublePair : IComparable
        {

            // 値
            private double value;

            // ポジション（インデックス）
            private int position;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="value"></param>
            /// <param name="position"></param>
            public IntDoublePair(double value, int position)
            {
                this.value = value;
                this.position = position;
            }

            /// <summary>
            /// 比較
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            //public int CompareTo(IntDoublePair other)
            public int CompareTo(object obj)
            {
                IntDoublePair other = obj as IntDoublePair;


                if (value == other.value)
                {
                    return 0;
                }
                else if (value > other.value)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }

            }

            /// <summary>
            /// 値の取得
            /// </summary>
            /// <returns></returns>
            public double getValue()
            {
                return value;
            }

            /// <summary>
            /// ポジションの取得
            /// </summary>
            /// <returns></returns>
            public int getPosition()
            {
                return position;
            }
        }

    }
}
