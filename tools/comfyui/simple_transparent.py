#!/usr/bin/env python3
"""
Simple transparent image generation using existing ComfyUI client 
and the RecraftRemoveBackgroundNode that's available on the server.
"""

from comfyui_client import ComfyUIClient
from config_loader import get_server_url
import json

class SimpleTransparentGenerator(ComfyUIClient):
    def create_recraft_background_removal_workflow(self, prompt, seed=42):
        """Create a workflow using RecraftRemoveBackgroundNode for transparency."""
        workflow = {
            "1": {
                "class_type": "CheckpointLoaderSimple",
                "inputs": {
                    "ckpt_name": "v1-5-pruned-emaonly.ckpt"
                }
            },
            "2": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["1", 1],
                    "text": f"{prompt}, isolated object, clean cutout, white background, product photography"
                }
            },
            "3": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["1", 1],
                    "text": "cluttered background, multiple objects, blurry, low quality"
                }
            },
            "4": {
                "class_type": "EmptyLatentImage",
                "inputs": {
                    "width": 512,
                    "height": 512,
                    "batch_size": 1
                }
            },
            "5": {
                "class_type": "KSampler",
                "inputs": {
                    "model": ["1", 0],
                    "positive": ["2", 0],
                    "negative": ["3", 0],
                    "latent_image": ["4", 0],
                    "seed": seed,
                    "steps": 25,
                    "cfg": 8.0,
                    "sampler_name": "euler",
                    "scheduler": "normal",
                    "denoise": 1.0
                }
            },
            "6": {
                "class_type": "VAEDecode",
                "inputs": {
                    "samples": ["5", 0],
                    "vae": ["1", 2]
                }
            },
            "7": {
                "class_type": "RecraftRemoveBackgroundNode",
                "inputs": {
                    "image": ["6", 0]
                }
            },
            "8": {
                "class_type": "SaveImage",
                "inputs": {
                    "images": ["7", 0],
                    "filename_prefix": "transparent_apple"
                }
            }
        }
        return workflow

    def generate_transparent_image(self, prompt, output_dir="transparent_images", seed=42):
        """Generate transparent image using background removal."""
        import os
        import websocket
        import uuid
        import time
        
        os.makedirs(output_dir, exist_ok=True)
        
        print(f"🎨 Generating image with background removal...")
        
        # Create workflow with background removal
        workflow = self.create_recraft_background_removal_workflow(prompt, seed)
        
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
                from PIL import Image
                import io
                image = Image.open(io.BytesIO(image_data))
                timestamp = int(time.time())
                filename = f"apple_transparent_recraft_{timestamp}_{node_id}_{i}.png"
                filepath = os.path.join(output_dir, filename)
                image.save(filepath)
                saved_files.append(filepath)
                print(f"   ✓ Transparent image saved: {filepath}")
        
        return saved_files

def main():
    server_url = get_server_url()
    
    print("🍎 Generating Transparent Apple with Background Removal")
    print("=" * 60)
    
    try:
        client = SimpleTransparentGenerator(server_url)
        
        prompt = "a simple red apple, product photography, clean isolated object"
        
        saved_files = client.generate_transparent_image(prompt)
        
        print(f"\n🎉 Generation Complete!")
        print(f"📁 Generated {len(saved_files)} transparent image(s):")
        for file_path in saved_files:
            print(f"   • {file_path}")
        
        print(f"\n💡 This image should have true transparency!")
        print(f"   • The background should be removed automatically")
        print(f"   • Open in image editor to verify alpha channel")
        
    except Exception as e:
        print(f"✗ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
