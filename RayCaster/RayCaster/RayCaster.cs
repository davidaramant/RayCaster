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
                var direction = new Vector2 { X = _player.Direction.Y, Y = -_player.Direction.X };

                _player.Move(_mapData, direction, moveSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.StrafeRight))
            {
                var direction = new Vector2 { X = -_player.Direction.Y, Y = _player.Direction.X };

                _player.Move(_mapData, direction, moveSpeed);
            }

            if (inputs.HasFlag(MovementInputs.TurnRight))
            {
                _player.Rotate(rotSpeed);
            }
            else if (inputs.HasFlag(MovementInputs.TurnLeft))
            {
                _player.Rotate(-rotSpeed);
            }
        }

        public void Render(ScreenBuffer buffer)
        {
            System.Threading.Tasks.Parallel.For(0, buffer.Width, column =>
            //for (int column = 0; column < buffer.Width; column++)
            {
                //calculate ray position and direction 
                //x-coordinate in camera space
                var cameraX = 2.0f * (buffer.Width - column) / (float)buffer.Width - 1f;
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

                bool wallHit = false;
                // which side of the sector was hit
                SectorSide sideHit = default(SectorSide);

                // Holds the possible north/south and east/west side that can be hit by this ray
                SectorSide northSouthSideHit = default(SectorSide);
                SectorSide eastWestSideHit = default(SectorSide);

                //calculate step and initial sideDist

                // Pointing left
                if (rayDir.X < 0)
                {
                    stepX = -1;
                    sideDistX = (rayPos.X - mapPos.X) * deltaDistX;
                    eastWestSideHit = SectorSide.East;
                }
                else // pointing right
                {
                    stepX = 1;
                    sideDistX = (mapPos.X + 1.0 - rayPos.X) * deltaDistX;
                    eastWestSideHit = SectorSide.West;
                }

                if (rayDir.Y < 0) // Pointing up
                {
                    stepY = -1;
                    sideDistY = (rayPos.Y - mapPos.Y) * deltaDistY;
                    northSouthSideHit = SectorSide.South;
                }
                else // pointing down
                {
                    stepY = 1;
                    sideDistY = (mapPos.Y + 1.0 - rayPos.Y) * deltaDistY;
                    northSouthSideHit = SectorSide.North;
                }

                //perform DDA                               
                while (!wallHit)
                {
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapPos.X += stepX;
                        sideHit = eastWestSideHit;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapPos.Y += stepY;
                        sideHit = northSouthSideHit;
                    }

                    wallHit = _mapData.HasWalls(mapPos);
                }
                //Calculate distance projected on camera direction (oblique distance will give fisheye effect!)
                if (sideHit == eastWestSideHit)
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

                var sectorHit = _mapData.GetSectorInfo(mapPos);
                var texture = sectorHit.GetWallTexture(sideHit);

                //calculate value of wallX
                double wallX; //where exactly the wall was hit
                if (sideHit == northSouthSideHit)
                    wallX = rayPos.X + ((mapPos.Y - rayPos.Y + (1 - stepY) / 2) / rayDir.Y) * rayDir.X;
                else
                    wallX = rayPos.Y + ((mapPos.X - rayPos.X + (1 - stepX) / 2) / rayDir.X) * rayDir.Y;
                wallX -= Math.Floor(wallX);

                //x coordinate on the texture
                int texX = (int)(wallX * texture.Width);

                var sectorInFrontOfWall = _mapData.GetSectorInfo(mapPos, sideHit);

                //draw the pixels of the stripe as a vertical line
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int d = y * 256 - buffer.Height * 128 + lineHeight * 128;
                    int texY = ((d * texture.Height) / lineHeight) / 256;

                    var color = texture[texX, texY];

                    var valueFactor = 1f;

                    //give x and y sides different brightness
                    if (sideHit == northSouthSideHit)
                    {
                        valueFactor *= 0.75f;
                    }

                    buffer[column, y] = sectorInFrontOfWall.Shade(color, perpWallDist).ScaleValue(valueFactor);
                }

                //FLOOR CASTING
                double floorXWall, floorYWall; //x, y position of the floor texel at the bottom of the wall

                //4 different wall directions possible
                switch (sideHit)
                {
                    case SectorSide.West:
                        floorXWall = mapPos.X;
                        floorYWall = mapPos.Y + wallX;
                        break;
                    case SectorSide.East:
                        floorXWall = mapPos.X + 1.0;
                        floorYWall = mapPos.Y + wallX;
                        break;
                    case SectorSide.South:
                        floorXWall = mapPos.X + wallX;
                        floorYWall = mapPos.Y + 1.0;
                        break;
                    case SectorSide.North:
                    default:
                        floorXWall = mapPos.X + wallX;
                        floorYWall = mapPos.Y;
                        break;
                }

                double distWall, distPlayer, currentDist;

                distWall = perpWallDist;
                distPlayer = 0.0;

                if (drawEnd < 0)
                    drawEnd = buffer.Height; //becomes < 0 when the integer overflows

                //draw the floor from drawEnd to the bottom of the screen
                for (int y = drawEnd + 1; y < buffer.Height; y++)
                {
                    currentDist = buffer.Height / (2.0 * y - buffer.Height); //you could make a small lookup table for this instead

                    double weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    double currentFloorX = weight * floorXWall + (1.0 - weight) * _player.Position.X;
                    double currentFloorY = weight * floorYWall + (1.0 - weight) * _player.Position.Y;

                    var sectorToDraw = _mapData.GetSectorInfo(new Point { X = (int)currentFloorX, Y = (int)currentFloorY });

                    int floorTexX = (int)((currentFloorX * sectorToDraw.FloorTexture.Width) % sectorToDraw.FloorTexture.Width);
                    int floorTexY = (int)((currentFloorY * sectorToDraw.FloorTexture.Height) % sectorToDraw.FloorTexture.Height);

                    //floor
                    buffer[column, y] =
                        sectorToDraw.Shade(sectorToDraw.FloorTexture[floorTexX, floorTexY], currentDist);
                    //ceiling
                    buffer[column, buffer.Height - y] =
                        sectorToDraw.Shade(sectorToDraw.CeilingTexture[floorTexX, floorTexY], currentDist);
                }
            });
        }
    }
}
