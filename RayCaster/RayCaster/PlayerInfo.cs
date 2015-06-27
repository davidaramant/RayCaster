using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class PlayerInfo
    {
        public Vector2 Position = new Vector2(22.5f, 10.5f);
        public Vector2 Direction = new Vector2(-1, 0);
        public Vector2 CameraPlane = new Vector2(0, 0.66f); // TODO: Figure this out

        public void Move(MapData mapData, Vector2 direction, float speed)
        {
            // Should MapData be passed in here?  Feels odd...
            var movement = direction * speed;
            var newPosition = Position + movement;

            if (mapData.IsPassable((int)newPosition.X, (int)Position.Y))
            {
                Position.X = newPosition.X;
            }
            if (mapData.IsPassable((int)Position.X, (int)newPosition.Y))
            {
                Position.Y = newPosition.Y;
            }
        }

        public void Rotate(float rotationRadians)
        {
            var rotation = Matrix.CreateRotationZ(rotationRadians);

            Direction = Vector2.Transform(Direction, rotation);
            CameraPlane = Vector2.Transform(CameraPlane, rotation);
        }
    }
}
