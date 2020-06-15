using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Dithering1
{
    public abstract class aDither
    {
        /* 8x8 threshold map */
        protected static readonly int[] map = new int[] {
     0,48,12,60, 3,51,15,63,
    32,16,44,28,35,19,47,31,
     8,56, 4,52,11,59, 7,55,
    40,24,36,20,43,27,39,23,
     2,50,14,62, 1,49,13,61,
    34,18,46,30,33,17,45,29,
    10,58, 6,54, 9,57, 5,53,
    42,26,38,22,41,25,37,21 };

        /* Palette */
        protected static int[] pal;
        protected Color[] friendlyPal;

        public aDither(Palettes.PalTypes paletteType)
        {
            switch (paletteType)
            {
                case Palettes.PalTypes.CGA:
                    pal = Palettes.CGA;
                    break;
                case Palettes.PalTypes.TANDY:
                    pal = Palettes.Tandy;
                    break;
                case Palettes.PalTypes.HERC:
                    pal = Palettes.Hercules;
                    break;
            }
            friendlyPal = Helpers.ToColorPalette(pal);
        }

        public abstract Bitmap DitherImage(Bitmap input);
    }
}
