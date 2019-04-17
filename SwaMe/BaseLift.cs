using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaveletLibrary
{

    public enum Direction
    {
        Forward,
        Inverse
    }

    public abstract class BaseLift
    {

        /// <summary>
        ///  Split the <i>vec</i> into even and odd elements,
        ///  where the even elements are in the first half
        ///  of the vector and the odd elements are in the
        ///  second half.
        /// </summary>
        protected void Split(Matrix data, int N)
        {

            double[] tmp = new double[N/2];

            //while (start < end)
            //{
            //    for (int i = start; i < end; i = i + 2)
            //    {
            //        tmp = data.GetVectorElement(i);
            //        data.SetVectorElement(i,data.GetVectorElement(i + 1));
            //        data.SetVectorElement(i + 1, tmp);
            //    }
            //    start = start + 1;
            //    end = end - 1;
            //}

            for (int i = 1; i < N; i++)
            {
                if (i >> 1 << 1 != i)
                    // Odd
                    tmp[i >> 1] = data.GetVectorElement(i);
                else
                    // Even
                    data.SetVectorElement(i >> 1, data.GetVectorElement(i));
            }

            for (int i = 0; i < N/2; i++)
            {
                data.SetVectorElement((N >> 1) + i, tmp[i]);
            }

        }

        /// <summary>
        ///  Merge the odd elements from the second half of the N element
        ///  region in the array with the even elements in the first
        ///  half of the N element region.  The result will be the
        ///  combination of the odd and even elements in a region
        ///  of length N.
        /// </summary>
        protected void Merge(Matrix data, int N)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Predict step, to be defined by the subclass
        /// </summary>
        protected abstract void Predict(Matrix data, int N, Direction direction);

        /// <summary>
        /// Update step, to be defined by the subclass 
        /// </summary>
        protected abstract void Update(Matrix data, int N, Direction direction);

        /// <summary>
        ///  Simple wavelet Lifting Scheme forward transform
        ///
        ///  forwardTrans is passed an array of doubles.  The array size must
        ///  be a power of two.  Lifting Scheme wavelet transforms are calculated
        ///  in-place and the result is returned in the argument array.
        ///  
        ///  The result of forwardTrans is a set of wavelet coefficients
        ///  ordered by increasing frequency and an approximate average
        ///  of the input data set in data.GetVectorElement(0].  The coefficient bands
        ///  follow this element in powers of two (e.g., 1, 2, 4, 8...).
        /// </summary>
        public virtual void ForwardTrans(Matrix data, int level)
        {
            int N = data.GetSelecedVectorLength();

            if (((N >> level) << level) != N)
                throw new ArgumentException("The vector size is not compatible with the lifting scheme.");

            int n = N / (int) Math.Pow(2, level-1);
            Split(data, n);
            Predict(data, n, Direction.Forward);
            Update(data, n, Direction.Forward);
        }


        /// <summary>
        ///  Default two step Lifting Scheme inverse wavelet transform
        ///  
        ///  inverseTrans is passed the result of an ordered wavelet 
        ///  transform, consisting of an average and a set of wavelet
        ///  coefficients.  The inverse transform is calculated
        ///  in-place and the result is returned in the argument array.
        /// </summary>
        public virtual void InverseTrans(Matrix data, int level)
        {
            int N = data.GetSelecedVectorLength();

            if (((N >> level) << level) != N)
                throw new ArgumentException("The vector size is not compatible with the lifting scheme.");

            int n = N / (int)Math.Pow(2, level-1);
            Update(data, n, Direction.Inverse);
            Predict(data, n, Direction.Inverse);
            Merge(data, n);
        }


    }
}
