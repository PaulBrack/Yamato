using System;

namespace SwaMe
{
    class MatrixMultiplier
    {
        public double[,] Multiply(double[,] a, double[,] b)
        {
            int m = a.GetLength(0), n = a.GetLength(1), p = b.GetLength(0), q = b.GetLength(1), i, j;
            double[,] outputMatrix = new double[m, q];
            if (n != p)
            {
                Console.WriteLine("Matrix multiplication not possible");
                return outputMatrix;
            }
            else
            {
                for (i = 0; i < m; i++)
                {
                    for (j = 0; j < q; j++)
                    {
                        outputMatrix[i, j] = 0;
                        for (int k = 0; k < n; k++)
                        {
                            outputMatrix[i, j] += a[i, k] * b[k, j];
                        }
                    }
                }
                /*Console.WriteLine("The product of the two matrices is :");
                for (i = 0; i < m; i++)
                {
                    for (j = 0; j < n; j++)
                    {
                        Console.Write(M[i, j] + "\t");
                    }
                    Console.WriteLine();
                }*/

                return outputMatrix;
            }
        }
    }
}


 
