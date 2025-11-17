# Integration with other programs

I added some code to integrate with other programs, such as Python scripts and Django projects. This allows you to easily use DeOldify.NET in your existing workflows.

## Features for Python Integration

- **JSON output mode** - Machine-readable structured output
- **Quiet mode** - Suppress console output for cleaner logs
- **Status file** - Monitor progress in real-time
- **Exit codes** - Proper status codes (0 = success, 1 = failure)
- **Batch processing** - Process multiple files efficiently

## Basic Python Example

```python
import subprocess
import json

def colorize_image(input_path, output_path=None):
    """Colorize a single image using DeOldify.NET"""
    cmd = ['DeOldify.exe', input_path, '--json']
    
    if output_path:
        cmd.insert(2, output_path)
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    
    if result.returncode == 0:
        data = json.loads(result.stdout)
        return data['results'][0]
    else:
        raise Exception(f"Colorization failed: {result.stderr}")

# Usage
result = colorize_image('old_photo.jpg')
print(f"Success! Output: {result['output']}")
print(f"Processing time: {result['processing_time_seconds']}s")
```

## Django Integration Example

### views.py

```python
from django.shortcuts import render
from django.http import JsonResponse
from django.core.files.storage import default_storage
import subprocess
import json
import os

def colorize_upload(request):
    if request.method == 'POST' and request.FILES.get('image'):
        # Save uploaded file
        uploaded_file = request.FILES['image']
        input_path = default_storage.save(f'uploads/{uploaded_file.name}', uploaded_file)
        input_full_path = default_storage.path(input_path)
        
        # Prepare output path
        output_dir = default_storage.path('colorized')
        os.makedirs(output_dir, exist_ok=True)
        
        # Run DeOldify.NET
        cmd = [
            'DeOldify.exe',
            input_full_path,
            '--json',
            '-o', output_dir
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode == 0:
            data = json.loads(result.stdout)
            file_result = data['results'][0]
            
            if file_result['success']:
                # Get relative path for serving
                output_relative = os.path.relpath(
                    file_result['output'],
                    default_storage.location
                )
                
                return JsonResponse({
                    'success': True,
                    'output_url': default_storage.url(output_relative),
                    'width': file_result['width'],
                    'height': file_result['height'],
                    'processing_time': file_result['processing_time_seconds']
                })
            else:
                return JsonResponse({
                    'success': False,
                    'error': file_result['error']
                }, status=400)
        else:
            return JsonResponse({
                'success': False,
                'error': 'Colorization process failed'
            }, status=500)
    
    return render(request, 'colorize_form.html')
```

### Celery Task (Async Processing)

```python
from celery import shared_task
import subprocess
import json

@shared_task
def colorize_image_async(input_path, output_path):
    """Async task for colorizing images"""
    cmd = [
        'DeOldify.exe',
        input_path,
        output_path,
        '--json',
        '--quiet'
    ]
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    
    if result.returncode == 0:
        data = json.loads(result.stdout)
        return data['results'][0]
    else:
        raise Exception(f"Colorization failed")
```

### Progress Monitoring with Status File

```python
import subprocess
import time
import os

def colorize_with_progress(input_path, callback=None):
    """Colorize with real-time progress updates"""
    status_file = f'/tmp/deoldify_status_{os.getpid()}.txt'
    
    cmd = [
        'DeOldify.exe',
        input_path,
        '--json',
        '--status', status_file
    ]
    
    # Start process
    process = subprocess.Popen(cmd, stdout=subprocess.PIPE, text=True)
    
    # Monitor progress
    while process.poll() is None:
        if os.path.exists(status_file):
            with open(status_file, 'r') as f:
                status = f.read().strip()
                if callback:
                    callback(status)
        time.sleep(0.5)
    
    # Get result
    stdout, _ = process.communicate()
    
    # Clean up status file
    if os.path.exists(status_file):
        os.remove(status_file)
    
    if process.returncode == 0:
        return json.loads(stdout)
    else:
        raise Exception("Colorization failed")

# Usage with progress callback
def progress_callback(status):
    if status.startswith('processing:'):
        percent = status.split(':')[1]
        print(f"Progress: {percent}%")
    else:
        print(f"Status: {status}")

result = colorize_with_progress('photo.jpg', progress_callback)
```

## Batch Processing Example

```python
import subprocess
import json
import glob

def colorize_batch(pattern, output_dir):
    """Colorize multiple images matching a pattern"""
    files = glob.glob(pattern)
    
    cmd = ['DeOldify.exe', '--json', '-o', output_dir] + files
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    
    if result.returncode == 0:
        data = json.loads(result.stdout)
        return data
    else:
        raise Exception("Batch processing failed")

# Usage
results = colorize_batch('photos/*.jpg', 'colorized_output')
print(f"Processed {results['successful']} images successfully")
print(f"Failed: {results['failed']}")
print(f"Total time: {results['processing_time_seconds']}s")
```

## JSON Output Format

```json
{
  "success": true,
  "total": 2,
  "successful": 2,
  "failed": 0,
  "processing_time_seconds": 45.32,
  "results": [
    {
      "input": "photo1.jpg",
      "output": "photo1-colorized.jpg",
      "success": true,
      "width": 1920,
      "height": 1080,
      "processing_time_seconds": 22.15
    },
    {
      "input": "photo2.jpg",
      "output": "photo2-colorized.jpg",
      "success": true,
      "width": 1280,
      "height": 720,
      "processing_time_seconds": 23.17
    }
  ]
}
```

## Error Handling

```json
{
  "success": false,
  "total": 1,
  "successful": 0,
  "failed": 1,
  "processing_time_seconds": 0.12,
  "results": [
    {
      "input": "invalid.jpg",
      "output": "invalid-colorized.jpg",
      "success": false,
      "error": "File is not a valid image"
    }
  ]
}
```

## Status File Format

The status file contains simple text status updates:
- `loading` - Loading input image
- `processing` - Starting colorization
- `processing:25` - 25% complete
- `processing:50` - 50% complete
- `processing:75` - 75% complete
- `saving` - Saving output
- `complete` - Done

## Command Line Options

```bash
# JSON output (for Python parsing)
DeOldify.exe input.jpg --json

# Quiet mode (no console output)
DeOldify.exe input.jpg --quiet

# Status file for progress monitoring
DeOldify.exe input.jpg --status /tmp/status.txt

# Batch with output directory
DeOldify.exe *.jpg --json -o results

# Combined options
DeOldify.exe photo.jpg --json --status /tmp/status.txt -o output
```

## Best Practices

1. **Use JSON mode** for parsing output in Python
2. **Use quiet mode** to avoid console clutter in logs
3. **Check exit codes** - 0 for success, 1 for failure
4. **Use status files** for long-running operations
5. **Handle exceptions** - subprocess can raise various errors
6. **Clean up temp files** - Remove status files after use
7. **Use absolute paths** - Avoid relative path issues
8. **Validate input** - Check file exists before calling
9. **Set timeouts** - Use subprocess timeout parameter
10. **Use async tasks** - For web applications, use Celery or similar

## Performance Tips

- Process multiple images in one batch call instead of multiple single calls
- Use the `-o` flag to organize outputs
- Monitor memory usage for large batches
- Consider splitting very large batches into smaller chunks
