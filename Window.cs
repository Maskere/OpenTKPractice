using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using MeshDataExtract;

namespace Practice{
    public class Window : GameWindow{
        private Stopwatch sw = new();
        private readonly string waveObj = "./Resources/CubeWave.obj";
        private readonly string waveObj1 = "./Resources/CubeWave.obj";
        private static readonly string? textureFile = "./Resources/container.png";
        private MeshDataExtractor extracter = new();
        private List<string> objects = new();
        private List<float> verticesToRender = new();
        private List<uint> indicesToRender = new();

        private Matrix4 model = Matrix4.Identity;
        private Matrix4 model2 = Matrix4.Identity;

        private Vector3 position = Vector3.Zero;
        private Vector3 position2 = Vector3.Zero;
        private Texture? texture;
        private Vector2 lastPos;
        private Camera? camera;
        private Shader? shader;

        private float startTime;
        private float Time;
        private bool firstMove = true;
        private int VertexBufferObject;
        private int VertexArrayObject;
        private int ElementBufferObject;
        private int totalIndexCount;
        private int waveObjIndexCount;
        Vector3 move = new();

        private List<int> indexCount = new();
        private List<Mesh> meshes = new();

        public Window(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() {
                ClientSize = (width,height), Title = title}){
            startTime = DateTime.Now.Second;
        }

        protected override void OnLoad(){
            base.OnLoad();
            GL.ClearColor(0.2f,0.3f,0.3f,1);
            GL.Enable(EnableCap.DepthTest);

            objects.Add(waveObj);
            objects.Add(waveObj1);

            // const int standard_stride = 8;
            extracter.WavefrontObjDataExtractor(objects[0],out List<float> obj1V, out List<uint> obj1UV);
            extracter.WavefrontObjDataExtractor(objects[1],out List<float> obj2V, out List<uint> obj2UV);

            shader = new("Shader/shader.vert","Shader/shader.frag");
            shader.Use();

            Texture texture1 = Texture.LoadFromFile(textureFile);
            texture1.Type = "texture_diffuse";
            Mesh cube1 = new(obj1V,obj1UV, new List<Texture>{texture1});

            Texture texture2 = Texture.LoadFromFile(textureFile);
            texture2.Type = "texture_diffuse";
            Mesh cube2 = new(obj2V,obj2UV, new List<Texture>{texture2});

            meshes.Add(cube1);
            meshes.Add(cube2);

            camera = new (Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            camera.Fov = 45;

            CursorState = CursorState.Grabbed;
        }

        Random rnd = new();
        protected override void OnRenderFrame(FrameEventArgs e){
            base.OnRenderFrame(e);
            if(camera == null) throw new Exception("Camera not found");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            if(shader == null) throw new NullReferenceException("Shader not found");
            // if(texture == null) throw new ArgumentNullException("Texture class not found");
            // texture.Use(TextureUnit.Texture0);
            shader.Use();

            shader.SetMatrix4("view",camera.GetViewMatrix());
            shader.SetMatrix4("projection",camera.GetProjectionMatrix());

            Matrix4 model1 = Matrix4.CreateTranslation(-1.5f,0,0);
            shader.SetMatrix4("model", model1);
            meshes[0].Draw(shader);

            Matrix4 model2 = Matrix4.CreateTranslation(1.5f,0,0);
            shader.SetMatrix4("model", model2);
            meshes[1].Draw(shader);

            // move = new(0,-2,0);
            // model =
            //     Matrix4.Identity *
            //     Matrix4.CreateScale(0.5f) *
            //     Matrix4.CreateRotationX(0) *
            //     Matrix4.CreateRotationY(0) *
            //     Matrix4.CreateRotationZ(0) *
            //     Matrix4.CreateTranslation(move);

            // shader.SetMatrix4("model", model2);
            GL.DrawElements(PrimitiveType.Triangles, indicesToRender.Count, DrawElementsType.UnsignedInt, 0);

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
            // if (input.IsKeyDown(Keys.Space)) {
            //     // moveY += movementSpeed * (float)e.Time;
            //     camera.Position += camera.Up * movementSpeed * (float)e.Time; // Up
            // }
            // if (input.IsKeyDown(Keys.LeftShift)) {
            //     // moveY -= movementSpeed * (float)e.Time;
            //     camera.Position -= camera.Up * movementSpeed * (float)e.Time; // Down
            // }

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

            // camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e) {
            base.OnResize(e);
            if(camera == null) throw new Exception("Camera not found");

            GL.Viewport(0,0,Size.X,Size.Y);

            camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}
