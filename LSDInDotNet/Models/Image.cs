using System;

namespace LSDInDotNet.Models
{
    public struct Image<TData, TMetadata>
    {
        public int Width { get; }
        public int Height { get; }
        public TMetadata Metadata { get; }

        private readonly TData[] _data;

        public Image(int width, int height, TData defaultValue, TMetadata metadata)
            : this(width, height, metadata)
        {
            for (var i = 0; i < _data.Length; i++)
            {
                _data[i] = defaultValue;
            }
        }

        public Image(int width, int height, TMetadata metadata)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be positive");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be positive");

            _data = new TData[width * height];
            Width = width;
            Height = height;
            Metadata = metadata;
        }

        public Image(int width, int height, TData[] data, TMetadata metadata)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be positive");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be positive");
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length != width * height) throw new ArgumentException("Data did not have the expected size for this image's width and height", nameof(data));

            _data = data;
            Width = width;
            Height = height;
            Metadata = metadata;
        }
        
        public TData this[int x, int y]
        {
            get => _data[x + y * Width];
            set => _data[x + y * Width] = value;
        }

        public TData this[Point point]
        {
            get => this[point.X, point.Y];
            set => this[point.X, point.Y] = value;
        }
    }
}