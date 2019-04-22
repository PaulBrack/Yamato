using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaveletLibrary
{
    public class HaarLift : BaseLift
    {

        protected override void Predict(Matrix data, int N, Direction direction)
        {
            int half = N >> 1;

            for (int i = 0; i < half; i++)
            {
                double predictVal = data.GetVectorElement(i);
                int j = i + half;

                if (direction == Direction.Forward)
                {
                    data.SetVectorElement(j, data.GetVectorElement(j) - predictVal);
                }
                else if (direction == Direction.Inverse)
                {
                    data.SetVectorElement(j, data.GetVectorElement(j) + predictVal);
                }
                else
                {
                    throw new ArgumentException("Direction is not valid.");
                }
            }

        }

        protected override void Update(Matrix data, int N, Direction direction)
        {
            int half = N >> 1;

            for (int i = 0; i < half; i++)
            {
                int j = i + half;
                double updateVal = data.GetVectorElement(j) / 2.0;

                if (direction == Direction.Forward)
                {
                    data.SetVectorElement(i, data.GetVectorElement(i) + updateVal);
                }
                else if (direction == Direction.Inverse)
                {
                    data.SetVectorElement(i, data.GetVectorElement(i) - updateVal);
                }
                else
                {
                    throw new ArgumentException("Direction is not valid.");
                }
            }
        }
    }
}
