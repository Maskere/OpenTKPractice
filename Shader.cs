using OpenTK.Graphics.OpenGL4;

namespace Practice{
    public class Shader{
        public int Handle;

        public Shader(string vertexPath, string fragmentPath){
            string FragmentShaderSource = File.ReadAllText(fragmentPath);
            string VertexShaderSource = File.ReadAllText(vertexPath);
            int FragmentShader;
            int VertexShader;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader,VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            GL.CompileShader(VertexShader);
            GL.GetShader(VertexShader,ShaderParameter.CompileStatus, out int vertexSuccess);
            if(vertexSuccess == 0){
                string infoLog = GL.GetShaderInfoLog(VertexShader);

                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);
            GL.GetShader(FragmentShader,ShaderParameter.CompileStatus, out int fragmentSuccess);
            if(fragmentSuccess == 0){
                string infoLog = GL.GetShaderInfoLog(FragmentShader);

                Console.WriteLine(infoLog);
            }

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle,GetProgramParameterName.LinkStatus, out int handleSuccess);
            if(handleSuccess == 0){
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(Handle,FragmentShader);
            GL.DetachShader(Handle,VertexShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        public void Use(){
            GL.UseProgram(Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing){
            if(!disposedValue){
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader(){
            if(disposedValue == false){
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }

        public void Dispose(){
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
