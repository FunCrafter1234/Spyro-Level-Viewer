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
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

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

    public class CompareTris : IComparer<shaded_triangle>
    {
        public int Compare(shaded_triangle x, shaded_triangle y)
        {
            float z1 = (x.tri.p[0].z + x.tri.p[1].z + x.tri.p[2].z) / 3f;
            float z2 = (y.tri.p[0].z + y.tri.p[1].z + y.tri.p[2].z) / 3f;

            if(z1 > z2)
                return -1;
            else 
                return 1;
        }
    }

    public struct vec3d
    {
        public float x, y, z;

        public vec3d(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct triangle
    {
        public vec3d[] p;

        public triangle(vec3d[] _p)
        {
            p = _p;
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
            for(int i = 0; i < lines.Length; i++)
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
        public float[,] m = new float[4,4];

        public mat4x4()
        {
            m = new float[4,4];
        }
    }

    public struct shaded_triangle
    {
        public triangle tri;
        public float lightdp;

        public shaded_triangle(triangle _tri, float _light)
        {
            tri = _tri;
            lightdp = _light;
        }
    }


    internal class Program
    {
        static mesh meshCube = new mesh();
        static mat4x4 matProj = new mat4x4();
        static vec3d vCamera = new vec3d(0f, 0f, 0f);
        static float fNear;
        static float fFar;
        static float fFov;
        static float fAspectRatio;
        static float fFovRad;
        static float fTheta;
        static Stopwatch stopWatch = new Stopwatch();
        static DrawMode drawMode = DrawMode.Model;

        static float fDist = 5f;


        static void Main(string[] args)
        {
            OnUserCreate();
            OnUserUpdate();
            OnUserEnd();
        }

        static void OnUserCreate()
        {
            Raylib.InitWindow(256 * 4, 240 * 4, "CSharp3D-Demo");

            meshCube.LoadFromObjectFile(@"C:\Users\Marvin\Desktop\simple3D\teepot.obj");

            fNear = 0.1f;
            fFar = 1000.0f;
            fFov = 90.0f;
            fAspectRatio = (float)Raylib.GetScreenHeight() / (float)Raylib.GetScreenWidth();
            float tan = (float)Math.Tan((double)(fFov * 0.5f / 180f * 3.14159f));
            fFovRad = 1f / tan;

            matProj.m[0, 0] = fAspectRatio * fFovRad;
            matProj.m[1, 1] = fFovRad;
            matProj.m[2, 2] = fFar / (fFar - fNear);
            matProj.m[3, 2] = (-fFar * fNear) / (fFar - fNear);
            matProj.m[2, 3] = 1f;
            matProj.m[3, 3] = 0f;


            stopWatch.Start();
        }

        static void OnUserUpdate()
        {
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
                    fDist += 0.1f;


                if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
                    fDist -= 0.1f;

                if (Raylib.IsKeyReleased(KeyboardKey.KEY_UP))
                    drawMode = drawMode.Next();

                mat4x4 matRotZ = new mat4x4(), matRotX = new mat4x4();
                fTheta = 1f * ((float)stopWatch.Elapsed.TotalMilliseconds / 1000);

                Raylib.DrawFPS(10, 10);

                // Rotation Z
                matRotZ.m[0,0] = (float)Math.Cos((double)fTheta);
                matRotZ.m[0,1] = (float)Math.Sin((double)fTheta);
                matRotZ.m[1,0] = -(float)Math.Sin((double)fTheta);
                matRotZ.m[1,1] = (float)Math.Cos((double)fTheta);
                matRotZ.m[2,2] = 1;
                matRotZ.m[3,3] = 1;

                // Rotation X
                matRotX.m[0,0] = 1;
                matRotX.m[1,1] = (float)Math.Cos((double)(fTheta * 0.5f));
                matRotX.m[1,2] = (float)Math.Sin((double)(fTheta * 0.5f));
                matRotX.m[2,1] = -(float)Math.Sin((double)(fTheta * 0.5f));
                matRotX.m[2,2] = (float)Math.Cos((double)(fTheta * 0.5f));
                matRotX.m[3,3] = 1;

                List<shaded_triangle> TrianglesToRaster = new List<shaded_triangle>();

                foreach (var tri in meshCube.tris)
                {
                    triangle triProjected = new triangle(new vec3d[] { new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f), new vec3d(0f, 0f, 0f) });
                   
                    triangle triRotatedZ = new triangle(new vec3d[] { new vec3d(tri.p[0].x, tri.p[0].y, tri.p[0].z), new vec3d(tri.p[1].x, tri.p[1].y, tri.p[1].z), new vec3d(tri.p[2].x, tri.p[2].y, tri.p[2].z) });
                    triangle triRotatedZX = new triangle(new vec3d[] { new vec3d(tri.p[0].x, tri.p[0].y, tri.p[0].z), new vec3d(tri.p[1].x, tri.p[1].y, tri.p[1].z), new vec3d(tri.p[2].x, tri.p[2].y, tri.p[2].z) });

                    MultiplyMatrixVector(ref tri.p[0], ref triRotatedZ.p[0], ref matRotZ);
                    MultiplyMatrixVector(ref tri.p[1], ref triRotatedZ.p[1], ref matRotZ);
                    MultiplyMatrixVector(ref tri.p[2], ref triRotatedZ.p[2], ref matRotZ);

                    // Rotate in X-Axis
                    MultiplyMatrixVector(ref triRotatedZ.p[0], ref triRotatedZX.p[0], ref matRotX);
                    MultiplyMatrixVector(ref triRotatedZ.p[1], ref triRotatedZX.p[1], ref matRotX);
                    MultiplyMatrixVector(ref triRotatedZ.p[2], ref triRotatedZX.p[2], ref matRotX);

                    triangle triTranslated = new triangle(new vec3d[] { new vec3d(triRotatedZX.p[0].x, triRotatedZX.p[0].y, triRotatedZX.p[0].z), new vec3d(triRotatedZX.p[1].x, triRotatedZX.p[1].y, triRotatedZX.p[1].z), new vec3d(triRotatedZX.p[2].x, triRotatedZX.p[2].y, triRotatedZX.p[2].z) });
                    triTranslated.p[0].z = triRotatedZX.p[0].z + fDist;
                    triTranslated.p[1].z = triRotatedZX.p[1].z + fDist;
                    triTranslated.p[2].z = triRotatedZX.p[2].z + fDist;

                    vec3d normal = new vec3d(0f, 0f, 0f), line1 = new vec3d(0f, 0f, 0f), line2 = new vec3d(0f, 0f, 0f);
                    line1.x = triTranslated.p[1].x - triTranslated.p[0].x;
                    line1.y = triTranslated.p[1].y - triTranslated.p[0].y;
                    line1.z = triTranslated.p[1].z - triTranslated.p[0].z;

                    line2.x = triTranslated.p[2].x - triTranslated.p[0].x;
                    line2.y = triTranslated.p[2].y - triTranslated.p[0].y;
                    line2.z = triTranslated.p[2].z - triTranslated.p[0].z;

                    normal.x = line1.y * line2.z - line1.z * line2.y;
                    normal.y = line1.z * line2.x - line1.x * line2.z;
                    normal.z = line1.x * line2.y - line1.y * line2.x;

                    float l = (float)Math.Sqrt((double)(normal.x * normal.x + normal.y * normal.y+normal.z*normal.z));
                    normal.x /= l; normal.y /= l; normal.z /= l;

                    //if(normal.z < 0)
                    if(normal.x * (triTranslated.p[0].x - vCamera.x) +
                       normal.y * (triTranslated.p[0].y - vCamera.y) +
                       normal.z * (triTranslated.p[0].z - vCamera.z) < 0)
                    {
                        //LIGHT
                        vec3d light_direction = new vec3d(0f, 0f, -1f);
                        float ll = (float)Math.Sqrt((double)light_direction.x * light_direction.x + light_direction.y * light_direction.y + light_direction.z * light_direction.z);
                        light_direction.x /= ll; light_direction.y /= ll; light_direction.z /= ll;
                        float lightdp = normal.x * light_direction.x + normal.y * light_direction.y + normal.z * light_direction.z;

                        MultiplyMatrixVector(ref triTranslated.p[0], ref triProjected.p[0], ref matProj);
                        MultiplyMatrixVector(ref triTranslated.p[1], ref triProjected.p[1], ref matProj);
                        MultiplyMatrixVector(ref triTranslated.p[2], ref triProjected.p[2], ref matProj);

                        triProjected.p[0].x += 1f; triProjected.p[0].y += 1f;
                        triProjected.p[1].x += 1f; triProjected.p[1].y += 1f;
                        triProjected.p[2].x += 1f; triProjected.p[2].y += 1f;

                        triProjected.p[0].x *= 0.5f * (float)Raylib.GetScreenWidth();
                        triProjected.p[0].y *= 0.5f * (float)Raylib.GetScreenHeight();
                        triProjected.p[1].x *= 0.5f * (float)Raylib.GetScreenWidth();
                        triProjected.p[1].y *= 0.5f * (float)Raylib.GetScreenHeight();
                        triProjected.p[2].x *= 0.5f * (float)Raylib.GetScreenWidth();
                        triProjected.p[2].y *= 0.5f * (float)Raylib.GetScreenHeight();

                        TrianglesToRaster.Add(new shaded_triangle(triProjected, lightdp));


                    }
                }

                //SORT
                TrianglesToRaster.Sort(0, TrianglesToRaster.Count, new CompareTris());


                foreach(var item in TrianglesToRaster)
                {
                    if(drawMode == DrawMode.Wireframe || drawMode == DrawMode.Both)
                    {
                        DrawCustomTriangle(ConvertVec(item.tri.p[0]), ConvertVec(item.tri.p[1]), ConvertVec(item.tri.p[2]), drawMode == DrawMode.Both ? Color.BLACK : Color.WHITE, 1f);
                    }
                    if(drawMode == DrawMode.Model || drawMode == DrawMode.Both)
                    {
                        int lightcolor = (int)Math.Round((decimal)255 * (decimal)item.lightdp);
                        if (lightcolor > 255) lightcolor = 255;
                        if (lightcolor < 0) lightcolor = -lightcolor;
                        Raylib.DrawTriangle(ConvertVec(item.tri.p[0]), ConvertVec(item.tri.p[1]), ConvertVec(item.tri.p[2]), new Color(lightcolor, lightcolor, lightcolor, 255));
                    }
                }

                Raylib.EndDrawing();
            }
        }

        static void OnUserEnd()
        {
            Raylib.CloseWindow();
        }

        private static void MultiplyMatrixVector(ref vec3d i, ref vec3d o, ref mat4x4 m)
        {
            o.x = i.x * m.m[0,0] + i.y * m.m[1,0] + i.z * m.m[2,0] + m.m[3,0];
            o.y = i.x * m.m[0,1] + i.y * m.m[1,1] + i.z * m.m[2,1] + m.m[3,1];
            o.z = i.x * m.m[0,2] + i.y * m.m[1,2] + i.z * m.m[2,2] + m.m[3,2];
            float w = i.x * m.m[0,3] + i.y * m.m[1,3] + i.z * m.m[2,3] + m.m[3,3];

            if (w != 0.0f)
            {
                o.x /= w; o.y /= w; o.z /= w;
            }
        }

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