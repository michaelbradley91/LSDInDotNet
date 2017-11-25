using System;

namespace LSDInDotNet.Models
{
    public struct Image<TData>
    {
        public Image(int width, int height, TData defaultValue)
            : this(width, height)
        {
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = defaultValue;
            }
        }

        public Image(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be positive");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be positive");

            Data = new TData[width * height];
            Width = width;
            Height = height;
        }

        public Image(int width, int height, TData[] data)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be positive");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be positive");
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length != width * height) throw new ArgumentException("Data did not have the expected size for this image's width and height", nameof(data));

            Data = data;
            Width = width;
            Height = height;
        }

        public TData[] Data;
        public int Width;
        public int Height;
    }
}