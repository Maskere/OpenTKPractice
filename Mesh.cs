using OpenTK.Graphics.OpenGL4;

namespace Practice{
    public class Mesh{
        public List<float> Vertices {get;set;}
        public List<uint> Indices {get;set;}
        public List<Texture> Textures {get;set;}

        private int VertexArrayObject;
        private int VertexBufferObject;
        private int ElementBufferObject;

        public Mesh(List<float> vertices, List<uint> indices, List<Texture> textures){
            this.Vertices = vertices;
            this.Indices = indices;
            this.Textures = textures;

            SetupMesh();
        }

        public void Draw(Shader shader){
            int diffuseNr = 0;
            int specularNr = 0;

            for(int i = 0; i < Textures.Count; i++){
                GL.ActiveTexture(TextureUnit.Texture0 + i);

                string? number = "";
                string? name = Textures[i].Type;

                if(name == "texture_diffuse"){
                    number = diffuseNr++.ToString();
                }
                else if(name == "texture_specular"){
                    number = specularNr++.ToString();
                }

                GL.BindTexture(TextureTarget.Texture2D, Textures[i].Handle);
                shader.SetInt($"{name}{number}",i);
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        private void SetupMesh(){
            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            ElementBufferObject = GL.GenBuffer();

            GL.BindVertexArray(VertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            float[] vertices = Vertices.ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count * sizeof(uint), Indices.ToArray(), BufferUsageHint.StaticDraw);

            int stride = (3+3+2) * sizeof(float);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1,3,VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2,2,VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            GL.BindVertexArray(0);
        }
    }
}
