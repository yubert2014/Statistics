using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatTest
{

    /// <summary>
    /// MEMクラス
    /// </summary>
    public class MEMClass
    {
        private int Lmax = 1001;        // 自己相関を求める最大ラグ数
        private int Mmax;               // 予測誤差フィルター項数
        private int Nmax;               // データ数
        private int NYQ;                // ナイキスト周波数（サンプリング周波数の1/2）
        private double DT;              // 1.0/Splngf;
        private double DT2;             // 2*DT;
        private double PD1;             // 2*Math.PI*DT;
        private double PM;              // 予測誤差フィルタの平均出力
        private double Xmax;
        private double[] b1;
        private double[] b2;
        private double[] g;             // 予測誤差フィルタ
        private double[] c;             // 自己相関係数
        private double[] fpe;           // 予測誤差の分散
        private double[] aic;           // 赤池情報基準（統計モデルの良さを評価するための指標）
        private double[] e;             // MEM計算結果

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="lagMax"></param>
        public MEMClass(int lagMax)
        {
            // ラグの設定
            Lmax = lagMax;
        }

        /// <summary>
        /// SetNN
        /// </summary>
        /// <param name="nn">データ数</param>
        /// <param name="splgf">サンプリング周波数</param>
        /// <param name="mmax"></param>
        public void setNN(int nn, int splgf, int mmax)
        {
            NYQ = splgf / 2;            // ナイキスト周波数
            e = new double[NYQ + 1];    // それ+1の大きさの配列を作る

            Nmax = nn;
            DT = 1.0 / splgf;
            PD1 = 2 * Math.PI;
            DT2 = 2 * DT;
            b1 = new double[Nmax + 1];
            b2 = new double[Nmax + 1];

            //initMEM(mmax);
            Mmax = mmax;                    // データ数
            g = new double[Mmax + 1];       // 予測誤差フィルタ
            fpe = new double[Mmax + 1];     // 予測誤差の分散
            aic = new double[Mmax + 1];     // 赤池情報基準（統計モデルの良さを評価するための指標）
            c = new double[Lmax + 1];       // 自己相関係数
        }

        /// <summary>
        /// MEMの計算
        /// </summary>
        /// <param name="sf">選択開始位置</param>
        /// <param name="dt">wave Data</param>
        public void calcMem(int sf, int[] dt)
        {
            read(sf, dt);
            burg();
            mem();
        }

        /// <summary>
        /// データ読み込み
        /// </summary>
        /// <param name="sf"></param>
        /// <param name="dt"></param>
        public void read(int sf, int[] dt)
        { // averge済
            double sum = 0;
            for (int i = 0; i < Nmax; i++)
                sum += Math.Pow(dt[i + sf], 2);
            PM = c[1] = sum / Nmax;

            b1[1] = dt[sf];
            for (int i = 2; i <= Nmax; i++)
                b1[i] = b2[i - 1] = dt[i - 1 + sf];
        }

        /// <summary>
        /// burg
        /// </summary>
        public void burg()
        {
            double stn, std, sum;
            double[] gg = new double[Mmax + 1];

            for (int m = 1; m <= Mmax; m++)
            {
                stn = std = 0;
                for (int i = 1; i <= Nmax - m; i++)
                {
                    stn += b1[i] * b2[i];
                    std += Math.Pow(b1[i], 2) + Math.Pow(b2[i], 2);
                }
                g[m] = -2.0 * stn / std;
                PM *= 1.0 - Math.Pow(g[m], 2);

                if (m != 1)
                {
                    for (int k = 1; k <= m - 1; k++)
                        g[k] = gg[k] + g[m] * gg[m - k];
                }

                for (int i = 1; i <= Nmax - m - 1; i++)
                {
                    b1[i] += g[m] * b2[i];
                    b2[i] = b2[i + 1] + g[m] * b1[i + 1];
                }
                Array.Copy(g, 1, gg, 1, m);

                sum = 0;
                for (int i = 1; i <= m; i++)
                    sum -= c[m + 1 - i] * g[i];
                c[m + 1] = sum;

                if (m != Nmax - 1)
                {
                    fpe[m] = (Nmax + m + 1) / (Nmax - m - 1) * PM;
                    aic[m] = Nmax * Math.Log(PM) + 2.0 * m;
                }
            }
        }

        /// <summary>
        /// MEM
        /// </summary>
        public void mem()
        {
            double f0 = PD1 / (Nmax - 1);
            double pd2 = DT2 * PM;
            ComplexClass c0 = new ComplexClass(0.0, 1.0);
            ComplexClass ci, cj, ex, sum, sum0 = new ComplexClass(1.0, 0);

            Xmax = 0;
            for (int i = 1; i <= NYQ; i++)
            {
                sum = sum0;
                ci = c0.multiply(f0 * (i - 1));
                for (int j = 1; j <= Mmax; j++)
                {
                    cj = ci.multiply(j);
                    ex = cj.exponent();
                    sum = sum.add(ex.multiply(g[j]));
                }
                e[i] = pd2 / Math.Pow(sum.magnitude(), 2);

                if (Xmax < e[i])
                    Xmax = e[i];
            }

            if (Mmax < Lmax)
            {
                double sm;
                for (int l = Mmax + 1; l < Lmax; l++)
                {
                    sm = 0;
                    for (int i = 1; i <= Mmax; i++)
                    {
                        sm -= c[l + 1 - i] * g[i];
                    }
                    c[l + 1] = sm;
                }
            }
        }

        /// <summary>
        /// 計算結果の取得
        /// </summary>
        /// <returns></returns>
        public double[] getData()
        {
            return e;
        }

        /// <summary>
        /// 計算結果の取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public double getData(int num)
        {
            return e[num];
        }
        public double getXmax()
        {
            return Xmax;
        }

        /// <summary>
        /// 予測誤差フィルタの取得
        /// </summary>
        /// <returns></returns>
        public double[] getG()
        {
            return g;
        }

        /// <summary>
        /// 予測誤差フィルタの取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public double getG(int num)
        {
            return g[num];
        }

        /// <summary>
        /// 自己相関係数の取得
        /// </summary>
        /// <returns></returns>
        public double[] getC()
        {
            return c;
        }

        /// <summary>
        /// 自己相関係数の取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public double getC(int num)
        {
            return c[num];
        }

        /// <summary>
        /// 予測誤差の分散の取得
        /// </summary>
        /// <returns></returns>
        public double[] getFPE()
        {
            return fpe;
        }

        /// <summary>
        /// 予測誤差の分散の取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public double getFPE(int num)
        {
            return fpe[num];
        }

        /// <summary>
        /// 赤池情報基準の取得
        /// </summary>
        /// <returns></returns>
        public double[] getAIC()
        {
            return aic;
        }

        /// <summary>
        /// 赤池情報基準の取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public double getAIC(int num)
        {
            return aic[num];
        }
    }

    /// <summary>
    /// 複素数クラス
    /// </summary>
    public class ComplexClass
    {
        public double real;
        public double imag;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ComplexClass()
        {
            real = 0.0;
            imag = 0.0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="re"></param>
        public ComplexClass(double re)
        {
            real = re;
            imag = 0.0;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        public ComplexClass(double re, double im)
        {
            real = re;
            imag = im;
        }

        /// <summary>
        /// リアルパートの取得 
        /// </summary>
        /// <returns></returns>
        public double re()
        {
            return real;
        }

        /// <summary>
        /// イマジナリパートの取得
        /// </summary>
        /// <returns></returns>
        public double im()
        {
            return imag;
        }

        /// <summary>
        /// 円を作る（実軸、虚数軸）
        /// </summary>
        /// <param name="re"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public ComplexClass createPolar(double re, double theta)
        {
            return new ComplexClass(re * Math.Cos(theta), re * Math.Sin(theta));
        }

        /// <summary>
        /// 加算（実＋実、虚＋虚）
        /// </summary>
        /// <param name="adder"></param>
        /// <returns></returns>
        public ComplexClass add(ComplexClass adder)
        {
            return new ComplexClass(real + adder.real, imag + adder.imag);
        }

        /// <summary>
        /// 減算（実ー実、虚ー虚）
        /// </summary>
        /// <param name="sub"></param>
        /// <returns></returns>
        public ComplexClass substract(ComplexClass sub)
        {
            return new ComplexClass(real - sub.real, imag - sub.imag);
        }

        /// <summary>
        /// 掛け算（定数倍）
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public ComplexClass multiply(double a)
        {
            return new ComplexClass(real * a, imag * a);
        }

        /// <summary>
        /// 掛け算（実×実、虚×虚）
        /// </summary>
        /// <param name="mul"></param>
        /// <returns></returns>
        public ComplexClass multiply(ComplexClass mul)
        {
            return new ComplexClass(real * mul.real - imag * mul.imag,
                       real * mul.imag + imag * mul.real);
        }

        /// <summary>
        /// 割り算（実÷実、虚÷虚）
        /// </summary>
        /// <param name="div"></param>
        /// <returns></returns>
        public ComplexClass divide(ComplexClass div)
        {
            return new ComplexClass((real * div.real + imag * div.imag) / div.square(),
                       (imag * div.real - real * div.imag) / div.square());
        }

        /// <summary>
        /// Expornent
        /// </summary>
        /// <returns></returns>
        public ComplexClass exponent()
        {
            double ex = Math.Exp(real);

            return new ComplexClass(ex * Math.Cos(imag), ex * Math.Sin(imag));
        }

        /// <summary>
        /// 虚数部と実数部を使い、絶対値を求める。
        /// </summary>
        /// <returns></returns>
        public double magnitude()
        {
            return Math.Sqrt(real * real + imag * imag);
        }

        /// <summary>
        /// 二乗和（絶対値の二乗）
        /// </summary>
        /// <returns></returns>
        public double square()
        {
            return real * real + imag * imag;
        }

        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public String toString()
        {
            StringBuilder sb = new StringBuilder().Append(real);

            return sb.Append("+").Append(imag).Append('i').ToString();
        }
    }
}
