﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayCasterGame
{
    sealed class RayCaster
    {
        const int _mapWidth = 24;
        const int _mapHeight = 24;

        int[][] _worldMap = new int[_mapWidth][]
        {
            new int[]{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
            new int[]{1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,0,0,3,0,0,0,1},
            new int[]{1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,0,0,0,0,5,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            new int[]{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };

        DoublePosition _playerPos = new DoublePosition { X = 22, Y = 12 };
        Vector2D _playerDirection = new Vector2D { X = -1, Y = 0 };
        Vector2D _cameraPlane = new Vector2D { X = 0, Y = 0.66 };

        sealed class DoublePosition
        {
            public double X;
            public double Y;

            public Position TruncateToPosition()
            {
                return new Position { X = (int)X, Y = (int)Y };
            }

            public DoublePosition Clone()
            {
                return new DoublePosition { X = X, Y = Y };
            }
        }

        sealed class Position
        {
            public int X;
            public int Y;
        }

        sealed class Vector2D
        {
            public double X;
            public double Y;
        }

        public void Update(MovementInputs inputs, GameTime gameTime)
        {
            var moveSpeed = 5.0 * (gameTime.ElapsedGameTime.Milliseconds / 1000.0);
            var rotSpeed = 3.0 * (gameTime.ElapsedGameTime.Milliseconds / 1000.0);

            //move forward if no wall in front of you
            if (inputs.HasFlag(MovementInputs.Forward))
            {
                if (_worldMap[(int)(_playerPos.X + _playerDirection.X * moveSpeed)][(int)_playerPos.Y] == 0)
                    _playerPos.X += _playerDirection.X * moveSpeed;
                if (_worldMap[(int)_playerPos.X][(int)(_playerPos.Y + _playerDirection.Y * moveSpeed)] == 0)
                    _playerPos.Y += _playerDirection.Y * moveSpeed;
            }
            //move backwards if no wall behind you
            if (inputs.HasFlag(MovementInputs.Backward))
            {
                if (_worldMap[(int)(_playerPos.X - _playerDirection.X * moveSpeed)][(int)_playerPos.Y] == 0)
                    _playerPos.X -= _playerDirection.X * moveSpeed;
                if (_worldMap[(int)_playerPos.X][(int)(_playerPos.Y - _playerDirection.Y * moveSpeed)] == 0)
                    _playerPos.Y -= _playerDirection.Y * moveSpeed;
            }
            //rotate to the right   
            if (inputs.HasFlag(MovementInputs.TurnRight))
            {
                //both camera direction and camera plane must be rotated
                double oldDirX = _playerDirection.X;
                _playerDirection.X = _playerDirection.X * Math.Cos(-rotSpeed) - _playerDirection.Y * Math.Sin(-rotSpeed);
                _playerDirection.Y = oldDirX * Math.Sin(-rotSpeed) + _playerDirection.Y * Math.Cos(-rotSpeed);
                double oldPlaneX = _cameraPlane.X;
                _cameraPlane.X = _cameraPlane.X * Math.Cos(-rotSpeed) - _cameraPlane.Y * Math.Sin(-rotSpeed);
                _cameraPlane.Y = oldPlaneX * Math.Sin(-rotSpeed) + _cameraPlane.Y * Math.Cos(-rotSpeed);
            }
            //rotate to the left
            if (inputs.HasFlag(MovementInputs.TurnLeft))
            {
                //both camera direction and camera plane must be rotated
                double oldDirX = _playerDirection.X;
                _playerDirection.X = _playerDirection.X * Math.Cos(rotSpeed) - _playerDirection.Y * Math.Sin(rotSpeed);
                _playerDirection.Y = oldDirX * Math.Sin(rotSpeed) + _playerDirection.Y * Math.Cos(rotSpeed);
                double oldPlaneX = _cameraPlane.X;
                _cameraPlane.X = _cameraPlane.X * Math.Cos(rotSpeed) - _cameraPlane.Y * Math.Sin(rotSpeed);
                _cameraPlane.Y = oldPlaneX * Math.Sin(rotSpeed) + _cameraPlane.Y * Math.Cos(rotSpeed);
            }
        }

        public void Render(ScreenBuffer buffer)
        {
            buffer.Clear();
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
                    if (_worldMap[mapPos.X][mapPos.Y] > 0) hit = true;
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
                Color color;
                switch (_worldMap[mapPos.X][mapPos.Y])
                {
                    case 1: color = Color.Red; break; //red
                    case 2: color = Color.Green; break; //green
                    case 3: color = Color.Blue; break; //blue
                    case 4: color = Color.White; break; //white
                    default: color = Color.Yellow; break; //yellow
                }

                HsvColor hsv = HsvColor.FromRgb(color);

                //give x and y sides different brightness
                if (side == 1)
                {
                    hsv.Value *= 0.75;
                }

                //draw the pixels of the stripe as a vertical line
                for (int y = drawStart; y <= drawEnd; y++)
                {
                    buffer[x, y] = hsv.ToPackedRgbColor();
                }
            }
        }
    }
}
