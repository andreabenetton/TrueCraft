// ---- AngelCode BmFont XML serializer ----------------------
// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
// ---- There's no license restrictions, use as you will. ----
// ---- Credits to http://www.angelcode.com/ -----------------

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TrueCraft.Client
{
    [Serializable]
    [XmlRoot ( "font" )]
    public class FontFile
    {
        [XmlElement ( "info" )]
        public FontInfo Info
        {
            get;
            set;
        }

        [XmlElement ( "common" )]
        public FontCommon Common
        {
            get;
            set;
        }

        [XmlArray ( "pages" )]
        [XmlArrayItem ( "page" )]
        public List<FontPage> Pages
        {
            get;
            set;
        }

        [XmlArray ( "chars" )]
        [XmlArrayItem ( "char" )]
        public List<FontChar> Chars
        {
            get;
            set;
        }

        [XmlArray ( "kernings" )]
        [XmlArrayItem ( "kerning" )]
        public List<FontKerning> Kernings
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontInfo
    {
        [XmlAttribute ( "face" )]
        public string Face
        {
            get;
            set;
        }

        [XmlAttribute ( "size" )]
        public int Size
        {
            get;
            set;
        }

        [XmlAttribute ( "bold" )]
        public int Bold
        {
            get;
            set;
        }

        [XmlAttribute ( "italic" )]
        public int Italic
        {
            get;
            set;
        }

        [XmlAttribute ( "charset" )]
        public string CharSet
        {
            get;
            set;
        }

        [XmlAttribute ( "unicode" )]
        public int Unicode
        {
            get;
            set;
        }

        [XmlAttribute ( "stretchH" )]
        public int StretchHeight
        {
            get;
            set;
        }

        [XmlAttribute ( "smooth" )]
        public int Smooth
        {
            get;
            set;
        }

        [XmlAttribute ( "aa" )]
        public int SuperSampling
        {
            get;
            set;
        }

        private Rectangle _padding;
        [XmlAttribute ( "padding" )]
        public string Padding
        {
            get => _padding.X + "," + _padding.Y + "," + _padding.Width + "," + _padding.Height;
            set
            {
                var padding = value.Split ( ',' );
                _padding = new Rectangle ( Convert.ToInt32 ( padding[0] ), Convert.ToInt32 ( padding[1] ), Convert.ToInt32 ( padding[2] ), Convert.ToInt32 ( padding[3] ) );
            }
        }

        private Point _spacing;
        [XmlAttribute ( "spacing" )]
        public string Spacing
        {
            get => _spacing.X + "," + _spacing.Y;
            set
            {
                var spacing = value.Split ( ',' );
                _spacing = new Point ( Convert.ToInt32 ( spacing[0] ), Convert.ToInt32 ( spacing[1] ) );
            }
        }

        [XmlAttribute ( "outline" )]
        public int OutLine
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontCommon
    {
        [XmlAttribute ( "lineHeight" )]
        public int LineHeight
        {
            get;
            set;
        }

        [XmlAttribute ( "base" )]
        public int Base
        {
            get;
            set;
        }

        [XmlAttribute ( "scaleW" )]
        public int ScaleW
        {
            get;
            set;
        }

        [XmlAttribute ( "scaleH" )]
        public int ScaleH
        {
            get;
            set;
        }

        [XmlAttribute ( "pages" )]
        public int Pages
        {
            get;
            set;
        }

        [XmlAttribute ( "packed" )]
        public int Packed
        {
            get;
            set;
        }

        [XmlAttribute ( "alphaChnl" )]
        public int AlphaChannel
        {
            get;
            set;
        }

        [XmlAttribute ( "redChnl" )]
        public int RedChannel
        {
            get;
            set;
        }

        [XmlAttribute ( "greenChnl" )]
        public int GreenChannel
        {
            get;
            set;
        }

        [XmlAttribute ( "blueChnl" )]
        public int BlueChannel
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontPage
    {
        [XmlAttribute ( "id" )]
        public int ID
        {
            get;
            set;
        }

        [XmlAttribute ( "file" )]
        public string File
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontChar
    {
        [XmlAttribute ( "id" )]
        public int ID
        {
            get;
            set;
        }

        [XmlAttribute ( "x" )]
        public int X
        {
            get;
            set;
        }

        [XmlAttribute ( "y" )]
        public int Y
        {
            get;
            set;
        }

        [XmlAttribute ( "width" )]
        public int Width
        {
            get;
            set;
        }

        [XmlAttribute ( "height" )]
        public int Height
        {
            get;
            set;
        }

        [XmlAttribute ( "xoffset" )]
        public int XOffset
        {
            get;
            set;
        }

        [XmlAttribute ( "yoffset" )]
        public int YOffset
        {
            get;
            set;
        }

        [XmlAttribute ( "xadvance" )]
        public int XAdvance
        {
            get;
            set;
        }

        [XmlAttribute ( "page" )]
        public int Page
        {
            get;
            set;
        }

        [XmlAttribute ( "chnl" )]
        public int Channel
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontKerning
    {
        [XmlAttribute ( "first" )]
        public int First
        {
            get;
            set;
        }

        [XmlAttribute ( "second" )]
        public int Second
        {
            get;
            set;
        }

        [XmlAttribute ( "amount" )]
        public int Amount
        {
            get;
            set;
        }
    }

    public class FontLoader
    {
        public static FontFile Load(Stream stream)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(FontFile));
            FontFile file = (FontFile) deserializer.Deserialize(stream);
            return file;
        }
    }
}