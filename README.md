# ComfyUI Python Client

A Python client for generating images using ComfyUI's REST and WebSocket API. This script allows you to generate images from text prompts using a remote ComfyUI server.

## Features

-   Text-to-image generation via ComfyUI API
-   Customizable generation parameters (size, steps, CFG scale, etc.)
-   Command-line interface
-   Python class for integration into other projects
-   Automatic image saving with organized filenames

## Installation

1. Install the required dependencies:

```bash
pip install -r requirements.txt
```

Or install them manually:

```bash
pip install websocket-client==1.8.0 Pillow==10.4.0
```

## Usage

### Command Line Interface

Generate a single image from the command line:

```bash
python comfyui_client.py "a beautiful sunset over mountains"
```

With custom parameters:

```bash
python comfyui_client.py "a cute cat" --width 768 --height 768 --steps 30 --cfg 7.5
```

All available options:

```bash
python comfyui_client.py "your prompt here" \
    --server "your-server-url:port" \
    --negative "things you don't want" \
    --width 512 \
    --height 512 \
    --steps 20 \
    --cfg 8.0 \
    --seed 12345 \
    --output "output_directory"
```

### Python API

Use the client in your own Python scripts:

```python
from comfyui_client import ComfyUIClient

# Initialize client with your server URL
client = ComfyUIClient("http://your-server:7860")

# Generate an image
saved_files = client.generate_image(
    positive_prompt="a beautiful landscape",
    negative_prompt="blurry, low quality",
    width=512,
    height=512,
    steps=20,
    cfg_scale=7.5,
    output_dir="my_images"
)

print(f"Generated images: {saved_files}")
```

### Example Script

Run the example script to generate multiple images:

```bash
python example_usage.py
```

## Configuration

### Server URL

The default server URL is set to your provided ComfyUI server:
`http://internal-a33139e0a38fd4765b058680f226dba3-2020099933.us-east-1.elb.amazonaws.com:7860`

You can override this by:

-   Using the `--server` flag in CLI
-   Passing a different URL to the `ComfyUIClient` constructor

### Default Model

The script uses `v1-5-pruned-emaonly.safetensors` as the default model. If your server has different models available, you can modify the `checkpoint` parameter in the `create_text2img_workflow` method.

## Parameters

| Parameter         | Description                       | Default                          |
| ----------------- | --------------------------------- | -------------------------------- |
| `positive_prompt` | Text description of desired image | Required                         |
| `negative_prompt` | What to avoid in the image        | "bad hands, blurry, low quality" |
| `width`           | Image width in pixels             | 512                              |
| `height`          | Image height in pixels            | 512                              |
| `steps`           | Number of sampling steps          | 20                               |
| `cfg_scale`       | CFG scale for prompt adherence    | 8.0                              |
| `seed`            | Random seed (None for random)     | None                             |
| `output_dir`      | Directory to save images          | "generated_images"               |

## API Endpoints Used

The client uses these ComfyUI API endpoints:

-   `POST /prompt` - Queue generation requests
-   `GET /view` - Download generated images
-   `GET /history/{prompt_id}` - Get generation results
-   `WebSocket /ws` - Real-time status updates

## Workflow Structure

The script creates a basic text-to-image workflow with these nodes:

1. **CheckpointLoaderSimple** - Loads the AI model
2. **CLIPTextEncode** - Encodes positive and negative prompts
3. **EmptyLatentImage** - Creates initial latent space
4. **KSampler** - Performs the generation sampling
5. **VAEDecode** - Decodes latent to image
6. **SaveImage** - Saves the final image

## Error Handling

The script includes error handling for:

-   Network connection issues
-   Invalid server responses
-   WebSocket connection problems
-   File saving errors

## Troubleshooting

### Common Issues

1. **Connection refused**: Check if your ComfyUI server is running and accessible
2. **Model not found**: Ensure the model file exists on your server
3. **WebSocket timeout**: Server might be overloaded, try again later
4. **Memory errors**: Reduce image size or number of steps

### Testing Connection

Test if your server is accessible:

```bash
curl http://your-server:7860/
```

## File Structure

```
├── comfyui_client.py    # Main client class and CLI
├── example_usage.py     # Example usage script
├── requirements.txt     # Python dependencies
├── README.md           # This file
└── generated_images/   # Default output directory (created automatically)
```

## Dependencies

-   **websocket-client**: For real-time communication with ComfyUI
-   **Pillow (PIL)**: For image processing and saving
-   **urllib** and **json**: Built-in Python libraries for HTTP requests

## License

This project is based on the official ComfyUI API examples and is intended for educational and personal use.
