using System;
using Tao.FreeGlut;
using OpenGL;

namespace FractalGenerator
{
    class Program
    {
        private static int width = 1200, height = 720;
        private static ShaderProgram program;
        private static VBO<Vector3> pyramid, cube;
        private static VBO<int> pyramidElements, cubeElements;
        private static VBO<Vector3> pyramidColor, cubeColor;
        private static System.Diagnostics.Stopwatch watch;
        private static float angle;
        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Open GL Tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);

            program = new ShaderProgram(VertexShader, FragmentShader);

            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up));

            pyramid = new VBO<Vector3>(new Vector3[] {
                new Vector3(0, 1, 0), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),    //front face
                new Vector3(0, 1, 0), new Vector3(1, -1, 1), new Vector3(1, -1, -1),    //right face
                new Vector3(0, 1, 0), new Vector3(1, -1, -1), new Vector3(-1, -1, -1),  //back face
                new Vector3(0, 1, 0), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1)    //left face
                });
            pyramidColor = new VBO<Vector3>(new Vector3[] { 
                new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0)
            });
            pyramidElements = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, BufferTarget.ElementArrayBuffer);

            cube = new VBO<Vector3>(new Vector3[] {
            new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) 
            });
        
            cubeColor = new VBO<Vector3>(new Vector3[] {
            new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) 
            });
            cubeElements = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, BufferTarget.ElementArrayBuffer);


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

            //draw triangle
            program["model_matrix"].SetValue(Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(new Vector3(-1.5f, 0, 0)));

            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(program.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);
            Gl.BindBuffer(pyramid);
            Gl.VertexAttribPointer(vertexPositionIndex, pyramid.Size, pyramid.PointerType, true, 12, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(pyramidColor, program, "vertexColor");
            Gl.BindBuffer(pyramidElements);

            Gl.DrawElements(BeginMode.Triangles, pyramidElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            
            //draw square 
            program["model_matrix"].SetValue(Matrix4.CreateRotationX(angle) * Matrix4.CreateTranslation(new Vector3(1.5f, 0, 0)));
            Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(cubeColor, program, "vertexColor");
            Gl.BindBuffer(cubeElements);

            Gl.DrawElements(BeginMode.Quads, cubeElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            Glut.glutSwapBuffers();
        }

        private static void OnClose()
        {
            pyramid.Dispose();
            pyramidColor.Dispose();
            pyramidElements.Dispose();
            cube.Dispose();
            cubeColor.Dispose();
            cubeElements.Dispose();
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
