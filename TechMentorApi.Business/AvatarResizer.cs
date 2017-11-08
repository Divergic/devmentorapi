namespace TechMentorApi.Business
{
    using System;
    using System.IO;
    using EnsureThat;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using TechMentorApi.Model;

    public class AvatarResizer : IAvatarResizer
    {
        public Avatar Resize(Avatar avatar, int maxHeight, int maxWidth)
        {
            Ensure.That(avatar, nameof(avatar)).IsNotNull();

            using (var image = Image.Load(null, avatar.Data))
            {
                if (RequiresResize(image, maxHeight, maxWidth) == false)
                {
                    return avatar;
                }
                
                var options = new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, maxHeight)
                };

                image.Mutate(ctx => ctx.Resize(options));

                var stream = new MemoryStream();

                IImageEncoder encoder;

                if (avatar.ContentType == "image/png")
                {
                    encoder = new PngEncoder();
                }
                else if (avatar.ContentType == "image/jpeg")
                {
                    encoder = new JpegEncoder();
                }
                else
                {
                    throw new NotSupportedException();
                }

                image.Save(stream, encoder);

                stream.Position = 0;

                return new Avatar(avatar, stream);
            }
        }

        private static bool RequiresResize(Image<Rgba32> image, int maxHeight, int maxWidth)
        {
            if (image.Width > maxWidth)
            {
                return true;
            }

            if (image.Height > maxHeight)
            {
                return true;
            }

            return false;
        }
    }
}