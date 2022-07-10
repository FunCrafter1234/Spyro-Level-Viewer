using Raylib_cs;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace CSRenderEngine
{
    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }

    public enum DrawMode
    {
        Wireframe,
        Model,
        Both
    }

    public class CompareTris : IComparer<triangle>
    {
        public int Compare(triangle x, triangle y)
        {
            float z1 = (x.p[0].z + x.p[1].z + x.p[2].z) / 3f;
            float z2 = (y.p[0].z + y.p[1].z + y.p[2].z) / 3f;

            if (z1 > z2)
                return -1;
            else
                return 1;
        }
    }

    public struct vec3d
    {
        public float x, y, z, w;

        public vec3d(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            w = 1f;
        }
    }

    public struct triangle
    {
        public vec3d[] p;
        public float lightdp;
        public Color clr;

        public triangle()
        {
            p = new vec3d[3];
            lightdp = 0f;
            clr = new Color();
            clr = calc_light();
        }

        public triangle(vec3d[] _p)
        {
            p = _p;
            lightdp = 0f;
            clr = new Color();
            clr = calc_light();
        }

        public triangle(vec3d[] _p, float _light)
        {
            p = _p;
            lightdp = _light;
            clr = new Color();
            clr = calc_light();
        }

        public triangle(triangle std, float _light)
        {
            p = new vec3d[3];
            p[0] = std.p[0];
            p[1] = std.p[1];
            p[2] = std.p[2];
            lightdp = _light;
            clr = new Color();
            clr = calc_light();
        }

        private Color calc_light()
        {
            int lightcolor = (int)Math.Round((decimal)255 * (decimal)lightdp);
            if (lightcolor > 255) lightcolor = 255;
            if (lightcolor < 0) lightcolor = -lightcolor;
            return new Color(lightcolor, lightcolor, lightcolor, 255);
        }
    }

    public struct mesh
    {
        public List<triangle> tris;

        public mesh()
        {
            tris = new List<triangle>();
        }

        public bool LoadFromObjectFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            List<vec3d> verts = new List<vec3d>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Length == 0)
                    continue;

                if (line[0] == 'v')
                {
                    string[] splitted = line.Split(' ');
                    verts.Add(new vec3d(float.Parse(splitted[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitted[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitted[3], CultureInfo.InvariantCulture.NumberFormat)));
                }

                if (line[0] == 'f')
                {
                    string[] splitted = line.Split(' ');
                    int[] arr = new int[3];
                    arr[0] = Convert.ToInt32(splitted[1]);
                    arr[1] = Convert.ToInt32(splitted[2]);
                    arr[2] = Convert.ToInt32(splitted[3]);

                    tris.Add(new triangle(new vec3d[] { verts[arr[0] - 1], verts[arr[1] - 1], verts[arr[2] - 1] }));
                }
            }
            return true;
        }
    }

    public struct mat4x4
    {
        public float[,] m = new float[4, 4];

        public mat4x4()
        {
            m = new float[4, 4];
        }
    }

    internal class Program
    {
        static mesh meshCube = new mesh();
        static mat4x4 matProj = new mat4x4();
        static vec3d vCamera = new vec3d(0f, 0f, 0f);
        static vec3d vLookDir = new vec3d(0f, 0f, 0f);
        static float fTheta;
        static float fYaw;
        static Stopwatch stopWatch = new Stopwatch();
        static DrawMode drawMode = DrawMode.Model;

        static float fDist = 5f;
        static float fSpeed = 1f;


        static void Main(string[] args)
        {
            OnUserCreate();
            OnUserUpdate();
            OnUserEnd();
        }

        static void OnUserCreate()
        {
            //Raylib.InitWindow(256 * 4, 240 * 4, "CSharp3D-Demo");
            Raylib.InitWindow(1080, 720, "CSharp3D-Demo");

            meshCube = new mesh();

            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 0.0f), new vec3d(0.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f) }));
            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f), new vec3d(1.0f, 0.0f, 0.0f) }));

            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f) }));
            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(1.0f, 0.0f, 1.0f) }));

            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f) }));
            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(0.0f, 0.0f, 1.0f) }));

            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(0.0f, 1.0f, 0.0f) }));
            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 0.0f), new vec3d(0.0f, 0.0f, 0.0f) }));

            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 1.0f, 0.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(1.0f, 1.0f, 1.0f) }));
            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(1.0f, 1.0f, 0.0f) }));

            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 0.0f) }));
            meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 0.0f), new vec3d(1.0f, 0.0f, 0.0f) }));

            //meshCube.LoadFromObjectFile(@"C:\Users\Marvin\Desktop\simple3D\mountains.obj");
            matProj = Matrix_MakeProjection(90f, (float)Raylib.GetScreenHeight() / (float)Raylib.GetScreenWidth(), 0.1f, 1000f);

            stopWatch.Start();
        }

        static void OnUserUpdate()
        {
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
                    vCamera.y -= 0.01f * fSpeed;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
                    vCamera.y += 0.01f * fSpeed;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
                    vCamera.x -= 0.01f * fSpeed;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
                    vCamera.x += 0.01f * fSpeed;

                vec3d vForward = Vector_Mul(vLookDir, 0.01f * fSpeed);

                if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
                    vCamera = Vector_Add(vCamera, vForward);

                if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
                    vCamera = Vector_Sub(vCamera, vForward);

                if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                    fYaw += 0.001f * fSpeed;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                    fYaw -= 0.001f * fSpeed;

                if (Raylib.IsKeyReleased(KeyboardKey.KEY_F1))
                    drawMode = drawMode.Next();

                if (Raylib.IsKeyReleased(KeyboardKey.KEY_F2))
                {
                    vCamera = new vec3d(0f, 0f, 0f);
                    fYaw = 0f;
                }




                mat4x4 matRotZ = new mat4x4(), matRotX = new mat4x4();
                //fTheta = 1f * ((float)stopWatch.Elapsed.TotalMilliseconds / 1000);

                Raylib.DrawFPS(10, 10);

                // Rotation Z
                matRotZ = Matrix_MakeRotationZ(3.141f); //3.141f;
                matRotX = Matrix_MakeRotationX(0f);

                mat4x4 matTrans = new mat4x4();
                matTrans = Matrix_MakeTranslation(0f, 0f, 16f);

                mat4x4 matWorld = new mat4x4();
                matWorld = Matrix_MakeIdentity();
                matWorld = Matrix_MultiplyMatrix(matRotZ, matRotX);
                matWorld = Matrix_MultiplyMatrix(matWorld, matTrans);

                vec3d vUp = new vec3d(0f, 1f, 0f);
                vec3d vTarget = new vec3d(0f,0f,1f);
                mat4x4 matCameraRot = Matrix_MakeRotationY(fYaw);
                vLookDir = Matrix_MultiplyVector(matCameraRot, vTarget);
                vTarget = Vector_Add(vCamera, vLookDir);

                mat4x4 matCamera = Matrix_PointAt(vCamera, vTarget, vUp);

                mat4x4 matView = Matrix_QuickInverse(matCamera);

                List<triangle> TrianglesToRaster = new List<triangle>();

                foreach (var tri in meshCube.tris)
                {
                    triangle triProjected = new triangle(new vec3d[] { new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f) });
                    triangle triTransformed = new triangle(new vec3d[] { new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f) });
                    triangle triViewed = new triangle(new vec3d[] { new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f) });


                    triTransformed.p[0] = Matrix_MultiplyVector(matWorld, tri.p[0]);
                    triTransformed.p[1] = Matrix_MultiplyVector(matWorld, tri.p[1]);
                    triTransformed.p[2] = Matrix_MultiplyVector(matWorld, tri.p[2]);

                    vec3d normal = new vec3d(0f, 0f, 0f), line1 = new vec3d(0f, 0f, 0f), line2 = new vec3d(0f, 0f, 0f);
                    line1 = Vector_Sub(triTransformed.p[1], triTransformed.p[0]);
                    line2 = Vector_Sub(triTransformed.p[2], triTransformed.p[0]);

                    normal = Vector_CrossProduct(line1, line2);

                    normal = Vector_Normalise(normal);

                    float l = (float)Math.Sqrt((double)(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z));
                    normal.x /= l; normal.y /= l; normal.z /= l;

                    vec3d vCameraRay = Vector_Sub(triTransformed.p[0], vCamera);

                    if(Vector_DotProduct(normal, vCameraRay) < 0f)
                    {
                        //LIGHT
                        vec3d light_direction = new vec3d(0f, 0f, -1f);
                        light_direction = Vector_Normalise(light_direction);

                        float lightdp = Math.Max(0.1f, Vector_DotProduct(light_direction, normal));

                        triViewed.p[0] = Matrix_MultiplyVector(matView, triTransformed.p[0]);
                        triViewed.p[1] = Matrix_MultiplyVector(matView, triTransformed.p[1]);
                        triViewed.p[2] = Matrix_MultiplyVector(matView, triTransformed.p[2]);

                        int nClippedTriangles = 0;
                        triangle[] clipped = new triangle[2];
                        nClippedTriangles = Trinagle_ClipAgainstPlane(new vec3d(0f, 0f, 0.5f), new vec3d(0f, 0f, 0.5f), ref triViewed, out clipped[0], out clipped[1]);

                        for(int n = 0; n < nClippedTriangles; n++)
                        {
                            triProjected.p[0] = Matrix_MultiplyVector(matProj, clipped[n].p[0]);
                            triProjected.p[1] = Matrix_MultiplyVector(matProj, clipped[n].p[1]);
                            triProjected.p[2] = Matrix_MultiplyVector(matProj, clipped[n].p[2]);

                            triProjected.p[0] = Vector_Div(triProjected.p[0], triProjected.p[0].w + fDist);
                            triProjected.p[1] = Vector_Div(triProjected.p[1], triProjected.p[1].w + fDist);
                            triProjected.p[2] = Vector_Div(triProjected.p[2], triProjected.p[2].w + fDist);

                            vec3d vOffsetView = new vec3d(1f, 1f, 0);
                            triProjected.p[0] = Vector_Add(triProjected.p[0], vOffsetView);
                            triProjected.p[1] = Vector_Add(triProjected.p[1], vOffsetView);
                            triProjected.p[2] = Vector_Add(triProjected.p[2], vOffsetView);

                            triProjected.p[0].x *= 0.5f * (float)Raylib.GetScreenWidth();
                            triProjected.p[0].y *= 0.5f * (float)Raylib.GetScreenHeight();
                            triProjected.p[1].x *= 0.5f * (float)Raylib.GetScreenWidth();
                            triProjected.p[1].y *= 0.5f * (float)Raylib.GetScreenHeight();
                            triProjected.p[2].x *= 0.5f * (float)Raylib.GetScreenWidth();
                            triProjected.p[2].y *= 0.5f * (float)Raylib.GetScreenHeight();

                            TrianglesToRaster.Add(new triangle(triProjected, lightdp));
                        }
                    }
                }

                //SORT
                TrianglesToRaster.Sort(0, TrianglesToRaster.Count, new CompareTris());

                foreach (var triToRaster in TrianglesToRaster)
                {
                    triangle[] clipped = new triangle[2];
                    List<triangle> listTriangles = new List<triangle>();
                    listTriangles.Add(triToRaster);
                    int nNewTriangles = 1;

                    for (int p = 0; p < 4; p++)
                    {
                        int nTrisToAdd = 0;
                        while (nNewTriangles > 0)
                        {
                            triangle test = listTriangles.First();
                            listTriangles.RemoveAt(0);
                            nNewTriangles--;

                            switch(p)
                            {
                                case 0: nTrisToAdd = Trinagle_ClipAgainstPlane(new vec3d(0f, 0f, 0f), new vec3d(0f, 1f, 0f), ref test, out clipped[0], out clipped[1]); break;
                                case 1: nTrisToAdd = Trinagle_ClipAgainstPlane(new vec3d(0f, (float)Raylib.GetScreenHeight() - 1, 0f), new vec3d(0f, -1f, 0f), ref test, out clipped[0], out clipped[1]); break;
                                case 2: nTrisToAdd = Trinagle_ClipAgainstPlane(new vec3d(0f, 0f, 0f), new vec3d(1f, 0f, 0f), ref test, out clipped[0], out clipped[1]); break;
                                case 3: nTrisToAdd = Trinagle_ClipAgainstPlane(new vec3d((float)Raylib.GetScreenWidth() - 1, 0f, 0f), new vec3d(-1f, 0f, 0f), ref test, out clipped[0], out clipped[1]); break;
                            }


                            for (int w = 0; w < nTrisToAdd; w++)
                                listTriangles.Add(clipped[w]);
                        }
                        nNewTriangles = listTriangles.Count;
                    }

                    for (int trinum = 0; trinum < listTriangles.Count; trinum++)
                    {
                        if (drawMode == DrawMode.Wireframe || drawMode == DrawMode.Both)
                        {
                            DrawCustomTriangle(ConvertVec(listTriangles[trinum].p[0]), ConvertVec(listTriangles[trinum].p[1]), ConvertVec(listTriangles[trinum].p[2]), drawMode == DrawMode.Both ? Color.RED : Color.WHITE, 1f);
                        }
                        if (drawMode == DrawMode.Model || drawMode == DrawMode.Both)
                        {
                            Raylib.DrawTriangle(ConvertVec(listTriangles[trinum].p[0]), ConvertVec(listTriangles[trinum].p[1]), ConvertVec(listTriangles[trinum].p[2]), listTriangles[trinum].clr);
                            Raylib.DrawTriangle(ConvertVec(listTriangles[trinum].p[2]), ConvertVec(listTriangles[trinum].p[1]), ConvertVec(listTriangles[trinum].p[0]), listTriangles[trinum].clr);
                        }
                    }
                }

                Raylib.EndDrawing();
            }
        }

        static void OnUserEnd()
        {
            Raylib.CloseWindow();
        }

        public static vec3d Matrix_MultiplyVector(mat4x4 m, vec3d i)
        {
            vec3d v = new vec3d();
            v.x = i.x * m.m[0, 0] + i.y * m.m[1, 0] + i.z * m.m[2, 0] + i.w * m.m[3, 0];
            v.y = i.x * m.m[0, 1] + i.y * m.m[1, 1] + i.z * m.m[2, 1] + i.w * m.m[3, 1];
            v.z = i.x * m.m[0, 2] + i.y * m.m[1, 2] + i.z * m.m[2, 2] + i.w * m.m[3, 2];
            v.w = i.x * m.m[0, 3] + i.y * m.m[1, 3] + i.z * m.m[2, 3] + i.w * m.m[3, 3];
            return v;
        }

        //AUSLAGERN
        public static mat4x4 Matrix_MakeIdentity()
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0, 0] = 1f;
            matrix.m[1, 1] = 1f;
            matrix.m[2, 2] = 1f;
            matrix.m[3, 3] = 1f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeRotationX(float fAngleRad)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = 1.0f;
            matrix.m[1,1] = (float)Math.Cos(fAngleRad);
            matrix.m[1,2] = (float)Math.Sin(fAngleRad);
            matrix.m[2,1] = (float)-Math.Sin(fAngleRad);
            matrix.m[2,2] = (float)Math.Cos(fAngleRad);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeRotationY(float fAngleRad)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = (float)Math.Cos(fAngleRad);
            matrix.m[0,2] = (float)Math.Sin(fAngleRad);
            matrix.m[2,0] = (float)-Math.Sin(fAngleRad);
            matrix.m[1,1] = 1.0f;
            matrix.m[2,2] = (float)Math.Cos(fAngleRad);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeRotationZ(float fAngleRad)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = (float)Math.Cos(fAngleRad);
            matrix.m[0,1] = (float)Math.Sin(fAngleRad);
            matrix.m[1,0] = (float)-Math.Sin(fAngleRad);
            matrix.m[1,1] = (float)Math.Cos(fAngleRad);
            matrix.m[2,2] = 1.0f;
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeTranslation(float x, float y, float z)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = 1.0f;
            matrix.m[1,1] = 1.0f;
            matrix.m[2,2] = 1.0f;
            matrix.m[3,3] = 1.0f;
            matrix.m[3,0] = x;
            matrix.m[3,1] = y;
            matrix.m[3,2] = z;
            return matrix;
        }

        public static mat4x4 Matrix_MakeProjection(float fFovDegrees, float fAspectRatio, float fNear, float fFar)
        {
            float fFovRad = 1.0f / (float)Math.Tan(fFovDegrees * 0.5f / 180f * 3.14159f);
            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = fAspectRatio * fFovRad;
            matrix.m[1,1] = fFovRad;
            matrix.m[2,2] = fFar / (fFar - fNear);
            matrix.m[3,2] = (-fFar * fNear) / (fFar - fNear);
            matrix.m[2,3] = 1.0f;
            matrix.m[3,3] = 0.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MultiplyMatrix(mat4x4 m1, mat4x4 m2)
        {
            mat4x4 matrix = new mat4x4();
            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 4; r++)
                    matrix.m[r,c] = m1.m[r,0] * m2.m[0,c] + m1.m[r,1] * m2.m[1,c] + m1.m[r,2] * m2.m[2,c] + m1.m[r,3] * m2.m[3,c];
            return matrix;
        }

        public static mat4x4 Matrix_PointAt(vec3d pos, vec3d target, vec3d up)
        {
            vec3d newForward = Vector_Sub(target, pos);
            newForward = Vector_Normalise(newForward);

            vec3d a = Vector_Mul(newForward, Vector_DotProduct(up, newForward));
            vec3d newUp = Vector_Sub(up, a);
            newUp = Vector_Normalise(newUp);

            vec3d newRight = Vector_CrossProduct(newUp, newForward);

            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = newRight.x; matrix.m[0,1] = newRight.y; matrix.m[0, 2] = newRight.z; matrix.m[0,3] = 0f;
            matrix.m[1,0] = newUp.x; matrix.m[1,1] = newUp.y; matrix.m[1, 2] = newUp.z; matrix.m[1, 3] = 0f;
            matrix.m[2,0] = newForward.x; matrix.m[2,1] = newForward.y; matrix.m[2, 2] = newForward.z; matrix.m[2, 3] = 0f;
            matrix.m[3,0] = pos.x; matrix.m[3,1] = pos.y; matrix.m[3, 2] = pos.z; matrix.m[3, 3] = 1f;
            return matrix;
        }

        public static mat4x4 Matrix_QuickInverse(mat4x4 m)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0,0] = m.m[0,0]; matrix.m[0,1] = m.m[1,0]; matrix.m[0,2] = m.m[2,0]; matrix.m[0,3] = 0.0f;
            matrix.m[1,0] = m.m[0,1]; matrix.m[1,1] = m.m[1,1]; matrix.m[1,2] = m.m[2,1]; matrix.m[1,3] = 0.0f;
            matrix.m[2,0] = m.m[0,2]; matrix.m[2,1] = m.m[1,2]; matrix.m[2,2] = m.m[2,2]; matrix.m[2,3] = 0.0f;
            matrix.m[3,0] = -(m.m[3,0] * matrix.m[0,0] + m.m[3,1] * matrix.m[1,0] + m.m[3,2] * matrix.m[2,0]);
            matrix.m[3,1] = -(m.m[3,0] * matrix.m[0,1] + m.m[3,1] * matrix.m[1,1] + m.m[3,2] * matrix.m[2,1]);
            matrix.m[3,2] = -(m.m[3,0] * matrix.m[0,2] + m.m[3,1] * matrix.m[1,2] + m.m[3,2] * matrix.m[2,2]);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static vec3d Vector_Add(vec3d v1, vec3d v2)
        {
            return new vec3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static vec3d Vector_Sub(vec3d v1, vec3d v2)
        {
            return new vec3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static vec3d Vector_Mul(vec3d v1, float k)
        {
            return new vec3d(v1.x * k, v1.y * k, v1.z * k);
        }

        public static vec3d Vector_Div(vec3d v1, float k)
        {
            return new vec3d(v1.x / k, v1.y / k, v1.z / k);
        }

        public static float Vector_DotProduct(vec3d v1, vec3d v2)
        {
            return  v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static vec3d Vector_Normalise(vec3d v)
        {
            float l = Vector_Length(v);
            return new vec3d(v.x / l, v.y / l, v.z / l);
        }

        public static float Vector_Length(vec3d v)
        {
            return (float)Math.Sqrt(Vector_DotProduct(v, v));
        }

        public static vec3d Vector_CrossProduct(vec3d v1, vec3d v2)
        {
            vec3d v = new vec3d();
            v.x = v1.y * v2.z - v1.z * v2.y;
            v.y = v1.z * v2.x - v1.x * v2.z;
            v.z = v1.x * v2.y - v1.y * v2.x;
            return v;
        }

        public static vec3d Vector_IntersectPlane(vec3d plane_p, vec3d plane_n, vec3d lineStart, vec3d lineEnd)
        {
            plane_n = Vector_Normalise(plane_n);
            float plane_d = -Vector_DotProduct(plane_n, plane_p);
            float ad = Vector_DotProduct(lineStart, plane_n);
            float bd = Vector_DotProduct(lineEnd, plane_n);
            float t = (-plane_d - ad) / (bd - ad);
            vec3d lineStartToEnd = Vector_Sub(lineEnd, lineStart);
            vec3d lineToIntersect = Vector_Mul(lineStartToEnd, t);
            return Vector_Add(lineStart, lineToIntersect);
        }

        public static int Trinagle_ClipAgainstPlane(vec3d plane_p, vec3d plane_n, ref triangle in_tri, out triangle out_tri1, out triangle out_tri2)
        {
            out_tri1 = new triangle();
            out_tri2 = new triangle();

            plane_n = Vector_Normalise(plane_n);

            float dist (vec3d p) {
                vec3d n = Vector_Normalise(p);
                return (plane_n.x * p.x + plane_n.y * p.y + plane_n.z * p.z - Vector_DotProduct(plane_n, plane_p));
            }

            vec3d[] inside_points = new vec3d[3]; int nInsidePointCount = 0;
            vec3d[] outside_points = new vec3d[3]; int nOutsidePointCount = 0;

            float d0 = dist(in_tri.p[0]);
            float d1 = dist(in_tri.p[1]);
            float d2 = dist(in_tri.p[2]);

            if (d0 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[0]; }
            else { outside_points[nOutsidePointCount++] = in_tri.p[0]; }
            if (d1 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[1]; }
            else { outside_points[nOutsidePointCount++] = in_tri.p[1]; }
            if (d2 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[2]; }
            else { outside_points[nOutsidePointCount++] = in_tri.p[2]; }

            if(nInsidePointCount == 0)
            {
                return 0;
            }

            if(nInsidePointCount == 3)
            {
                out_tri1 = in_tri;
                return 1;
            }

            if (nInsidePointCount == 1 && nOutsidePointCount == 2)
            {
                out_tri1.lightdp = in_tri.lightdp;
                out_tri1.clr = in_tri.clr;

                out_tri1.p[0] = inside_points[0];
                //out_tri1.clr = Color.BLUE;

                out_tri1.p[1] = Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0]);
                out_tri1.p[2] = Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[1]);

                return 1;
            }

            if (nInsidePointCount == 2 && nOutsidePointCount == 1)
            {
                out_tri1.lightdp = in_tri.lightdp;
                out_tri2.lightdp = in_tri.lightdp;
                out_tri1.clr = in_tri.clr;
                out_tri2.clr = in_tri.clr;
               //out_tri1.clr = Color.RED;
               //out_tri2.clr = Color.GREEN;

                out_tri1.p[0] = inside_points[0];
                out_tri1.p[1] = inside_points[1];
                out_tri1.p[2] = Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0]);

                out_tri2.p[0] = inside_points[1];
                out_tri2.p[1] = out_tri1.p[2];
                out_tri2.p[2] = Vector_IntersectPlane(plane_p, plane_n, inside_points[1], outside_points[0]);

                return 2;
            }

            return 0;
        }
        //!AUSLAGERN

        private static Vector2 ConvertVec(vec3d vec)
        {
            Vector2 vector = new Vector2() { X = vec.x, Y = vec.y };
            return vector;
        }

        private static void DrawCustomTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color clr, float thicc)
        {
            Raylib.DrawLineEx(p1, p2, thicc, clr);
            Raylib.DrawLineEx(p2, p3, thicc, clr);
            Raylib.DrawLineEx(p3, p1, thicc, clr);
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}