using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dithering1
{
    public static class Palettes
    {
        private static readonly int[] cga;
        private static readonly int[] tandy;
        private static readonly int[] hercules;

        static Palettes()
        {
            tandy = new int[] {0x000000,0x0000AA,0x00AA00,0x00AAAA,0xAA0000,0xAA00AA,0xAA5500,
                0xAAAAAA,0x555555,0x5555FF,0x55FF55,0x55FFFF,0xFF5555,0xFF55FF,0xFFFF55,0xFFFFFF};
            cga = new int[] { 0x090909,0x009c3a,0x4018ff,0x00c5ff,0xe3054b,0x9a9d96,0xff1dff,0xddc5ff,
                0x4c7400,0x07ff00,0x9e9a9a,0x53ffd2,0xff7800,0xf5ff00,0xff98fa,0xffffff};
            hercules = new int[] { 0x000000, 0xFFFFFF, 0x000000 , 0x000000 , 0x000000 , 0x000000 , 0x000000 , 0x000000 ,
                0x000000 , 0x000000 , 0x000000 , 0x000000 , 0x000000 , 0x000000 , 0x000000 , 0x000000 };
        }

        public enum PalTypes
        {
            CGA,
            TANDY,
            HERC
        }

        internal static int[] CGA
        {
            get
            {
                return cga;
            }
        }

        internal static int[] Tandy
        {
            get
            {
                return tandy;
            }
        }

        internal static int[] Hercules
        {
            get
            {
                return hercules;
            }
        }
    }
}
