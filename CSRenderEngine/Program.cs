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

    public struct vec2d
    {
        public float u, v, w = 1f;

        public vec2d(float u, float v)
        {
            this.u = u;
            this.v = v;
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
        public vec2d[] t;

        public triangle()
        {
            p = new vec3d[3];
            t = new vec2d[3];
            lightdp = 0f;
            clr = new Color();
            clr = calc_light();
        }

        public triangle(vec3d[] _p)
        {
            p = _p;
            lightdp = 0f;
            t = new vec2d[3];
            clr = new Color();
            clr = calc_light();
        }

        public triangle(vec3d[] _p, vec2d[] _t)
        {
            p = _p;
            t = _t;
            lightdp = 0f;
            clr = new Color();
            clr = calc_light();
        }

        public triangle(vec3d[] _p, float _light)
        {
            p = _p;
            lightdp = _light;
            t = new vec2d[3];
            clr = new Color();
            clr = calc_light();
        }

        public triangle(triangle std, float _light)
        {
            p = new vec3d[3];
            p[0] = std.p[0];
            p[1] = std.p[1];
            p[2] = std.p[2];
            t = new vec2d[3];
            t[0] = std.t[0];
            t[1] = std.t[1];
            t[2] = std.t[2];
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
            List<vec2d> texs = new List<vec2d>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Length == 0)
                    continue;

                if (line[0] == 'v')
                {
                    string[] splitted = line.Split(' ');
                    if (line[1] == 't')
                    {
                        vec2d vec = new vec2d(float.Parse(splitted[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitted[2], CultureInfo.InvariantCulture.NumberFormat));
                        //vec.u = 1f - vec.u;
                        //vec.v = 1f - vec.v;
                        texs.Add(vec);
                    }
                    else
                    {
                        verts.Add(new vec3d(float.Parse(splitted[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitted[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitted[3], CultureInfo.InvariantCulture.NumberFormat)));
                    }
                }

                if (line[0] == 'f')
                {
                    string[] splitted = line.Split(' ');

                    if (splitted.Length == 5)
                    {
                        string[] pos_tex0 = splitted[1].Split('/');
                        string[] pos_tex1 = splitted[2].Split('/');
                        string[] pos_tex2 = splitted[3].Split('/');

                        int[] arr_pos = new int[3];
                        arr_pos[0] = Convert.ToInt32(pos_tex0[0]);
                        arr_pos[1] = Convert.ToInt32(pos_tex1[0]);
                        arr_pos[2] = Convert.ToInt32(pos_tex2[0]);

                        int[] arr_tex = new int[3];
                        arr_tex[0] = Convert.ToInt32(pos_tex0[1]);
                        arr_tex[1] = Convert.ToInt32(pos_tex1[1]);
                        arr_tex[2] = Convert.ToInt32(pos_tex2[1]);

                        tris.Add(new triangle(new vec3d[] { verts[arr_pos[0] - 1], verts[arr_pos[1] - 1], verts[arr_pos[2] - 1] },
                            new vec2d[] { texs[arr_tex[0] - 1], texs[arr_tex[1] - 1], texs[arr_tex[2] - 1] }));

                        pos_tex0 = splitted[1].Split('/');
                        pos_tex1 = splitted[3].Split('/');
                        pos_tex2 = splitted[4].Split('/');

                        arr_pos = new int[3];
                        arr_pos[0] = Convert.ToInt32(pos_tex0[0]);
                        arr_pos[1] = Convert.ToInt32(pos_tex1[0]);
                        arr_pos[2] = Convert.ToInt32(pos_tex2[0]);

                        arr_tex = new int[3];
                        arr_tex[0] = Convert.ToInt32(pos_tex0[1]);
                        arr_tex[1] = Convert.ToInt32(pos_tex1[1]);
                        arr_tex[2] = Convert.ToInt32(pos_tex2[1]);

                        tris.Add(new triangle(new vec3d[] { verts[arr_pos[0] - 1], verts[arr_pos[1] - 1], verts[arr_pos[2] - 1] },
                            new vec2d[] { texs[arr_tex[0] - 1], texs[arr_tex[1] - 1], texs[arr_tex[2] - 1] }));
                    }
                    else
                    {
                        string[] pos_tex0 = splitted[1].Split('/');
                        string[] pos_tex1 = splitted[2].Split('/');
                        string[] pos_tex2 = splitted[3].Split('/');

                        int[] arr_pos = new int[3];
                        arr_pos[0] = Convert.ToInt32(pos_tex0[0]);
                        arr_pos[1] = Convert.ToInt32(pos_tex1[0]);
                        arr_pos[2] = Convert.ToInt32(pos_tex2[0]);

                        int[] arr_tex = new int[3];
                        arr_tex[0] = Convert.ToInt32(pos_tex0[1]);
                        arr_tex[1] = Convert.ToInt32(pos_tex1[1]);
                        arr_tex[2] = Convert.ToInt32(pos_tex2[1]);

                        tris.Add(new triangle(new vec3d[] { verts[arr_pos[0] - 1], verts[arr_pos[1] - 1], verts[arr_pos[2] - 1] },
                            new vec2d[] { texs[arr_tex[0] - 1], texs[arr_tex[1] - 1], texs[arr_tex[2] - 1] }));
                    }
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

        static Texture2D sprTex1;

        static Model TestModel;

        static float fDist = 5f;
        static float fSpeed = 10f;
        static float fTextureOffset = 1f;
        static Color[] imgclrs;


        static void Main(string[] args)
        {
            OnUserCreate();
            OnUserUpdate();
            OnUserEnd();
        }

        static unsafe void OnUserCreate()
        {
            //Raylib.InitWindow(256 * 4, 240 * 4, "CSharp3D-Demo");
            Raylib.InitWindow(1080, 720, "CSharp3D-Demo");

            meshCube = new mesh();

            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 0.0f), new vec3d(0.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(0f, 0f), new vec2d(1f, 0f) }));
            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f), new vec3d(1.0f, 0.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(1f, 0f), new vec2d(1f, 1f) }));

            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(0f, 0f), new vec2d(1f, 0f) }));
            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(1.0f, 0.0f, 1.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(1f, 0f), new vec2d(1f, 1f) }));

            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(0f, 0f), new vec2d(1f, 0f) }));
            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(0.0f, 0.0f, 1.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(1f, 0f), new vec2d(1f, 1f) }));

            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(0.0f, 1.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(0f, 0f), new vec2d(1f, 0f) }));
            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 1.0f, 0.0f), new vec3d(0.0f, 0.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(1f, 0f), new vec2d(1f, 1f) }));

            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 1.0f, 0.0f), new vec3d(0.0f, 1.0f, 1.0f), new vec3d(1.0f, 1.0f, 1.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(0f, 0f), new vec2d(1f, 0f) }));
            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(0.0f, 1.0f, 0.0f), new vec3d(1.0f, 1.0f, 1.0f), new vec3d(1.0f, 1.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(1f, 0f), new vec2d(1f, 1f) }));

            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(0f, 0f), new vec2d(1f, 0f) }));
            //meshCube.tris.Add(new triangle(new vec3d[] { new vec3d(1.0f, 0.0f, 1.0f), new vec3d(0.0f, 0.0f, 0.0f), new vec3d(1.0f, 0.0f, 0.0f) }, new vec2d[] { new vec2d(0f, 1f), new vec2d(1f, 0f), new vec2d(1f, 1f) }));

            sprTex1 = Raylib.LoadTexture(@"C:\Users\Marvin\Desktop\simple3D\Spyro\High.png");
            meshCube.LoadFromObjectFile(@"C:\Users\Marvin\Desktop\simple3D\Spyro\Artisans-Hub.obj");
            matProj = Matrix_MakeProjection(90f, (float)Raylib.GetScreenHeight() / (float)Raylib.GetScreenWidth(), 0.1f, 1000f);
            Color* clrs = Raylib.LoadImageColors(Raylib.LoadImage(@"C:\Users\Marvin\Desktop\simple3D\Spyro\Low.png"));
            imgclrs = Create<Color>(clrs, sprTex1.width - 1 * sprTex1.height);
            stopWatch.Start();
        }

        public unsafe static T[] Create<T>(T* ptr, int length) where T : unmanaged
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
                array[i] = ptr[i];
            return array;
        }

        static unsafe void OnUserUpdate()
        {
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(0, 0, 0, 255));

                if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
                    vCamera.y -= 0.01f * fSpeed;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
                    vCamera.y += 0.01f * fSpeed;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
                    fTextureOffset -= 1.1f;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
                    fTextureOffset += 1.1f;

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

                if (Raylib.IsKeyReleased(KeyboardKey.KEY_KP_ADD))
                    fSpeed += 2f;

                if (Raylib.IsKeyReleased(KeyboardKey.KEY_KP_SUBTRACT))
                    fSpeed -= 2f;

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
                vec3d vTarget = new vec3d(0f, 0f, 1f);
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
                    triTransformed.t[0] = tri.t[0];
                    triTransformed.t[1] = tri.t[1];
                    triTransformed.t[2] = tri.t[2];

                    vec3d normal = new vec3d(0f, 0f, 0f), line1 = new vec3d(0f, 0f, 0f), line2 = new vec3d(0f, 0f, 0f);
                    line1 = Vector_Sub(triTransformed.p[1], triTransformed.p[0]);
                    line2 = Vector_Sub(triTransformed.p[2], triTransformed.p[0]);

                    normal = Vector_CrossProduct(line1, line2);

                    normal = Vector_Normalise(normal);

                    float l = (float)Math.Sqrt((double)(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z));
                    normal.x /= l; normal.y /= l; normal.z /= l;

                    vec3d vCameraRay = Vector_Sub(triTransformed.p[0], vCamera);

                    if (Vector_DotProduct(normal, vCameraRay) < 0f)
                    {
                        //LIGHT
                        vec3d light_direction = new vec3d(0f, 0f, -1f);
                        light_direction = Vector_Normalise(light_direction);

                        float lightdp = Math.Max(0.1f, Vector_DotProduct(light_direction, normal));

                        triViewed.p[0] = Matrix_MultiplyVector(matView, triTransformed.p[0]);
                        triViewed.p[1] = Matrix_MultiplyVector(matView, triTransformed.p[1]);
                        triViewed.p[2] = Matrix_MultiplyVector(matView, triTransformed.p[2]);
                        triViewed.t[0] = triTransformed.t[0];
                        triViewed.t[1] = triTransformed.t[1];
                        triViewed.t[2] = triTransformed.t[2];

                        int nClippedTriangles = 0;
                        triangle[] clipped = new triangle[2];
                        nClippedTriangles = Trinagle_ClipAgainstPlane(new vec3d(0f, 0f, 1f), new vec3d(0f, 0f, 1f), ref triViewed, out clipped[0], out clipped[1]);

                        for (int n = 0; n < nClippedTriangles; n++)
                        {
                            triProjected.p[0] = Matrix_MultiplyVector(matProj, clipped[n].p[0]);
                            triProjected.p[1] = Matrix_MultiplyVector(matProj, clipped[n].p[1]);
                            triProjected.p[2] = Matrix_MultiplyVector(matProj, clipped[n].p[2]);
                            triProjected.t[0] = clipped[n].t[0];
                            triProjected.t[1] = clipped[n].t[1];
                            triProjected.t[2] = clipped[n].t[2];

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

                            switch (p)
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
                            DrawCustomTriangle(ConvertVec3(listTriangles[trinum].p[0]), ConvertVec3(listTriangles[trinum].p[1]), ConvertVec3(listTriangles[trinum].p[2]), drawMode == DrawMode.Both ? new Color(255, 0, 0, 255) : new Color(255, 255, 255, 255), 1f);
                        }
                        if (drawMode == DrawMode.Model || drawMode == DrawMode.Both)
                        {
                            Raylib.DrawTriangle(ConvertVec3(listTriangles[trinum].p[0]), ConvertVec3(listTriangles[trinum].p[1]), ConvertVec3(listTriangles[trinum].p[2]), listTriangles[trinum].clr);
                            Raylib.DrawTriangle(ConvertVec3(listTriangles[trinum].p[2]), ConvertVec3(listTriangles[trinum].p[1]), ConvertVec3(listTriangles[trinum].p[0]), listTriangles[trinum].clr);

                            Vector2[] converted_pos = new Vector2[3];
                            converted_pos[0] = ConvertVec3(listTriangles[trinum].p[0]);
                            converted_pos[1] = ConvertVec3(listTriangles[trinum].p[1]);
                            converted_pos[2] = ConvertVec3(listTriangles[trinum].p[2]);

                            Vector2[] converted_tex = new Vector2[3];
                            converted_tex[0] = ConvertVec2(listTriangles[trinum].t[0]);
                            converted_tex[1] = ConvertVec2(listTriangles[trinum].t[1]);
                            converted_tex[2] = ConvertVec2(listTriangles[trinum].t[2]);
                            //TexturedTriangle((int)listTriangles[trinum].p[0].x, (int)listTriangles[trinum].p[0].y, listTriangles[trinum].t[0].u, listTriangles[trinum].t[0].v, listTriangles[trinum].t[0].w,
                                             //(int)listTriangles[trinum].p[1].x, (int)listTriangles[trinum].p[1].y, listTriangles[trinum].t[1].u, listTriangles[trinum].t[1].v, listTriangles[trinum].t[1].w,
                                             //(int)listTriangles[trinum].p[2].x, (int)listTriangles[trinum].p[2].y, listTriangles[trinum].t[2].u, listTriangles[trinum].t[2].v, listTriangles[trinum].t[2].w, sprTex1);
                            //Raylib.DrawTexturePoly(sprTex1, CalculateCentroid(converted_pos, 2), converted_pos, converted_tex, 3, Color.WHITE);
                        }
                    }
                }
                Raylib.DrawText(fTextureOffset.ToString(), 10, 40, 10, Color.WHITE);
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
            matrix.m[0, 0] = 1.0f;
            matrix.m[1, 1] = (float)Math.Cos(fAngleRad);
            matrix.m[1, 2] = (float)Math.Sin(fAngleRad);
            matrix.m[2, 1] = (float)-Math.Sin(fAngleRad);
            matrix.m[2, 2] = (float)Math.Cos(fAngleRad);
            matrix.m[3, 3] = 1.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeRotationY(float fAngleRad)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0, 0] = (float)Math.Cos(fAngleRad);
            matrix.m[0, 2] = (float)Math.Sin(fAngleRad);
            matrix.m[2, 0] = (float)-Math.Sin(fAngleRad);
            matrix.m[1, 1] = 1.0f;
            matrix.m[2, 2] = (float)Math.Cos(fAngleRad);
            matrix.m[3, 3] = 1.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeRotationZ(float fAngleRad)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0, 0] = (float)Math.Cos(fAngleRad);
            matrix.m[0, 1] = (float)Math.Sin(fAngleRad);
            matrix.m[1, 0] = (float)-Math.Sin(fAngleRad);
            matrix.m[1, 1] = (float)Math.Cos(fAngleRad);
            matrix.m[2, 2] = 1.0f;
            matrix.m[3, 3] = 1.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MakeTranslation(float x, float y, float z)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0, 0] = 1.0f;
            matrix.m[1, 1] = 1.0f;
            matrix.m[2, 2] = 1.0f;
            matrix.m[3, 3] = 1.0f;
            matrix.m[3, 0] = x;
            matrix.m[3, 1] = y;
            matrix.m[3, 2] = z;
            return matrix;
        }

        public static mat4x4 Matrix_MakeProjection(float fFovDegrees, float fAspectRatio, float fNear, float fFar)
        {
            float fFovRad = 1.0f / (float)Math.Tan(fFovDegrees * 0.5f / 180f * 3.14159f);
            mat4x4 matrix = new mat4x4();
            matrix.m[0, 0] = fAspectRatio * fFovRad;
            matrix.m[1, 1] = fFovRad;
            matrix.m[2, 2] = fFar / (fFar - fNear);
            matrix.m[3, 2] = (-fFar * fNear) / (fFar - fNear);
            matrix.m[2, 3] = 1.0f;
            matrix.m[3, 3] = 0.0f;
            return matrix;
        }

        public static mat4x4 Matrix_MultiplyMatrix(mat4x4 m1, mat4x4 m2)
        {
            mat4x4 matrix = new mat4x4();
            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 4; r++)
                    matrix.m[r, c] = m1.m[r, 0] * m2.m[0, c] + m1.m[r, 1] * m2.m[1, c] + m1.m[r, 2] * m2.m[2, c] + m1.m[r, 3] * m2.m[3, c];
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
            matrix.m[0, 0] = newRight.x; matrix.m[0, 1] = newRight.y; matrix.m[0, 2] = newRight.z; matrix.m[0, 3] = 0f;
            matrix.m[1, 0] = newUp.x; matrix.m[1, 1] = newUp.y; matrix.m[1, 2] = newUp.z; matrix.m[1, 3] = 0f;
            matrix.m[2, 0] = newForward.x; matrix.m[2, 1] = newForward.y; matrix.m[2, 2] = newForward.z; matrix.m[2, 3] = 0f;
            matrix.m[3, 0] = pos.x; matrix.m[3, 1] = pos.y; matrix.m[3, 2] = pos.z; matrix.m[3, 3] = 1f;
            return matrix;
        }

        public static mat4x4 Matrix_QuickInverse(mat4x4 m)
        {
            mat4x4 matrix = new mat4x4();
            matrix.m[0, 0] = m.m[0, 0]; matrix.m[0, 1] = m.m[1, 0]; matrix.m[0, 2] = m.m[2, 0]; matrix.m[0, 3] = 0.0f;
            matrix.m[1, 0] = m.m[0, 1]; matrix.m[1, 1] = m.m[1, 1]; matrix.m[1, 2] = m.m[2, 1]; matrix.m[1, 3] = 0.0f;
            matrix.m[2, 0] = m.m[0, 2]; matrix.m[2, 1] = m.m[1, 2]; matrix.m[2, 2] = m.m[2, 2]; matrix.m[2, 3] = 0.0f;
            matrix.m[3, 0] = -(m.m[3, 0] * matrix.m[0, 0] + m.m[3, 1] * matrix.m[1, 0] + m.m[3, 2] * matrix.m[2, 0]);
            matrix.m[3, 1] = -(m.m[3, 0] * matrix.m[0, 1] + m.m[3, 1] * matrix.m[1, 1] + m.m[3, 2] * matrix.m[2, 1]);
            matrix.m[3, 2] = -(m.m[3, 0] * matrix.m[0, 2] + m.m[3, 1] * matrix.m[1, 2] + m.m[3, 2] * matrix.m[2, 2]);
            matrix.m[3, 3] = 1.0f;
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
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
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

        public static vec3d Vector_IntersectPlane(vec3d plane_p, vec3d plane_n, vec3d lineStart, vec3d lineEnd, out float t)
        {
            plane_n = Vector_Normalise(plane_n);
            float plane_d = -Vector_DotProduct(plane_n, plane_p);
            float ad = Vector_DotProduct(lineStart, plane_n);
            float bd = Vector_DotProduct(lineEnd, plane_n);
            t = (-plane_d - ad) / (bd - ad);
            vec3d lineStartToEnd = Vector_Sub(lineEnd, lineStart);
            vec3d lineToIntersect = Vector_Mul(lineStartToEnd, t);
            return Vector_Add(lineStart, lineToIntersect);
        }

        public static int Trinagle_ClipAgainstPlane(vec3d plane_p, vec3d plane_n, ref triangle in_tri, out triangle out_tri1, out triangle out_tri2)
        {
            out_tri1 = new triangle();
            out_tri2 = new triangle();

            plane_n = Vector_Normalise(plane_n);

            float dist(vec3d p)
            {
                vec3d n = Vector_Normalise(p);
                return (plane_n.x * p.x + plane_n.y * p.y + plane_n.z * p.z - Vector_DotProduct(plane_n, plane_p));
            }

            vec3d[] inside_points = new vec3d[3]; int nInsidePointCount = 0;
            vec3d[] outside_points = new vec3d[3]; int nOutsidePointCount = 0;

            vec2d[] inside_tex = new vec2d[3]; int nInsideTextCount = 0;
            vec2d[] outside_tex = new vec2d[3]; int nOutsideTextCount = 0;

            float d0 = dist(in_tri.p[0]);
            float d1 = dist(in_tri.p[1]);
            float d2 = dist(in_tri.p[2]);

            if (d0 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[0]; inside_tex[nInsideTextCount++] = in_tri.t[0]; }
            else { outside_points[nOutsidePointCount++] = in_tri.p[0]; outside_tex[nOutsideTextCount++] = in_tri.t[0]; }
            if (d1 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[1]; inside_tex[nInsideTextCount++] = in_tri.t[1]; }
            else { outside_points[nOutsidePointCount++] = in_tri.p[1]; outside_tex[nOutsideTextCount++] = in_tri.t[1]; }
            if (d2 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[2]; inside_tex[nInsideTextCount++] = in_tri.t[2]; }
            else { outside_points[nOutsidePointCount++] = in_tri.p[2]; outside_tex[nOutsideTextCount++] = in_tri.t[2]; }

            if (nInsidePointCount == 0)
            {
                return 0;
            }

            if (nInsidePointCount == 3)
            {
                out_tri1 = in_tri;
                return 1;
            }

            if (nInsidePointCount == 1 && nOutsidePointCount == 2)
            {
                out_tri1.lightdp = in_tri.lightdp;
                out_tri1.clr = in_tri.clr;

                out_tri1.p[0] = inside_points[0];
                out_tri1.clr = Color.BLUE;

                float t;
                out_tri1.p[1] = Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0], out t);
                out_tri1.t[1].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                out_tri1.t[1].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;


                out_tri1.p[2] = Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[1], out t);
                out_tri1.t[2].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                out_tri1.t[2].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;

                return 1;
            }

            if (nInsidePointCount == 2 && nOutsidePointCount == 1)
            {
                out_tri1.lightdp = in_tri.lightdp;
                out_tri2.lightdp = in_tri.lightdp;
                out_tri1.clr = in_tri.clr;
                out_tri2.clr = in_tri.clr;
                out_tri1.clr = Color.RED;
                out_tri2.clr = Color.GREEN;

                out_tri1.p[0] = inside_points[0];
                out_tri1.p[1] = inside_points[1];
                out_tri1.t[0] = inside_tex[0];
                out_tri1.t[1] = inside_tex[1];
                float t;
                out_tri1.p[2] = Vector_IntersectPlane(plane_p, plane_n, inside_points[0], outside_points[0], out t);
                out_tri1.t[2].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                out_tri1.t[2].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;

                out_tri2.p[0] = inside_points[1];
                out_tri2.p[1] = out_tri1.p[2];
                out_tri2.t[0] = inside_tex[0];
                out_tri2.t[1] = inside_tex[1];
                out_tri2.p[2] = Vector_IntersectPlane(plane_p, plane_n, inside_points[1], outside_points[0], out t);
                out_tri2.t[2].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                out_tri2.t[2].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;

                return 2;
            }

            return 0;
        }

        public static Vector2 CalculateCentroid(Vector2[] points, int lastPointIndex)
        {
            float area = 0.0f;
            float Cx = 0.0f;
            float Cy = 0.0f;
            float tmp = 0.0f;
            int k;

            for (int i = 0; i <= lastPointIndex; i++)
            {
                k = (i + 1) % (lastPointIndex + 1);
                tmp = points[i].X * points[k].Y -
                      points[k].X * points[i].Y;
                area += tmp;
                Cx += (points[i].X + points[k].X) * tmp;
                Cy += (points[i].Y + points[k].Y) * tmp;
            }
            area *= 0.5f;
            Cx *= 1.0f / (6.0f * area);
            Cy *= 1.0f / (6.0f * area);

            return new Vector2(Cx, Cy);
        }
        //!AUSLAGERN

        private static Vector2 ConvertVec3(vec3d vec)
        {
            Vector2 vector = new Vector2() { X = vec.x, Y = vec.y };
            return vector;
        }

        private static Vector2 ConvertVec2(vec2d vec)
        {
            Vector2 vector = new Vector2() { X = vec.v * fTextureOffset, Y = vec.u * fTextureOffset };
            return vector;
        }

        private static void DrawCustomTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color clr, float thicc)
        {
            Raylib.DrawLineEx(p1, p2, thicc, clr);
            Raylib.DrawLineEx(p2, p3, thicc, clr);
            Raylib.DrawLineEx(p3, p1, thicc, clr);
        }

        public static void TexturedTriangle(int x1, int y1, float u1, float v1, float w1,
                        int x2, int y2, float u2, float v2, float w2,
                        int x3, int y3, float u3, float v3, float w3,
        Texture2D tex)
        {
            if (y2 < y1)
            {
                swap(ref y1, ref y2);
                swap(ref x1, ref x2);
                swap(ref u1, ref u2);
                swap(ref v1, ref v2);
                swap(ref w1, ref w2);
            }

            if (y3 < y1)
            {
                swap(ref y1, ref y3);
                swap(ref x1, ref x3);
                swap(ref u1, ref u3);
                swap(ref v1, ref v3);
                swap(ref w1, ref w3);
            }

            if (y3 < y2)
            {
                swap(ref y2, ref y3);
                swap(ref x2, ref x3);
                swap(ref u2, ref u3);
                swap(ref v2, ref v3);
                swap(ref w2, ref w3);
            }

            int dy1 = y2 - y1;
            int dx1 = x2 - x1;
            float dv1 = v2 - v1;
            float du1 = u2 - u1;
            float dw1 = w2 - w1;

            int dy2 = y3 - y1;
            int dx2 = x3 - x1;
            float dv2 = v3 - v1;
            float du2 = u3 - u1;
            float dw2 = w3 - w1;

            float tex_u, tex_v, tex_w;

            float dax_step = 0, dbx_step = 0,
                du1_step = 0, dv1_step = 0,
                du2_step = 0, dv2_step = 0,
                dw1_step = 0, dw2_step = 0;

            if (dy1 > 0f) dax_step = dx1 / (float)Math.Abs(dy1);
            if (dy2 > 0f) dbx_step = dx2 / (float)Math.Abs(dy2);

            if (dy1 > 0f) du1_step = du1 / (float)Math.Abs(dy1);
            if (dy1 > 0f) dv1_step = dv1 / (float)Math.Abs(dy1);
            if (dy1 > 0f) dw1_step = dw1 / (float)Math.Abs(dy1);

            if (dy2 > 0f) du2_step = du2 / (float)Math.Abs(dy2);
            if (dy2 > 0f) dv2_step = dv2 / (float)Math.Abs(dy2);
            if (dy2 > 0f) dw2_step = dw2 / (float)Math.Abs(dy2);

            if (dy1 > 0f)
            {
                for (int i = y1; i <= y2; i++)
                {
                    int ax = (int)(x1 + (float)(i - y1) * dax_step);
                    int bx = (int)(x1 + (float)(i - y1) * dbx_step);

                    float tex_su = u1 + (float)(i - y1) * du1_step;
                    float tex_sv = v1 + (float)(i - y1) * dv1_step;
                    float tex_sw = w1 + (float)(i - y1) * dw1_step;

                    float tex_eu = u1 + (float)(i - y1) * du2_step;
                    float tex_ev = v1 + (float)(i - y1) * dv2_step;
                    float tex_ew = w1 + (float)(i - y1) * dw2_step;

                    if (ax > bx)
                    {
                        swap(ref ax, ref bx);
                        swap(ref tex_su, ref tex_eu);
                        swap(ref tex_sv, ref tex_ev);
                        swap(ref tex_sw, ref tex_ew);
                    }

                    tex_u = tex_su;
                    tex_v = tex_sv;
                    tex_w = tex_sw;

                    float tstep = 1.0f / ((float)(bx - ax));
                    float t = 0.0f;

                    for (int j = ax; j < bx; j++)
                    {
                        tex_u = (1.0f - t) * tex_su + t * tex_eu;
                        tex_v = (1.0f - t) * tex_sv + t * tex_ev;
                        tex_w = (1.0f - t) * tex_sw + t * tex_ew;
                        Raylib.DrawPixel(j, i, SampleColor((int)(tex_u * fTextureOffset), (int)(tex_v * fTextureOffset)));
                        //if (tex_w > pDepthBuffer[i * ScreenWidth() + j])
                        //{
                        //    Draw(j, i, tex->SampleGlyph(tex_u / tex_w, tex_v / tex_w), tex->SampleColour(tex_u / tex_w, tex_v / tex_w));
                        //    pDepthBuffer[i * ScreenWidth() + j] = tex_w;
                        //}
                        t += tstep;
                    }

                }
            }

            dy1 = y3 - y2;
            dx1 = x3 - x2;
            dv1 = v3 - v2;
            du1 = u3 - u2;
            dw1 = w3 - w2;

            if (dy1 > 0f) dax_step = dx1 / (float)Math.Abs(dy1);
            if (dy2 > 0f) dbx_step = dx2 / (float)Math.Abs(dy2);

            du1_step = 0; dv1_step = 0;
            if (dy1 > 0f) du1_step = du1 / (float)Math.Abs(dy1);
            if (dy1 > 0f) dv1_step = dv1 / (float)Math.Abs(dy1);
            if (dy1 > 0f) dw1_step = dw1 / (float)Math.Abs(dy1);

            if (dy1 > 0f)
            {
                for (int i = y2; i <= y3; i++)
                {
                    int ax = (int)(x2 + (float)(i - y2) * dax_step);
                    int bx = (int)(x1 + (float)(i - y1) * dbx_step);

                    float tex_su = u2 + (float)(i - y2) * du1_step;
                    float tex_sv = v2 + (float)(i - y2) * dv1_step;
                    float tex_sw = w2 + (float)(i - y2) * dw1_step;

                    float tex_eu = u1 + (float)(i - y1) * du2_step;
                    float tex_ev = v1 + (float)(i - y1) * dv2_step;
                    float tex_ew = w1 + (float)(i - y1) * dw2_step;

                    if (ax > bx)
                    {
                        swap(ref ax, ref bx);
                        swap(ref tex_su, ref tex_eu);
                        swap(ref tex_sv, ref tex_ev);
                        swap(ref tex_sw, ref tex_ew);
                    }

                    tex_u = tex_su;
                    tex_v = tex_sv;
                    tex_w = tex_sw;

                    float tstep = 1.0f / ((float)(bx - ax));
                    float t = 0.0f;

                    for (int j = ax; j < bx; j++)
                    {
                        tex_u = (1.0f - t) * tex_su + t * tex_eu;
                        tex_v = (1.0f - t) * tex_sv + t * tex_ev;
                        tex_w = (1.0f - t) * tex_sw + t * tex_ew;
                        Raylib.DrawPixel(j, i, SampleColor((int)(tex_u * fTextureOffset), (int)(tex_v * fTextureOffset)));
                        //if (tex_w > pDepthBuffer[i * ScreenWidth() + j])
                        //{
                        //    Draw(j, i, tex->SampleGlyph(tex_u / tex_w, tex_v / tex_w), tex->SampleColour(tex_u / tex_w, tex_v / tex_w));
                        //    pDepthBuffer[i * ScreenWidth() + j] = tex_w;
                        //}
                        t += tstep;
                    }
                }
            }
        }

        private static Color SampleColor(int x, int y)
        {
            int pos = x * y;
            if (pos > imgclrs.Length - 1 || pos < 0)
                return imgclrs[0];

            return imgclrs[pos];
        }

        private static void swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b; 
            b = temp;
        }
    }
}