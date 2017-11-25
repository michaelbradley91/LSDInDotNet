using System;
using System.Collections.Generic;

namespace LSDInDotNet.Models
{
    // TODO: see if this is really the correct structure
    public struct TupleList
    {
        // TODO: see if size can be included in the constructor
        public TupleList(int dimension)
        {
            if (dimension <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), "Must be positive");
            }

            Size = 0;
            Dimension = dimension;
            Values = new List<double>();
        }

        public int Size;
        public int Dimension;
        public IList<double> Values;

        public void AddTuple(params double[] values)
        {
            if (values.Length != Dimension)
            {
                throw new ArgumentException($"{values.Length} values passed in, but this is an {Dimension}-tuple", nameof(values));
            }

            for (var i = 0; i < values.Length; i++)
            {
                Values.Insert(Size * Dimension + i, values[i]);
            }

            Size++;
        }
    }
}
