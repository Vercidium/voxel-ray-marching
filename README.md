# voxel-ray-marching
Optimised ray marching for voxel worlds, written in C# and open sourced from the Vercidium Engine.

The explanation of the source code can be found in this [blog post](https://vercidium.com/blog/optimised-voxel-raymarching/).

## References
This voxel ray marching algorithm is based on [A Fast Voxel Traversal Algorithm for Ray Tracing](http://www.cse.yorku.ca/~amana/research/grid.pdf) by John Amanatides and Andrew Woo and has been optimised by keeping block lookups within the current working chunk.

## Benchmarks
Benchmarks were run with a Ryzen 5 1600 CPU.

| Ray length (blocks) | Ray march time (nanoseconds) | Rays per 16ms frame |
|:-------------------:|:----------------------------:|:-------------------:|
| 1-10                | 250                          | 64000               | 
| 200-400             | 3400                         | 4700                |    

## In Practice
In [Sector's Edge](https://www.youtube.com/watch?v=qoKzhIouzsk), ray marching is used to calculate particle and projectile collision with the map.

![Particles screenshot](https://vercidium.com/blog/content/images/size/w2000/2020/01/raymarching.jpg)
