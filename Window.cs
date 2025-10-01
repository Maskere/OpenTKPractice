using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using FBXReader;

namespace Practice{
    public class Window : GameWindow{
        private readonly string? cube = "./Resources/Cube.ply";
        private readonly string? cube1 = "./Resources/Church.ply";
        private readonly string? waveObj = "./Resources/suzanne.obj";
        private MeshDataExtractor extracter = new();

        private Matrix4 projection = Matrix4.Identity;
        private Matrix4 model = Matrix4.Identity;
        private Matrix4 model2 = Matrix4.Identity;
        private Matrix4 view = Matrix4.Identity;

        private Vector3 position = Vector3.Zero;
        private Vector3 position2 = Vector3.Zero;
        private Texture? texture;
        private Vector2 lastPos;
        private Camera camera;
        private Shader? shader;

        private float modelSize = 1f;
        private float startTime;
        private float Time;
        private bool firstMove = true;
        private int VertexBufferObject;
        private int VertexArrayObject;
        private int ElementBufferObject;
        private int totalIndexCount;

        private int obj1VertexCount;
        private int waveObjIndexCount;
        private int obj1IndexCount;
        private int obj2IndexCount;

        public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() {
                ClientSize = (width,height), Title = title}){
            camera = new (Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            startTime = DateTime.Now.Second;
            mouseScrollDelta = -15f;
        }

        protected override void OnLoad(){
            base.OnLoad();
            extracter.PlyExtractData(cube,out int stride1, out List<float> vertexColl1, out List<uint> indColl1);
            extracter.PlyExtractData(cube1,out int stride2, out List<float> vertexColl2, out List<uint> indColl2);
            extracter.WavefrontObjDataExtractor(waveObj, out List<float> wVertices,out List<uint> wIndices);

            int standard_stride = 8;

            // if(vertexColl1.Count % standard_stride != 0 || vertexColl2.Count % standard_stride != 0){
            //     Console.WriteLine($"FATAL ERROR: Mesh 1 or 2 does not have a float count divisible by {standard_stride}.");
            //     Console.WriteLine($"Mesh 1 Floats: {vertexColl1.Count}, Mesh 2 Floats: {vertexColl2.Count}");
            //     throw new Exception("Stride mismatch detected");
            // }

            obj1VertexCount = vertexColl1.Count / standard_stride;
            int vertexOffset = obj1VertexCount;

            for(int l = 0; l < indColl2.Count; l++){
                indColl2[l] += (uint)vertexOffset;
            }

            obj1IndexCount = indColl1.Count;
            obj2IndexCount = indColl2.Count;
            waveObjIndexCount = wIndices.Count;

            List<float> vertexColl = new();
            List<uint> indColl = new();
            vertexColl.AddRange(vertexColl1);
            vertexColl.AddRange(vertexColl2);
            indColl.AddRange(indColl1);
            indColl.AddRange(indColl2);

            // float[] vertices = vertexColl.ToArray();
            float[] vertices = wVertices.ToArray();
            // uint[] indices = indColl.ToArray();
            uint[] indices = wIndices.ToArray();

            totalIndexCount = indices.Length;

            int rowLength = standard_stride * sizeof(float);
            GL.ClearColor(0.2f,0.3f,0.3f,1);

            GL.Enable(EnableCap.DepthTest);

            VertexBufferObject = GL.GenBuffer();
            VertexArrayObject = GL.GenVertexArray();
            ElementBufferObject = GL.GenBuffer();

            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, totalIndexCount * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, rowLength, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1,2,VertexAttribPointerType.Float, false, rowLength, 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2,3,VertexAttribPointerType.Float,false,rowLength, 5 * sizeof(float));

            GL.BindVertexArray(0);

            shader = new("Shader/shader.vert","Shader/shader.frag");
            shader.Use();

            texture = Texture.LoadFromFile("./Resources/container.png");
            texture.Use(TextureUnit.Texture0);

            float near = 0.1f;
            float far = 100f;
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45),Size.X/(float)Size.Y,near,far);

            CursorState = CursorState.Grabbed;
            mouseScrollDelta = -5f;

            position = new Vector3(2,0,0);
            position2 = new Vector3(-2,0,0);
        }

        float mouseScrollDelta = 0;
        float moveX = 0;
        float moveY = 0;
        protected override void OnUpdateFrame(FrameEventArgs e){
            base.OnUpdateFrame(e);
            Time = DateTime.Now.Second - startTime;

            KeyboardState input = KeyboardState;
            if(input.IsKeyDown(Keys.Escape)){
                Close();
            }

            model = Matrix4.CreateScale(modelSize);
            model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0));
            model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(0));
            model *= Matrix4.CreateTranslation(position);

            model2 = Matrix4.CreateScale(modelSize);
            model2 *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0));
            model2 *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(0));
            model2 *= Matrix4.CreateTranslation(position2);

            MouseState mouse = MouseState;
            const float sensitivity = 0.2f;
            mouseScrollDelta += mouse.ScrollDelta.Y * 0.5f;

            if (firstMove) {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else {
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }

            float delta = 0.1f * (float)e.Time * 10;

            if(input.IsKeyDown(Keys.D)){
                moveX -= delta;
            }
            if(input.IsKeyDown(Keys.A)){
                moveX += delta;
            }
            if(input.IsKeyDown(Keys.W)){
                moveY += delta;
            }
            if(input.IsKeyDown(Keys.S)){
                moveY -= delta;
            }
            
            Matrix4 cameraMatrix = camera.GetViewMatrix();
            Matrix4 camMove = Matrix4.CreateTranslation(moveX,0,moveY);

            Vector3 camPos = new(0,0,mouseScrollDelta);
            Vector3 camTarget = Vector3.Zero;
            Vector3 camUp = Vector3.UnitY;

            view = Matrix4.LookAt(camPos, camTarget,camUp);
            view *= cameraMatrix;
            view *= camMove;
        }

        protected override void OnUnload(){
            base.OnUnload();
            shader?.Dispose();
        }


        protected override void OnRenderFrame(FrameEventArgs e){
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if(shader == null) throw new NullReferenceException("Shader not found");
            if(texture == null) throw new ArgumentNullException("Texture class not found");
            texture.Use(TextureUnit.Texture0);
            shader.Use();

            int obj2IndexByteOffset = obj1IndexCount * sizeof(uint);

            GL.BindVertexArray(VertexArrayObject);
            int modelLoc = GL.GetUniformLocation(shader.Handle,"model");
            int viewLoc = GL.GetUniformLocation(shader.Handle,"view");
            int projectionLoc = GL.GetUniformLocation(shader.Handle,"projection");

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projectionLoc,false,ref projection);
            GL.DrawElements(PrimitiveType.Triangles, waveObjIndexCount, DrawElementsType.UnsignedInt, 0);

            // GL.UniformMatrix4(modelLoc, false, ref model2);
            // GL.DrawElements(PrimitiveType.Triangles, obj2IndexCount, DrawElementsType.UnsignedInt,obj2IndexByteOffset);
            // GL.DrawArrays(PrimitiveType.Triangles,0,obj2IndexCount);
            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e){
            base.OnFramebufferResize(e);

            GL.Viewport(0,0,e.Width,e.Height);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e) {
            base.OnMouseMove(e);
            if(IsFocused){
            }
        }
    }
}
