using OpenTK;
using System.Linq;

namespace OpenTKTutorial8
{
    /// <summary>
    /// A cube with texture coordinates giving each side the entire texture
    /// </summary>
    class TexturedCube : Cube
    {
        public TexturedCube()
            : base()
        {
            VertCount = 24;
            IndiceCount = 36;
            TextureCoordsCount = 24;
        }

        public override Vector3[] GetVerts()
        {
            return new Vector3[] {
                //left
                new Vector3(-0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, 0.5f,  -0.5f),
                new Vector3(0.5f, -0.5f,  -0.5f),
                new Vector3(-0.5f, 0.5f,  -0.5f),

                //back
                new Vector3(0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, 0.5f,  -0.5f),
                new Vector3(0.5f, 0.5f,  0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),

                //right
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                new Vector3(0.5f, 0.5f,  0.5f),
                new Vector3(-0.5f, 0.5f,  0.5f),

                //top
                new Vector3(0.5f, 0.5f,  -0.5f),
                new Vector3(-0.5f, 0.5f,  -0.5f),
                new Vector3(0.5f, 0.5f,  0.5f),
                new Vector3(-0.5f, 0.5f,  0.5f),

                //front
                new Vector3(-0.5f, -0.5f,  -0.5f), 
                new Vector3(-0.5f, 0.5f,  0.5f), 
                new Vector3(-0.5f, 0.5f,  -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),

                //bottom
                new Vector3(-0.5f, -0.5f,  -0.5f), 
                new Vector3(0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f)

            };
        }

        public override int[] GetIndices(int offset = 0)
        {
            int[] inds = new int[] {
                //left
                0,1,2,0,3,1,

                //back
                4,5,6,4,6,7,

                //right
                8,9,10,8,10,11,

                //top
                13,14,12,13,15,14,

                //front
                16,17,18,16,19,17,

                //bottom 
                20,21,22,20,22,23
            };

            if (offset != 0)
            {
                for (int i = 0; i < inds.Length; i++)
                {
                    inds[i] += offset;
                }
            }

            return inds;
        }

        public override Vector2[] GetTextureCoords()
        {
            return new Vector2[] {
                // left
                new Vector2(0.0f, 0.0f),
                new Vector2(-1.0f, 1.0f),
                new Vector2(-1.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
 
                // back
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(-1.0f, 1.0f),
                new Vector2(-1.0f, 0.0f),
 
                // right
                new Vector2(-1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(-1.0f, 1.0f),
 
                // top
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(-1.0f, 0.0f),
                new Vector2(-1.0f, 1.0f),
 
                // front
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
 
                // bottom
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(-1.0f, 1.0f),
                new Vector2(-1.0f, 0.0f)
            };
        }

        public override Vector3[] GetColorData()
        {
            return base.GetColorData().Concat(base.GetColorData().Concat(base.GetColorData())).ToArray();
        }
    }
}
