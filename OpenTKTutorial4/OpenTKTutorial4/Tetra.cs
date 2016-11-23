using OpenTK;
using System.Collections.Generic;

namespace OpenTKTutorial4
{
    /// <summary>
    /// A four-sided pyramid, made of 4 triangles
    /// </summary>
    class Tetra : Volume
    {
        Vector3 PointApex;
        Vector3 PointA;
        Vector3 PointB;
        Vector3 PointC;

        public Tetra(Vector3 apex, Vector3 a, Vector3 b, Vector3 c)
        {
            PointApex = apex;
            PointA = a;
            PointB = b;
            PointC = c;

            VertCount = 4;
            IndiceCount = 12;
            ColorDataCount = 4;
        }

        public List<Tetra> Divide(int n = 0)
        {
            if (n == 0)
            {
                return new List<Tetra>(new Tetra[] { this });
            }
            else
            {

                Vector3 halfa = (PointApex + PointA) / 2.0f;
                Vector3 halfb = (PointApex + PointB) / 2.0f;
                Vector3 halfc = (PointApex + PointC) / 2.0f;

                // Calculate points half way between base points
                Vector3 halfab = (PointA + PointB) / 2.0f;
                Vector3 halfbc = (PointB + PointC) / 2.0f;
                Vector3 halfac = (PointA + PointC) / 2.0f;

                Tetra t1 = new Tetra(PointApex, halfa, halfb, halfc);
                Tetra t2 = new Tetra(halfa, PointA, halfab, halfac);
                Tetra t3 = new Tetra(halfb, halfab, PointB, halfbc);
                Tetra t4 = new Tetra(halfc, halfac, halfbc, PointC);

                List<Tetra> output = new List<Tetra>();

                output.AddRange(t1.Divide(n - 1));
                output.AddRange(t2.Divide(n - 1));
                output.AddRange(t3.Divide(n - 1));
                output.AddRange(t4.Divide(n - 1));

                return output;

            }
        }

        public override Vector3[] GetVerts()
        {
            return new Vector3[] { PointApex, PointA, PointB, PointC };
        }

        public override int[] GetIndices(int offset = 0)
        {
            int[] inds = new int[] { //bottom
                                1,3,2,
                                //other sides
                                0,1,2,
                                0,2,3,
                                0,3,1
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

        public override Vector3[] GetColorData()
        {
            return new Vector3[] { new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f), new Vector3(1f, 1f, 0f) };
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }
    }
}
