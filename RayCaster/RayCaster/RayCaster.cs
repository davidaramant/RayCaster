using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace RayCasterGame
{
    sealed class Renderer
    {
        PlayerInfo _player;
        MapData _mapData;

        public Renderer(PlayerInfo player, MapData mapData)
        {
            _player = player;
            _mapData = mapData;
        }

        public void Render(ScreenBuffer buffer)
        {
            //Parallel.For(0, buffer.Width, new ParallelOptions { MaxDegreeOfParallelism = 1 }, column =>
            Parallel.For(0, buffer.Width, column =>
            {
                //calculate ray position and direction 
                //x-coordinate in camera space
                var cameraX = 2.0f * (buffer.Width - column) / (float)buffer.Width - 1f;
                var rayPos = _player.Position;

                var rayDir = _player.Direction + _player.CameraPlane * cameraX;

                //which box of the map we're in  
                var mapPos = new Point((int)rayPos.X, (int)rayPos.Y);

                //length of ray from current position to next x or y-side
                float sideDistX;
                float sideDistY;

                //length of ray from one x or y-side to next x or y-side
                float deltaDistX = (float)Math.Sqrt(1f + (rayDir.Y * rayDir.Y) / (rayDir.X * rayDir.X));
                float deltaDistY = (float)Math.Sqrt(1f + (rayDir.X * rayDir.X) / (rayDir.Y * rayDir.Y));
                float perpWallDist;

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
                    sideDistX = (mapPos.X + 1.0f - rayPos.X) * deltaDistX;
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
                    sideDistY = (mapPos.Y + 1.0f - rayPos.Y) * deltaDistY;
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

                    wallHit = _mapData.HasWalls(mapPos.X, mapPos.Y);
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

                var texture = _mapData.GetWallTexture(mapPos.X, mapPos.Y, sideHit);

                //calculate value of wallX
                float wallX; //where exactly the wall was hit
                if (sideHit == northSouthSideHit)
                    wallX = rayPos.X + ((mapPos.Y - rayPos.Y + (1 - stepY) / 2) / rayDir.Y) * rayDir.X;
                else
                    wallX = rayPos.Y + ((mapPos.X - rayPos.X + (1 - stepX) / 2) / rayDir.X) * rayDir.Y;
                wallX -= FFloor(wallX);

                //x coordinate on the texture
                int texX = (int)(wallX * texture.Width);

                var lightLevel = _mapData.FindWallLightLevel(mapPos.X, mapPos.Y, wallX, sideHit);

                //draw the pixels of the stripe as a vertical line
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int d = y * 256 - buffer.Height * 128 + lineHeight * 128;
                    int texY = ((d * texture.Height) / lineHeight) / 256;

                    var color = texture[texX, texY];

                    buffer[column, y] = _mapData.Shade(color, lightLevel);
                }

                //FLOOR CASTING
                float floorXWall, floorYWall; //x, y position of the floor texel at the bottom of the wall

                //4 different wall directions possible
                switch (sideHit)
                {
                    case SectorSide.West:
                        floorXWall = mapPos.X;
                        floorYWall = mapPos.Y + wallX;
                        break;
                    case SectorSide.East:
                        floorXWall = mapPos.X + 1.0f;
                        floorYWall = mapPos.Y + wallX;
                        break;
                    case SectorSide.South:
                        floorXWall = mapPos.X + wallX;
                        floorYWall = mapPos.Y + 1.0f;
                        break;
                    case SectorSide.North:
                    default:
                        floorXWall = mapPos.X + wallX;
                        floorYWall = mapPos.Y;
                        break;
                }

                float distWall, distPlayer, currentDist;

                distWall = perpWallDist;
                distPlayer = 0.0f;

                if (drawEnd < 0)
                    drawEnd = buffer.Height; //becomes < 0 when the integer overflows

                //draw the floor from drawEnd to the bottom of the screen
                for (int y = drawEnd + 1; y < buffer.Height; y++)
                {
                    currentDist = buffer.Height / (2.0f * y - buffer.Height); //you could make a small lookup table for this instead

                    float weight = (currentDist - distPlayer) / (distWall - distPlayer);

                    float currentFloorX = weight * floorXWall + (1.0f - weight) * _player.Position.X;
                    float currentFloorY = weight * floorYWall + (1.0f - weight) * _player.Position.Y;

                    var floorTexture = _mapData.GetFloorTexture(currentFloorX, currentFloorY);
                    var ceilingTexture = _mapData.GetCeilingTexture(currentFloorX, currentFloorY);

                    int floorTexX = (int)((currentFloorX * floorTexture.Width) % floorTexture.Width);
                    int floorTexY = (int)((currentFloorY * floorTexture.Height) % floorTexture.Height);

                    //floor
                    buffer[column, y] =
                        _mapData.Shade(currentFloorX, currentFloorY, floorTexture[floorTexX, floorTexY], currentDist);
                    //ceiling
                    buffer[column, buffer.Height - y] =
                        _mapData.Shade(currentFloorX, currentFloorY, ceilingTexture[floorTexX, floorTexY], currentDist);
                }
            });
        }

        private static float FFloor( float f)
        {
            return (float)(int)f;
        }

        Vector2 RayCast(Vector2 origin, Vector2 direction)
        {
            // ---- ---- ---- ---- ---- ---- ---- ---- SETUP
            // starting positions
            float x_px = origin.X, x_py = origin.Y;
            float y_px = origin.X, y_py = origin.Y;

            // ---- ---- ---- ---- ---- ---- ---- ---- Y AXIS INTERSECTIONS
            if (direction.Y > 0)
            {
                float yp = direction.X / direction.Y;
                
                y_px += (FFloor(y_py + 1f) - y_py) * yp;
                y_py = FFloor(y_py + 1f);
                for (; ; )
                {
                    // Bounds checking of map disabled since we guarantee there will always be an edge
                    //if (FFloor(y_py) >= _mapData.MapHeight) break;
                    //if (FFloor(y_px) < 0) break;
                    //if (FFloor(y_px) >= _mapData.MapWidth) break;
                    
                    if (_mapData.HasWalls((int)y_px, (int)y_py))
                        break;
                    
                    y_px += yp;
                    y_py = (float)((int)y_py + 1);
                }
            }
            else if (direction.Y < 0)
            {
                float yp = direction.X / direction.Y;
                
                y_px -= (y_py - FFloor(y_py)) * yp;
                y_py = FFloor(y_py);
                for (; ; )
                {
                    // Bounds checking of map disabled since we guarantee there will always be an edge
                    //if (FFloor(y_py - 1f) < 0) break;
                    //if (FFloor(y_px) < 0f) break;
                    //if (FFloor(y_px) >= _mapData.MapWidth) break;
                    
                    if (_mapData.HasWalls((int)y_px, (int)(y_py - 1)))
                        break;
                    
                    y_px -= yp;
                    y_py = (float)((int)y_py - 1);
                }
            }

            // ---- ---- ---- ---- ---- ---- ---- ---- X AXIS INTERSECTIONS
            if (direction.X > 0)
            {
                float xp = direction.Y / direction.X;
                
                x_py += (FFloor(x_px + 1f) - x_px) * xp;
                x_px = FFloor(x_px + 1);
                for (; ; )
                {
                    // Bounds checking of map disabled since we guarantee there will always be an edge
                    //if (FFloor(x_px) >= _mapData.MapWidth) break;
                    //if (FFloor(x_py) < 0f) break;
                    //if (FFloor(x_py) >= _mapData.MapHeight) break;
                    
                    if (_mapData.HasWalls((int)x_px, (int)x_py))
                        break;
                    
                    x_px = (float)((int)x_px + 1f);
                    x_py += xp;
                }
            }
            else if (direction.X < 0)
            {
                float xp = direction.Y / direction.X;
                
                x_py -= (x_px - FFloor(x_px)) * xp;
                x_px = FFloor(x_px);
                for (; ; )
                {
                    // Bounds checking of map disabled since we guarantee there will always be an edge
                    //if (FFloor(x_px - 1f) < 0) break;
                    //if (FFloor(x_py) < 0) break;
                    //if (FFloor(x_py) >= _mapData.MapHeight) break;
                    
                    if (_mapData.HasWalls((int)(x_px - 1), (int)x_py))
                        break;
                    
                    x_px = (float)((int)x_px - 1);
                    x_py -= xp;
                }
            }

            // ---- ---- ---- ---- ---- ---- ---- ---- INTERSECTION SELECTION
            float dx1 = (x_px - origin.X) * (x_px - origin.X);
            float dy1 = (x_py - origin.Y) * (x_py - origin.Y);
            float dx2 = (y_px - origin.X) * (y_px - origin.X);
            float dy2 = (y_py - origin.Y) * (y_py - origin.Y);

            if ((dx1 + dy1) < (dx2 + dy2))
                return new Vector2(x_px, x_py);
            else
                return new Vector2(y_px, y_py);
        }
    }
}
