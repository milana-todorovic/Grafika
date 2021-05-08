// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Tekstura
        private static string textureDirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Textures");

        private enum TextureObjects { Asphalt = 0, Brick = 1, Grass = 2 };
        private readonly int textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] textures = null;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] textureFiles = { "asphalt.jpg", "brick.jpg", "grass.jpg" };
        #endregion


        #region Animacija
        private DispatcherTimer animationTimer;

        private AnimationState animationState = AnimationState.AnimationFinished;

        private static float rampDefaultAngle = 0.0f;
        private static float truckDefaultAngle = -90.0f;
        private static float truckDefaultX = 60.0f;
        private static float truckDefaultZ = 0.0f;

        private float rampAngle = rampDefaultAngle;
        private float rampWaitCounter = 0;

        private float truckAngle = truckDefaultAngle;
        private float truckX = truckDefaultX;
        private float truckZ = truckDefaultZ;
        #endregion

        #region Atributi

        /// <summary>
        ///	 Scena sa modelom tovarnog kamiona.
        /// </summary>
        private AssimpScene truck;

        private float[] reflectorAmbient = new float[] { 0.0f, 0.0f, 1.0f, 1.0f };

        float[] lightYellow = new float[] { 0.9f, 0.9f, 0.6f, 1.0f };

        float[] light0pos = new float[] { 100.0f, 100.0f, 0.0f, 1.0f };

        /// <summary>
        ///	 Skaliranost kamiona.
        /// </summary>
        private float truckScale = 1.0f;

        /// <summary>
        ///	 Skaliranost visine rampe.
        /// </summary>
        private float rampHeightScale = 1.0f;

        /// <summary>
        ///	 Kocka za crtanje ograda i stubića rampe.
        /// </summary>
        private Cube cube;

        /// <summary>
        ///	 Cilindar za crtanje prečke rampe.
        /// </summary>
        private Cylinder ramp;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 150.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return truck; }
            set { truck = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set
            {
                if (value >= 0 && value <= 50)
                    m_xRotation = value;
            }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float TruckScale
        {
            get { return truckScale; }
            set
            {
                if (value > 0)
                    truckScale = value;
            }
        }

        public float RampHeightScale
        {
            get { return rampHeightScale; }
            set
            {
                if (value > 0)
                    rampHeightScale = value;
            }
        }

        public float AmbientRedComponent
        {
            get { return reflectorAmbient[0]; }
            set
            {
                if (value >= 0 && value <= 1)
                    reflectorAmbient[0] = value;
            }
        }

        public float AmbientGreenComponent
        {
            get { return reflectorAmbient[1]; }
            set
            {
                if (value >= 0 && value <= 1)
                    reflectorAmbient[1] = value;
            }
        }

        public float AmbientBlueComponent
        {
            get { return reflectorAmbient[2]; }
            set
            {
                if (value >= 0 && value <= 1)
                    reflectorAmbient[2] = value;
            }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            textures = new uint[textureCount];
            this.truck = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.5f, 0.5f, 1.0f, 1.0f);

            // uključiti testiranje dubine i sakrivanje nevidljivih površina
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);

            SetupTextures(gl);

            SetupLighting(gl);

            // učitati model tovarnog kamiona
            truck.LoadScene();
            truck.Initialize();

            cube = new Cube();
            InitializeRamp(gl);
        }

        private void SetupTextures(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            // za teksture podesiti wrapping da bude GL_REPEAT po obema osama
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

            // podesiti filtere za teksture da budu najbliži sused filtriranje
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);

            // način stapanja teksture sa materijalom postaviti da bude GL_MODULATE 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            LoadTextures(gl);
        }

        private void LoadTextures(OpenGL gl)
        {
            gl.GenTextures(textureFiles.Length, textures);
            for (uint i = 0; i < textureFiles.Length; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);
                // Ucitaj sliku i podesi parametre teksture               
                Bitmap image = new Bitmap(Path.Combine(textureDirPath, textureFiles[i]).ToString());
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        private void InitializeRamp(OpenGL gl)
        {
            ramp = new Cylinder();
            ramp.Height = 56.0f;
            ramp.BaseRadius = 1.0f;
            ramp.TopRadius = 1.0f;
            ramp.NormalGeneration = Normals.Smooth;
            ramp.CreateInContext(gl);
        }

        private void SetupLighting(OpenGL gl)
        {
            /*
             * Definisati tačkasti svetlosni izvorsvetlo-žuteboje i pozicionirati ga gore-desno u odnosu 
             * na centar scene (na pozitivnom delu vertikalne i horizontalne ose). 
             */
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, lightYellow);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, lightYellow);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, lightYellow);

            // reflektor plave boje (cutoff 40 stepeni)
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, reflectorAmbient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, reflectorAmbient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, reflectorAmbient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 40.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_EXPONENT, 5.0f);

            // uključiti automatsku normalizaciju nad normalama
            gl.Enable(OpenGL.GL_NORMALIZE);

            // uključiti color tracking mehanizam i podesiti da se pozivom metode glColor 
            // definiše ambijentalna i difuzna komponenta materijala
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.ShadeModel(OpenGL.GL_SMOOTH);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.FrontFace(OpenGL.GL_CCW);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);


            gl.PushMatrix();

            // transformacije nad modelom ne utiču na tačkasti svetlosni izvor
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            // Pozicionirati kameru, tako da gleda na scenu odgore sa leve strane (ne previše izdignuta od podloge). 
            gl.LookAt(-200, 30, 0, 0, 0, -350, 0, 1, 0);

            // pozicioniranje svijeta
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);

            AddTruck(gl);
            AddSurface(gl);
            AddStreet(gl);
            AddWalls(gl);
            AddRamp(gl);

            AddText(gl);

            gl.PopMatrix();

            gl.Flush();
        }

        private void AddTruck(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(truckX, 13.0f * truckScale, truckZ);

            gl.Rotate(0.0f, truckAngle, 0.0f);
            gl.Scale(0.07f * truckScale, 0.07f * truckScale, 0.07f * truckScale);
            truck.Draw();
            gl.PopMatrix();
        }

        private void AddSurface(OpenGL gl)
        {
            gl.Color(0.0f, 0.3f, 0.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Grass]);

            gl.Begin(OpenGL.GL_QUADS);

            gl.Normal(0, 1, 0);
            gl.TexCoord(60, 10);
            gl.Vertex(-600.0f, 0.0f, 150.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 10);
            gl.Vertex(600.0f, 0.0f, 150.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 0);
            gl.Vertex(600.0f, 0.0f, 20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(60, 0);
            gl.Vertex(-600.0f, 0.0f, 20.0f);

            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 3);
            gl.Vertex(-600.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 3);
            gl.Vertex(-20.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 0);
            gl.Vertex(-20.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 0);
            gl.Vertex(-600.0f, 0.0f, -100.0f);

            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 3);
            gl.Vertex(20.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 3);
            gl.Vertex(600.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 0);
            gl.Vertex(600.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 0);
            gl.Vertex(20.0f, 0.0f, -100.0f);

            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 10);
            gl.Vertex(-600.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 10);
            gl.Vertex(-100.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 0);
            gl.Vertex(-100.0f, 0.0f, -500.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 0);
            gl.Vertex(-600.0f, 0.0f, -500.0f);

            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 10);
            gl.Vertex(100.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 10);
            gl.Vertex(600.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 0);
            gl.Vertex(600.0f, 0.0f, -500.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 0);
            gl.Vertex(100.0f, 0.0f, -500.0f);

            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 10);
            gl.Vertex(-100.0f, 0.0f, -300.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 10);
            gl.Vertex(100.0f, 0.0f, -300.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0, 0);
            gl.Vertex(100.0f, 0.0f, -500.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(25, 0);
            gl.Vertex(-100.0f, 0.0f, -500.0f);

            gl.End();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }

        private void AddStreet(OpenGL gl)
        {
            gl.Color(0.2f, 0.2f, 0.2f);

            // ulici i gradilištu pridružiti teksturu asfalta
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Asphalt]);

            gl.Begin(OpenGL.GL_QUADS);

            gl.Normal(0, 1, 0);
            gl.TexCoord(10, 10);
            gl.Vertex(-100.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(-10, 10);
            gl.Vertex(100.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(-10, 0);
            gl.Vertex(100.0f, 0.0f, -300.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(10, 0);
            gl.Vertex(-100.0f, 0.0f, -300.0f);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(0.5f, 0.5f, 1.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Normal(0, 1, 0);
            gl.TexCoord(20, 10);
            gl.Vertex(-600.0f, 0.0f, 20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(-20, 10);
            gl.Vertex(600.0f, 0.0f, 20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(-20, 0);
            gl.Vertex(600.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(20, 5);
            gl.Vertex(-600.0f, 0.0f, -20.0f);

            gl.Normal(0, 1, 0);
            gl.TexCoord(5, 10);
            gl.Vertex(-20.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(-5, 10);
            gl.Vertex(20.0f, 0.0f, -20.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(-5, 0);
            gl.Vertex(20.0f, 0.0f, -100.0f);
            gl.Normal(0, 1, 0);
            gl.TexCoord(5, 5);
            gl.Vertex(-20.0f, 0.0f, -100.0f);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.End();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }

        private void AddRamp(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -97.0f);
            gl.Color(1.0f, 1.0f, 0.0f);

            // stubići
            gl.PushMatrix();
            gl.Scale(1.0f, 0.5f + 6.0f * rampHeightScale, 1.0f);
            gl.Translate(-25.0f, 0.0f, 0.0f);
            cube.Render(gl, RenderMode.Render);
            gl.Translate(50.0f, 0.0f, 0.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // prečka
            gl.PushMatrix();
            gl.Translate(-28.0f, 6.0f * rampHeightScale, 0.0f);
            gl.Rotate(0f, 0f, rampAngle);
            gl.Rotate(0f, 90f, 0f);
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PopMatrix();
        }

        private void AddWalls(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -100.0f);

            //pozicioniranje reflektora        
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, reflectorAmbient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, reflectorAmbient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, reflectorAmbient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0.0f, -1.0f, -1.0f });
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, new float[] { 0.0f, 110.0f, 0.0f, 1.0f });
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT1);

            // kocka da bi se vidjela boja reflektora
            gl.PushMatrix();
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.Translate(0.0f, 10.0f, -100.0f);
            gl.Scale(10.0f, 10.0f, 10.0f);
            Sphere sphere = new Sphere();
            sphere.Radius = 1;
            sphere.NormalGeneration = Normals.Smooth;
            sphere.CreateInContext(gl);
            sphere.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //gl.Color(0.8f, 0.2f, 0.0f);
            gl.Color(0.4f, 0.2f, 0.0f);

            // zidovima oko gradilišta pridružiti teksturu zida od cigle
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Brick]);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(0.5f, 0.5f, 1.0f);
            gl.Rotate(0.0f, 0.0f, -90.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            // zadnji zid
            gl.PushMatrix();
            gl.Translate(-80.0f, 0.0f, -200.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-40.0f, 0.0f, -200.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -200.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(40.0f, 0.0f, -200.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(80.0f, 0.0f, -200.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //lijevi zid
            gl.PushMatrix();
            gl.Translate(-98.0f, 0.0f, -125.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-98.0f, 0.0f, -175.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-98.0f, 0.0f, -75.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-98.0f, 0.0f, -25.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //desni zid
            gl.PushMatrix();
            gl.Translate(98.0f, 0.0f, -125.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(98.0f, 0.0f, -175.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(98.0f, 0.0f, -75.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(98.0f, 0.0f, -25.0f);
            gl.Scale(2.0f, 9.0f, 25.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // lijeva strana prednjeg zida
            gl.PushMatrix();
            gl.Translate(-80.0f, 0.0f, 0.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-45.0f, 0.0f, 0.0f);
            gl.Scale(15.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // desna strana prednjeg zida
            gl.PushMatrix();
            gl.Translate(80.0f, 0.0f, 0.0f);
            gl.Scale(20.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(45.0f, 0.0f, 0.0f);
            gl.Scale(15.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            gl.PopMatrix();
        }

        private void AddText(OpenGL gl)
        {
            String[] text = {   "Sifra zad: 5.2",
                                "Prezime: Todorovic",
                                "Ime: Milana",
                                "Sk. god: 2020/21",
                                "Predmet: Racunarska grafika" };

            gl.Viewport(m_width - 210, 0, 210, m_height / 2);

            for (int i = 0; i < text.Length; i++)
                gl.DrawText(0, 30 + i * 30, 1.0f, 0.0f, 0.0f, "Helvetica Bold", 14.0f, text[i]);

            gl.Viewport(0, 0, m_width, m_height);
        }

        private void SetViewportAndProjection(OpenGL gl)
        {
            // definisati viewport preko cijelog prozora
            gl.Viewport(0, 0, m_width, m_height);

            // definisati projekciju u perspektivi (fov=50, near=1, a vrednost far po potrebi) 
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(50.0f, (double)m_width / m_height, 1.0f, 1000.0f);
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;

            // definisati projekciju i viewport
            SetViewportAndProjection(gl);
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                truck.Dispose();
            }
        }

        #endregion Metode

        #region MetodeAnimacija
        public void StartAnimation()
        {
            m_sceneDistance = 150.0f;
            m_xRotation = 0.0f;
            m_yRotation = 0.0f;
            rampWaitCounter = 0;
            truckScale = 1;
            rampAngle = 0.0f;
            truckAngle = -90f;
            truckX = 400f;
            truckZ = 0f;
            animationState = AnimationState.TruckArriving;
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(10);
            animationTimer.Tick += new EventHandler(AnimationStep);
            animationTimer.Start();
        }

        public void StopAnimation()
        {
            animationTimer.Stop();
            rampAngle = rampDefaultAngle;
            truckAngle = truckDefaultAngle;
            truckX = truckDefaultX;
            truckZ = truckDefaultZ;
            animationState = AnimationState.AnimationFinished;
        }


        private void AnimationStep(object sender, EventArgs e)
        {
            switch (animationState)
            {
                case AnimationState.TruckArriving:
                    truckX -= 5.0f;
                    if (truckX <= 0.0f)
                        animationState = AnimationState.TruckTurning;
                    break;
                case AnimationState.TruckTurning:
                    truckAngle -= 5.0f;
                    if (truckAngle <= -180.0f)
                        animationState = AnimationState.TruckTowardsRamp;
                    break;
                case AnimationState.TruckTowardsRamp:
                    truckZ -= 5.0f;
                    if (truckZ <= -40.0f)
                        animationState = AnimationState.RampRising;
                    break;
                case AnimationState.RampRising:
                    rampAngle += 1.0f;
                    if (rampAngle >= 25.0f)
                        animationState = AnimationState.RampWait;
                    break;
                case AnimationState.RampWait:
                    rampWaitCounter += 1;
                    if (rampWaitCounter >= 30)
                        animationState = AnimationState.RampLowering;
                    break;
                case AnimationState.RampLowering:
                    rampAngle -= 1.0f;
                    if (rampAngle <= 0.0f)
                        animationState = AnimationState.TruckAwayFromRamp;
                    break;
                case AnimationState.TruckAwayFromRamp:
                    truckZ += 5.0f;
                    if (truckZ >= 0.0f)
                        animationState = AnimationState.TruckTurningBack;
                    break;
                case AnimationState.TruckTurningBack:
                    truckAngle += 5.0f;
                    if (truckAngle >= -90.0f)
                        animationState = AnimationState.TruckLeaving;
                    break;
                case AnimationState.TruckLeaving:
                    truckX -= 5.0f;
                    if (truckX <= -1000.0f)
                        animationState = AnimationState.AnimationFinished;
                    break;
                default: break;
            }
        }

        #endregion

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
