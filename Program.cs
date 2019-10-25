using System;
using Tao.FreeGlut;
using OpenGL;
using System.Collections.Generic;

namespace FractalGenerator
{
    class Program
    {
        private static int width = 1200, height = 720;
        private static ShaderProgram program;
        private static System.Diagnostics.Stopwatch watch;
        private static float angle;
        public static Square s1, s2, s3, s4, s5, s6;
        public static int n;
        public static List<Cube> cubes;

        static void Main(string[] args)
        {
            n = 4;
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Fractal");

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

            Cube initialCube = new Cube();
            initialCube.Add(s1, s2, s3, s4, s5, s6);
            cubes = new List<Cube>();
            cubes.Add(initialCube);

            addCubes(n, initialCube, 2f);

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


            foreach (Cube c in cubes)
            {
                foreach (Square s in c.squares)
                {
                    drawSquare(s, 0);
                }
            }


            Glut.glutSwapBuffers();
            
        }

        public static void drawSquare(Square square, float translateX)
        {
            //program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(translateX, 0, 0)));

            program["model_matrix"].SetValue(Matrix4.CreateRotationY(angle/10f) * Matrix4.CreateRotationX(angle/10f) * Matrix4.CreateTranslation(new Vector3(translateX, 0, 0)));
            Gl.BindBufferToShaderAttribute(square.square, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(square.color, program, "vertexColor");
            Gl.BindBuffer(square.elements);
            Gl.DrawElements(BeginMode.Quads, square.elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public static void addCubes(int n, Cube originalCube, float shrinkFactor)
        {
            if (n != 0) 
            {
                List<Cube> newCubes = originalCube.translateAll(shrinkFactor);
                cubes.AddRange(newCubes);
                foreach (Cube c in newCubes)
                {
                    addCubes(n - 1, c, shrinkFactor);
                }
            }
        }

        private static void OnClose()
        {
            foreach (Cube c in cubes)
            {
                foreach (Square s in c.squares)
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
