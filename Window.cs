using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using MeshDataExtract;

namespace Practice{
    public class Window : GameWindow{
        private BoundedBuffer<Mesh> meshBuffer = new(10);
        private readonly string waveObj = "./Resources/CubeWave.obj";
        private readonly string waveObj1 = "./Resources/Rock.obj";
        private static readonly string? woodenWall = "./Resources/Textures/container.png";
        private static readonly string? rockTex = "./Resources/Textures/Rock030_4K-JPG_Color.jpg";
        private MeshDataExtractor extracter = new();
        private List<string> objects = new();
        private List<float> verticesToRender = new();
        private List<uint> indicesToRender = new();

        private Matrix4 model = Matrix4.Identity;
        private Matrix4 model2 = Matrix4.Identity;

        private Vector3 position = Vector3.Zero;
        private Vector3 position2 = Vector3.Zero;
        private Vector2 lastPos;
        private Camera? camera;
        private Shader? shader;

        private float startTime;
        private float Time;
        private bool firstMove = true;

        private List<int> indexCount = new();
        private List<Model> models = new();

        public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() {
                ClientSize = (width,height), Title = title}){
            startTime = DateTime.Now.Second;
        }

        protected override void OnLoad(){
            base.OnLoad();
            // GL.ClearColor(0.2f,0.3f,0.3f,1);
            GL.ClearColor(0.5f,0.5f,0.5f,1);
            GL.Enable(EnableCap.DepthTest);

            objects.Add(waveObj);
            objects.Add(waveObj1);

            extracter.WavefrontObjDataExtractor(objects[0],out List<float> obj1V, out List<uint> obj1UV);
            extracter.WavefrontObjDataExtractor(objects[1],out List<float> obj2V, out List<uint> obj2UV);

            shader = new("Shader/shader.vert","Shader/shader.frag");
            shader.Use();

            Texture texture1 = Texture.LoadFromFile(woodenWall);
            texture1.Type = "texture_diffuse";
            Mesh cube1 = new(obj1V,obj1UV, new List<Texture>{texture1});

            Texture texture2 = Texture.LoadFromFile(rockTex);
            texture2.Type = "texture_diffuse";
            Mesh cube2 = new(obj2V,obj2UV, new List<Texture>{texture2});

            meshBuffer.Insert(cube1);
            meshBuffer.Insert(cube2);

            Model cube = new Model(cube1, new Vector3(-2,0,0));
            Model rock = new Model(cube2, new Vector3(2,0,0));
            models.Add(cube);
            models.Add(rock);

            camera = new (Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            camera.Fov = 45;

            CursorState = CursorState.Grabbed;
        }

        Random rnd = new();
        protected override void OnRenderFrame(FrameEventArgs e){
            base.OnRenderFrame(e);
            if(camera == null) throw new Exception("Camera not found");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if(shader == null) throw new NullReferenceException("Shader not found");
            shader.Use();

            shader.SetMatrix4("view",camera.GetViewMatrix());
            shader.SetMatrix4("projection",camera.GetProjectionMatrix());

            foreach(Model m in models){
                m.Draw(shader);
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e){
            base.OnUpdateFrame(e);
            if(!IsFocused) return;
            if(camera == null) throw new Exception("Camera not found");
            Time = DateTime.Now.Second - startTime;

            KeyboardState input = KeyboardState;
            if(input.IsKeyDown(Keys.Escape)){
                Close();
            }

            const float movementSpeed = 2f;
            const float sensitivity = 0.2f;


            Vector3 front = camera.Front;
            Vector3 right = camera.Right;
            front.Y = 0;
            right.Y = 0;

            if (input.IsKeyDown(Keys.W)) {
                camera.Position += front * movementSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Keys.S)) {
                camera.Position -= front * movementSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A)) {
                camera.Position -= right * movementSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D)) {
                camera.Position += right * movementSpeed * (float)e.Time; // Right
            }
            if(input.IsKeyDown(Keys.Space)){
                camera.Position += new Vector3(0,1,0) * movementSpeed * (float)e.Time; // Right
            }
            if(input.IsKeyDown(Keys.LeftControl)){
                camera.Position += new Vector3(0,-1,0) * movementSpeed * (float)e.Time; // Right
            }

            MouseState mouse = MouseState;

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
        }

        protected override void OnUnload(){
            base.OnUnload();
            shader?.Dispose();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            base.OnMouseWheel(e);
            if(camera == null) throw new Exception("Camera not found");
        }

        protected override void OnResize(ResizeEventArgs e) {
            base.OnResize(e);
            if(camera == null) throw new Exception("Camera not found");

            GL.Viewport(0,0,Size.X,Size.Y);
            camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}
