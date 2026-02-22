using System;
using System.Collections.Generic;
using AdRev.Core.Services;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;

namespace AdRev.Verify
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ADREV LOGIC VERIFICATION ===");
            
            var service = new RecodeService();
            
            // 1. Test Quantitative Recoding (Range)
            var ages = new List<object> { 12, 18, 25, 45, 10, null, "invalid" };
            var ageInstructions = new List<RecodeInstruction>
            {
                new RecodeInstruction { RangeMin = 0, RangeMax = 18, TargetValue = "Enfant/Ado", IsRange = true },
                new RecodeInstruction { RangeMin = 18, RangeMax = 120, TargetValue = "Adulte", IsRange = true }
            };
            
            var recodedAges = service.Recode(ages, ageInstructions, VariableType.QuantitativeDiscrete);
            
            Console.WriteLine("\n[1] Recodage des Ages (0-18: Enfant/Ado, 18+: Adulte):");
            for(int i=0; i<ages.Count; i++)
            {
                Console.WriteLine($"{ages[i] ?? "NULL"} -> {recodedAges[i]}");
            }

            // 2. Test Qualitative Recoding (Exact Match)
            var gender = new List<object> { "M", "F", "m", "f", "Inconnu" };
            var genderInstructions = new List<RecodeInstruction>
            {
                new RecodeInstruction { SourceValue = "M", TargetValue = "Homme" },
                new RecodeInstruction { SourceValue = "F", TargetValue = "Femme" }
            };
            
            var recodedGender = service.Recode(gender, genderInstructions, VariableType.Text);
            
            Console.WriteLine("\n[2] Recodage du Sexe (M->Homme, F->Femme):");
            for(int i=0; i<gender.Count; i++)
            {
                Console.WriteLine($"{gender[i]} -> {recodedGender[i]}");
            }

            Console.WriteLine("\n=== VERIFICATION COMPLETE ===");
        }
    }
}
