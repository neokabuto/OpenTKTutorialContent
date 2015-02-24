using OpenTK;

namespace OpenTKTutorial6
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    public abstract class Volume
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public int VertCount;
        public int IndiceCount;
        public int ColorDataCount;
        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewProjectionMatrix = Matrix4.Identity;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Identity;

        public abstract Vector3[] GetVerts();
        public abstract int[] GetIndices(int offset = 0);
        public abstract Vector3[] GetColorData();
        public abstract void CalculateModelMatrix();

        public bool IsTextured = false;
        public int TextureID;
        public int TextureCoordsCount;
        public abstract Vector2[] GetTextureCoords();
    }
}
