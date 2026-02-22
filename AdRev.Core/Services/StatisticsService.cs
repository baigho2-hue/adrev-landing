using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics;

namespace AdRev.Core.Services
{
    public class StatisticsService
    {
        // --- Descriptive Statistics ---

        public Dictionary<string, double> CalculateDescriptiveStats(List<double> data)
        {
            if (data == null || data.Count == 0)
                return new Dictionary<string, double>();

            var stats = new Dictionary<string, double>();
            var s = new DescriptiveStatistics(data);
            
            stats["N"] = s.Count;
            stats["Mean"] = s.Mean;
            stats["Median"] = data.Median();
            stats["Min"] = s.Minimum;
            stats["Max"] = s.Maximum;
            stats["Variance"] = s.Variance;
            stats["StdDev"] = s.StandardDeviation;
            stats["Range"] = s.Maximum - s.Minimum;
            stats["Skewness"] = s.Skewness;
            stats["Kurtosis"] = s.Kurtosis;

            return stats;
        }

        public Dictionary<string, int> CalculateFrequencies(List<string> data)
        {
            if (data == null) return new Dictionary<string, int>();
            
            return data.GroupBy(x => x ?? "N/A")
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        // --- Normality Test ---
        public (double statistic, double pValue, bool isNormal) CalculateShapiroWilk(List<double> data)
        {
             // Shapiro-Wilk is not directly in core MathNet, using approximation or switching to Jarque-Bera which is available if N is large.
             // For simplicity and dependency reduction, we can use a basic Jarque-Bera (Skewness/Kurtosis based) which is standard for large samples,
             // or check if MathNet provides it in newer versions. MathNet doesn't have Shapiro built-in.
             // We will implement Jarque-Bera:
             if (data.Count < 2) return (0, 0, false);

             double n = data.Count;
             double s = Statistics.Skewness(data);
             double k = Statistics.Kurtosis(data);
             
             double jb = (n / 6.0) * (Math.Pow(s, 2) + Math.Pow(k, 2) / 4.0);
             double pValue = 1.0 - ChiSquared.CDF(2, jb); // 2 degrees of freedom

             return (jb, pValue, pValue > 0.05);
        }

        // --- Inferential Statistics ---

        public double CalculatePearsonCorrelation(List<double> x, List<double> y)
        {
            return Correlation.Pearson(x, y);
        }

        public double CalculateSpearmanCorrelation(List<double> x, List<double> y)
        {
            return Correlation.Spearman(x, y);
        }

        /// <summary>
        /// Student's T-Test (Independent Samples)
        /// Returns t-statistic, dof, and P-Value (Two-Tailed)
        /// </summary>
        public (double tStat, double dof, double pValue) CalculateTTest(List<double> group1, List<double> group2)
        {
            if (group1.Count < 2 || group2.Count < 2) return (0, 0, 1.0);

            var s1 = new DescriptiveStatistics(group1);
            var s2 = new DescriptiveStatistics(group2);

            // Welch's T-Test (Unequal Variance assumption is safer)
            double vn1 = s1.Variance / s1.Count;
            double vn2 = s2.Variance / s2.Count;
            double stdErr = Math.Sqrt(vn1 + vn2);
            double t = (s1.Mean - s2.Mean) / stdErr;

            // Welch-Satterthwaite DoF
            double dofNumerator = Math.Pow(vn1 + vn2, 2);
            double dofDenominator = (Math.Pow(vn1, 2) / (s1.Count - 1)) + (Math.Pow(vn2, 2) / (s2.Count - 1));
            double dof = dofNumerator / dofDenominator;

            // Two-tailed P-Value
            double pValue = 2.0 * (1.0 - StudentT.CDF(0, 1, dof, Math.Abs(t)));

            return (t, dof, pValue);
        }

        /// <summary>
        /// Paired Samples T-Test
        /// </summary>
        public (double tStat, double dof, double pValue) CalculatePairedTTest(List<double> before, List<double> after)
        {
            if (before.Count != after.Count || before.Count < 2) return (0, 0, 1.0);

            var diffs = new List<double>();
            for(int i=0; i<before.Count; i++) diffs.Add(after[i] - before[i]);

            double meanDiff = diffs.Average();
            double stdErr = Statistics.StandardDeviation(diffs) / Math.Sqrt(diffs.Count);
            double t = meanDiff / stdErr;
            double dof = diffs.Count - 1;

            double pValue = 2.0 * (1.0 - StudentT.CDF(0, 1, dof, Math.Abs(t)));

            return (t, dof, pValue);
        }

        /// <summary>
        /// One-Way ANOVA
        /// Returns F-Statistic, dfBetween, dfWithin, P-Value
        /// </summary>
        public (double fStat, int dfBetween, int dfWithin, double pValue) CalculateOneWayANOVA(List<List<double>> groups)
        {
            var validGroups = groups.Where(g => g.Count > 0).ToList();
            int k = validGroups.Count;
            if (k < 2) return (0, 0, 0, 1.0);

            int N = validGroups.Sum(g => g.Count);
            double grandMean = validGroups.SelectMany(x => x).Average();

            double ssBetween = 0;
            double ssWithin = 0;

            foreach(var g in validGroups)
            {
                ssBetween += g.Count * Math.Pow(g.Average() - grandMean, 2);
                ssWithin += g.Sum(x => Math.Pow(x - g.Average(), 2));
            }

            int dfBetween = k - 1;
            int dfWithin = N - k;

            if (dfBetween == 0 || dfWithin == 0) return (0, 0, 0, 1.0);

            double msBetween = ssBetween / dfBetween;
            double msWithin = ssWithin / dfWithin;
            double f = msBetween / msWithin;

            double pValue = 1.0 - FisherSnedecor.CDF(dfBetween, dfWithin, f);

            return (f, dfBetween, dfWithin, pValue);
        }

        /// <summary>
        /// Mann-Whitney U Test with Normal Approximation
        /// </summary>
        public (double uStat, double zScore, double pValue) CalculateMannWhitneyU(List<double> group1, List<double> group2)
        {
             // Same manual implementation but with proper P-Value from Normal Distribution
             if (group1.Count == 0 || group2.Count == 0) return (0, 0, 1.0);
             
             // ... Rank Logic (Reusing previous logic for brevity, optimized) ...
             var combined = group1.Select(x => new { Val = x, Group = 1 })
                                .Concat(group2.Select(x => new { Val = x, Group = 2 }))
                                .OrderBy(x => x.Val)
                                .ToList();
            
             double[] ranks = new double[combined.Count];
             for (int i = 0; i < combined.Count; ) {
                 int j = i + 1;
                 while (j < combined.Count && Math.Abs(combined[j].Val - combined[i].Val) < 1e-9) j++;
                 double rankMean = (i + 1 + j) / 2.0;
                 for (int m = i; m < j; m++) ranks[m] = rankMean;
                 i = j;
             }

             double rankSum1 = 0;
             for(int i=0; i<combined.Count; i++) if (combined[i].Group == 1) rankSum1 += ranks[i];
             
             int n1 = group1.Count, n2 = group2.Count;
             double u1 = rankSum1 - (n1 * (n1 + 1)) / 2.0;
             double u = Math.Min(u1, (n1 * n2) - u1);

             double mu = (n1 * n2) / 2.0;
             double sigma = Math.Sqrt((n1 * n2 * (n1 + n2 + 1)) / 12.0);
             double z = (u - mu) / sigma;

             // Two-tailed P-Value from Normal
             double pValue = 2.0 * (1.0 - Normal.CDF(0, 1, Math.Abs(z)));

             return (u, z, pValue);
        }

        /// <summary>
        /// Chi-Square Test of Independence
        /// Returns Chi-Square statistic, DoF, P-Value
        /// </summary>
        public (double chiSquare, int dof, double pValue) CalculateChiSquare(List<string> var1, List<string> var2)
        {
             if (var1.Count != var2.Count) return (0, 0, 1.0);

             var distinct1 = var1.Distinct().OrderBy(x => x).ToList();
             var distinct2 = var2.Distinct().OrderBy(x => x).ToList();
             int rows = distinct1.Count; 
             int cols = distinct2.Count;
             if (rows < 2 || cols < 2) return (0,0, 1.0);

             int[,] observed = new int[rows, cols];
             int[] rowTotals = new int[rows];
             int[] colTotals = new int[cols];
             int grandTotal = var1.Count;

             for (int i = 0; i < grandTotal; i++)
             {
                 int r = distinct1.IndexOf(var1[i]);
                 int c = distinct2.IndexOf(var2[i]);
                 observed[r, c]++;
                 rowTotals[r]++;
                 colTotals[c]++;
             }

             double chiSq = 0;
             for (int r = 0; r < rows; r++)
             {
                 for (int c = 0; c < cols; c++)
                 {
                     double expected = (double)rowTotals[r] * colTotals[c] / grandTotal;
                     if (expected > 0) chiSq += Math.Pow(observed[r, c] - expected, 2) / expected;
                 }
             }

             int dof = (rows - 1) * (cols - 1);
             double pValue = 1.0 - ChiSquared.CDF(dof, chiSq);
             return (chiSq, dof, pValue);
        }

        public double CalculateCronbachAlpha(List<List<double>> itemScores)
        {
            // Same manual implementation (MathNet doesn't have Cronbach specifically standard)
             int k = itemScores.Count; 
             if (k < 2) return 0;
             int n = itemScores[0].Count;
             if (n < 2) return 0;

             double sumItemVariances = 0;
             var subjectTotalScores = new double[n];

             for(int j=0; j<k; j++)
             {
                 var scores = itemScores[j];
                 double sMean = scores.Average();
                 double sVar = scores.Sum(x => Math.Pow(x-sMean, 2)) / (n - 1);
                 sumItemVariances += sVar;
                 for(int i=0; i<n; i++) subjectTotalScores[i] += scores[i];
             }

             double meanTotal = subjectTotalScores.Average();
             double totalVariance = subjectTotalScores.Sum(x => Math.Pow(x - meanTotal, 2)) / (n - 1);

             if (totalVariance == 0) return 0;
             return (k / (double)(k - 1)) * (1 - (sumItemVariances / totalVariance));
        }

        // --- Regression ---
        // Using MathNet Fit for Regression

        public (double slope, double intercept, double rSquared) CalculateLinearRegression(List<double> x, List<double> y)
        {
            if (x.Count != y.Count) return (0,0,0);
            var p = Fit.Line(x.ToArray(), y.ToArray());
            var intercept = p.Item1;
            var slope = p.Item2;
            var r2 = GoodnessOfFit.RSquared(x.Select(val => intercept + slope * val), y);
            return (slope, intercept, r2);
        }

        /// <summary>
        /// Simple Logistic Regression using Gradient Descent (Binary Y: 0 or 1)
        /// Returns (Beta0, Beta1, Accuracy)
        /// </summary>
        public (double beta0, double beta1, double accuracy) CalculateLogisticRegression(List<double> x, List<double> y)
        {
            if (x.Count != y.Count) return (0,0,0);

            // Scale X for stability
            double minX = x.Min();
            double maxX = x.Max();
            var scaledX = x.Select(val => (val - minX) / (maxX - minX)).ToList();

            double b0 = 0; // Intercept
            double b1 = 0; // Slope
            double learningRate = 0.1;
            int epochs = 1000;
            int n = x.Count;

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double sumErr0 = 0;
                double sumErr1 = 0;

                for (int i = 0; i < n; i++)
                {
                    double linear = b0 + b1 * scaledX[i];
                    double prediction = 1.0 / (1.0 + Math.Exp(-linear)); // Sigmoid
                    double error = prediction - y[i];

                    sumErr0 += error;
                    sumErr1 += error * scaledX[i];
                }

                b0 -= learningRate * (sumErr0 / n);
                b1 -= learningRate * (sumErr1 / n);
            }

            // Rescale Beta1 to original scale roughly (approximation)
            b1 = b1 / (maxX - minX);
            b0 = b0 - b1 * minX;

            // Calculate Accuracy
            int correct = 0;
            for(int i=0; i<n; i++)
            {
                double prob = 1.0 / (1.0 + Math.Exp(-(b0 + b1*x[i])));
                double predictedClass = prob >= 0.5 ? 1.0 : 0.0;
                if (Math.Abs(predictedClass - y[i]) < 0.1) correct++;
            }

            return (b0, b1, (double)correct/n);
        }

