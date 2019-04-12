using System;


namespace SwaMe
{
    class sgSmooth
    {
        public double[,] swap(int numRowElements,double[,] Ab, int rowNumber1, int rowNumber2)
            {
                // Size of float.
                int doubSize = sizeof(double);

                // Temporary array for an intermediate step in the swap operation.
                double [] temp = new double[numRowElements];

                // Copy first row into a temporary array.
                System.Buffer.BlockCopy(Ab, rowNumber1 * doubSize, temp, 0, numRowElements* doubSize);

                // Copy second row into the first row.
                System.Buffer.BlockCopy(Ab, rowNumber2 * doubSize, Ab, rowNumber1 * doubSize, numRowElements* doubSize);

                // Copy temporary array into the second row.
                System.Buffer.BlockCopy(temp, 0, Ab, rowNumber2 * doubSize, numRowElements*doubSize);
            return Ab;
            }


        //! permute() orders the rows of A to match the integers in the index array.
        public void permute(double[,]A, int[] idx)
        {
            int[] i= new int[idx.GetLength(0)];
            int j, k;

            for (j = 0; j < A.GetLength(0); ++j)
            {
                i[j] = j;
            }

           

            // loop over permuted indices
            for (j = 0; j < A.GetLength(0); ++j)
            {
                if (i[j] != idx[j])
                {

                    // search only the remaining indices
                    for (k = j + 1; k < A.GetLength(0); ++k)
                    {
                        if (i[k] == idx[j])
                        {
                            
                            A = swap(A.GetLength(0),A, j, k);// swap the rows and
                            i[k] = i[j];     // the elements of
                            i[j] = idx[j];   // the ordered index.
                            break; // next j
                        }
                    }
                }
            }
        }

        public double[] sg_smooth(double[] v, int width, int deg)
        {
            double[] res = new double[v.GetLength(0)];


            if ((width < 1) || (deg < 0) || (v.GetLength(0) < (2 * width + 2)))
            {
                System.Console.WriteLine("sgsmooth: parameter error.");
                return res;
            }


            int window = 2 * width + 1;
            int endidx = v.GetLength(0) - 1;

            // do a regular sliding window average
            int i, j;
            if (deg == 0)
            {
                // handle border cases first because we need different coefficients

                for (i = 0; i < width; ++i)
                {
                    double degZScale = 1.0 / i + 1;
                    double[] c1 = { width, degZScale };
                    for (j = 0; j <= i; ++j)
                    {
                        res[i] += c1[j] * v[j];
                        res[endidx - i] += c1[j] * v[endidx - j];
                    }
                }

                // now loop over rest of data. reusing the "symmetric" coefficients.
                double thisScale = 1.0 / window;
                double[] c2 = new double[window];
                for (int iii = 0; iii < c2.Length; iii++)
                {
                    c2[iii] = thisScale;
                }

                for (i = 0; i <= (v.GetLength(0) - window); ++i)
                {
                    for (j = 0; j < window; ++j)
                    {
                        res[i + width] += c2[j] * v[i + j];
                    }
                }
                return res;
            }

            // handle border cases first because we need different coefficients

            for (i = 0; i < width; ++i)
            {
                double[] b1 = new double[window];
                b1[i] = 1.0;
                double[] c1 = sg_coeff(b1, deg);
                for (j = 0; j < window; ++j)
                {
                    res[i] += c1[j] * v[j];
                    res[endidx - i] += c1[j] * v[endidx - j];
                }

            }
            // now loop over rest of data. reusing the "symmetric" coefficients.

            return res;
        }



        /*! \brief Implicit partial pivoting.
         *
         * The function looks for pivot element only in rows below the current
         * element, A[idx[row]][column], then swaps that row with the current one in
         * the index map. The algorithm is for implicit pivoting (i.e., the pivot is
         * chosen as if the max coefficient in each row is set to 1) based on the
         * scaling information in the vector scale. The map of swapped indices is
         * recorded in swp. The return value is +1 or -1 depending on whether the
         * number of row swaps was even or odd respectively. */
       public int partial_pivot(double[,] A, int row, int col,
                                 double[] scale, int[] idx, double tol)
            {
                if (tol <= 0.0)
                    tol = 1.0e-300;

                int swapNum = 1;

                    // default pivot is the current position, [row,col]
                    int pivot = row;
                    double piv_elem = Math.Abs(A[idx[row],col]) * scale[idx[row]];

                    // loop over possible pivots below current
                    int j;
                for (j = row + 1; j<A.GetLength(0); ++j) {

                    double tmp = Math.Abs(A[idx[j],col]) * scale[idx[j]];

                    // if this elem is larger, then it becomes the pivot
                    if (tmp > piv_elem) {
                        pivot = j;
                        piv_elem = tmp;
                    }
            }


                if(piv_elem < tol)
            {
                System.Console.WriteLine("partial_pivot(): Zero pivot encountered.\n");
            }
         

                if(pivot > row) {           // bring the pivot to the diagonal
                    j = idx[row];           // reorder swap array
                    idx[row] = idx[pivot];
                    idx[pivot] = j;
                    swapNum = -swapNum;     // keeping track of odd or even swap
                }
                return swapNum;
            }



      public  void lu_backsubst(double[,] A, double[,] a, bool diag = false)
        {
            int r, c, k;

            for (r = (A.GetLength(0) - 1); r >= 0; --r)
            {
                for (c = (A.GetLength(1) - 1); c > r; --c)
                {
                    for (k = 0; k < A.GetLength(0); ++k)
                    {
                        a[r,k] -= A[r,c] * a[c,k];
                    }
                }
                if (!diag)
                {
                    for (k = 0; k < A.GetLength(1); ++k)
                    {
                        a[r,k] /= A[r,r];
                    }
                }
            }
        }


