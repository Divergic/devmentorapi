namespace TechMentorApi.Business
{
    using System;
    using System.IO;
    using EnsureThat;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using TechMentorApi.Model;

    public class PhotoResizer : IPhotoResizer
    {
        public Photo Resize(Photo photo, int maxHeight, int maxWidth)
        {
            Ensure.Any.IsNotNull(photo, nameof(photo));

            using (var image = Image.Load(null, photo.Data))
            {
                if (RequiresResize(image, maxHeight, maxWidth) == false)
                {
                    return photo;
                }
                
                var options = new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, maxHeight)
                };

                image.Mutate(ctx => ctx.Resize(options));

                var stream = new MemoryStream();

                IImageEncoder encoder;

                if (photo.ContentType == "image/png")
                {
                    encoder = new PngEncoder();
                }
                else if (photo.ContentType == "image/jpeg")
                {
                    encoder = new JpegEncoder();
                }
                else
                {
                    throw new NotSupportedException();
                }

                image.Save(stream, encoder);

                stream.Position = 0;

                return new Photo(photo, stream);
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