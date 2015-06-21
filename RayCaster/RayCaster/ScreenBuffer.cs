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
            get { return _colorBuffer[y * Width + x]; }
            set { _colorBuffer[y * Width + x] = value; }
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
                _buffer[i] = _colorBuffer[i].ToPackedRgbColor();
            });

            texture.SetData(_buffer);
        }

        public void Clear()
        {
            for (int i = 0; i < _buffer.Length; i++)
            {
                _colorBuffer[i] = HsvColor.Black;
            }
        }
    }
}
