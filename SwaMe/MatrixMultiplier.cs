using System;
using System.Collections.Generic;
using System.Text;

namespace SwaMe
{

    class MatrixMultiplier
    {
        public double[,] Multiply(double[,] A, double[,] B)
        {

            int m = A.GetLength(0), n = A.GetLength(1), p = B.GetLength(0), q = B.GetLength(1), i, j;
            double[,] M = new double[m, q];
            if (n != p)
            {
                Console.WriteLine("Matrix multiplication not possible");
                return M;
            }
            else
            {
                for (i = 0; i < m; i++)
                {
                    for (j = 0; j < q; j++)
                    {
                        M[i, j] = 0;
                        for (int k = 0; k < n; k++)
                        {
                            M[i, j] += A[i, k] * B[k, j];
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

                return M;

            }
        }
    }
}


 
