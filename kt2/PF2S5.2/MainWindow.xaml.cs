using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        private bool animationActive = false;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();
            this.DataContext = this;

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\scania_3ds"), "scania.3ds", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }

            TruckScale.Text = m_world.TruckScale.ToString();
            RampHeightScale.Text = m_world.RampHeightScale.ToString();
            Red.Text = m_world.AmbientRedComponent.ToString();
            Blue.Text = m_world.AmbientBlueComponent.ToString();
            Green.Text = m_world.AmbientGreenComponent.ToString();
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (animationActive)
                return;
            switch (e.Key)
            {
                // Sa P se pokreće animacija
                case Key.P: StartAnimation(); break;
                // Sa Q se izlazi iz aplikacije
                case Key.Q: this.Close(); break;
                // Sa I i K rotacija oko horizontalne ose
                case Key.K: m_world.RotationX -= 5.0f; break;
                case Key.I: m_world.RotationX += 5.0f; break;
                // Sa J i L rotacija oko vertikalne ose
                case Key.J: m_world.RotationY -= 5.0f; break;
                case Key.L: m_world.RotationY += 5.0f; break;
                // Sa plus i minus udaljavanje i približavanje od centra scene
                case Key.Add: m_world.SceneDistance -= 50.0f; break;
                case Key.Subtract: m_world.SceneDistance += 50.0f; break;
            }
        }

        private async void StartAnimation()
        {
            animationActive = true;
            Toolbar.Visibility = Visibility.Collapsed;
            AnimationBorder.BorderThickness = new Thickness(4);
            TruckScale.Text = "1";
            m_world.StartAnimation();
            await Task.Delay(15000);
            m_world.StopAnimation();
            Toolbar.Visibility = Visibility.Visible;
            AnimationBorder.BorderThickness = new Thickness(0);
            animationActive = false;
        }

        private void TruckScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            String scale = TruckScale.Text;
            try
            {
                float num = float.Parse(scale);
                m_world.TruckScale = num;
            }
            catch (Exception ex)
            {
            }
        }

        private void RampHeightScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            String scale = RampHeightScale.Text;
            try
            {
                float num = float.Parse(scale);
                m_world.RampHeightScale = num;
            }
            catch (Exception ex)
            {
            }
        }

        private void Red_TextChanged(object sender, TextChangedEventArgs e)
        {
            String color = Red.Text;
            try
            {
                float num = float.Parse(color);
                m_world.AmbientRedComponent = num;
            }
            catch (Exception ex)
            {
            }
        }

        private void Green_TextChanged(object sender, TextChangedEventArgs e)
        {
            String color = Green.Text;
            try
            {
                float num = float.Parse(color);
                m_world.AmbientGreenComponent = num;
            }
            catch (Exception ex)
            {
            }
        }

        private void Blue_TextChanged(object sender, TextChangedEventArgs e)
        {
            String color = Blue.Text;
            try
            {
                float num = float.Parse(color);
                m_world.AmbientBlueComponent = num;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
