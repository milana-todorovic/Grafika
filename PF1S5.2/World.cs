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

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena sa modelom tovarnog kamiona.
        /// </summary>
        private AssimpScene truck;

        /// <summary>
        ///	 Tekst koji treba prikazati.
        /// </summary>
        private String[] text = {   "Sifra zad: 5.2",
                                    "Prezime: Todorovic",
                                    "Ime: Milana",
                                    "Sk. god: 2020/21",
                                    "Predmet: Racunarska grafika" };

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
            set { m_xRotation = value; }
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

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
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

            // model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);

            // uključiti testiranje dubine i sakrivanje nevidljivih površina
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);

            // učitati model tovarnog kamiona
            truck.LoadScene();
            truck.Initialize();

            cube = new Cube();
            InitializeRamp(gl);
        }

        private void InitializeRamp(OpenGL gl)
        {
            ramp = new Cylinder();
            ramp.Height = 56.0f;
            ramp.BaseRadius = 1.0f;
            ramp.TopRadius = 1.0f;
            ramp.CreateInContext(gl);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.FrontFace(OpenGL.GL_CCW);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.PushMatrix();

            // pozicioniranje svijeta
            gl.Translate(0.0f, -25.0f, -m_sceneDistance);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);

            AddTruck(gl);
            AddSurface(gl);
            AddStreet(gl);
            AddWalls(gl);
            AddRamp(gl);

            gl.PopMatrix();

            AddText(gl);
        }

        private void AddTruck(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(-60.0f, 0.0f, 0.0f);
            gl.Rotate(0.0f, 90.0f, 0.0f);
            gl.Scale(0.1f, 0.1f, 0.1f);
            truck.Draw();
            gl.PopMatrix();
        }

        private void AddSurface(OpenGL gl)
        {
            gl.Color(0.0f, 0.3f, 0.0f);
            gl.Begin(OpenGL.GL_QUADS);

            gl.Vertex(-600.0f, 0.0f, 150.0f);
            gl.Vertex(600.0f, 0.0f, 150.0f);
            gl.Vertex(600.0f, 0.0f, 20.0f);
            gl.Vertex(-600.0f, 0.0f, 20.0f);

            gl.Vertex(-600.0f, 0.0f, -20.0f);
            gl.Vertex(-20.0f, 0.0f, -20.0f);
            gl.Vertex(-20.0f, 0.0f, -100.0f);
            gl.Vertex(-600.0f, 0.0f, -100.0f);

            gl.Vertex(20.0f, 0.0f, -20.0f);
            gl.Vertex(600.0f, 0.0f, -20.0f);
            gl.Vertex(600.0f, 0.0f, -100.0f);
            gl.Vertex(20.0f, 0.0f, -100.0f);

            gl.Vertex(-600.0f, 0.0f, -100.0f);
            gl.Vertex(600.0f, 0.0f, -100.0f);
            gl.Vertex(600.0f, 0.0f, -500.0f);
            gl.Vertex(-600.0f, 0.0f, -500.0f);

            gl.End();
        }

        private void AddStreet(OpenGL gl)
        {
            gl.Color(0.5f, 0.5f, 0.5f);
            gl.Begin(OpenGL.GL_QUADS);

            gl.Vertex(-600.0f, 0.0f, 20.0f);
            gl.Vertex(600.0f, 0.0f, 20.0f);
            gl.Vertex(600.0f, 0.0f, -20.0f);
            gl.Vertex(-600.0f, 0.0f, -20.0f);

            gl.Vertex(-20.0f, 0.0f, -20.0f);
            gl.Vertex(20.0f, 0.0f, -20.0f);
            gl.Vertex(20.0f, 0.0f, -100.0f);
            gl.Vertex(-20.0f, 0.0f, -100.0f);

            gl.End();
        }

        private void AddRamp(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -97.0f);
            gl.Color(1.0f, 1.0f, 0.0f);

            // stubići
            gl.PushMatrix();
            gl.Scale(1.0f, 6.0f, 1.0f);
            gl.Translate(-25.0f, 0.0f, 0.0f);
            cube.Render(gl, RenderMode.Render);
            gl.Translate(50.0f, 0.0f, 0.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // prečka
            gl.PushMatrix();
            gl.Translate(-28.0f, 6.0f, 0.0f);
            gl.Rotate(0f, 90f, 0f);
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PopMatrix();
        }

        private void AddWalls(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -100.0f);
            gl.Color(0.8f, 0.2f, 0.0f);

            // zadnji zid
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -200.0f);
            gl.Scale(100.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //lijevi zid
            gl.PushMatrix();
            gl.Translate(-98.0f, 0.0f, -100.0f);
            gl.Scale(2.0f, 9.0f, 100.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //desni zid
            gl.PushMatrix();
            gl.Translate(98.0f, 0.0f, -100.0f);
            gl.Scale(2.0f, 9.0f, 100.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // lijeva strana prednjeg zida
            gl.PushMatrix();
            gl.Translate(-65.0f, 0.0f, 0.0f);
            gl.Scale(35.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // desna strana prednjeg zida
            gl.PushMatrix();
            gl.Translate(65.0f, 0.0f, 0.0f);
            gl.Scale(35.0f, 9.0f, 2.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PopMatrix();
        }

        private void AddText(OpenGL gl)
        {
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
