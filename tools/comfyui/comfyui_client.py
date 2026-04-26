#!/usr/bin/env python3
"""
ComfyUI Client for Text-to-Image Generation

This script allows you to generate images from text prompts using a ComfyUI server.
Based on the official ComfyUI websockets API example.
"""

import websocket
import uuid
import json
import urllib.request
import urllib.parse
import io
import os
from PIL import Image
from typing import Dict, List, Optional
import argparse
import time
from config_loader import get_server_url


class ComfyUIClient:
    def __init__(self, server_url: str):
        """
        Initialize the ComfyUI client.
        
        Args:
            server_url: The URL of your ComfyUI server (e.g., "http://your-server:7860")
        """
        # Parse server URL and extract host:port
        if server_url.startswith('http://'):
            server_url = server_url[7:]
        elif server_url.startswith('https://'):
            server_url = server_url[8:]
        
        if server_url.endswith('/'):
            server_url = server_url[:-1]
            
        self.server_address = server_url
        self.client_id = str(uuid.uuid4())
        
    def queue_prompt(self, prompt: Dict, prompt_id: str) -> None:
        """Queue a prompt for execution on the ComfyUI server."""
        p = {"prompt": prompt, "client_id": self.client_id, "prompt_id": prompt_id}
        data = json.dumps(p).encode('utf-8')
        req = urllib.request.Request(f"http://{self.server_address}/prompt", data=data)
        urllib.request.urlopen(req).read()

    def get_image(self, filename: str, subfolder: str, folder_type: str) -> bytes:
        """Download an image from the ComfyUI server."""
        data = {"filename": filename, "subfolder": subfolder, "type": folder_type}
        url_values = urllib.parse.urlencode(data)
        with urllib.request.urlopen(f"http://{self.server_address}/view?{url_values}") as response:
            return response.read()

    def get_history(self, prompt_id: str) -> Dict:
        """Get the execution history for a prompt."""
        with urllib.request.urlopen(f"http://{self.server_address}/history/{prompt_id}") as response:
            return json.loads(response.read())

    def get_images(self, ws: websocket.WebSocket, prompt: Dict) -> Dict[str, List[bytes]]:
        """
        Execute a prompt and retrieve the generated images.
        
        Args:
            ws: WebSocket connection to the server
            prompt: The workflow prompt to execute
            
        Returns:
            Dictionary mapping node IDs to lists of image data
        """
        prompt_id = str(uuid.uuid4())
        self.queue_prompt(prompt, prompt_id)
        output_images = {}
        
        while True:
            out = ws.recv()
            if isinstance(out, str):
                message = json.loads(out)
                if message['type'] == 'executing':
                    data = message['data']
                    if data['node'] is None and data['prompt_id'] == prompt_id:
                        break  # Execution is done
            else:
                continue  # previews are binary data

        history = self.get_history(prompt_id)[prompt_id]
        for node_id in history['outputs']:
            node_output = history['outputs'][node_id]
            images_output = []
            if 'images' in node_output:
                for image in node_output['images']:
                    image_data = self.get_image(image['filename'], image['subfolder'], image['type'])
                    images_output.append(image_data)
            output_images[node_id] = images_output

        return output_images

    def create_text2img_workflow(self, 
                                positive_prompt: str, 
                                negative_prompt: str = "bad hands, blurry, low quality",
                                width: int = 512,
                                height: int = 512,
                                steps: int = 20,
                                cfg_scale: float = 8.0,
                                seed: Optional[int] = None,
                                checkpoint: str = "v1-5-pruned-emaonly.ckpt") -> Dict:
        """
        Create a basic text-to-image workflow.
        
        Args:
            positive_prompt: The text description of what you want to generate
            negative_prompt: What you don't want in the image
            width: Image width in pixels
            height: Image height in pixels
            steps: Number of sampling steps
            cfg_scale: CFG scale for prompt adherence
            seed: Random seed (None for random)
            checkpoint: Model checkpoint to use
            
        Returns:
            Workflow dictionary ready for execution
        """
        if seed is None:
            seed = int(time.time())
            
        workflow = {
            "3": {
                "class_type": "KSampler",
                "inputs": {
                    "cfg": cfg_scale,
                    "denoise": 1,
                    "latent_image": ["5", 0],
                    "model": ["4", 0],
                    "negative": ["7", 0],
                    "positive": ["6", 0],
                    "sampler_name": "euler",
                    "scheduler": "normal",
                    "seed": seed,
                    "steps": steps
                }
            },
            "4": {
                "class_type": "CheckpointLoaderSimple",
                "inputs": {
                    "ckpt_name": checkpoint
                }
            },
            "5": {
                "class_type": "EmptyLatentImage",
                "inputs": {
                    "batch_size": 1,
                    "height": height,
                    "width": width
                }
            },
            "6": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["4", 1],
                    "text": positive_prompt
                }
            },
            "7": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["4", 1],
                    "text": negative_prompt
                }
            },
            "8": {
                "class_type": "VAEDecode",
                "inputs": {
                    "samples": ["3", 0],
                    "vae": ["4", 2]
                }
            },
            "9": {
                "class_type": "SaveImage",
                "inputs": {
                    "filename_prefix": "ComfyUI",
                    "images": ["8", 0]
                }
            }
        }
        
        return workflow

    def generate_image(self, 
                      positive_prompt: str,
                      negative_prompt: str = "bad hands, blurry, low quality",
                      width: int = 512,
                      height: int = 512,
                      steps: int = 20,
                      cfg_scale: float = 8.0,
                      seed: Optional[int] = None,
                      checkpoint: str = "v1-5-pruned-emaonly.ckpt",
                      output_dir: str = "generated_images",
                      filename_prefix: str = "generated") -> List[str]:
        """
        Generate an image from a text prompt.
        
        Args:
            positive_prompt: The text description of what you want to generate
            negative_prompt: What you don't want in the image
            width: Image width in pixels
            height: Image height in pixels
            steps: Number of sampling steps
            cfg_scale: CFG scale for prompt adherence
            seed: Random seed (None for random)
            output_dir: Directory to save generated images
            filename_prefix: Prefix for saved image filenames
            
        Returns:
            List of paths to generated image files
        """
        # Create output directory if it doesn't exist
        os.makedirs(output_dir, exist_ok=True)
        
        # Create workflow
        workflow = self.create_text2img_workflow(
            positive_prompt, negative_prompt, width, height, 
            steps, cfg_scale, seed, checkpoint
        )
        
        # Connect to websocket and generate
        ws = websocket.WebSocket()
        try:
            ws.connect(f"ws://{self.server_address}/ws?clientId={self.client_id}")
            images = self.get_images(ws, workflow)
        finally:
            ws.close()
        
        # Save images and return file paths
        saved_files = []
        for node_id, image_list in images.items():
            for i, image_data in enumerate(image_list):
                # Open image and save
                image = Image.open(io.BytesIO(image_data))
                timestamp = int(time.time())
                filename = f"{filename_prefix}_{timestamp}_{node_id}_{i}.png"
                filepath = os.path.join(output_dir, filename)
                image.save(filepath)
                saved_files.append(filepath)
                print(f"Image saved: {filepath}")
        
        return saved_files


