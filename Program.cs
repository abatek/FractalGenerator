using System;
using Tao.FreeGlut;
using OpenGL;
using System.Collections.Generic;

namespace FractalGenerator
{
    class Program
    {
        private static int width = 1920, height = 1080;
        private static ShaderProgram program;
        private static VBO<Vector3> pyramid, cube;
        private static VBO<int> pyramidElements, cubeElements;
        private static VBO<Vector3> pyramidColor, cubeColor;
        private static System.Diagnostics.Stopwatch watch;
        private static float angle;
        public static Square s1, s2, s3, s4, s5, s6;
        public static List<Square> sqs1, sqs2, sqs3, sqs4, sqs5, sqs6, sqs7;
        public static List<List<Square>> listoflist;
        public static int n;

        static void Main(string[] args)
        {
            n = 4;
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Open GL Tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);

            Gl.Enable(EnableCap.DepthTest);

            program = new ShaderProgram(VertexShader, FragmentShader);
            s1 = new Square(
                new Vector3[] { new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, -1, -1), new Vector3(-1, -1, -1) },
                new Vector3[] { new Vector3(0, 0.5f, 1), new Vector3(0, 0.5f, 1), new Vector3(0, 0.5f, 1), new Vector3(0, 0.5f, 1) }
                ); //back
            s2 = new Square(
                new Vector3[] { new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1) },
                new Vector3[] { new Vector3(0.5f, 0.5f, 1), new Vector3(0.5f, 0.5f, 1), new Vector3(0.5f, 0.5f, 1), new Vector3(0.5f, 0.5f, 1) }
                ); //top
            s3 = new Square(
                new Vector3[] { new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1) },
                new Vector3[] { new Vector3(1, 0.5f, 1), new Vector3(1, 0.5f, 1), new Vector3(1, 0.5f, 1), new Vector3(1, 0.5f, 1) }
            ); //front
            s4 = new Square(
                new Vector3[] { new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1) },
                new Vector3[] { new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) }
                ); //bottom
            s5 = new Square(
                new Vector3[] { new Vector3(1, 1, -1), new Vector3(1, -1, -1),  new Vector3(1, -1, 1), new Vector3(1, 1, 1) },
                new Vector3[] { new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0) }
                ); //right
            s6 = new Square(
                new Vector3[] { new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), new Vector3(-1, 1, 1) },
                new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0) }
                ); //left

            listoflist = new List<List<Square>>();


            sqs1 = new List<Square>();
            sqs1.Add(s1);
            sqs1.Add(s2);
            sqs1.Add(s3);
            sqs1.Add(s4);
            sqs1.Add(s5);
            sqs1.Add(s6);
            //listoflist.Add(sqs1);

            recur(n, sqs1, 2f);

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.75f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up));


            watch = System.Diagnostics.Stopwatch.StartNew();

            Glut.glutMainLoop();
        }

        private static void OnDisplay() { }

        private static void OnRenderFrame()
        {
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            angle += deltaTime;

            Gl.Viewport(0,0,width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Use();

            //draw square 
            foreach (List<Square> list in listoflist)
            {
                foreach (Square s in list)
                {
                    drawSquare(s, 0);
                }
            }


            Glut.glutSwapBuffers();
            

        }

        public static void drawSquare(Square square, float translateX)
        {
            //program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(translateX, 0, 0)));

            program["model_matrix"].SetValue(Matrix4.CreateRotationY(angle) * Matrix4.CreateRotationX(angle) * Matrix4.CreateTranslation(new Vector3(translateX, 0, 0)));
            Gl.BindBufferToShaderAttribute(square.square, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(square.color, program, "vertexColor");
            Gl.BindBuffer(square.elements);
            Gl.DrawElements(BeginMode.Quads, square.elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public static List<Square> changeSquare(List<Square> sqs, float div, float transX, float transY, float transZ, float xangle)
        {
            List<Square> ret = new List<Square>();
            foreach (Square s in sqs)
            {
                Square temp = new Square(new Vector3[]
                    {
                        new Vector3(s.v[0].X / div + transX, s.v[0].Y / div + transY, s.v[0].Z / div + transZ),
                        new Vector3(s.v[1].X / div + transX, s.v[1].Y / div + transY, s.v[1].Z / div + transZ),
                        new Vector3(s.v[2].X / div + transX, s.v[2].Y / div + transY, s.v[2].Z / div + transZ),
                        new Vector3(s.v[3].X / div + transX, s.v[3].Y / div + transY, s.v[3].Z / div + transZ)
                    }
                    , s.c);
                ret.Add(temp);
            }
            return ret;
        }

        public static void recur(int n, List<Square> sqs, float div, float angle) 
        {
            if (n != 0)
            {
                List<Square> l1 = changeSquare(sqs, div, 1.5f, 0, 0, angle);
                List<Square> l2 = changeSquare(sqs, div, -1.5f, 0, 0, angle);
                List<Square> l3 = changeSquare(sqs, div, 0, 1.5f, 0, angle);
                List<Square> l4 = changeSquare(sqs, div, 0, -1.5f, 0, angle);
                List<Square> l5 = changeSquare(sqs, div, 0, 0, 1.5f, angle);
                List<Square> l6 = changeSquare(sqs, div, 0, 0, -1.5f, angle);
                listoflist.Add(l1);
                listoflist.Add(l2);
                listoflist.Add(l3);
                listoflist.Add(l4);
                listoflist.Add(l5);
                listoflist.Add(l6);
                recur(n - 1, l1, div, angle);
                recur(n - 1, l2, div, angle);
                recur(n - 1, l3, div, angle);
                recur(n - 1, l4, div, angle);
                recur(n - 1, l5, div, angle);
                recur(n - 1, l6, div, angle);
            }
        }

        public static Vector3 rotatePoint(Vector3 vector, float xangle, float yangle, float zangle) 
        {
            Vector3 newVec;
            if (xangle != 0) 
            {
            
            }
            if (yangle != 0)
            {

            }
            if(zangle != 0)
            {

            }
            return newVec;
        }

        private static void OnClose()
        {
            foreach (List<Square> list in listoflist)
            {
                foreach (Square s in list)
                {
                    s.square.Dispose();
                    s.elements.Dispose();
                }
            }
            program.DisposeChildren = true;
            program.Dispose();
        }

        public static string VertexShader = @"
#version 130

in vec3 vertexPosition;
in vec3 vertexColor;

out vec3 color;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    color = vertexColor;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130

in vec3 color;

out vec4 fragment;

void main(void)
{
    fragment = vec4(color, 1);
}
";
    }
}
