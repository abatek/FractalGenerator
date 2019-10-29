using System;
using Tao.FreeGlut;
using OpenGL;
using System.Collections.Generic;
using System.IO;

namespace FractalGenerator
{
    class Program
    {
        public const int n = 4;

        private static int width = 1200, height = 720;
        private static ShaderProgram program;
        private static System.Diagnostics.Stopwatch watch;
        private static float angle;
        public static Square s1, s2, s3, s4, s5, s6;
        public static List<Cube> cubes;

        static void Main(string[] args)
        {
            //Create OpenGL window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Fractal");

            //Glut callbacks for rendering
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);

            //Enable depth testing - ensures correct z-ordering of fragments
            Gl.Enable(EnableCap.DepthTest);

            //Compile shaders
            program = new ShaderProgram(VertexShader, FragmentShader);

            //Create initial squares for shape
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
                new Vector3[] { new Vector3(1, 1, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), new Vector3(1, 1, 1) },
                new Vector3[] { new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0) }
                ); //right
            s6 = new Square(
                new Vector3[] { new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), new Vector3(-1, 1, 1) },
                new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0) }
                ); //left

            //Create initial cube
            Cube initialCube = new Cube();
            //Add squares to cube
            initialCube.Add(s1, s2, s3, s4, s5, s6);
            //Initialize cubes list and add initial cube
            cubes = new List<Cube>();
            cubes.Add(initialCube);

            //Call recursive method for first time
            addCubes(n, initialCube, 2f);

            //Output to obj file
            exportToObj(cubes);

            //Set projection and view matricies
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.75f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up));

            //Start stopwatch for rotation timing
            watch = System.Diagnostics.Stopwatch.StartNew();

            Glut.glutMainLoop();
        }

        private static void exportToObj(List<Cube> cubes)
        {
            //Output to obj file
            string path = String.Format("{0}\\kuba.obj", Environment.CurrentDirectory);
            StreamWriter sr = new StreamWriter(File.Open(path, FileMode.Create));

            foreach (Cube c in cubes)
            {
                foreach (Square s in c.squares)
                {
                    foreach (Vector3 v in s.v)
                    {
                        sr.Write("v ");
                        sr.WriteLine(String.Format("{0} {1} {2}", v.X, v.Y, v.Z));
                    }
                }
            }
            sr.WriteLine();
            long i = 1;
            foreach (Cube c in cubes)
            {
                foreach (Square s in c.squares)
                {
                    sr.Write("f ");
                    sr.WriteLine(String.Format("{0} {1} {2} {3}", i, i + 1, i + 2, i + 3));
                    i += 4;
                }
            }
            sr.Close();
        }

        private static void OnDisplay() { }

        private static void OnRenderFrame()
        {
            //Calculate time elapsed since last frame
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            //Adjust angle based on time passed
            angle += deltaTime;

            //Set up OpenGL viewport and clear colour and depth bits
            Gl.Viewport(0,0,width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Use shader program
            program.Use();

            //For each cube in cubes list, then for each squares in each of these cubes, bind elements, colours and vertices, then finally draw the cube
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
            //set viewing matrix
            program["model_matrix"].SetValue(Matrix4.CreateRotationY(angle/3f) * Matrix4.CreateRotationX(angle) * Matrix4.CreateTranslation(new Vector3(translateX, 0, 0)));
            //bind elements, colours and vertices
            Gl.BindBufferToShaderAttribute(square.square, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(square.color, program, "vertexColor");
            Gl.BindBuffer(square.elements);
            //draw square
            Gl.DrawElements(BeginMode.Quads, square.elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public static void addCubes(int n, Cube originalCube, float shrinkFactor)
        {
            //termination conditioN
            if (n != 0) 
            {
                //create new cubes based on original cube passed, translated and scaled down
                List<Cube> newCubes = originalCube.translateAll(shrinkFactor);
                //add new cubes to list of all cubes
                cubes.AddRange(newCubes);
                //recursively call method for each of the cubes just generated from new cubes
                foreach (Cube c in newCubes)
                {
                    addCubes(n - 1, c, shrinkFactor);
                }
            }
        }

        private static void OnClose()
        {
            //Dispose of all resources created
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
