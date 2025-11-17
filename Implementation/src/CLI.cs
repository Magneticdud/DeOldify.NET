//*************************************************************************************************
//* (C) ColorfulSoft corp., 2021 - 2022. All Rights reserved.
//*************************************************************************************************

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ColorfulSoft.DeOldify
{

    /// <summary>
    /// Command-line interface handler.
    /// </summary>
    public static class CLI
    {
        /// <summary>
        /// CLI options.
        /// </summary>
        private class Options
        {
            public bool Quiet = false;
            public bool JsonOutput = false;
            public string StatusFile = null;
            public string OutputDir = null;
        }

        /// <summary>
        /// Result of processing a single file.
        /// </summary>
        private class ProcessResult
        {
            public string InputPath;
            public string OutputPath;
            public bool Success;
            public string Error;
            public int Width;
            public int Height;
            public double ProcessingTimeSeconds;
        }

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
            Console.WriteLine("  -q, --quiet    Quiet mode (minimal output)");
            Console.WriteLine("  -j, --json     Output results in JSON format");
            Console.WriteLine("  --status <f>   Write progress to status file");
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
        /// Writes status to file if specified.
        /// </summary>
        /// <param name="options">CLI options.</param>
        /// <param name="message">Status message.</param>
        private static void WriteStatus(Options options, string message)
        {
            if(options.StatusFile != null)
            {
                try
                {
                    File.WriteAllText(options.StatusFile, message);
                }
                catch
                {
                    // Ignore status file write errors
                }
            }
        }

        /// <summary>
        /// Processes a single image file.
        /// </summary>
        /// <param name="inputPath">Input file path.</param>
        /// <param name="outputPath">Output file path.</param>
        /// <param name="options">CLI options.</param>
        /// <returns>Process result.</returns>
        private static ProcessResult ProcessImage(string inputPath, string outputPath, Options options)
        {
            var result = new ProcessResult
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                Success = false
            };

            var startTime = DateTime.Now;

            // Validate input file exists
            if(!File.Exists(inputPath))
            {
                result.Error = "Input file not found";
                if(!options.Quiet) Console.WriteLine("Error: Input file not found: " + inputPath);
                return result;
            }

            // Validate input file extension
            var inputExt = Path.GetExtension(inputPath).ToLower();
            if(string.IsNullOrEmpty(inputExt))
            {
                result.Error = "Input file has no extension";
                if(!options.Quiet) Console.WriteLine("Error: Input file has no extension: " + inputPath);
                return result;
            }

            // Validate output format
            ImageFormat outputFormat;
            if(!TryGetImageFormat(outputPath, out outputFormat))
            {
                result.Error = "Unsupported output format";
                if(!options.Quiet) Console.WriteLine("Error: Unsupported output format: " + Path.GetExtension(outputPath));
                return result;
            }

            // Validate output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if(!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                    if(!options.Quiet) Console.WriteLine("Created output directory: " + outputDir);
                }
                catch(Exception ex)
                {
                    result.Error = "Cannot create output directory: " + ex.Message;
                    if(!options.Quiet) Console.WriteLine("Error: " + result.Error);
                    return result;
                }
            }

            Bitmap inputImage = null;
            Bitmap outputImage = null;
            
            try
            {
                if(!options.Quiet) Console.WriteLine("Loading: " + Path.GetFileName(inputPath));
                WriteStatus(options, "loading");
                
                try
                {
                    inputImage = new Bitmap(inputPath);
                }
                catch(ArgumentException)
                {
                    result.Error = "File is not a valid image";
                    if(!options.Quiet) Console.WriteLine("Error: File is not a valid image: " + inputPath);
                    return result;
                }
                catch(OutOfMemoryException)
                {
                    result.Error = "Image is too large or corrupted";
                    if(!options.Quiet) Console.WriteLine("Error: Image is too large or corrupted: " + inputPath);
                    return result;
                }

                result.Width = inputImage.Width;
                result.Height = inputImage.Height;

                if(!options.Quiet) Console.WriteLine("  Size: " + inputImage.Width + "x" + inputImage.Height);
                
                // Check for reasonable image dimensions
                if(inputImage.Width < 10 || inputImage.Height < 10)
                {
                    if(!options.Quiet) Console.WriteLine("  Warning: Image is very small, results may not be optimal");
                }
                else if(inputImage.Width > 4096 || inputImage.Height > 4096)
                {
                    if(!options.Quiet) Console.WriteLine("  Warning: Large image, processing may take time");
                }

                if(!options.Quiet) Console.WriteLine("  Colorizing...");
                WriteStatus(options, "processing");

                // Set up progress handler
                var lastProgress = -1f;
                DeOldify.Progress += (percent) =>
                {
                    var currentProgress = (int)percent;
                    if(currentProgress != (int)lastProgress && currentProgress % 10 == 0)
                    {
                        WriteStatus(options, "processing:" + currentProgress);
                        if(!options.Quiet) Console.Write("\r  Progress: " + currentProgress + "%");
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
                    result.Error = "Out of memory during colorization";
                    if(!options.Quiet) Console.WriteLine("\r  Error: Out of memory during colorization");
                    return result;
                }

                if(!options.Quiet) Console.WriteLine("\r  Progress: 100%");
                WriteStatus(options, "saving");

                // Save output
                try
                {
                    outputImage.Save(outputPath, outputFormat);
                    if(!options.Quiet) Console.WriteLine("  Saved: " + Path.GetFileName(outputPath));
                }
                catch(System.Runtime.InteropServices.ExternalException)
                {
                    result.Error = "Failed to save (file in use or no write permission)";
                    if(!options.Quiet) Console.WriteLine("  Error: " + result.Error);
                    return result;
                }
                catch(IOException ex)
                {
                    result.Error = "Failed to save: " + ex.Message;
                    if(!options.Quiet) Console.WriteLine("  Error: " + result.Error);
                    return result;
                }

                result.Success = true;
                result.ProcessingTimeSeconds = (DateTime.Now - startTime).TotalSeconds;
                WriteStatus(options, "complete");
                return result;
            }
            catch(Exception ex)
            {
                result.Error = "Unexpected error: " + ex.Message;
                if(!options.Quiet) Console.WriteLine("  Unexpected error: " + ex.Message);
                return result;
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
            var options = new Options();
            string explicitOutput = null;
            
            for(int i = 0; i < args.Length; i++)
            {
                if(args[i] == "-o" && i + 1 < args.Length)
                {
                    options.OutputDir = args[i + 1];
                    i++; // Skip next argument
                }
                else if(args[i] == "-q" || args[i] == "--quiet")
                {
                    options.Quiet = true;
                }
                else if(args[i] == "-j" || args[i] == "--json")
                {
                    options.JsonOutput = true;
                    options.Quiet = true; // JSON mode implies quiet
                }
                else if(args[i] == "--status" && i + 1 < args.Length)
                {
                    options.StatusFile = args[i + 1];
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
            if(inputFiles.Count == 2 && options.OutputDir == null)
            {
                // Check if second file exists - if not, treat as output path
                if(!File.Exists(inputFiles[1]))
                {
                    explicitOutput = inputFiles[1];
                    inputFiles.RemoveAt(1);
                }
            }

            // Validate output directory if specified
            if(options.OutputDir != null && !Directory.Exists(options.OutputDir))
            {
                try
                {
                    Directory.CreateDirectory(options.OutputDir);
                    if(!options.Quiet) Console.WriteLine("Created output directory: " + options.OutputDir);
                }
                catch(Exception ex)
                {
                    if(!options.Quiet) Console.WriteLine("Error: Cannot create output directory: " + ex.Message);
                    if(options.JsonOutput)
                    {
                        Console.WriteLine("{\"error\": \"Cannot create output directory: " + ex.Message.Replace("\"", "\\\"") + "\"}");
                    }
                    Environment.Exit(1);
                    return;
                }
            }

            // Process files
            var startTime = DateTime.Now;
            var results = new System.Collections.Generic.List<ProcessResult>();

            if(!options.Quiet)
            {
                Console.WriteLine("DeOldify.NET - Batch Processing");
                Console.WriteLine("================================");
                Console.WriteLine("Files to process: " + inputFiles.Count);
                Console.WriteLine();
            }

            for(int i = 0; i < inputFiles.Count; i++)
            {
                var inputPath = inputFiles[i];
                if(!options.Quiet) Console.WriteLine("[" + (i + 1) + "/" + inputFiles.Count + "] " + Path.GetFileName(inputPath));
                
                string outputPath;
                if(explicitOutput != null && inputFiles.Count == 1)
                {
                    outputPath = explicitOutput;
                }
                else
                {
                    outputPath = GenerateOutputPath(inputPath, options.OutputDir);
                }

                var result = ProcessImage(inputPath, outputPath, options);
                results.Add(result);

                if(!options.Quiet) Console.WriteLine();
                
                // Force garbage collection between files to free memory
                if(inputFiles.Count > 1)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            // Output results
            var elapsed = DateTime.Now - startTime;
            var successCount = results.FindAll(r => r.Success).Count;
            var failCount = results.Count - successCount;

            if(options.JsonOutput)
            {
                // JSON output
                var json = new StringBuilder();
                json.Append("{");
                json.Append("\"success\": " + (failCount == 0 ? "true" : "false") + ",");
                json.Append("\"total\": " + results.Count + ",");
                json.Append("\"successful\": " + successCount + ",");
                json.Append("\"failed\": " + failCount + ",");
                json.Append("\"processing_time_seconds\": " + elapsed.TotalSeconds.ToString("F2") + ",");
                json.Append("\"results\": [");
                
                for(int i = 0; i < results.Count; i++)
                {
                    var r = results[i];
                    json.Append("{");
                    json.Append("\"input\": \"" + r.InputPath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\",");
                    json.Append("\"output\": \"" + r.OutputPath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\",");
                    json.Append("\"success\": " + (r.Success ? "true" : "false") + ",");
                    if(r.Success)
                    {
                        json.Append("\"width\": " + r.Width + ",");
                        json.Append("\"height\": " + r.Height + ",");
                        json.Append("\"processing_time_seconds\": " + r.ProcessingTimeSeconds.ToString("F2"));
                    }
                    else
                    {
                        json.Append("\"error\": \"" + (r.Error ?? "Unknown error").Replace("\"", "\\\"") + "\"");
                    }
                    json.Append("}");
                    if(i < results.Count - 1) json.Append(",");
                }
                
                json.Append("]}");
                Console.WriteLine(json.ToString());
            }
            else if(!options.Quiet)
            {
                // Human-readable summary
                Console.WriteLine("================================");
                Console.WriteLine("Batch processing complete!");
                Console.WriteLine("  Successful: " + successCount);
                Console.WriteLine("  Failed: " + failCount);
                Console.WriteLine("  Total time: " + elapsed.ToString(@"hh\:mm\:ss"));
            }
            
            // Clean up status file
            if(options.StatusFile != null)
            {
                try
                {
                    File.Delete(options.StatusFile);
                }
                catch
                {
                    // Ignore
                }
            }
            
            if(failCount > 0)
            {
                Environment.Exit(1);
            }
        }

    }

}
