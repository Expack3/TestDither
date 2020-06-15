using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Dithering1;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Media;

namespace TestDither
{
    class Program
    {
        private const int avgMSperFrame = 460; //estimated milliseconds-per-frame based on prior manual testing
        private const string matchingPattern = "^o[0-9]*.bmp"; //regex for identifying valid input files
        private static int currentFrames = 0;

        static void Main(string[] args)
        {
            bool verbosity = true; //determines if we need to know when individual frames are completed.
            switch (args.Length)
            {
                case 0:
                case 1:
                    Console.WriteLine("Usage: TestDither DIRECTORY COLOR_TYPE");
                    Console.WriteLine(@"Example: TestDither c:\pictures\output CGA");
                    Console.WriteLine(@"Example: TestDither c:\pictures\output TANDY");
                    Console.WriteLine(@"Example: TestDither c:\pictures\output HERC");
                    Console.WriteLine();
                    Console.WriteLine("NOTE: Output files must be in the following format: o$$$$$$$.bmp");
                    Console.WriteLine("($ = any number between 0 and 9");
                    return;
                case 3:
                    if (args[2].Equals("-v"))
                        verbosity = false;
                    break;
            }
            if (!Directory.Exists(args[0]))
                throw new FileNotFoundException();

            Console.WriteLine("Creating list of frames...");

            //parses the directory given at the command-line to see if any of the files match the regex matchingPattern
            //regardless, puts the results into an array (length 0-n)
            string[] files = Directory.EnumerateFiles(args[0]).Where(id => Regex.Match(id.Substring(id.LastIndexOf('\\') + 1), matchingPattern).Success).ToArray();

            //Did we actually get files matching our regex?
            switch(files.Length)
            {
                case 0:
                    throw new FileNotFoundException("No output files were found.");
            }

            //Sanity check to ensure files are MS-DOS 8.3 character-compliant
            /*
            if(files[0] > 12)
            {
                throw new FormatException(string.Format("File {0} is {1} characters long. Files must be at most 12 characters long, including the period and file extension.",files[0], files[0].Length));
            }
            */

            Array.Sort(files);//just in case the array is out-of-order

            Dithering1.Dithering1 ditherMethod = null;


            switch(args[1].ToUpper())
            {
                case "CGA":
                    ditherMethod = new Dithering1.Dithering1(Palettes.PalTypes.CGA);
                    break;
                case "TANDY":
                    ditherMethod = new Dithering1.Dithering1(Palettes.PalTypes.TANDY);
                    break;
                case "HERC":
                    ditherMethod = new Dithering1.Dithering1(Palettes.PalTypes.HERC);
                    break;
                default:
                    throw new ArgumentException("No valid color type detected. Valid types include CGA, TANDY, and HERC.");
            }

            int numOfFrames = files.Length;

            //generate a new TimeSpan using the average MS per frame, the # of files, and the numbers of system ticks per millisecond
            TimeSpan estTime = new TimeSpan(TimeSpan.TicksPerMillisecond * avgMSperFrame * files.Length);

            Console.WriteLine("Converting frames for XDC...");
            Console.WriteLine("Estimated time for converting {0} frames: {1}", files.Length, estTime.ToString());

            Stopwatch timer = new Stopwatch();

            timer.Start();
            Parallel.For(0, files.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 }, i =>
            {
                //load a new image per frame
                Bitmap image = new Bitmap(files[i]);
                Stopwatch frameTimer = new Stopwatch();

                frameTimer.Start();
                Bitmap result = ditherMethod.DitherImage(image);
                frameTimer.Stop();

                //dispose the source image
                image.Dispose();

                switch (verbosity)
                {
                    case true:
                        //Output the total time taken to dither the frame
                        Console.WriteLine(string.Format("Frame {0} of {1} was successfully dithered in {2}", i + 1, numOfFrames, frameTimer.Elapsed.ToString()));
                        break;
                    case false:
                        UpdateTime(numOfFrames);
                        break;
                }

                //delete original file
                File.Delete(files[i]);

                //save new version
                result.Save(files[i],ImageFormat.Bmp);

                result.Dispose();
            });
            timer.Stop();

            //alert the user to successful completion
            SystemSounds.Exclamation.Play();

            //output the total time taken to dither the image sequence
            Console.WriteLine("Conversion completed in {0}", timer.Elapsed.ToString());
        }

        private static void UpdateTime(int maxFrames)
        {
            currentFrames++;
            Console.WriteLine(currentFrames + " frames processed; " + ((float)currentFrames / (float)maxFrames) * 100 + "%");
        }

    }
}
