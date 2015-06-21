using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class RayCaster
    {
        MapData _mapData = new MapData();

        DoublePosition _playerPos = new DoublePosition { X = 22, Y = 12 };
        Vector2D _playerDirection = new Vector2D { X = -1, Y = 0 };
        Vector2D _cameraPlane = new Vector2D { X = 0, Y = 0.66 };

        public void Update(MovementInputs inputs, GameTime gameTime)
        {
            var moveSpeed = 5.0 * (gameTime.ElapsedGameTime.Milliseconds / 1000.0);
            var rotSpeed = 3.0 * (gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            if (inputs.HasFlag(MovementInputs.Forward))
            {
                Move(_playerDirection, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.Backward))
            {
                var direction = new Vector2D { X = -_playerDirection.X, Y = -_playerDirection.Y };

                Move(direction, moveSpeed);
            }
            if (inputs.HasFlag(MovementInputs.StrafeLeft))
            {
                var direction = new Vector2D { X = -_playerDirection.Y, Y = _playerDirection.X };

                Move(direction, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.StrafeRight))
            {
                var direction = new Vector2D { X = _playerDirection.Y, Y = -_playerDirection.X };

                Move(direction, moveSpeed);
            }

            if (inputs.HasFlag(MovementInputs.TurnRight))
            {
                Rotate(-rotSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.TurnLeft))
            {
                Rotate(rotSpeed);
            }
        }

        private void Move(Vector2D direction, double speed)
        {
            if (_mapData.IsEmpty((int)(_playerPos.X + direction.X * speed), (int)_playerPos.Y))
                _playerPos.X += direction.X * speed;
            if (_mapData.IsEmpty((int)_playerPos.X, (int)(_playerPos.Y + direction.Y * speed)))
                _playerPos.Y += direction.Y * speed;
        }

        private void Rotate(double rotSpeed)
        {
            //both camera direction and camera plane must be rotated
            double oldDirX = _playerDirection.X;
            _playerDirection.X = _playerDirection.X * Math.Cos(rotSpeed) - _playerDirection.Y * Math.Sin(rotSpeed);
            _playerDirection.Y = oldDirX * Math.Sin(rotSpeed) + _playerDirection.Y * Math.Cos(rotSpeed);
            double oldPlaneX = _cameraPlane.X;
            _cameraPlane.X = _cameraPlane.X * Math.Cos(rotSpeed) - _cameraPlane.Y * Math.Sin(rotSpeed);
            _cameraPlane.Y = oldPlaneX * Math.Sin(rotSpeed) + _cameraPlane.Y * Math.Cos(rotSpeed);
        }

        public void Render(ScreenBuffer buffer)
        {
            for (int x = 0; x < buffer.Width; x++)
            {
                //calculate ray position and direction 
                double cameraX = 2.0 * x / (double)buffer.Width - 1.0; //x-coordinate in camera space
                var rayPos = _playerPos.Clone();

                var rayDir = new Vector2D
                {
                    X = _playerDirection.X + _cameraPlane.X * cameraX,
                    Y = _playerDirection.Y + _cameraPlane.Y * cameraX
                };

                //which box of the map we're in  
                var mapPos = rayPos.TruncateToPosition();

                //length of ray from current position to next x or y-side
                double sideDistX;
                double sideDistY;

                //length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Sqrt(1 + (rayDir.Y * rayDir.Y) / (rayDir.X * rayDir.X));
                double deltaDistY = Math.Sqrt(1 + (rayDir.X * rayDir.X) / (rayDir.Y * rayDir.Y));
                double perpWallDist;

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                bool hit = false; //was there a wall hit?
                int side = 0; //was a NS or a EW wall hit?
                //calculate step and initial sideDist
                if (rayDir.X < 0)
                {
                    stepX = -1;
                    sideDistX = (rayPos.X - mapPos.X) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapPos.X + 1.0 - rayPos.X) * deltaDistX;
                }
                if (rayDir.Y < 0)
                {
                    stepY = -1;
                    sideDistY = (rayPos.Y - mapPos.Y) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapPos.Y + 1.0 - rayPos.Y) * deltaDistY;
                }
                //perform DDA
                while (!hit)
                {
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapPos.X += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapPos.Y += stepY;
                        side = 1;
                    }
                    //Check if ray has hit a wall
                    if (!_mapData.IsEmpty(mapPos))
                    {
                        hit = true;
                    }
                }
                //Calculate distance projected on camera direction (oblique distance will give fisheye effect!)
                if (side == 0)
                    perpWallDist = Math.Abs((mapPos.X - rayPos.X + (1 - stepX) / 2) / rayDir.X);
                else
                    perpWallDist = Math.Abs((mapPos.Y - rayPos.Y + (1 - stepY) / 2) / rayDir.Y);

                //Calculate height of line to draw on screen
                int lineHeight = Math.Abs((int)(buffer.Height / perpWallDist));

                //calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + buffer.Height / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + buffer.Height / 2;
                if (drawEnd >= buffer.Height) drawEnd = buffer.Height - 1;

                //choose wall color
                HsvColor color = _mapData.GetColor(mapPos);

                var valueFactor = 1f;

                //give x and y sides different brightness
                if (side == 1)
                {
                    valueFactor *= 0.75f;
                }

                //shade by distance
                valueFactor *= (float)Math.Min(1, 5.0 / perpWallDist);
                color = color.ScaleValue(valueFactor);

                //draw the pixels of the stripe as a vertical line
                for (int y = drawStart; y <= drawEnd; y++)
                {
                    buffer[x, y] = color;
                }
            }
        }
    }
}
