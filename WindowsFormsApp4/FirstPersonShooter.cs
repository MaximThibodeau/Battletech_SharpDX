using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using SharpDX.XAudio2;

namespace WindowsFormsApp4
{
    public class FirstPersonShooter
    {
        StaticObject O;

        Matrix view;
        Matrix proj;

        Shooter Master;

        SwapChainDescription desc;
        SwapChain swapChain;
        SharpDX.Direct3D11.Device device;
        SharpDX.Direct3D11.DeviceContext context;

        Thread T;

        int ScreenWidth = 0;
        int ScreenHeight = 0;

        //***************
        private int lastTick;
        private int lastFrameRate;
        private int frameRate;

        private int FPS = 0;

        private long Frame_Time = 0;

        Stopwatch stopwatch = new Stopwatch();
        //*****************

        bool m_W = false;
        bool m_S = false;
        bool m_Q = false;
        bool m_E = false;
        bool m_A = false;
        bool m_D = false;
        bool m_Z = false;
        bool m_X = false;
        bool m_Space = false;
        bool Cntrl_Is_Down = false;

        FPScamera Cam;

        Vector3 Camera_Position;

        Keyboard keyboard;
        Mouse mouse;
        DirectInput DI = new DirectInput();

        

        public int Mouse_X = 0;
        public int Mouse_Y = 0;
        public Vector2 Mouse_Previous = new Vector2(0, 0);

        
        public int ClientNbr = -1;

        
        //***************

        public FirstPersonShooter(Shooter master)
        {

            Master = master;
            ScreenWidth = Master.ClientSize.Width;
            ScreenHeight = Master.ClientSize.Height;// - Master.statusStrip1.Height;
            

            desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(ScreenWidth, ScreenHeight,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = master.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new[] { SharpDX.Direct3D.FeatureLevel.Level_11_0 }, desc, out device, out swapChain);
            context = device.ImmediateContext;
            
            keyboard = new Keyboard(DI);
            mouse = new Mouse(DI);

            Cam = new FPScamera(Master.ClientSize.Width, Master.ClientSize.Height);

            // Acquire the joystick
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();

            mouse.Acquire();
                        
            O = new StaticObject("tutorial_0.blend.txt", device, context);

            {
                T = new Thread(WorkThreadFunction);
                T.Start();
            }//);

            //WorkThreadFunction();
        }