        /*! \brief Perform forward substitution.
 *
 * Solves the system of equations A*b=a, ASSUMING that A is lower
 * triangular. If diag==1, then the diagonal elements are additionally
 * assumed to be 1.  Note that the upper triangular elements are never
 * checked, so this function is valid to use after a LU-decomposition in
 * place.  A is not modified, and the solution, b, is returned in a. */
     public  void lu_forwsubst(double[,]A, double[,] a, bool diag = true)
        {
            int r, k, c;
            for (r = 0; r < A.GetLength(0); ++r)
            {
                for (c = 0; c < r; ++c)
                {
                    for (k = 0; k < A.GetLength(1); ++k)
                    {
                        a[r,k] -= A[r,c] * a[c,k];
                    }
                }
                if (!diag)
                {
                    for (k = 0; k < A.GetLength(1); ++k)
                    {
                        a[r,k] /= A[r,r];
                    }
                }
            }
        }

      public  double[,] transpose(double[,] a)
        {
            double[,] res = new double[a.GetLength(1), a.GetLength(0)];
                int i, j;

            for (i = 0; i< a.GetLength(0); ++i) 
                    {
                        for (j = 0; j<a.GetLength(1); ++j)
                            {
                            res[j,i] = a[i,j];
                            }
                    }
            return res;
        }

        /*! \brief Performs LU factorization in place.
         *
         * This is Crout's algorithm (cf., Num. Rec. in C, Section 2.3).  The map of
         * swapped indeces is recorded in idx. The return value is +1 or -1
         * depending on whether the number of row swaps was even or odd
         * respectively.  idx must be preinitialized to a valid set of indices
         * (e.g., {1,2, ... ,A.nr_rows()}). */
    public    int lu_factorize(double[,] A, int[] idx, double tol = 1.0e-300)
        {
            if (tol <= 0.0)
                tol = 1.0e-300;

            if ((A.GetLength(0) == 0) || (A.GetLength(1) != A.GetLength(0)))
            {
                //sgs_error("lu_factorize(): cannot handle empty "
                //           "or nonsquare matrices.\n");

                return 0;
            }

            double[] scale = new double[A.GetLength(0)];  // implicit pivot scaling
            int i, j;
            for (i = 0; i < A.GetLength(0); ++i)
            {
                double maxval = 0.0;
                for (j = 0; j < A.GetLength(1); ++j)
                {
                    if (Math.Abs(A[i,j]) > maxval)
                        maxval = Math.Abs(A[i,j]);
                }
                if (maxval == 0.0)
                {
                    //sgs_error("lu_factorize(): zero pivot found.\n");
                    return 0;
                }
                scale[i] = 1.0 / maxval;
            }

            int swapNum = 1;
            int c, r;
            for (c = 0; c < A.GetLength(1); ++c)
            {            // loop over columns
                swapNum *= partial_pivot(A, c, c, scale, idx, tol); // bring pivot to diagonal
                for (r = 0; r < A.GetLength(0); ++r)
                {      //  loop over rows
                    int lim = (r < c) ? r : c;
                    for (j = 0; j < lim; ++j)
                    {
                        A[idx[r],c] -= A[idx[r],j] * A[idx[j],c];
                    }
                    if (r > c)
                        A[idx[r],c] /= A[idx[c],c];
                }
            }
            permute(A, idx);
            return swapNum;
        }

        /*! \brief Solve a system of linear equations.
         * Solves the inhomogeneous matrix problem with lu-decomposition. Note that
         * inversion may be accomplished by setting a to the identity_matrix. */
     public double[,] lin_solve(double[,] A, double[,] a,
                                   double tol = 1.0e-300)
        {
            double[,] B = A;
            double[,] b = a;
            int[] idx = new int[B.GetLength(0)];
            int j;

            for (j = 0; j < B.GetLength(0); ++j)
            {
                idx[j] = j;  // init row swap label array
            }
            lu_factorize(B, idx, tol); // get the lu-decomp.
            permute(b, idx);          // sort the inhomogeneity to match the lu-decomp
            lu_forwsubst(B, b);       // solve the forward problem
            lu_backsubst(B, b);       // solve the backward problem
            return b;
        }


      public double[,] invert(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] E = new double[n, n];
            double[,] B = A;
            int i;

            for (i = 0; i < n; ++i)
            {
                E[i, i] = 1.0;
            }

            return lin_solve(B, E);
        }




     public double[] sg_coeff(double[] b, int deg)
        {
            int rows = b.GetLength(0);
            int cols = deg + 1;
            double[,] A = new double[rows, cols];
            double[] res = new double[rows];


            // generate input matrix for least squares fit
            int i, j;
            for (i = 0; i < rows; ++i)
            {
                for (j = 0; j < cols; ++j)
                {
                    A[i, j] = System.Math.Pow(i, j);
                }
            }
            double[,] bbb = new double[b.GetLength(0), 1];
            for (int iterator=0;iterator< b.GetLength(0);iterator++)
            { bbb[iterator,0] = b[iterator]; }
            
            double[,] c = new double[A.GetLength(1), A.GetLength(0)];
            MatrixMultiplier matrix = new MatrixMultiplier { };
            c = matrix.Multiply(invert(matrix.Multiply(transpose(A), A)) , (matrix.Multiply(transpose(A),transpose(bbb))));


            for (i = 0; i < rows; ++i)
            {
                for (j = 0; j < cols; ++j)
                {
                    c[j, i] = A[i, j];
                }
            };


            for (i = 0; i < b.GetLength(0); ++i)
            {
                res[i] = c[0,0];
                for (j = 1; j <= deg; ++j)
                {
                    res[i] += c[j,0] * System.Math.Pow(i, j);
                }
            }
            return res;
        }

    }


}
