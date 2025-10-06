using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Practice{
    public class Shader{
        public int Handle;
        private readonly Dictionary<string,int> uniformLocation;

        public Shader(string vertexPath, string fragmentPath){
            string FragmentShaderSource = File.ReadAllText(fragmentPath);
            string VertexShaderSource = File.ReadAllText(vertexPath);
            int FragmentShader;
            int VertexShader;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader,VertexShaderSource);
            CompileShader(VertexShader);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            CompileShader(FragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle,VertexShader);
            GL.DetachShader(Handle,FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);

            GL.GetProgram(Handle,GetProgramParameterName.ActiveUniforms,out int numberOfUniforms);

            uniformLocation = new();

            for(int i = 0; i < numberOfUniforms; i++){
                string key = GL.GetActiveUniform(Handle,i,out _, out _);
                int location = GL.GetUniformLocation(Handle,key);

                uniformLocation.Add(key,location);
            }
        }

        public static void CompileShader(int shader){
            GL.CompileShader(shader);
            GL.GetShader(shader,ShaderParameter.CompileStatus, out int fragmentSuccess);
            if(fragmentSuccess == 0){
                string infoLog = GL.GetShaderInfoLog(shader);

                Console.WriteLine(infoLog);
            }
        }

        private static void LinkProgram(int program){
            GL.LinkProgram(program);

            GL.GetProgram(program,GetProgramParameterName.LinkStatus, out int handleSuccess);
            if(handleSuccess == 0){
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine(infoLog);
            }
        }

        public void Use(){
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName){
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data){
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocation[name],data);
        }

        public void SetFloat(string name, float data){
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocation[name],data);
        }

        public void SetMatrix4(string name, Matrix4 data){
            GL.UseProgram(Handle);
            GL.UniformMatrix4(uniformLocation[name],true, ref data);
        }

        public void SetVector3(string name, Vector3 data){
            GL.UseProgram(Handle);
            GL.Uniform3(uniformLocation[name],data);
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
