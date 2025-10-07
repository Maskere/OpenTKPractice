using OpenTK.Mathematics;

namespace Practice{
    public class Ray{
        private Vector3 origin;
        private Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction){
            this.origin = origin;
            this.direction = direction;
        }
    }
}
