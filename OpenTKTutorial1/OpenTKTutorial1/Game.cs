using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace OpenTKTutorial1
{
    class Game: GameWindow
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Hello OpenTK!";

            GL.ClearColor(Color.CornflowerBlue);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            /* The first thing we need to do is to tell OpenGL which direction we're looking at.
             * Because we'll be actually making something in 3D, the direction the camera faces is important. */
            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);


            /* Now we'll want to draw the triangle itself.
             * The first step is to tell OpenGL we want to draw something. We do this with the GL.Begin function.
             * This takes a single parameter, which is the drawing mode to use.
             * There are options to draw quadrilaterals, triangles, points, polygons, and "strips".*/
            GL.Begin(BeginMode.Triangles);

            /* Now that we've told it how we want to draw, we need to give it the vertices for our shape.
             * To do this, we use the GL.Vertex3 function. It takes three floats as coordinates for a single point in 3D space.*/
            GL.Color3(1.0f, 0.0f, 0.0f); 
            GL.Vertex3(-1.0f, -1.0f, 4.0f);

            GL.Color3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(1.0f, -1.0f, 4.0f);

            GL.Color3(0.0f, 0.0f, 1.0f); 
            GL.Vertex3(0.0f, 1.0f, 4.0f);   

            GL.End();

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            /* OpenGL needs to be told how to adjust for the new window size, so we need some code that handles it. */
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }
    }
}
