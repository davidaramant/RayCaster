using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class PlayerInfo
    {
        public Vector2 Position = new Vector2(22.5f, 10.5f);
        public Vector2 Direction = new Vector2(-1, 0);
        public Vector2 CameraPlane = new Vector2(0, 0.66f); // TODO: Figure this out

        private float _radius = 0.25f;

        public void Update(MapData mapData, MovementInputs inputs, GameTime gameTime)
        {
            var moveSpeed = 5.0f * (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            var rotSpeed = 3.0f * (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            if (inputs.HasFlag(MovementInputs.Forward))
            {
                Move(mapData, Direction, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.Backward))
            {
                var direction = new Vector2 { X = -Direction.X, Y = -Direction.Y };

                Move(mapData, direction, moveSpeed);
            }
            if (inputs.HasFlag(MovementInputs.StrafeLeft))
            {
                var direction = new Vector2 { X = Direction.Y, Y = -Direction.X };

                Move(mapData, direction, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.StrafeRight))
            {
                var direction = new Vector2 { X = -Direction.Y, Y = Direction.X };

                Move(mapData, direction, moveSpeed);
            }

            if (inputs.HasFlag(MovementInputs.TurnRight))
            {
                Rotate(rotSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.TurnLeft))
            {
                Rotate(-rotSpeed);
            }
        }

        public void Move(MapData mapData, Vector2 direction, float speed)
        {
            // Should MapData be passed in here?  Feels odd...
            var movement = direction * speed;
            var newPosition = Position + movement;
            var newBoundingEdge = newPosition + direction * _radius;

            if (mapData.IsPassable((int)newBoundingEdge.X, (int)Position.Y))
            {
                Position.X = newPosition.X;
            }
            if (mapData.IsPassable((int)Position.X, (int)newBoundingEdge.Y))
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
