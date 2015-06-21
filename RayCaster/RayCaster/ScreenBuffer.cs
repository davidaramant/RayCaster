using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace RayCasterGame
{
    sealed class ScreenBuffer
    {
        readonly int _width;
        readonly int _height;

        readonly uint[] _buffer;
        readonly HsvColor[] _colorBuffer;

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public HsvColor this[int x, int y]
        {
            get { return _colorBuffer[y + x * Height]; }
            set { _colorBuffer[y + x * Height] = value; }
        }

        public ScreenBuffer(int width, int height)
        {
            _width = width;
            _height = height;
            _buffer = new uint[width * height];
            _colorBuffer = new HsvColor[width * height];
        }

        public void CopyToTexture(Texture2D texture)
        {
            Parallel.For(0, _colorBuffer.Length, i =>
            {
                var y = i % _height;
                var x = i / _height;

                var j = x + y * _width;

                _buffer[j] = _colorBuffer[i].ToPackedRgbColor();
            });

            texture.SetData(_buffer);
        }

        public void Clear()
        {
            System.Array.Clear(_colorBuffer, 0, _colorBuffer.Length);
        }
    }
}
