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
            Console.WriteLine("  DeOldify.exe <input_file1> <input_file2> ... [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <input_file>   Path to input black and white image(s)");
            Console.WriteLine("  [output_file]  Path to save colorized output image (optional, single file only)");
            Console.WriteLine("                 If not specified, saves as <input>-colorized.<ext>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help     Show this help message");
            Console.WriteLine("  -o <dir>       Output directory for batch processing");
            Console.WriteLine();
            Console.WriteLine("Supported formats:");
            Console.WriteLine("  Input:  BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF");
            Console.WriteLine("  Output: BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Single file:");
            Console.WriteLine("    DeOldify.exe input.jpg");
            Console.WriteLine("    DeOldify.exe input.jpg output.png");
            Console.WriteLine();
            Console.WriteLine("  Batch processing:");
            Console.WriteLine("    DeOldify.exe photo1.jpg photo2.jpg photo3.jpg");
            Console.WriteLine("    DeOldify.exe *.jpg -o colorized_output");
            Console.WriteLine("    DeOldify.exe image1.png image2.png -o results");
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
        /// Generates output path for a file.
        /// </summary>
        /// <param name="inputPath">Input file path.</param>
        /// <param name="outputDir">Output directory (optional).</param>
        /// <returns>Output file path.</returns>
        private static string GenerateOutputPath(string inputPath, string outputDir)
        {
            var directory = string.IsNullOrEmpty(outputDir) ? Path.GetDirectoryName(inputPath) : outputDir;
            var filename = Path.GetFileNameWithoutExtension(inputPath);
            var extension = Path.GetExtension(inputPath);
            
            if(string.IsNullOrEmpty(directory))
            {
                directory = ".";
            }
            
            var outputPath = Path.Combine(directory, filename + "-colorized" + extension);
            
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
            
            return outputPath;
        }

        /// <summary>
        /// Processes a single image file.
        /// </summary>
        /// <param name="inputPath">Input file path.</param>
        /// <param name="outputPath">Output file path.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private static bool ProcessImage(string inputPath, string outputPath)
        {
            // Validate input file exists
            if(!File.Exists(inputPath))
            {
                Console.WriteLine("Error: Input file not found: " + inputPath);
                return false;
            }

            // Validate input file extension
            var inputExt = Path.GetExtension(inputPath).ToLower();
            if(string.IsNullOrEmpty(inputExt))
            {
                Console.WriteLine("Error: Input file has no extension: " + inputPath);
                return false;
            }

            // Validate output format
            ImageFormat outputFormat;
            if(!TryGetImageFormat(outputPath, out outputFormat))
            {
                var outputExt = Path.GetExtension(outputPath);
                Console.WriteLine("Error: Unsupported output format: " + outputExt);
                return false;
            }

            // Validate output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if(!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                    Console.WriteLine("Created output directory: " + outputDir);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error: Cannot create output directory: " + ex.Message);
                    return false;
                }
            }

            Bitmap inputImage = null;
            Bitmap outputImage = null;
            
            try
            {
                Console.WriteLine("Loading: " + Path.GetFileName(inputPath));
                
                try
                {
                    inputImage = new Bitmap(inputPath);
                }
                catch(ArgumentException)
                {
                    Console.WriteLine("Error: File is not a valid image: " + inputPath);
                    return false;
                }
                catch(OutOfMemoryException)
                {
                    Console.WriteLine("Error: Image is too large or corrupted: " + inputPath);
                    return false;
                }

                Console.WriteLine("  Size: " + inputImage.Width + "x" + inputImage.Height);
                
                // Check for reasonable image dimensions
                if(inputImage.Width < 10 || inputImage.Height < 10)
                {
                    Console.WriteLine("  Warning: Image is very small, results may not be optimal");
                }
                else if(inputImage.Width > 4096 || inputImage.Height > 4096)
                {
                    Console.WriteLine("  Warning: Large image, processing may take time");
                }

                Console.WriteLine("  Colorizing...");

                // Set up progress handler
                var lastProgress = -1f;
                DeOldify.Progress += (percent) =>
                {
                    var currentProgress = (int)percent;
                    if(currentProgress != (int)lastProgress && currentProgress % 10 == 0)
                    {
                        Console.Write("\r  Progress: " + currentProgress + "%");
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
                    Console.WriteLine("\r  Error: Out of memory during colorization");
                    return false;
                }

                Console.WriteLine("\r  Progress: 100%");

                // Save output
                try
                {
                    outputImage.Save(outputPath, outputFormat);
                    Console.WriteLine("  Saved: " + Path.GetFileName(outputPath));
                }
                catch(System.Runtime.InteropServices.ExternalException)
                {
                    Console.WriteLine("  Error: Failed to save (file in use or no write permission)");
                    return false;
                }
                catch(IOException ex)
                {
                    Console.WriteLine("  Error: Failed to save: " + ex.Message);
                    return false;
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("  Unexpected error: " + ex.Message);
                return false;
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

            // Parse arguments
            var inputFiles = new System.Collections.Generic.List<string>();
            string outputDir = null;
            string explicitOutput = null;
            
            for(int i = 0; i < args.Length; i++)
            {
                if(args[i] == "-o" && i + 1 < args.Length)
                {
                    outputDir = args[i + 1];
                    i++; // Skip next argument
                }
                else if(!args[i].StartsWith("-"))
                {
                    inputFiles.Add(args[i]);
                }
            }

            if(inputFiles.Count == 0)
            {
                Console.WriteLine("Error: No input files specified.");
                Console.WriteLine();
                PrintUsage();
                Environment.Exit(1);
                return;
            }

            // Check if second argument is explicit output (single file mode)
            if(inputFiles.Count == 2 && outputDir == null)
            {
                // Check if second file exists - if not, treat as output path
                if(!File.Exists(inputFiles[1]))
                {
                    explicitOutput = inputFiles[1];
                    inputFiles.RemoveAt(1);
                }
            }

            // Validate output directory if specified
            if(outputDir != null && !Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                    Console.WriteLine("Created output directory: " + outputDir);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error: Cannot create output directory: " + ex.Message);
                    Environment.Exit(1);
                    return;
                }
            }

            // Process files
            var startTime = DateTime.Now;
            var successCount = 0;
            var failCount = 0;

            Console.WriteLine("DeOldify.NET - Batch Processing");
            Console.WriteLine("================================");
            Console.WriteLine("Files to process: " + inputFiles.Count);
            Console.WriteLine();

            for(int i = 0; i < inputFiles.Count; i++)
            {
                var inputPath = inputFiles[i];
                Console.WriteLine("[" + (i + 1) + "/" + inputFiles.Count + "] " + Path.GetFileName(inputPath));
                
                string outputPath;
                if(explicitOutput != null && inputFiles.Count == 1)
                {
                    outputPath = explicitOutput;
                }
                else
                {
                    outputPath = GenerateOutputPath(inputPath, outputDir);
                }

                var success = ProcessImage(inputPath, outputPath);
                
                if(success)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }

                Console.WriteLine();
                
                // Force garbage collection between files to free memory
                if(inputFiles.Count > 1)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            // Summary
            var elapsed = DateTime.Now - startTime;
            Console.WriteLine("================================");
            Console.WriteLine("Batch processing complete!");
            Console.WriteLine("  Successful: " + successCount);
            Console.WriteLine("  Failed: " + failCount);
            Console.WriteLine("  Total time: " + elapsed.ToString(@"hh\:mm\:ss"));
            
            if(failCount > 0)
            {
                Environment.Exit(1);
            }
        }

    }

}
