using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dithering1
{
    internal static class Helpers
    {
        internal static Color[] ToColorPalette(int[] palette)
        {
            Color[] tempArray = new Color[palette.Length];
            Parallel.For(0, tempArray.Length, i =>
            {
                tempArray[i] = Color.FromArgb(255, palette[i] >> 16, (palette[i] >> 8) & 0xFF, palette[i] & 0xFF);
            });
            return tempArray;
        }
        internal static int ToRGB(Color color)
        {
            return color.B + (color.G << 8) + (color.R << 16);
        }
    }
}
