using Microsoft.Xna.Framework.Graphics;

namespace RayCasterGame
{
    sealed class ScreenBuffer
    {
        readonly int _width;
        readonly int _height;

        readonly int[] _buffer;

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public int this[int x, int y]
        {
            get { return _buffer[y * Width + x]; }
            set { _buffer[y * Width + x] = value; }
        }

        public ScreenBuffer(int width, int height)
        {
            _width = width;
            _height = height;
            _buffer = new int[width * height];
        }

        public void CopyToTexture(Texture2D texture)
        {
            texture.SetData(_buffer);
        }
    }
}
