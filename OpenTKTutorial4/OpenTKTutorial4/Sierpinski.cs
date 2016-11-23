using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTKTutorial4
{
    class Sierpinski : Volume
    {
        /// <summary>
        /// Create a Sierpiński triangle-based pyramid
        /// </summary>
        /// <param name="numSubdivisions">Subdivisions in the Sierpiński triangle on each side</param>
        public Sierpinski(int numSubdivisions = 1)
        {
            int NumTris = (int)Math.Pow(4, numSubdivisions + 1);

            VertCount = NumTris;
            ColorDataCount = NumTris;
            IndiceCount = 3 * NumTris;

            Tetra twhole = new Tetra(new Vector3(0.0f, 0.0f, 1.0f),  // Apex center
                            new Vector3(0.943f, 0.0f, -0.333f),  // Base center top
                            new Vector3(-0.471f, 0.816f, -0.333f),  // Base left bottom
                            new Vector3(-0.471f, -0.816f, -0.333f));

            List<Tetra> allTets = twhole.Divide(numSubdivisions);

            int offset = 0;
            foreach (Tetra t in allTets)
            {
                verts.AddRange(t.GetVerts());
                indices.AddRange(t.GetIndices(offset * 4));
                colors.AddRange(t.GetColorData());
                offset++;
            }

        }

        private List<Vector3> verts = new List<Vector3>();
        private List<int> indices = new List<int>();
        private List<Vector3> colors = new List<Vector3>();

        public override Vector3[] GetVerts()
        {
            return verts.ToArray();
        }

        public override Vector3[] GetColorData()
        {
            return colors.ToArray();
        }

        public override int[] GetIndices(int offset = 0)
        {
            int[] inds = indices.ToArray();

            if (offset != 0)
            {
                for (int i = 0; i < inds.Length; i++)
                {
                    inds[i] += offset;
                }
            }

            return inds;
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

    }
}
