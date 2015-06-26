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
        MapData _mapData;

        public RayCaster(MapData mapData)
        {
            _mapData = mapData;
        }

        PlayerInfo _player = new PlayerInfo();

        public void Update(MovementInputs inputs, GameTime gameTime)
        {
            var moveSpeed = 5.0f * (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            var rotSpeed = 3.0f * (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            if (inputs.HasFlag(MovementInputs.Forward))
            {
                _player.Move(_mapData, _player.Direction, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.Backward))
            {
                var direction = new Vector2 { X = -_player.Direction.X, Y = -_player.Direction.Y };

                _player.Move(_mapData, direction, moveSpeed);
            }
            if (inputs.HasFlag(MovementInputs.StrafeLeft))
            {
                var direction = new Vector2 { X = -_player.Direction.Y, Y = _player.Direction.X };

                _player.Move(_mapData, direction, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.StrafeRight))
            {
                var direction = new Vector2 { X = _player.Direction.Y, Y = -_player.Direction.X };

                _player.Move(_mapData, direction, moveSpeed);
            }

            if (inputs.HasFlag(MovementInputs.TurnRight))
            {
                _player.Rotate(-rotSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.TurnLeft))
            {
                _player.Rotate(rotSpeed);
            }
        }       

        public void Render(ScreenBuffer buffer)
        {
            for (int x = 0; x < buffer.Width; x++)
            {
                //calculate ray position and direction 
                var cameraX = 2.0f * x / (float)buffer.Width - 1f; //x-coordinate in camera space
                var rayPos = _player.Position;

                var rayDir = _player.Direction + _player.CameraPlane * cameraX;

                //which box of the map we're in  
                var mapPos = new Point((int)rayPos.X, (int)rayPos.Y);

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

                // TODO: Replace this stupid 'side' variable with an enum
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
                    if (!_mapData.IsEmpty(mapPos.X, mapPos.Y))
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


                var texture = _mapData.GetWallTexture(mapPos);

                //calculate value of wallX
                double wallX; //where exactly the wall was hit
                if (side == 1) wallX = rayPos.X + ((mapPos.Y - rayPos.Y + (1 - stepY) / 2) / rayDir.Y) * rayDir.X;
                else wallX = rayPos.Y + ((mapPos.X - rayPos.X + (1 - stepX) / 2) / rayDir.X) * rayDir.Y;
                wallX -= Math.Floor(wallX);

                //x coordinate on the texture
                int texX = (int)(wallX * texture.Width);
                if (side == 0 && rayDir.X > 0) texX = texture.Width - texX - 1;
                if (side == 1 && rayDir.Y < 0) texX = texture.Width - texX - 1;


                //draw the pixels of the stripe as a vertical line
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int d = y * 256 - buffer.Height * 128 + lineHeight * 128;
                    int texY = ((d * texture.Height) / lineHeight) / 256;

                    var color = texture[texX, texY];

                    var valueFactor = 1f;

                    //give x and y sides different brightness
                    if (side == 1)
                    {
                        valueFactor *= 0.75f;
                    }

                    //shade by distance
                    valueFactor *= (float)Math.Min(1, 3.0 / perpWallDist);
                    color = color.ScaleValue(valueFactor);


                    buffer[x, y] = color;
                }

                //FLOOR CASTING
                double floorXWall, floorYWall; //x, y position of the floor texel at the bottom of the wall

                //4 different wall directions possible
                if (side == 0 && rayDir.X > 0)
                {
                    floorXWall = mapPos.X;
                    floorYWall = mapPos.Y + wallX;
                }
                else if (side == 0 && rayDir.X < 0)
                {
                    floorXWall = mapPos.X + 1.0;
                    floorYWall = mapPos.Y + wallX;
                }
                else if (side == 1 && rayDir.Y > 0)
                {
                    floorXWall = mapPos.X + wallX;
                    floorYWall = mapPos.Y;
                }
                else
                {
                    floorXWall = mapPos.X + wallX;
                    floorYWall = mapPos.Y + 1.0;
                }

                double distWall, distPlayer, currentDist;

                distWall = perpWallDist;
                distPlayer = 0.0;

                if (drawEnd < 0) drawEnd = buffer.Height; //becomes < 0 when the integer overflows

                //draw the floor from drawEnd to the bottom of the screen
                for (int y = drawEnd + 1; y < buffer.Height; y++)
                {
                    currentDist = buffer.Height / (2.0 * y - buffer.Height); //you could make a small lookup table for this instead

                    double weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    double currentFloorX = weight * floorXWall + (1.0 - weight) * _player.Position.X;
                    double currentFloorY = weight * floorYWall + (1.0 - weight) * _player.Position.Y;

                    var floorTexture = _mapData.GetFloorTexture(new Point { X = (int)currentFloorX, Y = (int)currentFloorY });
                    var ceilingTexture = _mapData.GetCeilingTexture(new Point { X = (int)currentFloorX, Y = (int)currentFloorY });

                    int floorTexX = (int)((currentFloorX * floorTexture.Width) % floorTexture.Width);
                    int floorTexY = (int)((currentFloorY * floorTexture.Height) % floorTexture.Height);

                    //floor
                    buffer[x, y] = floorTexture[floorTexX, floorTexY].ScaleValue((float)Math.Min(1, 3.0 / currentDist));
                    //ceiling
                    buffer[x, buffer.Height - y] = ceilingTexture[floorTexX, floorTexY].ScaleValue((float)Math.Min(1, 3.0 / currentDist));
                }
            }
        }
    }
}