def main():
    """Command line interface for the ComfyUI client."""
    parser = argparse.ArgumentParser(description="Generate images using ComfyUI API")
    parser.add_argument("prompt", help="Text prompt for image generation")
    parser.add_argument("--server", default=get_server_url().rstrip('/'),
                       help="ComfyUI server URL")
    parser.add_argument("--negative", default="bad hands, blurry, low quality",
                       help="Negative prompt")
    parser.add_argument("--width", type=int, default=512, help="Image width")
    parser.add_argument("--height", type=int, default=512, help="Image height")
    parser.add_argument("--steps", type=int, default=20, help="Sampling steps")
    parser.add_argument("--cfg", type=float, default=8.0, help="CFG scale")
    parser.add_argument("--seed", type=int, help="Random seed")
    parser.add_argument("--output", default="generated_images", help="Output directory")
    
    args = parser.parse_args()
    
    try:
        client = ComfyUIClient(args.server)
        print(f"Connecting to ComfyUI server at: {args.server}")
        print(f"Generating image for prompt: '{args.prompt}'")
        
        saved_files = client.generate_image(
            positive_prompt=args.prompt,
            negative_prompt=args.negative,
            width=args.width,
            height=args.height,
            steps=args.steps,
            cfg_scale=args.cfg,
            seed=args.seed,
            output_dir=args.output
        )
        
        print(f"\nGeneration complete! {len(saved_files)} image(s) saved:")
        for filepath in saved_files:
            print(f"  - {filepath}")
            
    except Exception as e:
        print(f"Error: {e}")
        return 1
    
    return 0


if __name__ == "__main__":
    exit(main())
