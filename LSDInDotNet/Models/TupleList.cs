using System;
using System.Collections.Generic;

namespace LSDInDotNet.Models
{
    // TODO: see if this is really the correct structure
    public class TupleList
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
            _values = new List<double>();
        }

        public int Size { get; private set; }
        public int Dimension;
        private readonly IList<double> _values;

        public double this[int x, int y]
        {
            get => _values[x + y * Dimension];
            set => _values[x + y * Dimension] = value;
        }

        /// <summary>
        /// Auxiliary index to access and modify data in just the first tuple
        /// </summary>
        public double this[int x]
        {
            get => this[x, 0];
            set => this[x, 0] = value;
        }

        public void AddTuple(params double[] values)
        {
            if (values.Length != Dimension)
            {
                throw new ArgumentException($"{values.Length} values passed in, but this is an {Dimension}-tuple", nameof(values));
            }

            for (var i = 0; i < values.Length; i++)
            {
                _values.Insert(Size * Dimension + i, values[i]);
            }

            Size++;
        }
    }
}
