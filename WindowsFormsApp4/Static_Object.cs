using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public class StaticObject
    {        
        readonly Collection<float[]> m_Vector_Vertex = new Collection<float[]>();
        readonly Collection<float[]> m_Vector_UV = new Collection<float[]>();
        readonly Collection<float[]> m_Vector_Normals = new Collection<float[]>();
        
        public Collection<VertexType> m_Ready_To_Buffer = new Collection<VertexType>();
        
        VertexShader vertexShader;
        PixelShader pixelShader;

        SharpDX.Direct3D11.Buffer[] verticesBuffer;
        InputLayout layout;
        SharpDX.Direct3D11.Buffer contantBuffer;
        RasterizerState _rasterState;
        ShaderResourceView[] textureView;

        public struct VertexType
        {
            public Vector3 pos;
            public Vector2 uv;
            public Vector3 norm;
        }        

        public StaticObject(string filename, SharpDX.Direct3D11.Device device, DeviceContext context)
        {

            if (filename.EndsWith(".txt"))
            {
                string line;

                Collection<string> LinesVector = new Collection<string>();

                StreamReader myfile = new StreamReader(filename);

                while ((line = myfile.ReadLine()) != null)
                {
                    LinesVector.Add(line);
                }

                myfile.Close();


                line = LinesVector[0];


                string ObjectName = line.Split(' ')[0];
                int Nbr_Meshes = int.Parse(line.Split(' ')[4]);


                int linesConter = 0;
                for (int mm = 0; mm < 1; mm++)
                {
                    linesConter++;// formating

                    linesConter++;
                    line = LinesVector[linesConter];

                    int Nbr_Triangles = int.Parse(line.Split(' ')[0]);

                    string Mesh_Name = line.Split(' ')[9];

                    float[] Translation = new float[3];
                    float[] Quaternion = new float[4];
                    float[] Scaling = new float[3];

                    linesConter++; // formating

                    linesConter++;
                    line = LinesVector[linesConter];


                    Translation[0] = float.Parse(line.Split(' ')[0]);
                    Translation[1] = float.Parse(line.Split(' ')[1]);
                    Translation[2] = float.Parse(line.Split(' ')[2]);

                    linesConter++; // formating

                    linesConter++;
                    line = LinesVector[linesConter];


                    Quaternion[0] = float.Parse(line.Split(' ')[0]);
                    Quaternion[1] = float.Parse(line.Split(' ')[1]);
                    Quaternion[2] = float.Parse(line.Split(' ')[2]);
                    Quaternion[3] = float.Parse(line.Split(' ')[3]);

                    linesConter++; // formating

                    linesConter++;
                    line = LinesVector[linesConter];


                    Scaling[0] = float.Parse(line.Split(' ')[0]);
                    Scaling[1] = float.Parse(line.Split(' ')[1]);
                    Scaling[2] = float.Parse(line.Split(' ')[2]);


                    Read_Vertex_Coordinates(LinesVector, linesConter, out linesConter, Nbr_Triangles, out m_Vector_Vertex, false);
                    Read_UV_Coordinates(LinesVector, linesConter, out linesConter, Nbr_Triangles, out m_Vector_UV);
                    Read_Normals_Coordinates(LinesVector, linesConter, out linesConter, Nbr_Triangles, out m_Vector_Normals);


                    Apply_Object_Transform(Nbr_Triangles, Scaling, Quaternion, Translation);

                    Create_Buffer();

                    Initialize(device, context);
                }
            }
        }


        void Create_Buffer()
        {
            Collection<VertexType> tmp = new Collection<VertexType>();
            for (int i = 0; i < m_Vector_Normals.Count() / 3; i++)
            {


                VertexType VT = new VertexType();

                VT.pos = new Vector3(m_Vector_Vertex[i * 3 + 0][0], m_Vector_Vertex[i * 3 + 0][1], m_Vector_Vertex[i * 3 + 0][2]);
                VT.uv = new Vector2(m_Vector_UV[i * 3 + 0][0], m_Vector_UV[i * 3 + 0][1]);
                VT.norm = new Vector3(m_Vector_Normals[i * 3 + 0][0], m_Vector_Normals[i * 3 + 0][1], m_Vector_Normals[i * 3 + 0][2]);


                tmp.Add(VT);

                VT.pos = new Vector3(m_Vector_Vertex[i * 3 + 1][0], m_Vector_Vertex[i * 3 + 1][1], m_Vector_Vertex[i * 3 + 1][2]);
                VT.uv = new Vector2(m_Vector_UV[i * 3 + 1][0], m_Vector_UV[i * 3 + 1][1]);
                VT.norm = new Vector3(m_Vector_Normals[i * 3 + 1][0], m_Vector_Normals[i * 3 + 1][1], m_Vector_Normals[i * 3 + 1][2]);


                tmp.Add(VT);

                VT.pos = new Vector3(m_Vector_Vertex[i * 3 + 2][0], m_Vector_Vertex[i * 3 + 2][1], m_Vector_Vertex[i * 3 + 2][2]);
                VT.uv = new Vector2(m_Vector_UV[i * 3 + 2][0], m_Vector_UV[i * 3 + 2][1]);
                VT.norm = new Vector3(m_Vector_Normals[i * 3 + 2][0], m_Vector_Normals[i * 3 + 2][1], m_Vector_Normals[i * 3 + 2][2]);


                tmp.Add(VT);

            }
            
            m_Ready_To_Buffer = tmp;
        }


        void Apply_Object_Transform(int Nbr_Triangles, float[] Object_Scaling, float[] Object_Quaternion, float[] Object_Location)
        {
            for (int i = 0; i < Nbr_Triangles * 3; i++)
            {
                m_Vector_Vertex[i][0] *= Object_Scaling[0];
                m_Vector_Vertex[i][1] *= Object_Scaling[1];
                m_Vector_Vertex[i][2] *= Object_Scaling[2];
            }

            //ofstream test("test.txt");

            for (int i = 0; i < Nbr_Triangles * 3; i++)
            {
                float a, b, c, w, x, y, z, r1, r2, r3, r4, r5, r6, r7, r8;

                a = m_Vector_Vertex[i][0];
                b = m_Vector_Vertex[i][1];
                c = m_Vector_Vertex[i][2];


                x = y = z = w = 0;

                Quaternion_Normalize(Object_Quaternion[1], Object_Quaternion[2], Object_Quaternion[3], Object_Quaternion[0], out x, out y, out z, out w);

                //test << Object_Quaternion[0] << " " << Object_Quaternion[1] << " " << Object_Quaternion[2] << " " << Object_Quaternion[3] << "\n";

                Hamilton(out r1, out r2, out r3, out r4, w, x, y, z, 0, a, b, c);
                Hamilton(out r5, out r6, out r7, out r8, r1, r2, r3, r4, w, -x, -y, -z);

                m_Vector_Vertex[i][0] = r6;
                m_Vector_Vertex[i][1] = r7;
                m_Vector_Vertex[i][2] = r8;

                //test << r6 << " " << r7 << " " << r8 << "\n";
            }

            for (int i = 0; i < Nbr_Triangles * 3; i++)
            {
                float a, b, c, w, x, y, z, r1, r2, r3, r4, r5, r6, r7, r8;

                a = m_Vector_Normals[i][0];
                b = m_Vector_Normals[i][1];
                c = m_Vector_Normals[i][2];


                x = y = z = w = 0;

                Quaternion_Normalize(Object_Quaternion[1], Object_Quaternion[2], Object_Quaternion[3], Object_Quaternion[0], out x, out y, out z, out w);


                Hamilton(out r1, out r2, out r3, out r4, w, x, y, z, 0, a, b, c);
                Hamilton(out r5, out r6, out r7, out r8, r1, r2, r3, r4, w, -x, -y, -z);

                m_Vector_Normals[i][0] = r6;
                m_Vector_Normals[i][1] = r7;
                m_Vector_Normals[i][2] = r8;
            }

            for (int i = 0; i < Nbr_Triangles * 3; i++)
            {
                m_Vector_Vertex[i][0] += Object_Location[0];
                m_Vector_Vertex[i][1] += Object_Location[1];
                m_Vector_Vertex[i][2] += Object_Location[2];
            }

        }

        void Hamilton(out float a, out float b, out float c, out float d, float a1, float b1, float c1, float d1, float a2, float b2, float c2, float d2)
        {
            a = a1 * a2 - b1 * b2 - c1 * c2 - d1 * d2;
            b = a1 * b2 + b1 * a2 + c1 * d2 - d1 * c2;
            c = a1 * c2 - b1 * d2 + c1 * a2 + d1 * b2;
            d = a1 * d2 + b1 * c2 - c1 * b2 + d1 * a2;
        }

        float Quaternion_Length(float X, float Y, float Z, float W)
        {
            return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        void Quaternion_Normalize(float X, float Y, float Z, float W, out float a, out float b, out float c, out float d)
        {
            a = b = c = d = 0;
            float length = Quaternion_Length(X, Y, Z, W);
            if (length > 0.1f)
            {
                float inverse = 1.0f / length;
                a = X * inverse;
                b = Y * inverse;
                c = Z * inverse;
                d = W * inverse;
            }
        }


        void Read_Vertex_Coordinates(Collection<string> data, int linecntr, out int linecntrout, int nbr_Triangles, out Collection<float[]> Vertex_Array, bool Is_BB)
        {
            string line;

            linecntr++; // formating
            linecntr++; // formating

            Collection<float[]> tmp = new Collection<float[]>();

            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);


            for (int m = 0; m < nbr_Triangles * 3; m++)
            {
                linecntr++;
                line = data[linecntr];


                float[] Vertices_Array = new float[3];
                Vertices_Array[0] = float.Parse(line.Split(' ')[0]);
                Vertices_Array[1] = float.Parse(line.Split(' ')[1]);
                Vertices_Array[2] = float.Parse(line.Split(' ')[2]);

                tmp.Add(Vertices_Array);
                //--------------------------;

                if (max.X < Vertices_Array[0]) max.X = Vertices_Array[0];
                if (max.Y < Vertices_Array[1]) max.Y = Vertices_Array[1];
                if (max.Z < Vertices_Array[2]) max.Z = Vertices_Array[2];

                if (min.X > Vertices_Array[0]) min.X = Vertices_Array[0];
                if (min.Y > Vertices_Array[1]) min.Y = Vertices_Array[1];
                if (min.Z > Vertices_Array[2]) min.Z = Vertices_Array[2];
            }           

            linecntrout = linecntr;
            Vertex_Array = tmp;
        }

        void Read_UV_Coordinates(Collection<string> data, int linecntr, out int lineconter, int nbr_Triangles, out Collection<float[]> Vertex_Array)
        {
            string line;

            linecntr++; // formating

            Collection<float[]> tmp = new Collection<float[]>();

            for (int m = 0; m < nbr_Triangles * 3; m++)
            {
                linecntr++;
                line = data[linecntr];


                float[] Vertices_Array = new float[2];
                Vertices_Array[0] = float.Parse(line.Split(' ')[0]);
                Vertices_Array[1] = -float.Parse(line.Split(' ')[1]);

                tmp.Add(Vertices_Array);
            }
            lineconter = linecntr;
            Vertex_Array = tmp;
        }

        void Read_Normals_Coordinates(Collection<string> data, int linecntr, out int lineconter, int nbr_Triangles, out Collection<float[]> Vertex_Array)
        {
            string line;

            linecntr++; // formating
                        //linecntr++; // formating
            Collection<float[]> tmp = new Collection<float[]>();

            for (int m = 0; m < nbr_Triangles * 3; m++)
            {
                linecntr++;
                line = data[linecntr];


                float[] Vertices_Array = new float[3];
                Vertices_Array[0] = float.Parse(line.Split(' ')[0]);
                Vertices_Array[1] = float.Parse(line.Split(' ')[1]);
                Vertices_Array[2] = float.Parse(line.Split(' ')[2]);

                tmp.Add(Vertices_Array);
            }

            lineconter = linecntr;
            Vertex_Array = tmp;
        }


        private bool Initialize(SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.DeviceContext context)
        {
            


            // Initialize the vertex and index buffers.
            InitializeBuffers(device, context);


            textureView = new ShaderResourceView[1];
            for (int i = 0; i < 1; i++)
            {
                //MessageBox.Show(m_Vector_Textures[i]);
                SharpDX.Direct3D11.Resource R2;

                R2 = TextureLoader.CreateTex2DFromFile(device, ".\\textures\\" + "red.jpg");
                ShaderResourceView effects2 = new ShaderResourceView(device, R2);
                textureView[i] = effects2;
            }


            return true;
        }

        bool InitializeBuffers(SharpDX.Direct3D11.Device device, DeviceContext context)
        {
            // shaders TexturePixelShader
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("static.vs", "TextureVertexShader", "vs_5_0");
            vertexShader = new VertexShader(device, vertexShaderByteCode);


            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("static.ps", "TexturePixelShader", "ps_5_0");
            pixelShader = new PixelShader(device, pixelShaderByteCode);


            VertexType[] tmpf = new VertexType[m_Ready_To_Buffer.Count];
            
            for (int k = 0; k < m_Ready_To_Buffer.Count; k++)
            {
                VertexType V = new VertexType();

                    V.pos = new Vector3(m_Ready_To_Buffer[k].pos[0], m_Ready_To_Buffer[k].pos[1], m_Ready_To_Buffer[k].pos[2]);
                    V.norm = new Vector3(m_Ready_To_Buffer[k].norm[0], m_Ready_To_Buffer[k].norm[1], m_Ready_To_Buffer[k].norm[2]);
                    V.uv = new Vector2(m_Ready_To_Buffer[k].uv[0], m_Ready_To_Buffer[k].uv[1]);

                
                tmpf[k] = V;
            }

            verticesBuffer = new SharpDX.Direct3D11.Buffer[1];

            verticesBuffer[0] = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, tmpf, sizeof(float) * 8 * m_Ready_To_Buffer.Count, ResourceUsage.Dynamic, CpuAccessFlags.Write, ResourceOptionFlags.None);
            

            layout = new InputLayout(
                device,
                ShaderSignature.GetInputSignature(vertexShaderByteCode),
                new[] {
                    new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float,0,0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float,12,0),
                    new InputElement("NORMAL", 0, Format.R32G32B32_Float,20,0)
            });

            contantBuffer = new SharpDX.Direct3D11.Buffer(device, sizeof(float) * 16*3, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);



            var sampler = (new SamplerState(device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = new Color4(0, 0, 0, 0),
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            }));

            // this is how you bind the sampler to slot 1, I dont know how to do it in toolkit:
            context.PixelShader.SetSampler(0, sampler);

            //TextureLoader.CreateTex2DFromFile(device, "test.jpg");
            var rdesc = RasterizerStateDescription.Default();
            rdesc.CullMode = CullMode.None;
            //rdesc.FillMode = FillMode.Wireframe;
            _rasterState = new RasterizerState(device, rdesc);
            context.Rasterizer.State = _rasterState;


            return true;
        }

        public void Set_Wireframe(SharpDX.Direct3D11.Device device, bool Wireframe)
        {
            var rdesc = RasterizerStateDescription.Default();
            rdesc.CullMode = CullMode.Back;

            if (Wireframe);// rdesc.FillMode = FillMode.Wireframe;
            else rdesc.FillMode = FillMode.Solid;


            _rasterState = new RasterizerState(device, rdesc);
        }



        public void Render(Matrix view, Matrix Proj, Matrix world, DeviceContext context, Vector4 CamPos)
        {

            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            // Prepare All the stages
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.Rasterizer.State = _rasterState;

            Matrix[] bones = new Matrix[3];

            bones[0] = Matrix.Transpose(world);
            bones[1] = Matrix.Transpose(view);
            bones[2] = Matrix.Transpose(Proj);


            context.UpdateSubresource(bones, contantBuffer);

            for (int i = 0; i < 1; i++)
            {
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(verticesBuffer[i], Utilities.SizeOf<float>() * 8, 0));

                context.PixelShader.SetShaderResource(0, textureView[i]); // new ShaderResourceView(device, texture));

                // Draw the cube
                context.Draw(m_Ready_To_Buffer.Count(), 0);
            }

        }
    }
}