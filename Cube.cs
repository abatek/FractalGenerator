using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.FreeGlut;
using OpenGL;

namespace FractalGenerator
{
    public class Square
    {
        public VBO<Vector3> square;
        public VBO<Vector3> color;
        public VBO<uint> elements;
        public Vector3[] v;
        public Vector3[] c;

        public Square(Vector3[] v, Vector3[] c)
        {
            this.v = v;
            this.c = c;
            square = new VBO<Vector3>(v);
            elements = new VBO<uint>(new uint[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            color = new VBO<Vector3>(c);
        }
    }

    public class Cube
    {
        public List<Square> squares;
        public Cube()
        {
            squares = new List<Square>();
        }
        //adds squares to list of squares - 6 squares make up cube
        public void Add(Square s1, Square s2, Square s3, Square s4, Square s5, Square s6) {
            squares.Add(s1);
            squares.Add(s2);
            squares.Add(s3);
            squares.Add(s4);
            squares.Add(s5);
            squares.Add(s6);
        }
        //translate and scale current cube, returns new cube
        public Cube translate(float shrinkFactor, float translate_x, float translate_y, float translate_z)
        {
            Cube newCube = new Cube();
            foreach (Square s in this.squares)
            {
                newCube.squares.Add(new Square(new Vector3[] {
                        new Vector3(s.v[0].X / shrinkFactor + translate_x, s.v[0].Y / shrinkFactor + translate_y, s.v[0].Z / shrinkFactor + translate_z),
                        new Vector3(s.v[1].X / shrinkFactor + translate_x, s.v[1].Y / shrinkFactor + translate_y, s.v[1].Z / shrinkFactor + translate_z),
                        new Vector3(s.v[2].X / shrinkFactor + translate_x, s.v[2].Y / shrinkFactor + translate_y, s.v[2].Z / shrinkFactor + translate_z),
                        new Vector3(s.v[3].X / shrinkFactor + translate_x, s.v[3].Y / shrinkFactor + translate_y, s.v[3].Z / shrinkFactor + translate_z)
                        }
                    , s.c));
            }
            return newCube;
        }
        //translate and scales list of cubes by preset factors for standard fractal to clean up recursive method
        public List<Cube> translateAll(float shrinkFactor)
        {
            List<Cube> cubes = new List<Cube>();
            Cube cube_posX = this.translate(shrinkFactor, 1.5f, 0, 0);
            Cube cube_negX = this.translate(shrinkFactor, -1.5f, 0, 0);
            Cube cube_posY = this.translate(shrinkFactor, 0, 1.5f, 0);
            Cube cube_negY = this.translate(shrinkFactor, 0, -1.5f, 0);
            Cube cube_posZ = this.translate(shrinkFactor, 0, 0, 1.5f);
            Cube cube_negZ = this.translate(shrinkFactor, 0, 0, -1.5f);
            cubes.Add(cube_posX);
            cubes.Add(cube_posY);
            cubes.Add(cube_posZ);
            cubes.Add(cube_negX);
            cubes.Add(cube_negY);
            cubes.Add(cube_negZ);
            return cubes;
        }

    }
}
