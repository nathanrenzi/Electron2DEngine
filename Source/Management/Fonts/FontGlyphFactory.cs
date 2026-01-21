using Electron2D.Rendering.Text;
using Electron2D.UserInterface;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using System.Numerics;
using static Electron2D.OpenGL.GL;
using static FreeTypeSharp.Native.FT;

namespace Electron2D.Management
{
    public static class FontGlyphFactory
    {
        public const int ATLAS_PADDING = 2;

        public static unsafe FontGlyphStore Load(string fontFile, int fontSize, float fontScale, int outlineWidth)
        {
            FreeTypeLibrary library = new FreeTypeLibrary();
            IntPtr face;
            FT_Error error;

            error = FT_New_Face(library.Native, fontFile, 0, out face);
            if (error == FT_Error.FT_Err_Unknown_File_Format)
            {
                Debug.LogError($"FREETYPE: The font file {fontFile} could be opened and read, but it appears that its font format is unsupported");
                return null;
            }
            else if (error != FT_Error.FT_Err_Ok)
            {
                Debug.LogError($"FREETYPE: The font file {fontFile} could not be opened or read");
                return null;
            }

            // Setting font pixel size, 0 width means dynamically scaled with height
            float scale = UICanvas.Instance.Scale * fontScale;
            FT_Set_Pixel_Sizes(face, 0, (uint)(fontSize * scale));

            FreeTypeFaceFacade f = new FreeTypeFaceFacade(library, face);

            //// Stroker is used for outline
            //IntPtr stroker = IntPtr.Zero;
            //if (_outlineWidth > 0)
            //{
            //    FT_Stroker_New(library.Native, out stroker);
            //    FT_Stroker_Set(stroker, _outlineWidth, FT_Stroker_LineCap.FT_STROKER_LINECAP_ROUND, FT_Stroker_LineJoin.FT_STROKER_LINEJOIN_ROUND, IntPtr.Zero);
            //}

            // Measuring the necessary size for the texture atlas
            int atlasWidth = 0;
            int atlasHeight = 0;
            for(uint c = 0; c < 128; c++)
            {
                if (FT_Load_Char(face, c, FT_LOAD_RENDER) != FT_Error.FT_Err_Ok)
                {
                    Debug.LogError($"FREETYPE: Failed to load glyph '{(char)c}'");
                    continue;
                }
                atlasWidth += (int)f.GlyphBitmap.width + ATLAS_PADDING;
                atlasHeight = (int)MathF.Max(atlasHeight, (int)f.GlyphBitmap.rows);
            }

            // Generating the texture
            uint texture = glGenTexture();
            glActiveTexture(GL_TEXTURE10);
            glBindTexture(GL_TEXTURE_2D, texture);
            glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, atlasWidth, atlasHeight + ATLAS_PADDING, 0, GL_RED, GL_UNSIGNED_BYTE, IntPtr.Zero);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

            int size = atlasWidth * (atlasHeight + ATLAS_PADDING);
            byte[] clear = new byte[size]; // initialized to 0
            fixed (byte* p = clear)
            {
                glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0,
                    atlasWidth, atlasHeight + ATLAS_PADDING,
                    GL_RED, GL_UNSIGNED_BYTE, (IntPtr)p);
            }

            FontGlyphStore store = new FontGlyphStore(texture, atlasWidth, atlasHeight, fontSize, fontFile, library, face, f.HasKerningFlag);

            int pos = 0;
            for (uint c = 0; c < 128; c++)
            {
                if (FT_Load_Char(face, c, FT_LOAD_RENDER) != FT_Error.FT_Err_Ok)
                {
                    Debug.LogError($"FREETYPE: Failed to load glyph '{(char)c}'");
                    continue;
                }

                glTexSubImage2D(GL_TEXTURE_2D, 0, pos, 0, (int)f.GlyphBitmap.width, (int)f.GlyphBitmap.rows, GL_RED, GL_UNSIGNED_BYTE, f.GlyphBitmap.buffer);

                Character character = new Character(
                    new Vector2(f.GlyphBitmap.width / scale, f.GlyphBitmap.rows / scale),
                    new Vector2(pos / (float)atlasWidth, (pos + f.GlyphBitmap.width) / (float)atlasWidth),
                    new Vector2(0, f.GlyphBitmap.rows / (float)(atlasHeight + ATLAS_PADDING)),
                    new Vector2(f.GlyphBitmapLeft / scale, f.GlyphBitmapTop / scale),
                    (uint)(f.GlyphMetricHorizontalAdvance / scale)
                );
                store.AddCharacter((char)c, character);

                pos += (int)f.GlyphBitmap.width + ATLAS_PADDING;
            }

            store.Done();

            glPixelStorei(GL_UNPACK_ALIGNMENT, 4);

            return store;
        }
    }
}
