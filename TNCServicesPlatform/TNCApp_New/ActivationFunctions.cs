using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCAnimalLabelWebAPI
{
    static class ActivationFunctions
    {
        public static float[] Softmax(IList<float> values)
        {
            float[] value_exp = new float[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                value_exp[i] = (float)Math.Exp(values[i]);
            }

            float sum_exp = value_exp.Sum();
            float[] softmax_values = new float[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                //softmax_values[i] = Math.Round(value_exp[i] / sum_exp, 3);
                softmax_values[i] = value_exp[i] / sum_exp;
            }

            return softmax_values;

        }


        public static float[] Softmax (float[] values)
        {
            float[] value_exp = new float[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                value_exp[i] = (float)Math.Exp(values[i]);
            }

            float sum_exp = value_exp.Sum();
            float[] softmax_values = new float[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                //softmax_values[i] = Math.Round(value_exp[i] / sum_exp, 3);
                softmax_values[i] = value_exp[i] / sum_exp;
            }

            return softmax_values;


        }
    }
}
