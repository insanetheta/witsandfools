#!/usr/bin/env python3
"""
Specialized script for generating truly transparent PNG images using ComfyUI.
Uses background removal techniques and specialized workflows.
"""

from comfyui_client import ComfyUIClient
import json
import urllib.request
import urllib.parse
import uuid
import websocket
import io
import time
from PIL import Image
import os

class TransparentImageGenerator:
    def __init__(self, server_url):
        self.server_url = server_url
        if server_url.startswith('http://'):
            server_url = server_url[7:]
        elif server_url.startswith('https://'):
            server_url = server_url[8:]
        if server_url.endswith('/'):
            server_url = server_url[:-1]
        self.server_address = server_url
        self.client_id = str(uuid.uuid4())

    def check_available_nodes(self):
        """Check what nodes are available for transparency generation."""
        try:
            with urllib.request.urlopen(f"http://{self.server_address}/object_info", timeout=10) as response:
                object_info = json.loads(response.read())
                
            transparency_nodes = []
            background_removal_nodes = []
            
            # Look for nodes that might support transparency
            for node_name in object_info.keys():
                node_lower = node_name.lower()
                if any(keyword in node_lower for keyword in ['alpha', 'transparent', 'mask', 'remove', 'background']):
                    if 'background' in node_lower or 'remove' in node_lower:
                        background_removal_nodes.append(node_name)
                    else:
                        transparency_nodes.append(node_name)
            
            return transparency_nodes, background_removal_nodes, object_info
        except Exception as e:
            print(f"Error checking nodes: {e}")
            return [], [], {}

    def create_basic_cutout_workflow(self, prompt, negative_prompt="background", seed=42):
        """Create a workflow optimized for clean cutouts that can be post-processed."""
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
                    "text": f"{prompt}, isolated object, clean cutout, studio lighting, white background"
                }
            },
            "3": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["1", 1],
                    "text": f"{negative_prompt}, multiple objects, cluttered, blurry, low quality"
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
                    "steps": 30,
                    "cfg": 8.0,
                    "sampler_name": "dpmpp_2m",
                    "scheduler": "karras",
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
                "class_type": "SaveImage",
                "inputs": {
                    "images": ["6", 0],
                    "filename_prefix": "cutout"
                }
            }
        }
        return workflow

    def create_mask_workflow(self, prompt, negative_prompt="background", seed=42):
        """Create a workflow that generates both image and mask for transparency."""
        # This is a more advanced workflow that tries to create a mask
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
                    "text": f"{prompt}, isolated on white background, clean cutout, product photography"
                }
            },
            "3": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["1", 1],
                    "text": f"{negative_prompt}, cluttered background, multiple objects, blurry"
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
                    "steps": 30,
                    "cfg": 9.0,
                    "sampler_name": "dpmpp_2m",
                    "scheduler": "karras",
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
            # Generate mask from white background
            "7": {
                "class_type": "CLIPTextEncode",
                "inputs": {
                    "clip": ["1", 1],
                    "text": "white background mask, solid white"
                }
            },
            "8": {
                "class_type": "KSampler",
                "inputs": {
                    "model": ["1", 0],
                    "positive": ["7", 0],
                    "negative": ["2", 0],  # Use object as negative
                    "latent_image": ["4", 0],
                    "seed": seed + 1,
                    "steps": 20,
                    "cfg": 7.0,
                    "sampler_name": "euler",
                    "scheduler": "normal",
                    "denoise": 1.0
                }
            },
            "9": {
                "class_type": "VAEDecode",
                "inputs": {
                    "samples": ["8", 0],
                    "vae": ["1", 2]
                }
            },
            "10": {
                "class_type": "SaveImage",
                "inputs": {
                    "images": ["6", 0],
                    "filename_prefix": "object"
                }
            },
            "11": {
                "class_type": "SaveImage",
                "inputs": {
                    "images": ["9", 0],
                    "filename_prefix": "mask"
                }
            }
        }
        return workflow

    def generate_with_transparency(self, prompt, output_dir="transparent_images"):
        """Generate an image optimized for transparency conversion."""
        os.makedirs(output_dir, exist_ok=True)
        
        print(f"🔍 Checking available nodes for transparency...")
        transparency_nodes, bg_removal_nodes, object_info = self.check_available_nodes()
        
        print(f"   Found {len(transparency_nodes)} transparency-related nodes")
        print(f"   Found {len(bg_removal_nodes)} background removal nodes")
        
        if transparency_nodes:
            print(f"   Transparency nodes: {transparency_nodes[:5]}")  # Show first 5
        if bg_removal_nodes:
            print(f"   Background removal nodes: {bg_removal_nodes[:5]}")  # Show first 5
        
        print(f"\n🎨 Generating optimized cutout image...")
        
        # Use the basic cutout workflow
        workflow = self.create_basic_cutout_workflow(prompt)
        
        # Execute workflow
        prompt_id = str(uuid.uuid4())
        p = {"prompt": workflow, "client_id": self.client_id, "prompt_id": prompt_id}
        data = json.dumps(p).encode('utf-8')
        req = urllib.request.Request(f"http://{self.server_address}/prompt", data=data)
        urllib.request.urlopen(req).read()
        
        # Wait for completion via websocket
        ws = websocket.WebSocket()
        try:
            ws.connect(f"ws://{self.server_address}/ws?clientId={self.client_id}")
            
            while True:
                out = ws.recv()
                if isinstance(out, str):
                    message = json.loads(out)
                    if message['type'] == 'executing':
                        data = message['data']
                        if data['node'] is None and data['prompt_id'] == prompt_id:
                            break
        finally:
            ws.close()
        
        # Get the generated image
        with urllib.request.urlopen(f"http://{self.server_address}/history/{prompt_id}") as response:
            history = json.loads(response.read())
        
        saved_files = []
        for node_id in history[prompt_id]['outputs']:
            node_output = history[prompt_id]['outputs'][node_id]
            if 'images' in node_output:
                for image_info in node_output['images']:
                    # Download image
                    data = {"filename": image_info['filename'], "subfolder": image_info['subfolder'], "type": image_info['type']}
                    url_values = urllib.parse.urlencode(data)
                    with urllib.request.urlopen(f"http://{self.server_address}/view?{url_values}") as response:
                        image_data = response.read()
                    
                    # Save original
                    original_path = os.path.join(output_dir, f"cutout_original_{int(time.time())}.png")
                    with open(original_path, 'wb') as f:
                        f.write(image_data)
                    saved_files.append(original_path)
                    print(f"   ✓ Saved original: {original_path}")
                    
                    # Process for transparency
                    transparent_path = self.process_for_transparency(image_data, output_dir)
                    if transparent_path:
                        saved_files.append(transparent_path)
                        print(f"   ✓ Saved transparent: {transparent_path}")
        
        return saved_files

    def process_for_transparency(self, image_data, output_dir):
        """Process image to add transparency by removing white background."""
        try:
            from PIL import Image, ImageChops
            import numpy as np
            
            # Load image
            image = Image.open(io.BytesIO(image_data)).convert('RGBA')
            
            # Convert to numpy array
            data = np.array(image)
            
            # Define white background (with some tolerance)
            white_threshold = 240  # Adjust this value as needed
            
            # Create mask where pixels are NOT white
            # A pixel is considered white if all RGB values are above threshold
            mask = ~((data[:,:,0] > white_threshold) & 
                     (data[:,:,1] > white_threshold) & 
                     (data[:,:,2] > white_threshold))
            
            # Apply mask to alpha channel
            data[:,:,3] = mask * 255
            
            # Create new image with transparency
            transparent_image = Image.fromarray(data, 'RGBA')
            
            # Save transparent version
            timestamp = int(time.time())
            transparent_path = os.path.join(output_dir, f"apple_transparent_alpha_{timestamp}.png")
            transparent_image.save(transparent_path, 'PNG', optimize=True)
            
            return transparent_path
            
        except Exception as e:
            print(f"   ⚠️ Error processing transparency: {e}")
            return None

def main():
    server_url = "http://internal-a4c0bb3b32e4744da9fae4ac09c88b1c-1385635584.us-east-1.elb.amazonaws.com:7860"
    
    print("🍎 Generating Transparent Apple Image")
    print("=" * 50)
    
    generator = TransparentImageGenerator(server_url)
    
    prompt = "a simple red apple, product photography, clean isolated object"
    
    try:
        saved_files = generator.generate_with_transparency(prompt)
        
        print(f"\n🎉 Generation Complete!")
        print(f"📁 Generated {len(saved_files)} files:")
        for file_path in saved_files:
            print(f"   • {file_path}")
        
        print(f"\n💡 Tips for transparency:")
        print(f"   • The '*_alpha_*.png' file should have true transparency")
        print(f"   • Open in image editor to verify alpha channel")
        print(f"   • Adjust white_threshold in code if needed")
        
    except Exception as e:
        print(f"✗ Error: {e}")

if __name__ == "__main__":
    main()
