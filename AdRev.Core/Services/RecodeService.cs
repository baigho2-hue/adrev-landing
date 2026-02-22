using System;
using System.Collections.Generic;
using System.Linq;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;

namespace AdRev.Core.Services
{
    public class RecodeService
    {
        public List<object> Recode(List<object> values, List<RecodeInstruction> instructions, VariableType inputType)
        {
            var result = new List<object>();

            foreach (var val in values)
            {
                object newVal = val; // Default: keep original

                foreach (var rule in instructions)
                {
                    if (rule.IsRange && (inputType == VariableType.QuantitativeContinuous || inputType == VariableType.QuantitativeDiscrete))
                    {
                        if (double.TryParse(val?.ToString(), out double dVal))
                        {
                            bool lowerOk = !rule.RangeMin.HasValue || dVal >= rule.RangeMin.Value;
                            bool upperOk = !rule.RangeMax.HasValue || dVal < rule.RangeMax.Value;

                            if (lowerOk && upperOk)
                            {
                                newVal = rule.TargetValue;
                                break;
                            }
                        }
                    }
                    else // Simple replacement
                    {
                        if (string.Equals(val?.ToString(), rule.SourceValue, StringComparison.OrdinalIgnoreCase))
                        {
                            newVal = rule.TargetValue;
                            break;
                        }
                    }
                }

                result.Add(newVal);
            }

            return result;
        }
    }
}
