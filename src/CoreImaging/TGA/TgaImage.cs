using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImaging.TGA
{
    public class TgaImage : Image
    {
        public TgaHeader Header { get; set; }
        /// <summary>
        /// Contains a free-form identification field of the length
        /// specified in byte 1 of the image record.  It's usually
        /// omitted ( length in byte 1 = 0 ), but can be up to 255
        /// characters.  If more identification information is
        /// required, it can be stored after the image data.
        /// </summary>
        public string ImageIdentificationField { get; set; }

        public TgaImage(string path)
        {
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                Header = new TgaHeader(stream);
                Height = Header.Height;
                Width = Header.Width;
                
                var buffer = new byte[Header.IdLength];
                stream.Read(buffer, 0, buffer.Length);

                ImageIdentificationField = System.Text.Encoding.ASCII.GetString(buffer);

                if(Header.DataTypeCode == TgaHeader.DataType.UncompressedRGB)
                {
                    if(Header.BitsPerPixel == 32)
                    {
                        DataStructure = ImageDataStructure.Bgra;
                        Data = new byte[Width * Height * 4];
                        stream.Read(Data, 0, Data.Length);
                    }
                    else if (Header.BitsPerPixel == 24)
                    {
                        DataStructure = ImageDataStructure.Bgr;
                        Data = new byte[Width * Height * 3];
                        stream.Read(Data, 0, Data.Length);
                    }
                }
            }
        }
    }
}
