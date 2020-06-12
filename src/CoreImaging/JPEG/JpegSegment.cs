using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreImaging.JPEG
{
    public class JpegSegment
    {
        public static JpegSegment ReadSegment(BinaryReader reader)
        {
            reader.ReadByte();

            var marker = (JpegImage.Marker)reader.ReadByte();

            switch (marker)
            {
                case JpegImage.Marker.SOI:
                    return new SoiSegment();
                case JpegImage.Marker.APP0:
                    return new App0Segment(reader);
                case JpegImage.Marker.APP1:
                    break;
                case JpegImage.Marker.APP2:
                    break;
                case JpegImage.Marker.APP3:
                    break;
                case JpegImage.Marker.APP4:
                    break;
                case JpegImage.Marker.APP5:
                    break;
                case JpegImage.Marker.APP6:
                    break;
                case JpegImage.Marker.APP7:
                    break;
                case JpegImage.Marker.APP8:
                    break;
                case JpegImage.Marker.APP9:
                    break;
                case JpegImage.Marker.APPA:
                    break;
                case JpegImage.Marker.APPB:
                    break;
                case JpegImage.Marker.APPC:
                    break;
                case JpegImage.Marker.APPD:
                    break;
                case JpegImage.Marker.APPE:
                    break;
                case JpegImage.Marker.APPF:
                    break;
                case JpegImage.Marker.SOF0:
                    break;
                case JpegImage.Marker.SOF1:
                    break;
                case JpegImage.Marker.SOF2:
                    break;
                case JpegImage.Marker.SOF3:
                    break;
                case JpegImage.Marker.SOF5:
                    break;
                case JpegImage.Marker.SOF6:
                    break;
                case JpegImage.Marker.SOF7:
                    break;
                case JpegImage.Marker.SOF9:
                    break;
                case JpegImage.Marker.SOF10:
                    break;
                case JpegImage.Marker.SOF11:
                    break;
                case JpegImage.Marker.SOF13:
                    break;
                case JpegImage.Marker.SOF14:
                    break;
                case JpegImage.Marker.SOF15:
                    break;
                case JpegImage.Marker.DHT:
                    return new DhtSegment(reader);
                case JpegImage.Marker.DQT:
                    return new DqtSegment(reader);
                case JpegImage.Marker.SOS:
                    return new SosSegment(reader);
                case JpegImage.Marker.EOI:
                    return new EoiSegment();
                default:
                    break;
            }

            return null;
        }
    }
}
