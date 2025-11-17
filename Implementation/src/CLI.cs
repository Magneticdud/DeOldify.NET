//*************************************************************************************************
//* (C) ColorfulSoft corp., 2021 - 2022. All Rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ColorfulSoft.DeOldify
{

    /// <summary>
    /// Command-line interface handler.
    /// </summary>
    public static class CLI
    {

        /// <summary>
        /// Prints usage information.
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("DeOldify.NET - Command Line Interface");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  DeOldify.exe <input_file> [output_file] [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <input_file>   Path to input black and white image");
            Console.WriteLine("  [output_file]  Path to save colorized output image (optional)");
            Console.WriteLine("                 If not specified, saves as <input>-colorized.<ext>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help     Show this help message");
            Console.WriteLine();
            Console.WriteLine("Supported formats:");
            Console.WriteLine("  Input:  BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF");
            Console.WriteLine("  Output: BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  DeOldify.exe input.jpg");
            Console.WriteLine("  DeOldify.exe input.jpg output.png");
            Console.WriteLine("  DeOldify.exe old_photo.bmp colorized.jpg");
        }

        /// <summary>
        /// Gets image format from file extension.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="format">Output image format.</param>
        /// <returns>True if format is supported, false otherwise.</returns>
        private static bool TryGetImageFormat(string filename, out ImageFormat format)
        {
            var ext = Path.GetExtension(filename).ToLower();
            switch(ext)
            {
                case ".bmp":
                    format = ImageFormat.Bmp;
                    return true;
                case ".emf":
                    format = ImageFormat.Emf;
                    return true;
                case ".exif":
                    format = ImageFormat.Exif;
                    return true;
                case ".gif":
                    format = ImageFormat.Gif;
                    return true;
                case ".ico":
                    format = ImageFormat.Icon;
                    return true;
                case ".jpg":
                case ".jpeg":
                    format = ImageFormat.Jpeg;
                    return true;
                case ".png":
                    format = ImageFormat.Png;
                    return true;
                case ".tiff":
                case ".tif":
                    format = ImageFormat.Tiff;
                    return true;
                case ".wmf":
                    format = ImageFormat.Wmf;
                    return true;
                default:
                    format = null;
                    return false;
            }
        }

        /// <summary>
        /// Runs CLI mode.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Run(string[] args)
        {
            // Check for help flag
            if(args.Length >= 1 && (args[0] == "-h" || args[0] == "--help"))
            {
                PrintUsage();
                return;
            }

            // Validate arguments
            if(args.Length < 1)
            {
                Console.WriteLine("Error: Missing required input file argument.");
                Console.WriteLine();
                PrintUsage();
                Environment.Exit(1);
                return;
            }

            var inputPath = args[0];
            
            // Generate default output path if not specified
            string outputPath;
            if(args.Length >= 2)
            {
                outputPath = args[1];
            }
            else
            {
                var directory = Path.GetDirectoryName(inputPath);
                var filename = Path.GetFileNameWithoutExtension(inputPath);
                var extension = Path.GetExtension(inputPath);
                
                if(string.IsNullOrEmpty(directory))
                {
                    directory = ".";
                }
                
                outputPath = Path.Combine(directory, filename + "-colorized" + extension);
                
                // Check if file exists and add number if needed
                if(File.Exists(outputPath))
                {
                    var counter = 1;
                    string numberedPath;
                    do
                    {
                        numberedPath = Path.Combine(directory, filename + "-colorized-" + counter + extension);
                        counter++;
                    }
                    while(File.Exists(numberedPath));
                    
                    outputPath = numberedPath;
                }
            }

            // Validate input file exists
            if(!File.Exists(inputPath))
            {
                Console.WriteLine("Error: Input file not found: " + inputPath);
                Console.WriteLine();
                Console.WriteLine("Please check that:");
                Console.WriteLine("  - The file path is correct");
                Console.WriteLine("  - The file exists");
                Console.WriteLine("  - You have permission to read the file");
                Environment.Exit(1);
                return;
            }

            // Validate input file extension
            var inputExt = Path.GetExtension(inputPath).ToLower();
            if(string.IsNullOrEmpty(inputExt))
            {
                Console.WriteLine("Error: Input file has no extension: " + inputPath);
                Console.WriteLine("Supported formats: BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF");
                Environment.Exit(1);
                return;
            }

            // Validate output format
            ImageFormat outputFormat;
            if(!TryGetImageFormat(outputPath, out outputFormat))
            {
                var outputExt = Path.GetExtension(outputPath);
                Console.WriteLine("Error: Unsupported output format: " + outputExt);
                Console.WriteLine("Supported formats: BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF");
                Console.WriteLine();
                Console.WriteLine("Tip: Change the output file extension to a supported format.");
                Environment.Exit(1);
                return;
            }

            // Validate output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if(!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Console.WriteLine("Error: Output directory does not exist: " + outputDir);
                Console.WriteLine();
                Console.WriteLine("Please create the directory first or specify a different output path.");
                Environment.Exit(1);
                return;
            }

            Bitmap inputImage = null;
            Bitmap outputImage = null;
            
            try
            {
                Console.WriteLine("Loading input image: " + inputPath);
                
                try
                {
                    inputImage = new Bitmap(inputPath);
                }
                catch(ArgumentException)
                {
                    Console.WriteLine("Error: File is not a valid image: " + inputPath);
                    Console.WriteLine("Please ensure the file is a valid image in a supported format.");
                    Environment.Exit(1);
                    return;
                }
                catch(OutOfMemoryException)
                {
                    Console.WriteLine("Error: Image is too large or corrupted: " + inputPath);
                    Console.WriteLine("Try using a smaller image or check if the file is corrupted.");
                    Environment.Exit(1);
                    return;
                }

                Console.WriteLine("Image size: " + inputImage.Width + "x" + inputImage.Height);
                
                // Check for reasonable image dimensions
                if(inputImage.Width < 10 || inputImage.Height < 10)
                {
                    Console.WriteLine("Warning: Image is very small (" + inputImage.Width + "x" + inputImage.Height + ")");
                    Console.WriteLine("Results may not be optimal for very small images.");
                }
                else if(inputImage.Width > 4096 || inputImage.Height > 4096)
                {
                    Console.WriteLine("Warning: Image is very large (" + inputImage.Width + "x" + inputImage.Height + ")");
                    Console.WriteLine("Processing may take a long time and require significant memory.");
                }

                Console.WriteLine("Starting colorization...");

                // Set up progress handler
                var lastProgress = -1f;
                DeOldify.Progress += (percent) =>
                {
                    var currentProgress = (int)percent;
                    if(currentProgress != (int)lastProgress && currentProgress % 5 == 0)
                    {
                        Console.Write("\rProgress: " + currentProgress + "%");
                        lastProgress = percent;
                    }
                };

                // Colorize
                try
                {
                    outputImage = DeOldify.Colorize(inputImage);
                }
                catch(OutOfMemoryException)
                {
                    Console.WriteLine("\rError: Out of memory during colorization.");
                    Console.WriteLine("The image may be too large. Try:");
                    Console.WriteLine("  - Using a smaller image");
                    Console.WriteLine("  - Closing other applications to free up memory");
                    Console.WriteLine("  - Using a system with more RAM");
                    Environment.Exit(1);
                    return;
                }

                Console.WriteLine("\rProgress: 100%");
                Console.WriteLine("Colorization complete!");

                // Save output
                Console.WriteLine("Saving output image: " + outputPath);
                
                try
                {
                    outputImage.Save(outputPath, outputFormat);
                }
                catch(System.Runtime.InteropServices.ExternalException)
                {
                    Console.WriteLine("Error: Failed to save image. The file may be in use or you may lack write permissions.");
                    Environment.Exit(1);
                    return;
                }
                catch(IOException ex)
                {
                    Console.WriteLine("Error: Failed to save image: " + ex.Message);
                    Environment.Exit(1);
                    return;
                }

                Console.WriteLine("Done! Output saved to: " + outputPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Unexpected error: " + ex.Message);
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
            finally
            {
                // Clean up resources
                if(inputImage != null)
                {
                    inputImage.Dispose();
                }
                if(outputImage != null)
                {
                    outputImage.Dispose();
                }
            }
        }

    }

}
