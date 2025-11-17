# DeOldify.NET - Command Line Interface

The CLI mode has been added to DeOldify.NET, allowing you to colorize images from the command line without opening the GUI.

## Usage

### Basic Usage
```bash
DeOldify.exe <input_file> [output_file]
```

The output file is optional. If not specified, the colorized image will be saved in the same directory as the input with a `-colorized` suffix.

### Examples

**Command Line:**

Colorize with automatic output naming:
```bash
DeOldify.exe old_photo.jpg
# Creates: old_photo-colorized.jpg
# If file exists: old_photo-colorized-1.jpg, old_photo-colorized-2.jpg, etc.
```

Specify custom output path:
```bash
DeOldify.exe old_photo.jpg colorized.png
```

Process a BMP file and save as JPEG:
```bash
DeOldify.exe input.bmp output.jpg
```

**Drag & Drop (Windows):**

Simply drag and drop an image file onto the DeOldify.exe icon. The colorized image will be saved in the same folder with a `-colorized` suffix. A console window will appear showing the progress.

### Help
Display help information:
```bash
DeOldify.exe --help
```

## Supported Formats

**Input formats:** BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF

**Output formats:** BMP, EMF, EXIF, GIF, ICO, JPG, PNG, TIFF, WMF

## Features

- **Drag & Drop support**: On Windows, drag an image onto the .exe to colorize it instantly
- **Progress indication**: Shows colorization progress in the console
- **Automatic format detection**: Output format is determined by file extension
- **Smart file naming**: Automatically avoids overwriting existing files by adding numbers (e.g., `-colorized-1`, `-colorized-2`)
- **Error handling**: Clear error messages for missing files or invalid inputs
- **No GUI required**: Perfect for batch processing and automation

## GUI Mode

To launch the GUI, simply run the executable without any arguments:
```bash
DeOldify.exe
```

## Notes

- The CLI mode uses the same neural network model as the GUI version
- Processing time depends on image size and your hardware
- At least 1.5 GB of free RAM is required for Artistic model, ~3 GB for Stable model
- The program will automatically convert color images to grayscale before colorization
