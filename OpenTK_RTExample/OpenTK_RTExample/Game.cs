using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Threading;
using System.Windows.Forms;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenTK_RTExample
{
    class Game : GameWindow
    {
        public Game()
            : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {

        }

        Vector3[] vertdata;
        Vector3[] coldata;
        Vector3[] normdata;
        Vector2[] texcoorddata;
        int[] indicedata;

        int ibo_elements;

        /// <summary>
        /// The camera the main view looks through
        /// </summary>
        Camera mainCamera = new Camera();

        Vector2 lastMousePos;

        Matrix4 view = Matrix4.Identity;

        /// <summary>
        /// Lights enabled in the scene
        /// </summary>
        List<Light> activeLights = new List<Light>();
        
        /// <summary>
        /// Camera to be displayed on the TV
        /// </summary>
        Camera screenCamera = new Camera();

        ObjVolume earth;
        ObjVolume camArrow;
        ObjVolume tvScreen;

        List<Volume> objects = new List<Volume>();
        Dictionary<string, int> textures = new Dictionary<string, int>();
        Dictionary<String, Material> materials = new Dictionary<string, Material>();

        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";

        // Prevents attempting to render until resources are loaded
        private bool resourcesLoaded = false;

        float time = 0.0f;
        private const int MAX_LIGHTS = 5;

        /// <summary>
        /// Width/Height of the texture for the TV screen
        /// </summary>
        const int TextureSize = 256;
        private int fbo_screen;
        
        /// <summary>
        /// Sets up the scene and everything required to view it
        /// </summary>
        void initProgram()
        {
            GL.Enable(EnableCap.Multisample);

            lastMousePos = new Vector2(Mouse.X, Mouse.Y);
            CursorVisible = false;
            mainCamera.MouseSensitivity = 0.0025f;

            GL.GenBuffers(1, out ibo_elements);

            setupScreen();
            loadResources();
            initScene();

            resourcesLoaded = true;
            CursorVisible = false;
        }

        /// <summary>
        /// Creates framebuffer and textures for screen in scene
        /// </summary>
        private void setupScreen()
        {
            // Setup texture, depth buffer, and framebuffer for screen
            textures.Add("screen", GL.GenTexture());
            textures.Add("screendepth", GL.GenTexture());

            // Set up color texture 
            GL.BindTexture(TextureTarget.Texture2D, textures["screen"]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureSize, TextureSize, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // Set up depth texture
            GL.BindTexture(TextureTarget.Texture2D, textures["screendepth"]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.DepthComponent32, TextureSize, TextureSize, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // Create framebuffer for camera image
            GL.GenFramebuffers(1, out fbo_screen);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo_screen);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, textures["screen"], 0); // Color info goes into a texture
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, textures["screendepth"], 0); // Depth info goes into a texture

            // Check for error
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            if (status != FramebufferErrorCode.FramebufferComplete &&
                status != FramebufferErrorCode.FramebufferCompleteExt)
            {
                Console.WriteLine("Error creating framebuffer: {0}", status);
            }

        }

        private void initScene()
        {
            // Add example objects
            TexturedCube floor1 = new TexturedCube
            {
                TextureID = textures["opentksquare.png"],
                Material = new Material(new Vector3(0.2f), new Vector3(1), new Vector3(0.1f), 5)
            };
            floor1.Position += new Vector3(0, 0, 0);
            floor1.Scale = new Vector3(10, 0.1f, 10);
            floor1.CalculateNormals();
            objects.Add(floor1);

            TexturedCube floor2 = new TexturedCube
            {
                TextureID = textures["opentksquare.png"],
                Material = new Material(new Vector3(0.2f), new Vector3(1), new Vector3(0.1f), 5)
            };
            floor2.Position += new Vector3(20, 0, 0);
            floor2.Scale = new Vector3(10, 0.1f, 10);
            floor2.CalculateNormals();
            objects.Add(floor2);

            earth = ObjVolume.LoadFromFile("earth.obj");
            earth.TextureID = textures["earth.png"];
            earth.Material = materials["earth"];
            earth.Position = new Vector3(20, 3, 0);
            objects.Add(earth);

            ObjVolume tv = ObjVolume.LoadFromFile("tv.obj");
            tv.TextureID = textures["tvtexture.png"];
            tv.Material = materials["tv"];
            tv.Scale = new Vector3(2);
            objects.Add(tv);
            
            camArrow = ObjVolume.LoadFromFile("arrow.obj");
            camArrow.TextureID = textures["blank.png"];
            camArrow.Material = new Material(new Vector3(0.2f), new Vector3(0.7f, 0.5f, 0.5f), new Vector3(0.1f));
            camArrow.Scale = new Vector3(0.2f);
            objects.Add(camArrow);
            
            tvScreen = ObjVolume.LoadFromFile("tvscreen.obj");
            tvScreen.Scale = new Vector3(2);
            tvScreen.TextureID = textures["screen"];
            tvScreen.Material = new Material(new Vector3(1.0f), new Vector3(1.0f),  new Vector3(0.5f) );
            tvScreen.Shader = "screen";
            objects.Add(tvScreen);

            // Setup camera
            mainCamera.Position += new Vector3(0f, 2f, 3f);
            screenCamera.Position += new Vector3(20f, 3f, 3f);

            camArrow.Position = screenCamera.Position;
            camArrow.Rotation = screenCamera.Orientation;

            // Add lights
            activeLights.Add(new Light(new Vector3(4, 5, 0), new Vector3(0.3f, 0.3f, 0.4f)));
            activeLights.Add(new Light(new Vector3(22, 5, 1), new Vector3(0.4f, 0.4f, 0.4f)));
        }

        private void loadResources()
        {
            // Create shaders
            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("lit_advanced", new ShaderProgram("vs_lit.glsl", "fs_lit_advanced.glsl", true));
            shaders.Add("screen", new ShaderProgram("vs_lit.glsl", "fs_lit_advanced_screen.glsl", true));

            // Main shader is the multiple light shader
            activeShader = "lit_advanced";

            // Load textures
            textures.Add("opentksquare.png", loadImage("opentksquare.png"));
            textures.Add("blank.png", loadImage("blank.png"));

            // Load materials
            loadMaterials("tv.mtl");
            loadMaterials("earth.mtl");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            Title = "OpenTK Render Target Example";
            GL.ClearColor(Color.CornflowerBlue);

            // Sets triangles facing away from us to not be drawn
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            initProgram();
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!resourcesLoaded)
                return;

            base.OnRenderFrame(e);

            // Render scene onto framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_screen);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, TextureSize, TextureSize); // Instead of rendering to the whole screen, we want the texture
            GL.BindTexture(TextureTarget.Texture2D, textures["screen"]);
            GL.Enable(EnableCap.DepthTest);
            RenderScene(screenCamera);

            // Render scene onto screen
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            RenderScene(mainCamera);
            SwapBuffers();
        }

        /// <summary>
        /// Render the scene from the perspective of a given camera
        /// </summary>
        /// <param name="camera">Camera the video is from the perspective of</param>
        private void RenderScene(Camera camera)
        {
            if (!resourcesLoaded || time < float.Epsilon)
                return;

            GL.UseProgram(shaders[activeShader].ProgramID);

            int indiceat = 0;

            foreach (Volume v in objects)
            {
                // Determine which shader to use
                string shader = activeShader;
                if (v.Shader == "default")
                {
                    GL.UseProgram(shaders[activeShader].ProgramID);
                }
                else
                {
                    GL.UseProgram(shaders[v.Shader].ProgramID);
                    shader = v.Shader;
                }

                shaders[shader].EnableVertexAttribArrays();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);

                v.ViewProjectionMatrix = camera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 40.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;

                // Matrices
                GL.UniformMatrix4(shaders[shader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[shader].GetUniform("view") != -1)
                {
                    GL.UniformMatrix4(shaders[shader].GetUniform("view"), false, ref view);
                }

                if (shaders[shader].GetUniform("model") != -1)
                {
                    GL.UniformMatrix4(shaders[shader].GetUniform("model"), false, ref v.ModelMatrix);
                }

                if (shaders[shader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[shader].GetAttribute("maintexture"), 0);
                }

                // Send material information
                if (shaders[shader].GetUniform("material_ambient") != -1)
                {
                    GL.Uniform3(shaders[shader].GetUniform("material_ambient"), ref v.Material.AmbientColor);
                }

                if (shaders[shader].GetUniform("material_diffuse") != -1)
                {
                    GL.Uniform3(shaders[shader].GetUniform("material_diffuse"), ref v.Material.DiffuseColor);
                }

                if (shaders[shader].GetUniform("material_specular") != -1)
                {
                    GL.Uniform3(shaders[shader].GetUniform("material_specular"), ref v.Material.SpecularColor);
                }

                if (shaders[shader].GetUniform("material_specExponent") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("material_specExponent"), v.Material.SpecularExponent);
                }

                // Fix for screen UV coordinates
                if (shaders[shader].GetUniform("hflip") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("hflip"), (v.TextureID == textures["screen"] || v.TextureID == textures["screendepth"])? 1 : 0);
                }



                if (shaders[shader].GetUniform("map_specular") != -1)
                {
                    // Object has a specular map
                    if (v.Material.SpecularMap != "")
                    {
                        GL.ActiveTexture(TextureUnit.Texture1);
                        GL.BindTexture(TextureTarget.Texture2D, textures[v.Material.SpecularMap]);
                        GL.Uniform1(shaders[shader].GetUniform("map_specular"), 1);
                        GL.Uniform1(shaders[shader].GetUniform("hasSpecularMap"), 1);
                    }
                    else // Object has no specular map
                    {
                        GL.Uniform1(shaders[shader].GetUniform("hasSpecularMap"), 0);
                    }
                }

                // If shader only supports one light, send first light
                if (shaders[shader].GetUniform("light_position") != -1)
                {
                    GL.Uniform3(shaders[shader].GetUniform("light_position"), ref activeLights[0].Position);
                }

                if (shaders[shader].GetUniform("light_color") != -1)
                {
                    GL.Uniform3(shaders[shader].GetUniform("light_color"), ref activeLights[0].Color);
                }

                if (shaders[shader].GetUniform("light_diffuseIntensity") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("light_diffuseIntensity"), activeLights[0].DiffuseIntensity);
                }

                if (shaders[shader].GetUniform("light_ambientIntensity") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("light_ambientIntensity"), activeLights[0].AmbientIntensity);
                }

                if (shaders[shader].GetUniform("light_direction") != -1)
                {
                    GL.Uniform3(shaders[shader].GetUniform("light_direction"), ref activeLights[0].Direction);
                }

                if (shaders[shader].GetUniform("light_type") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("light_type"), (int) activeLights[0].Type);
                }

                if (shaders[shader].GetUniform("light_coneAngle") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("light_coneAngle"), activeLights[0].ConeAngle);
                }

                // Send all lights to shader
                for (int i = 0; i < Math.Min(activeLights.Count, MAX_LIGHTS); i++)
                {
                    if (shaders[shader].GetUniform("lights[" + i + "].position") != -1)
                    {
                        GL.Uniform3(shaders[shader].GetUniform("lights[" + i + "].position"), ref activeLights[i].Position);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].color") != -1)
                    {
                        GL.Uniform3(shaders[shader].GetUniform("lights[" + i + "].color"), ref activeLights[i].Color);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].diffuseIntensity") != -1)
                    {
                        GL.Uniform1(shaders[shader].GetUniform("lights[" + i + "].diffuseIntensity"),
                            activeLights[i].DiffuseIntensity);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].ambientIntensity") != -1)
                    {
                        GL.Uniform1(shaders[shader].GetUniform("lights[" + i + "].ambientIntensity"),
                            activeLights[i].AmbientIntensity);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].direction") != -1)
                    {
                        GL.Uniform3(shaders[shader].GetUniform("lights[" + i + "].direction"),
                            ref activeLights[i].Direction);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].type") != -1)
                    {
                        GL.Uniform1(shaders[shader].GetUniform("lights[" + i + "].type"), (int) activeLights[i].Type);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].coneAngle") != -1)
                    {
                        GL.Uniform1(shaders[shader].GetUniform("lights[" + i + "].coneAngle"), activeLights[i].ConeAngle);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].attenuationLinear") != -1)
                    {
                        GL.Uniform1(shaders[shader].GetUniform("lights[" + i + "].attenuationLinear"),
                            activeLights[i].LinearAttenuation);
                    }

                    if (shaders[shader].GetUniform("lights[" + i + "].attenuationQuadratic") != -1)
                    {
                        GL.Uniform1(shaders[shader].GetUniform("lights[" + i + "].attenuationQuadratic"),
                            activeLights[i].QuadraticAttenuation);
                    }
                }

                // Time is used for the static effect
                if (shaders[shader].GetUniform("time") != -1)
                {
                    GL.Uniform1(shaders[shader].GetUniform("time"), time);
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat*sizeof(uint));
                indiceat += v.IndiceCount;

                shaders[shader].DisableVertexAttribArrays();
            }

            GL.Flush();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!resourcesLoaded)
                return;

            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            // Gather data from objects
            int vertcount = 0;
            foreach (Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                normals.AddRange(v.GetNormals().ToList());
                texcoords.AddRange(v.GetTextureCoords());
                vertcount += v.VertCount;
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            normdata = normals.ToArray();
            texcoorddata = texcoords.ToArray();

            // Send data to the graphics card
            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            if (shaders[activeShader].GetAttribute("vNormal") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vNormal"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(normdata.Length * Vector3.SizeInBytes), normdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

            // Update time for shaders
            time += (float)e.Time;

            // Rotate the planet
            earth.Rotation += Vector3.UnitY * (float)e.Time;

            // Update camera arrow
            camArrow.Position = screenCamera.Position;
            camArrow.Rotation = screenCamera.Orientation;

            // Update model and view matrices
            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
            }

            view = mainCamera.GetViewMatrix();
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
                mainCamera.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                mainCamera.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                mainCamera.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                mainCamera.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                mainCamera.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                mainCamera.Move(0f, 0f, -0.1f);
            }

            // Move screen camera
            if (Keyboard.GetState().IsKeyDown(Key.U))
            {
                screenCamera.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.J))
            {
                screenCamera.Move(0f, 0f, -0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.H))
            {
                screenCamera.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.K))
            {
                screenCamera.Move(0.1f, 0f, 0f);
            }

            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;

                mainCamera.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            }
        }


        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            switch (e.KeyChar)
            {
                // Change shader of screen
                case 'v':
                    tvScreen.Shader = (tvScreen.Shader == "screen") ? "lit_advanced" : "screen";
                    break;
                // Switch between camera view and depth
                case 'c':
                    tvScreen.TextureID = (tvScreen.TextureID == textures["screendepth"]) ? textures["screen"] : (int) textures["screendepth"];
                    break;
            }
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        /// <summary>
        /// Loads an image into a texture
        /// </summary>
        /// <param name="image">Bitmap image to create texture from</param>
        /// <returns>ID of created texture</returns>
        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        /// <summary>
        /// Loads an image into a texture
        /// </summary>
        /// <param name="filename">Location of image to create texture from</param>
        /// <returns>ID of created texture</returns>
        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Loads all materials in a file, along with their required maps.
        /// Materials will not overwrite existing materials with the same name.
        /// </summary>
        /// <param name="filename">MTL file to load from</param>
        private void loadMaterials(String filename)
        {
            foreach (var mat in Material.LoadFromFile(filename))
            {
                if (!materials.ContainsKey(mat.Key))
                {
                    materials.Add(mat.Key, mat.Value);
                }
            }

            // Load textures
            foreach (Material mat in materials.Values)
            {
                if (File.Exists(mat.AmbientMap) && !textures.ContainsKey(mat.AmbientMap))
                {
                    textures.Add(mat.AmbientMap, loadImage(mat.AmbientMap));
                }

                if (File.Exists(mat.DiffuseMap) && !textures.ContainsKey(mat.DiffuseMap))
                {
                    textures.Add(mat.DiffuseMap, loadImage(mat.DiffuseMap));
                }

                if (File.Exists(mat.SpecularMap) && !textures.ContainsKey(mat.SpecularMap))
                {
                    textures.Add(mat.SpecularMap, loadImage(mat.SpecularMap));
                }

                if (File.Exists(mat.NormalMap) && !textures.ContainsKey(mat.NormalMap))
                {
                    textures.Add(mat.NormalMap, loadImage(mat.NormalMap));
                }

                if (File.Exists(mat.OpacityMap) && !textures.ContainsKey(mat.OpacityMap))
                {
                    textures.Add(mat.OpacityMap, loadImage(mat.OpacityMap));
                }
            }
        }
    }
}
