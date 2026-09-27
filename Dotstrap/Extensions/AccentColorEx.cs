using System.Windows;
using System.Windows.Media;

namespace Dotstrap.Extensions
{
    public struct AccentPalette
    {
        public Color Base;
        public Color Primary;
        public Color Secondary;
        public Color Tertiary;

        // used for the bootstrapper dialog background gradient (darkened variant of the accent)
        public Color GradientStart;
        public Color GradientEnd;
    }

    public static class AccentColorEx
    {
        public static IReadOnlyCollection<AccentColor> Selections => Enum.GetValues<AccentColor>();

        private static Color ParseHex(string hex, Color fallback)
        {
            try
            {
                var converted = ColorConverter.ConvertFromString(hex);
                if (converted is Color color)
                    return color;
            }
            catch
            {
                // fall through to fallback
            }

            return fallback;
        }

        private static Color Darken(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
        }

        private static Color Blend(Color color, Color with, double amount)
        {
            return Color.FromRgb(
                (byte)(color.R + (with.R - color.R) * amount),
                (byte)(color.G + (with.G - color.G) * amount),
                (byte)(color.B + (with.B - color.B) * amount)
            );
        }

        /// <summary>
        /// Diagonal accent-tinted gradient used as the background for Dotstrap windows.
        /// Dark theme uses the darkened gradient colours, light theme a soft pastel tint.
        /// </summary>
        public static Brush GetBackgroundBrush(this AccentColor accent, Theme theme)
        {
            var palette = accent.GetPalette();

            if (theme == Theme.Light)
            {
                var white = Color.FromRgb(0xF7, 0xF7, 0xF7);

                return new LinearGradientBrush(
                    Blend(palette.Secondary, white, 0.86),
                    Blend(palette.Tertiary, white, 0.80),
                    new Point(0, 0), new Point(1, 1)
                );
            }

            return new LinearGradientBrush(palette.GradientStart, palette.GradientEnd, new Point(0, 0), new Point(1, 1));
        }

        public static AccentPalette GetPalette(this AccentColor accent)
        {
            switch (accent)
            {
                case AccentColor.Blue:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0x2E, 0x7D, 0xC7),
                        Primary = Color.FromRgb(0x4F, 0xC3, 0xF7),
                        Secondary = Color.FromRgb(0x3A, 0x9C, 0xEA),
                        Tertiary = Color.FromRgb(0x7C, 0x6C, 0xE8),
                        GradientStart = Color.FromRgb(0x0D, 0x22, 0x33),
                        GradientEnd = Color.FromRgb(0x18, 0x14, 0x32)
                    };
                case AccentColor.Purple:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0x7B, 0x4D, 0xC7),
                        Primary = Color.FromRgb(0xB0, 0x8A, 0xF7),
                        Secondary = Color.FromRgb(0x8A, 0x5C, 0xF6),
                        Tertiary = Color.FromRgb(0xC7, 0x5C, 0xE0),
                        GradientStart = Color.FromRgb(0x20, 0x11, 0x33),
                        GradientEnd = Color.FromRgb(0x2E, 0x11, 0x2A)
                    };
                case AccentColor.Red:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0xB0, 0x3A, 0x3A),
                        Primary = Color.FromRgb(0xF0, 0x6A, 0x6A),
                        Secondary = Color.FromRgb(0xE0, 0x4F, 0x4F),
                        Tertiary = Color.FromRgb(0xE8, 0x8A, 0x2E),
                        GradientStart = Color.FromRgb(0x30, 0x0D, 0x0D),
                        GradientEnd = Color.FromRgb(0x33, 0x1E, 0x0B)
                    };
                case AccentColor.Orange:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0xC7, 0x7B, 0x2E),
                        Primary = Color.FromRgb(0xF7, 0xB2, 0x4D),
                        Secondary = Color.FromRgb(0xE8, 0x8A, 0x2E),
                        Tertiary = Color.FromRgb(0xE0, 0xC0, 0x38),
                        GradientStart = Color.FromRgb(0x33, 0x1E, 0x0B),
                        GradientEnd = Color.FromRgb(0x33, 0x2C, 0x0B)
                    };
                case AccentColor.Teal:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0x2E, 0xA0, 0x9C),
                        Primary = Color.FromRgb(0x59, 0xDB, 0xD2),
                        Secondary = Color.FromRgb(0x2E, 0xC4, 0xB6),
                        Tertiary = Color.FromRgb(0x38, 0xA1, 0xC0),
                        GradientStart = Color.FromRgb(0x0B, 0x2C, 0x2A),
                        GradientEnd = Color.FromRgb(0x0B, 0x24, 0x33)
                    };
                case AccentColor.Pink:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0xC7, 0x4D, 0x9C),
                        Primary = Color.FromRgb(0xF7, 0x8A, 0xC7),
                        Secondary = Color.FromRgb(0xE0, 0x5C, 0x9E),
                        Tertiary = Color.FromRgb(0xE0, 0x5C, 0x5C),
                        GradientStart = Color.FromRgb(0x33, 0x0B, 0x24),
                        GradientEnd = Color.FromRgb(0x33, 0x0B, 0x16)
                    };
                case AccentColor.Custom:
                    {
                        var custom = ParseHex(App.Settings.Prop.CustomAccentColorHex, Color.FromRgb(0x5C, 0xB9, 0x3F));

                        return new AccentPalette
                        {
                            Base = Darken(custom, 0.85),
                            Primary = custom,
                            Secondary = custom,
                            Tertiary = custom,
                            GradientStart = Darken(custom, 0.22),
                            GradientEnd = Darken(custom, 0.34)
                        };
                    }
                case AccentColor.Green:
                default:
                    return new AccentPalette
                    {
                        Base = Color.FromRgb(0x4D, 0xB0, 0x4D),
                        Primary = Color.FromRgb(0x59, 0xDB, 0xA2),
                        Secondary = Color.FromRgb(0x5C, 0xB9, 0x3F),
                        Tertiary = Color.FromRgb(0xA1, 0xC0, 0x38),
                        GradientStart = Color.FromRgb(0x0F, 0x2E, 0x22),
                        GradientEnd = Color.FromRgb(0x2A, 0x33, 0x0E)
                    };
            }
        }
    }
}
