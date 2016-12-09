using System;
using System.Collections.Generic;
using CSML;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snek1
{
    public class NN
    {
        public double[] inputs { get; set; }
        public double[,] ih1Weights { get; set; }
        private double[] hiddenLayer1;
        public double[,] h1oWeights { get; set; }
        public double[] outputs { get; set; }
        public double fitness { get; set; }

        public NN()
        {
            inputs = new double[32];
            ih1Weights = new double[16,32];
            hiddenLayer1 = new double[16];
            h1oWeights = new double[4,16];
            outputs = new double[4];
        }

        public void CalcOuts()
        {
            try
            {
                for (int z = 0; z < 16; z++) //hiddenlayer
                {
                    double[] tmph = new double[32];
                    for (int y = 0; y < 32; y++)
                    {
                        tmph[y] = ih1Weights[z, y];
                    }

                    hiddenLayer1[z] = Sig(Mult(inputs,tmph));
                }
                for (int z = 0; z < 4; z++) //outputs
                {
                    double[] tmph = new double[16];
                    for (int y = 0; y < 16; y++)
                    {
                        tmph[y] = h1oWeights[z, y];
                    }

                    outputs[z] = OutputActivation(Sig(Mult(hiddenLayer1,tmph)));
                }
                Console.WriteLine("here");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //hiddenLayer1 = inputs;

        }

        public double OutputActivation(double x)
        {
            return x;
        }

        public double Mult(double[] one, double[] two)
        {
            Matrix tmp1 = new Matrix(one);
            Matrix tmp2 = new Matrix(two);

            return Matrix.Dot(tmp1, tmp2).Re;

        }

        public double Sig(double x)
        {
            return 2/(1 + Math.Exp(-2*x)) - 1;
        }
    }
}
