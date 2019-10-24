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
        List<Square> squares;
        public Cube(List<Square> squares)
        {
            /*
            this.squares = squares;
            foreach (Square s in this.squares)
            {
                Square temp = new Square(new Vector3[4], s.c);

                for(int i = 0; i < 4; i++) {
                    Console.WriteLine(s.v[i].X);
                    temp.v[i].X = s.v[i].X / 6f;
                    Console.WriteLine(s.v[i].X);
                    //Console.ReadKey();
                    temp.v[i].Y = s.v[i].Y / 6f;
                    temp.v[i].Z = s.v[i].Z / 6f;
                }
                Program.drawSquare(temp, 0.5f);
            }
            */
        }

        
    }
}
