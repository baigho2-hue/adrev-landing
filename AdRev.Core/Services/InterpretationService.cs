using System;
using System.Collections.Generic;
using System.Linq;
using AdRev.Domain.Variables;

namespace AdRev.Core.Services
{
    public class InterpretationService
    {
        public string InterpretDescriptive(StudyVariable variable, Dictionary<string, double> stats)
        {
            if (stats == null || !stats.ContainsKey("Mean")) return string.Empty;

            double mean = stats["Mean"];
            double stdDev = stats["StdDev"];
            double median = stats["Median"];
            double min = stats["Min"];
            double max = stats["Max"];
            double skewness = stats.ContainsKey("Skewness") ? stats["Skewness"] : 0;

            string intro = $"📝 **Interprétation :** Pour la variable '{variable.Name}', ";
            string central = $"la valeur moyenne est de {mean:F2}, ce qui représente la tendance centrale de votre échantillon. ";
            
            string dispersion = "";
            if (stdDev < (mean * 0.1)) dispersion = "L'écart-type est faible, indiquant que les données sont très regroupées autour de la moyenne (forte homogénéité). ";
            else if (stdDev > (mean * 0.5)) dispersion = "L'écart-type est élevé, ce qui suggère une grande dispersion des données (forte hétérogénéité). ";
            else dispersion = "La dispersion est modérée. ";

            string shape = "";
            if (Math.Abs(mean - median) < (stdDev * 0.1)) shape = "La moyenne est proche de la médiane, suggérant une distribution relativement symétrique. ";
            else if (mean > median) shape = "La moyenne est supérieure à la médiane, indiquant une asymétrie positive (étalement vers les valeurs élevées). ";
            else shape = "La moyenne est inférieure à la médiane, suggérant une asymétrie négative (étalement vers les valeurs basses). ";

            string extremes = "";
            double distMax = Math.Abs(max - mean);
            double distMin = Math.Abs(min - mean);

            if (distMax > distMin * 2)
                extremes = $"On note une valeur maximale particulièrement élevée ({max:F2}) qui s'éloigne significativement de la moyenne. ";
            else if (distMin > distMax * 2)
                extremes = $"On note une valeur minimale particulièrement basse ({min:F2}) qui pèse sur la distribution. ";
            else
                extremes = $"Les valeurs extrêmes ({min:F2} et {max:F2}) sont réparties de manière équilibrée autour du centre. ";

            return intro + central + dispersion + shape + extremes;
        }

        public string InterpretFrequencies(StudyVariable variable, List<KeyValuePair<string, double>> frequencies)
        {
            if (frequencies == null || frequencies.Count == 0) return string.Empty;

            var sorted = frequencies.OrderByDescending(f => f.Value).ToList();
            var dominant = sorted[0];
            var rarest = sorted.Last();

            string interpretation = $"📝 **Interprétation :** ";
            
            // Highlight Most Frequent
            interpretation += $"Le groupe le plus représenté est '{dominant.Key}' ({dominant.Value:F1}%). ";
            
            // Highlight Least Frequent (if significant)
            if (sorted.Count > 1)
            {
                if (rarest.Value < 5)
                    interpretation += $"À l'inverse, la catégorie '{rarest.Key}' est très minoritaire ({rarest.Value:F1}%), ce qui peut indiquer un événement rare ou une sous-représentation. ";
                else
                    interpretation += $"La catégorie la moins fréquente est '{rarest.Key}' avec {rarest.Value:F1}%. ";
            }

            // Concentration analysis
            if (sorted.Count > 3)
            {
                double top3Sum = sorted.Take(3).Sum(f => f.Value);
                if (top3Sum > 80) interpretation += $"On observe une forte concentration : les 3 premières catégories représentent {top3Sum:F1}% de l'ensemble.";
            }

            return interpretation;
        }

        public string InterpretHypothesis(string testName, double pValue, string? variable1 = null, string? variable2 = null)
        {
            string context = (variable1 != null && variable2 != null) ? $"entre '{variable1}' et '{variable2}'" : "";
            string result = pValue < 0.05 ? "statistiquement significative" : "non statistiquement significative";
            
            string interpretation = $"📝 **Analyse :** La différence/relation {context} est {result} (p = {pValue:F4}). ";

            if (pValue < 0.05)
            {
                interpretation += "Il est très probable que l'effet observé ne soit pas dû au hasard. Vous pouvez rejeter l'hypothèse nulle avec un risque d'erreur inférieur à 5%.";
            }
            else
            {
                interpretation += "Nous ne pouvons pas conclure à une différence réelle. L'effet observé pourrait être dû à des fluctuations aléatoires de l'échantillonnage.";
            }

            return interpretation;
        }
    }
}
