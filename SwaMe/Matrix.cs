using System;

namespace WaveletLibrary
{
    public class Matrix : MatrixLibrary.Matrix
    {
		public Matrix(int noRows, int noCols) : base(noRows, noCols) {}
		public Matrix(double[,] data) : base(data) {}

        int _row = -1;
        int _col = -1;

        public void SelectRow(int row)
        {
            _row = row;
            _col = -1;
        }

        public void SelectCol(int col)
        {
            _col = col;
            _row = -1;
        }

        public int GetSelecedVectorLength()
        {
            if (_row != -1)
                return NoCols;
            else if (_col != -1)
                return NoRows;
            else
                throw new RankException("No column or row has been selected.");
        }

        public double GetVectorElement(int index)
        {
            if (_row != -1)
                return in_Mat[_row, index];
            else if (_col != -1)
                return in_Mat[index, _col];
            else
                throw new RankException("No column or row has been selected.");
        }

        public void SetVectorElement(int index, double value)
        {
            if (_row != -1)
                in_Mat[_row, index] = value;
            else if (_col != -1)
                in_Mat[index, _col] = value;
            else
                throw new RankException("No column or row has been selected.");
        }
    }
}
