using System;
using System.Runtime.CompilerServices;

namespace raymarching
{
    class Map
    {
        public Chunk[,,] chunks;

        // Voxel ray marching from http://www.cse.chalmers.se/edu/year/2010/course/TDA361/grid.pdf
        // Optimised by keeping block lookups within the current chunk, which minimises bitshifts, masks and multiplication operations
        public void RayMarch(in Vector3 start, Vector3 velocity, in double max, ref bool hit, ref Axis axis)
        {
            int x = (int)start.X;
            int y = (int)start.Y;
            int z = (int)start.Z;

            if (y < 0 || y >= Constants.MAP_SIZE_Y || x < 0 || x >= Constants.MAP_SIZE_X || z < 0 || z >= Constants.MAP_SIZE_Z)
            {
                hit = false;
                return;
            }

            // 2^5 = 32 (chunkSize)
            int chunkIndexX = x >> Constants.SHIFT;
            int chunkIndexY = y >> Constants.SHIFT;
            int chunkIndexZ = z >> Constants.SHIFT;

            var c = chunks[chunkIndexX, chunkIndexY, chunkIndexZ];

            // Determine the chunk-relative position of the ray using a bit-mask
            int i = x & Constants.MASK;
            int j = y & Constants.MASK;
            int k = z & Constants.MASK;

            // Calculate the index of this block in the chunk data[] array
            int access = j + i * Constants.CHUNK_SIZE + k * Constants.CHUNK_SIZE_SQUARED;

            // Calculate the end position of the ray
            var end = start + velocity;

            // If the start and end positions of the ray both lie on the same coordinate on the voxel grid
            if (x == (int)end.X && y == (int)end.Y && z == (int)end.Z)
            {
                // The chunk is null if it contains no blocks
                if (c == null)
                {
                    hit = false;
                }

                // If the block is empty
                else if (c.data[access].kind == 0)
                {
                    hit = false;
                }

                // Else the ray begins and ends within the same non-empty block
                else
                {
                    hit = true;
                }

                return;
            }

            // These variables are used to determine whether the ray has left the current working chunk.
            //  For example when travelling in the negative Y direction,
            //  if j == -1 then we have left the current working chunk
            int iComparison, jComparison, kComparison;

            // When leaving the current working chunk, the chunk-relative position must be reset.
            //  For example when travelling in the negative Y direction,
            //  j should be reset to CHUNK_SIZE - 1 when entering the new current working chunk
            int iReset, jReset, kReset;

            // When leaving the current working chunk, the access variable must also be updated.
            //  These values store how much to add or subtract from the access, depending on
            //  the direction of the ray:
            int xAccessReset, yAccessReset, zAccessReset;

            // The amount to increase i, j and k in each axis (either 1 or -1)
            int iStep, jStep, kStep;

            // When incrementing j, the chunk access is simply increased by 1
            // When incrementing i, the chunk access is increased by 32 (CHUNK_SIZE)
            // When incrementing k, the chunk access is increased by 1024 (CHUNK_SIZE_SQUARED)
            // These variables store whether to increase or decrease by the above amounts
            int xAccessIncrement, zAccessIncrement;

            // The distance to the closest voxel boundary in map units
            double xDist, yDist, zDist;

            if (velocity.X > 0)
            {
                iStep = 1;
                iComparison = Constants.CHUNK_SIZE;
                iReset = 0;
                xAccessReset = -Constants.CHUNK_SIZE_SQUARED;
                xAccessIncrement = Constants.CHUNK_SIZE;
                xDist = (x - start.X + 1);
            }
            else
            {
                iStep = -1;
                iComparison = -1;
                iReset = Constants.CHUNK_SIZE - 1;
                xAccessReset = Constants.CHUNK_SIZE_SQUARED;
                xAccessIncrement = -Constants.CHUNK_SIZE;
                xDist = (start.X - x);
            }

            if (velocity.Y > 0)
            {
                jStep = 1;
                jComparison = Constants.CHUNK_SIZE;
                jReset = 0;
                yAccessReset = -Constants.CHUNK_SIZE;
                yDist = (y - start.Y + 1);
            }
            else
            {
                jStep = -1;
                jComparison = -1;
                jReset = Constants.CHUNK_SIZE - 1;
                yAccessReset = Constants.CHUNK_SIZE;
                yDist = (start.Y - y);
            }

            if (velocity.Z > 0)
            {
                kStep = 1;
                kComparison = Constants.CHUNK_SIZE;
                kReset = 0;
                zAccessIncrement = Constants.CHUNK_SIZE_SQUARED;
                zAccessReset = -Constants.CHUNK_SIZE_CUBED;
                zDist = (z - start.Z + 1);
            }
            else
            {
                kStep = -1;
                kComparison = -1;
                kReset = Constants.CHUNK_SIZE - 1;
                zAccessIncrement = -Constants.CHUNK_SIZE_SQUARED;
                zAccessReset = Constants.CHUNK_SIZE_CUBED;
                zDist = (start.Z - z);
            }

            // This variable is used to track the current progress throughout the ray march
            double t = 0.0;

            velocity.Normalize();
            double xInverted = Math.Abs(1 / velocity.X);
            double yInverted = Math.Abs(1 / velocity.Y);
            double zInverted = Math.Abs(1 / velocity.Z);

            // Determine the distance to the closest voxel boundary in units of t
            //  - These values indicate how far we have to travel along the ray to reach the next voxel
            //  - If any component of the direction is perpendicular to an axis, the distance is double.PositiveInfinity
            double xDistance = velocity.X == 0 ? double.PositiveInfinity : xInverted * xDist;
            double yDistance = velocity.Y == 0 ? double.PositiveInfinity : yInverted * yDist;
            double zDistance = velocity.Z == 0 ? double.PositiveInfinity : zInverted * zDist;

            while (t <= max)
            {
                // Exit check
                if (c != null && c.data[access].kind != 0)
                {
                    hit = true;
                    return;
                }

                // Determine the closest voxel boundary
                if (yDistance < xDistance)
                {
                    if (yDistance < zDistance)
                    {
                        // Advance to the closest voxel boundary in the Y direction

                        // Increment the chunk-relative position and the block access position
                        j += jStep;
                        access += jStep;

                        // Check if we have exited the current working chunk.
                        // This means that j is either -1 or 32
                        if (j == jComparison)
                        {
                            // If moving in the positive direction, reset j to 0.
                            // If moving in the negative Y direction, reset j to 31
                            j = jReset;

                            // Reset the chunk access
                            access += yAccessReset;

                            // Calculate the new chunk index
                            chunkIndexY += jStep;

                            // If the new chunk is outside the map, exit
                            if (chunkIndexY < 0 || chunkIndexY >= Constants.CHUNK_AMOUNT_Y)
                            {
                                hit = false;
                                return;
                            }

                            // Get a reference to the new working chunk
                            c = chunks[chunkIndexX, chunkIndexY, chunkIndexZ];
                        }

                        // Update our progress in the ray 
                        t = yDistance;

                        // Set the new distance to the next voxel Y boundary
                        yDistance += yInverted;

                        // For collision purposes we also store the last axis that the ray collided with
                        // This allows us to reflect particle velocity on the correct axis
                        axis = Axis.Y;
                    }
                    else
                    {
                        k += kStep;
                        access += zAccessIncrement;

                        if (k == kComparison)
                        {
                            k = kReset;
                            access += zAccessReset;

                            chunkIndexZ += kStep;

                            if (chunkIndexZ < 0 || chunkIndexZ >= Constants.CHUNK_AMOUNT_Z)
                            {
                                hit = false;
                                return;
                            }

                            c = chunks[chunkIndexX, chunkIndexY, chunkIndexZ];
                        }

                        t = zDistance;
                        zDistance += zInverted;
                        axis = Axis.Z;
                    }
                }
                else if (xDistance < zDistance)
                {
                    i += iStep;
                    access += xAccessIncrement;

                    if (i == iComparison)
                    {
                        i = iReset;
                        access += xAccessReset;

                        chunkIndexX += iStep;

                        if (chunkIndexX < 0 || chunkIndexX >= Constants.CHUNK_AMOUNT_X)
                        {
                            hit = false;
                            return;
                        }

                        c = chunks[chunkIndexX, chunkIndexY, chunkIndexZ];
                    }

                    t = xDistance;
                    xDistance += xInverted;
                    axis = Axis.X;
                }
                else
                {
                    k += kStep;
                    access += zAccessIncrement;

                    if (k == kComparison)
                    {
                        k = kReset;
                        access += zAccessReset;

                        chunkIndexZ += kStep;

                        if (chunkIndexZ < 0 || chunkIndexZ >= Constants.CHUNK_AMOUNT_Z)
                        {
                            hit = false;
                            return;
                        }

                        c = chunks[chunkIndexX, chunkIndexY, chunkIndexZ];
                    }

                    t = zDistance;
                    zDistance += zInverted;
                    axis = Axis.Z;
                }
            }

            hit = false;
        }
    }
}