        public void WorkThreadFunction()
        {

            // Prepare matrices
            view = Matrix.LookAtLH(new Vector3(0, 0, -40.0f), new Vector3(0, 0, 0), Vector3.UnitY);
            proj = Matrix.Identity;
                        
            Texture2D backBuffer = null;
            RenderTargetView renderView = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;

            float rotation = 0.0f;
            int T = 0;

            Master.Invoke(new Action(() =>

            RenderLoop.Run(Master, () =>
            {

                // If Form resized
                if (Master.ResizeForm1)
                {
                    Random R = new Random(DateTime.Now.Millisecond);

                    T = R.Next(3);

                    // Dispose all previous allocated resources
                    Utilities.Dispose(ref backBuffer);
                    Utilities.Dispose(ref renderView);
                    Utilities.Dispose(ref depthBuffer);
                    Utilities.Dispose(ref depthView);

                    ScreenWidth = Master.ClientSize.Width;
                    ScreenHeight = Master.ClientSize.Height;

                    // Resize the backbuffer
                    swapChain.ResizeBuffers(desc.BufferCount, ScreenWidth, ScreenHeight, Format.Unknown, SwapChainFlags.None);

                    // Get the backbuffer from the swapchain
                    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

                    // Renderview on the backbuffer
                    renderView = new RenderTargetView(device, backBuffer);

                    // Create the depth buffer
                    depthBuffer = new Texture2D(device, new Texture2DDescription()
                    {
                        Format = Format.D32_Float_S8X24_UInt,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = ScreenWidth,
                        Height = ScreenHeight,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    // Create the depth buffer view
                    depthView = new DepthStencilView(device, depthBuffer);

                    // Setup targets and viewport for rendering
                    context.Rasterizer.SetViewport(new Viewport(0, 0, ScreenWidth, ScreenHeight, 0.0f, 1.0f));
                    context.OutputMerger.SetTargets(depthView, renderView);

                    // Setup new projection matrix with correct aspect ratio
                    proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, ScreenWidth / (float)ScreenHeight, 0.1f, 1000.0f);

                    // We are done resizing
                    Master.ResizeForm1 = false;
                }

                SharpDX.Color C;

                
                if (T == 0) C = SharpDX.Color.Red;
                else if(T == 1) C = SharpDX.Color.Gray;
                else if (T == 2) C = SharpDX.Color.Black;
                else C = SharpDX.Color.SpringGreen;

                // Clear views
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(renderView, C);

                keyboard.Poll();
                var datas = keyboard.GetBufferedData();
                foreach (var state in datas)
                {
                    if (state.IsPressed == true)
                    {

                        if (state.Key == Key.W)
                        {
                            //MessageBox.Show("allo");
                            m_W = true;
                        }
                        if (state.Key == Key.S)
                            m_S = true;
                        if (state.Key == Key.Q)
                            m_Q = true;
                        if (state.Key == Key.E)
                            m_E = true;
                        if (state.Key == Key.A)
                            m_A = true;
                        if (state.Key == Key.D)
                            m_D = true;
                        if (state.Key == Key.Z)
                            m_Z = true;
                        if (state.Key == Key.X)
                            m_X = true;
                        if (state.Key == Key.Space)
                            m_Space = true;
                        if (state.Key == Key.LeftControl)
                        {
                            Cntrl_Is_Down = true;
                            //Master.Close();
                        }
                    }
                    if (state.IsReleased == true)
                    {
                        if (state.Key == Key.W)
                            m_W = false;
                        if (state.Key == Key.S)
                            m_S = false;
                        if (state.Key == Key.Q)
                            m_Q = false;
                        if (state.Key == Key.E)
                            m_E = false;
                        if (state.Key == Key.A)
                            m_A = false;
                        if (state.Key == Key.D)
                            m_D = false;
                        if (state.Key == Key.Z)
                            m_Z = false;
                        if (state.Key == Key.X)
                            m_X = false;
                        if (state.Key == Key.Space)
                            m_Space = false;
                        if (state.Key == Key.LeftControl)
                        {
                            Cntrl_Is_Down = false;
                            //if (Mouse_Is_In == false) m_LockInput = true;
                        }
                    }
                }

                float facteur = 0.1f;

                if (m_W) Camera_Position += new Vector3(facteur, 0.0f, 0.0f);
                if (m_S) Camera_Position += new Vector3(-facteur, 0.0f, 0.0f);
                if (m_A) Camera_Position += new Vector3(0.0f, facteur, 0.0f);
                if (m_D) Camera_Position += new Vector3(0.0f, -facteur, 0.0f);
                if (m_Q) Camera_Position += new Vector3(0.0f, 0.0f, facteur);
                if (m_E) Camera_Position += new Vector3(0.0f, 0.0f, -facteur);

                mouse.Poll();


                MouseState Mo = mouse.GetCurrentState();
                
                
                Cam.Set_Frame(Mo.X, Mo.Y, Camera_Position, Mo.Z, 0, 0);



                rotation += 0.001f;
                    O.Render(Cam.Get_View_Matrix(), proj, Matrix.Identity * Matrix.RotationY(rotation) * Matrix.RotationX(2*rotation), context, new Vector4(0, 0, -100.0f, 0));

                
                // Present!
                swapChain.Present(1, PresentFlags.None);
                //});
            })));
            depthBuffer.Dispose();
            depthView.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();            
        }       
    }
}