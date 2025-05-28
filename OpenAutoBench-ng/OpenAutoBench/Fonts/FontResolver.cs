using OpenAutoBench_ng.Properties;
using PdfSharpCore.Fonts;

namespace OpenAutoBench_ng.OpenAutoBench.Fonts
{
    public class FontResolver : IFontResolver
    {
        public string DefaultFontName { get { return "OpenSans-Regular"; } }

        public byte[] GetFont(string fontName)
        {
            switch (fontName)
            {
                case "Jost":
                case "Jost-Regular":
                    return Resources.Jost_Regular;
                case "Jost-Bold":
                    return Resources.Jost_Bold;
                case "OpenSans":
                case "OpenSans-Regular":
                    return Resources.OpenSans_Regular;
                case "OpenSans-Bold":
                    return Resources.OpenSans_Bold;
                case "RobotoMono":
                case "RobotoMono-Regular":
                    return Resources.RobotoMono_Regular;
                default:
                    return Resources.OpenSans_Regular;
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Jost", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold)
                {
                    return new FontResolverInfo("Jost-Bold");
                }
                else
                {
                    return new FontResolverInfo("Jost");
                }
            }
            else if (familyName.Equals("RobotoMono", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold)
                {
                    return new FontResolverInfo("RobotoMono-Bold");
                }
                else
                {
                    return new FontResolverInfo("RobotoMono");
                }
            }
            else
            {
                if (isBold)
                {
                    return new FontResolverInfo("OpenSans-Bold");
                }
                else
                {
                    return new FontResolverInfo("OpenSans");
                }
            }
        }
    }
}
