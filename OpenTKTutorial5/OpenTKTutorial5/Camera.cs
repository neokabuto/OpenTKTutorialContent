using OpenTK;
using System;

namespace OpenTKTutorial5
{
    /// <summary>
    /// A basic camera using Euler angles
    /// </summary>
    class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Orientation = new Vector3((float)Math.PI, 0f, 0f);
        public float MoveSpeed = 0.2f;
        public float MouseSensitivity = 0.01f;

        /// <summary>
        /// Calculate a view matrix for this camera
        /// </summary>
        /// <returns>A view matrix from this camera</returns>
        public Matrix4 GetViewMatrix()
        {
            /**This code uses some trigonometry to create a vector in the direction that the camera is looking,
             * and then uses the LookAt static function of the Matrix4 class to use that vector and
             * the position to create a view matrix we can use to change where our scene is viewed from. 
             * The Vector3.UnitY is being assigned to the "up" parameter,
             * which will keep our camera angle so that the right side is up.*/
            Vector3 lookat = new Vector3();

            lookat.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            lookat.Y = (float)Math.Sin((float)Orientation.Y);
            lookat.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));

            return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitY);
        }

        /// <summary>
        /// Moves the camera in local space
        /// </summary>
        /// <param name="x">Distance to move along the screen's x axis</param>
        /// <param name="y">Distance to move along the axis of the camera</param>
        /// <param name="z">Distance to move along the screen's y axis</param>
        public void Move(float x, float y, float z)
        {
            /** When the camera moves, we don't want it to move relative to the world coordinates 
             * (like the XYZ space its position is in), but instead relative to the camera's view. 
             * Like the view angle, this requires a bit of trigonometry. */

            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        /// <summary>
        /// Changes the rotation of the camera based on mouse input
        /// </summary>
        /// <param name="x">The x distance the mouse moved</param>
        /// <param name="y">The y distance the mouse moved</param>
        public void AddRotation(float x, float y)
        { 
            /** In this case, our rotation is due to mouse input, so it's based on the distances the mouse moved along each axis.*/
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