        public (double[] betas, double rSquared) CalculateMultipleLinearRegression(List<double[]> x, List<double> y)
        {
             if (x.Count == 0) return (new double[0], 0);
             // Flatten X for MathNet
             int n = x.Count;
             int p = x[0].Length;
             
             // MathNet MultiDim Regression requires double[][]
             var xArr = x.ToArray();
             var yArr = y.ToArray();
             
             var betas = Fit.MultiDim(xArr, yArr, intercept: true);
             // Betas from Fit.MultiDim: [Intercept, b1, b2...]
             
             // Calculate R2 manually or via localized prediction
             double ssTotal = y.Sum(val => Math.Pow(val - y.Average(), 2));
             double ssRes = 0;
             for(int i=0; i<n; i++) {
                 double pred = betas[0];
                 for(int j=0; j<p; j++) pred += betas[j+1] * x[i][j];
                 ssRes += Math.Pow(y[i] - pred, 2);
             }
             double r2 = 1.0 - (ssRes / ssTotal);
             
             return (betas, r2);
        }

        public string GetSignificanceLevel(double pValue)
        {
            if (pValue < 0.001) return "*** (p < 0.001)";
            if (pValue < 0.01) return "** (p < 0.01)";
            if (pValue < 0.05) return "* (p < 0.05)";
            return "NS (Non Significatif)";
        }

        public string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) return number.ToString();
            if (number < 1) return string.Empty;

            string[] thousands = { "", "M", "MM", "MMM" };
            string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

            return thousands[number / 1000] + hundreds[(number % 1000) / 100] + tens[(number % 100) / 10] + ones[number % 10];
        }
    }
}
