using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace Dithering1
{
    public class Dithering1 : aDither
    {
        /* Luminance for each palette entry, to be initialized as soon as the program begins */
        static int[] luma = new int[16];

        int PaletteCompareLuma(int index1, int index2)
        {
            return luma[index1].CompareTo(luma[index2]);
        }

        //used to store an array of 16 * 4 possible colors to use for a given pixel
        struct MixingPlan
        {
            public int[] colors;
        };

        #region scalar
#if (SCALAR)
        double ColorCompare(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            double luma1 = (r1 * 299 + g1 * 587 + b1 * 114) / (255.0 * 1000);
            double luma2 = (r2 * 299 + g2 * 587 + b2 * 114) / (255.0 * 1000);
            double lumadiff = luma1 - luma2;
            double diffR = (r1 - r2) / 255.0, diffG = (g1 - g2) / 255.0, diffB = (b1 - b2) / 255.0;
            return (diffR * diffR * 0.299 + diffG * diffG * 0.587 + diffB * diffB * 0.114) * 0.75
                 + lumadiff * lumadiff;
        }

        MixingPlan DeviseBestMixingPlan(int color)
        {
            MixingPlan result = new MixingPlan();
            result.colors = new int[64];

            int[] src = new int[] { color >> 16, (color >> 8) & 0xFF, color & 0xFF };

            const double X = 0.09;  // Error multiplier
            int[] e = new int[] { 0, 0, 0 }; // Error accumulator
            for (int c = 0; c < 64; ++c)
            {
                // Current temporary value
                int[] t = new int[] { (int)(src[0] + e[0] * X), (int)(src[1] + e[1] * X), (int)(src[2] + e[2] * X) };
                // Clamp it in the allowed RGB range
                if (t[0] < 0) t[0] = 0; else if (t[0] > 255) t[0] = 255;
                if (t[1] < 0) t[1] = 0; else if (t[1] > 255) t[1] = 255;
                if (t[2] < 0) t[2] = 0; else if (t[2] > 255) t[2] = 255;
                // Find the closest color from the palette
                double least_penalty = 1e99;
                int chosen = c % 16;
                for (int index = 0; index < 16; ++index)
                {
                    color = pal[index];
                    int[] pc = new int[] { color >> 16, (color >> 8) & 0xFF, color & 0xFF };
                    double penalty = ColorCompare(pc[0], pc[1], pc[2], t[0], t[1], t[2]);
                    if (penalty < least_penalty)
                    { least_penalty = penalty; chosen = index; }
                }
                // Add it to candidates and update the error
                result.colors[c] = chosen;
                color = pal[chosen];
                int[] pc1 = new int[] { color >> 16, (color >> 8) & 0xFF, color & 0xFF };
                e[0] += src[0] - pc1[0];
                e[1] += src[1] - pc1[1];
                e[2] += src[2] - pc1[2];
            }
            // Sort the colors according to luminance
            Array.Sort(result.colors, PaletteCompareLuma);
            return result;
        }
#endif
        #endregion

        #region scalar-single
#if (SCALAR_SINGLE)
        float ColorCompare(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            float luma1 = (r1 * 299 + g1 * 587 + b1 * 114) / (255.0f * 1000);
            float luma2 = (r2 * 299 + g2 * 587 + b2 * 114) / (255.0f * 1000);
            float lumadiff = luma1 - luma2;
            float diffR = (r1 - r2) / 255.0f, diffG = (g1 - g2) / 255.0f, diffB = (b1 - b2) / 255.0f;
            return (diffR * diffR * 0.299f + diffG * diffG * 0.587f + diffB * diffB * 0.114f) * 0.75f
                 + lumadiff * lumadiff;
        }

        MixingPlan DeviseBestMixingPlan(int color)
        {
            MixingPlan result = new MixingPlan();
            result.colors = new int[64];

            int[] src = new int[] { color >> 16, (color >> 8) & 0xFF, color & 0xFF };

            const float X = 0.09f;  // Error multiplier
            int[] e = new int[] { 0, 0, 0 }; // Error accumulator
            for (int c = 0; c < 64; ++c)
            {
                // Current temporary value
                int[] t = new int[] { (int)(src[0] + e[0] * X), (int)(src[1] + e[1] * X), (int)(src[2] + e[2] * X) };
                // Clamp it in the allowed RGB range
                if (t[0] < 0) t[0] = 0; else if (t[0] > 255) t[0] = 255;
                if (t[1] < 0) t[1] = 0; else if (t[1] > 255) t[1] = 255;
                if (t[2] < 0) t[2] = 0; else if (t[2] > 255) t[2] = 255;
                // Find the closest color from the palette
                float least_penalty = float.MaxValue;
                int chosen = c % 16;
                for (int index = 0; index < 16; ++index)
                {
                    color = pal[index];
                    int[] pc = new int[] { color >> 16, (color >> 8) & 0xFF, color & 0xFF };
                    float penalty = ColorCompare(pc[0], pc[1], pc[2], t[0], t[1], t[2]);
                    if (penalty < least_penalty)
                    { least_penalty = penalty; chosen = index; }
                }
                // Add it to candidates and update the error
                result.colors[c] = chosen;
                color = pal[chosen];
                int[] pc1 = new int[] { color >> 16, (color >> 8) & 0xFF, color & 0xFF };
                e[0] += src[0] - pc1[0];
                e[1] += src[1] - pc1[1];
                e[2] += src[2] - pc1[2];
            }
            // Sort the colors according to luminance
            Array.Sort(result.colors, PaletteCompareLuma);
            return result;
        }
#endif
        #endregion

        #region vectorized
#if X64 && !SCALAR && !SCALAR_SINGLE
        //constants used
        static readonly Vector3 lumaMults = new Vector3(299, 587, 114);
        static readonly Vector3 diffCalcs = new Vector3(0.299f, 0.587f, 0.114f);
        const float maxLight = 255;

        float ColorCompare(Vector3 vecRGB1, Vector3 vecRGB2)
        {

            const int lumaConst = 255000;

            Vector3 vecLuma1 = vecRGB1 * lumaMults;
            Vector3 vecLuma2 = vecRGB2 * lumaMults;

            float luma1 = (vecLuma1.X + vecLuma1.Y + vecLuma1.Z) / lumaConst;
            float luma2 = (vecLuma2.X + vecLuma2.Y + vecLuma2.Z) / lumaConst;

            float lumadiff = luma1 - luma2;

            Vector3 diffRGB = Vector3.Divide(Vector3.Subtract(vecRGB1,vecRGB2), maxLight);
            diffRGB = diffRGB * diffRGB * diffCalcs;

            return (diffRGB.X + diffRGB.Y + diffRGB.Z) * 0.75f + lumadiff * lumadiff;
        }
        MixingPlan DeviseBestMixingPlan(int color)
        {
            Vector3 clampMin = Vector3.Zero;
            Vector3 clampMax = new Vector3(255, 255, 255);

            MixingPlan result = new MixingPlan
            {
                colors = new int[64]
            };

            Vector3 src = new Vector3( color >> 16, (color >> 8) & 0xFF, color & 0xFF);

            const float X = 0.09f;  // Error multiplier
            Vector3 e = Vector3.Zero; // Error accumulator
            for (int c = 0; c < 64; ++c)
            {
                // Current temporary value
                Vector3 t = src + Vector3.Multiply(e, X);
                // Clamp it in the allowed RGB range
                t = Vector3.Clamp(t, clampMin, clampMax);
                // Find the closest color from the palette
                float least_penalty = float.MaxValue;
                int chosen = c % 16;
                for (int index = 0; index < 16; ++index)
                {
                    int color1 = pal[index];
                    Vector3 pc1;
                    pc1 = new Vector3(color1 >> 16, (color1 >> 8) & 0xFF, color1 & 0xFF);
                    float penalty = ColorCompare(pc1, t);
                    if (penalty < least_penalty)
                    { least_penalty = penalty; chosen = index; }
                }
                // Add it to candidates and update the error
                result.colors[c] = chosen;
                color = pal[chosen];
                Vector3 pc = new Vector3(color >> 16, (color >> 8) & 0xFF, color & 0xFF);
                e += src - pc;
            }
            // Sort the colors according to luminance
            Array.Sort(result.colors, PaletteCompareLuma);
            return result;
        }
#endif
        #endregion

        public Dithering1(Palettes.PalTypes paletteType) : base(paletteType)
        {

        }

        public override Bitmap DitherImage(Bitmap input)
        {
            //get depth and bytes-per-pixel for input
            int depth = Image.GetPixelFormatSize(input.PixelFormat);
            int cCount = depth / 8;

            Bitmap result = new Bitmap(input.Width, input.Height, PixelFormat.Format8bppIndexed);

            //prepare result's palette using pre-defined palette
            ColorPalette resultPalette = result.Palette;

            Parallel.For(0, pal.Length, c =>
           {
               int r = pal[c] >> 16, g = (pal[c] >> 8) & 0xFF, b = pal[c] & 0xFF;
               resultPalette.Entries[c] = friendlyPal[c];
               luma[c] = r * 299 + g * 587 + b * 114;
           });

            //load palette into result
            result.Palette = resultPalette;

            //lock the input data into memory
            BitmapData inputData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, input.PixelFormat);

            BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //create byte arrays for data
            byte[] resultColorData = new byte[resultData.Stride * resultData.Height];
            byte[] inputColorData = new byte[inputData.Width * inputData.Height * cCount];

            //store inputs' height and width in thread-safe variables
            int inputHeight = input.Height;
            int inputWidth = input.Width;
            int inputDataWidth = inputData.Width;

            //copy data into byte arrays
            Marshal.Copy(inputData.Scan0, inputColorData, 0, inputColorData.Length);

            Marshal.Copy(resultData.Scan0, resultColorData, 0, resultColorData.Length);

#if DEBUG
            try
            {
                Parallel.For(0, inputHeight, y =>
                 {
                     for (int x = 0; x < inputWidth; x++)
                     {
                         //get current position in the array
                         int i = ((y * inputDataWidth) + x) * cCount;
                         int color = inputColorData[i] + (inputColorData[i + 1] << 8) + (inputColorData[i + 2] << 16);
                         int map_value = map[(x & 7) + ((y & 7) << 3)];
                         MixingPlan plan = DeviseBestMixingPlan(color);
                         resultColorData[i / cCount] = (byte)plan.colors[map_value];
                     };
                 });

                ////Scalar testing code
                //for (int y = 0; y < input.Height; ++y)
                //{
                //    for (int x = 0; x < input.Width; ++x)
                //    {
                //        int i = ((y * inputData.Width) + x) * cCount;
                //        int color = inputColorData[i] + (inputColorData[i + 1] << 8) + (inputColorData[i + 2] << 16);
                //        int map_value = map[(x & 7) + ((y & 7) << 3)];
                //        MixingPlan plan = DeviseBestMixingPlan(color);
                //        resultColorData[i / cCount] = (byte)plan.colors[map_value];
                //    };
                //};
                Marshal.Copy(resultColorData, 0, resultData.Scan0, resultColorData.Length);
            }
            finally
            {
                result.UnlockBits(resultData);
                input.UnlockBits(inputData);
            }
#endif
#if !DEBUG
            Parallel.For(0, inputHeight, y =>
            {
                for (int x = 0; x < inputWidth; x++)
                {
                    //get current position in the array
                    int i = ((y * inputDataWidth) + x) * cCount;
                    int color = inputColorData[i] + (inputColorData[i + 1] << 8) + (inputColorData[i + 2] << 16);
                    int map_value = map[(x & 7) + ((y & 7) << 3)];
                    MixingPlan plan = DeviseBestMixingPlan(color);
                    resultColorData[i / cCount] = (byte)plan.colors[map_value];
                };
            });
            Marshal.Copy(resultColorData, 0, resultData.Scan0, resultColorData.Length);
#endif
            return result;
        }
    }
}
