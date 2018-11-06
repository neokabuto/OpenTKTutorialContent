using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTKTutorial6
{
    class Game : GameWindow
    {
        public Game()
            : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {

        }

        Vector3[] vertdata;
        Vector3[] coldata;
        Vector2[] texcoorddata;
        int[] indicedata;
        int ibo_elements;
        Camera cam = new Camera();
        Vector2 lastMousePos = new Vector2();

        List<Volume> objects = new List<Volume>();
        Dictionary<string, int> textures = new Dictionary<string, int>();
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";
        
        float time = 0.0f;

        void initProgram()
        {
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);
            CursorVisible = false;
            cam.MouseSensitivity = 0.0025f;

            GL.GenBuffers(1, out ibo_elements);

            // Load shaders from file
            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("textured", new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", true));

            activeShader = "textured";

            // Load textures from file
            textures.Add("opentksquare.png", loadImage("opentksquare.png"));
            textures.Add("opentksquare2.png", loadImage("opentksquare2.png"));

            // Create our objects
            TexturedCube tc = new TexturedCube();
            tc.TextureID = textures["opentksquare.png"];
            objects.Add(tc);

            TexturedCube tc2 = new TexturedCube();
            tc2.Position += new Vector3(1f, 1f, 1f);
            tc2.TextureID = textures["opentksquare2.png"];
            objects.Add(tc2);

            // Move camera away from origin
            cam.Position += new Vector3(0f, 0f, 3f);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            Title = "Hello OpenTK!";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            shaders[activeShader].EnableVertexAttribArrays();

            int indiceat = 0;

            // Draw all our objects
            foreach (Volume v in objects)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);

                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetUniform("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("maintexture"), 0);
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            // Assemble vertex and indice data for all volumes
            int vertcount = 0;
            foreach (Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                texcoords.AddRange(v.GetTextureCoords());
                vertcount += v.VertCount;
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }


            // Buffer texture coordinates if shader supports it
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Update object positions
            time += (float)e.Time;

            objects[0].Position = new Vector3(0.3f, -0.5f + (float)Math.Sin(time), -3.0f);
            objects[0].Rotation = new Vector3(0.55f * time, 0.25f * time, 0);
            objects[0].Scale = new Vector3(0.5f, 0.5f, 0.5f);

            objects[1].Position = new Vector3(-1f, 0.5f + (float)Math.Cos(time), -2.0f);
            objects[1].Rotation = new Vector3(-0.25f * time, -0.35f * time, 0);
            objects[1].Scale = new Vector3(0.7f, 0.7f, 0.7f);

            // Update model view matrices
            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f); 
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Handles keyboard and mouse input
        /// </summary>
        private void ProcessInput()
        {
            /* We'll use the escape key to make the program easy to exit from.
             * Otherwise it's annoying since the mouse cursor is trapped inside.*/
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }

            /** Let's start by adding WASD input (feel free to change the keys if you want,
             * hopefully a later tutorial will have a proper input manager) for translating the camera. */
            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                cam.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                cam.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                cam.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                cam.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                cam.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                cam.Move(0f, 0f, -0.1f);
            }

            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;

                cam.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            }
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException e)
            {
                return -1;
            }
        }
    }
}
