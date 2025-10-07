using OpenTK.Mathematics;

namespace Practice{
    public class Model{
        public Mesh Mesh {get;set;}
        public Vector3 Position {get;set;} = Vector3.Zero;
        public Vector3 Scale {get;set;} = Vector3.One;
        public Quaternion Rotation {get;set;} = Quaternion.Identity;

        public Model(Mesh mesh, Vector3 position){
            Mesh = mesh;
            Position = position;
        }

        public void Draw(Shader shader){
            Matrix4 model = 
                Matrix4.CreateScale(Scale) *
                Matrix4.CreateFromQuaternion(Rotation) *
                Matrix4.CreateTranslation(Position);

            shader.SetMatrix4("model",model);
            Mesh.Draw(shader);
        }
    }
}
