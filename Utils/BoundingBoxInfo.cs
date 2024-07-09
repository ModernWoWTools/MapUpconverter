using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace MapUpconverter.Utils
{
    public static class BoundingBoxInfo
    {
        public static Dictionary<string, CAaBox> boundingBoxBlobDict;

        public static void Initialize(string configPath)
        {
            boundingBoxBlobDict = JsonConvert.DeserializeObject<Dictionary<string, CAaBox>>(File.ReadAllText(configPath));
        }
    }

    public struct CAaBox
    {
        public CAaBox(C3Vector<float> inBottomCorner, C3Vector<float> inTopCorner)
        {
            this.BottomCorner = inBottomCorner;
            this.TopCorner = inTopCorner;
        }

        public override string ToString()
        {
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
            defaultInterpolatedStringHandler.AppendLiteral("Min : ");
            defaultInterpolatedStringHandler.AppendFormatted<C3Vector<float>>(this.BottomCorner);
            defaultInterpolatedStringHandler.AppendLiteral(", Max : ");
            defaultInterpolatedStringHandler.AppendFormatted<C3Vector<float>>(this.TopCorner);
            return defaultInterpolatedStringHandler.ToStringAndClear();
        }

        public C3Vector<float> BottomCorner;

        public C3Vector<float> TopCorner;
    }

    public struct C3Vector<T>
    {
        public T x { readonly get; set; }
        public T y { readonly get; set; }
        public T z { readonly get; set; }

        public override string ToString()
        {
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 3);
            defaultInterpolatedStringHandler.AppendLiteral("X : ");
            defaultInterpolatedStringHandler.AppendFormatted<T>(this.x);
            defaultInterpolatedStringHandler.AppendLiteral(", Y : ");
            defaultInterpolatedStringHandler.AppendFormatted<T>(this.y);
            defaultInterpolatedStringHandler.AppendLiteral(", Z : ");
            defaultInterpolatedStringHandler.AppendFormatted<T>(this.z);
            return defaultInterpolatedStringHandler.ToStringAndClear();
        }
    }
}
