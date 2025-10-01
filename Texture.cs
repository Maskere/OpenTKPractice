using System;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Practice{
    public class Texture{
        public readonly int Handle;

        public Texture(int glHandle){
            Handle = glHandle;
        }

        public static Texture LoadFromFile(string? filePath){
            if(filePath == null) throw new ArgumentNullException("Invalid filepath");
            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D,handle);

            StbImage.stbi_set_flip_vertically_on_load(1);

            using(Stream stream = File.OpenRead(filePath)){
                ImageResult image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba, image.Width,image.Height,0, PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
            }

            GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Linear);

            return new Texture(handle);
        }

        public void Use(TextureUnit unit){
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D,Handle);
        }
    }
}
