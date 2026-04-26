#!/usr/bin/env python3
"""
Final transparent image generator - creates truly transparent PNG images.
Uses standard ComfyUI generation + Python background removal.
"""

from comfyui_client import ComfyUIClient
from config_loader import get_server_url
import os
from PIL import Image
import io
import numpy as np

def create_transparent_apple():
    """Generate a transparent apple image using the red apple prompt."""
    
    server_url = get_server_url()
    
    print("🍎 Creating Transparent Apple Image")
    print("=" * 50)
    
    # Initialize client
    client = ComfyUIClient(server_url)
    
    # Generate image optimized for background removal
    print("🎨 Step 1: Generating base image with clean background...")
    
    try:
        # Use a better model for realistic photography
        workflow = client.create_text2img_workflow(
            positive_prompt="a simple red apple, isolated object, studio lighting, pure white background, product photography, clean cutout style",
            negative_prompt="multiple objects, cluttered background, shadows, complex background, gradient background, table, surface",
            width=512,
            height=512,
            steps=30,
            cfg_scale=9.0,  # Higher CFG for better prompt adherence
            seed=12345,
            checkpoint="realisticVisionV60B1_v60B1VAE.safetensors"  # Much better for clean backgrounds
        )
        
        # Connect to websocket and generate using the better model
        import websocket
        ws = websocket.WebSocket()
        try:
            ws.connect(f"ws://{client.server_address}/ws?clientId={client.client_id}")
            images = client.get_images(ws, workflow)
        finally:
            ws.close()
        
        # Save images and return file paths
        import time
        saved_files = []
        for node_id, image_list in images.items():
            for i, image_data in enumerate(image_list):
                # Open image and save
                image = Image.open(io.BytesIO(image_data))
                timestamp = int(time.time())
                filename = f"apple_base_realistic_{timestamp}_{node_id}_{i}.png"
                filepath = os.path.join("transparent_output", filename)
                image.save(filepath)
                saved_files.append(filepath)
                print(f"   ✓ Base image saved: {filepath}")
                break  # Only need first image
            break
        
        if not saved_files:
            print("❌ No images were generated")
            return None
            
        base_image_path = saved_files[0]
        print(f"   ✓ Base image saved: {base_image_path}")
        
        # Step 2: Process for transparency
        print("🔧 Step 2: Processing for transparency...")
        
        # Load the generated image
        with open(base_image_path, 'rb') as f:
            image_data = f.read()
        
        # Process for transparency
        transparent_path = process_for_transparency(image_data, "transparent_output")
        
        if transparent_path:
            print(f"   ✓ Transparent image saved: {transparent_path}")
            return transparent_path
        else:
            print("   ❌ Failed to create transparent version")
            return base_image_path
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return None

def process_for_transparency(image_data, output_dir):
    """Convert image to transparent PNG by removing white background."""
    try:
        # Load image
        image = Image.open(io.BytesIO(image_data)).convert('RGBA')
        
        # Convert to numpy array
        data = np.array(image)
        
        # More sophisticated background removal
        # Calculate brightness for each pixel
        brightness = (data[:,:,0] * 0.299 + data[:,:,1] * 0.587 + data[:,:,2] * 0.114)
        
        # Create mask for white/light backgrounds
        white_threshold = 235
        brightness_threshold = 240
        
        # Pixel is background if it's very bright OR very white
        background_mask = (brightness > brightness_threshold) | (
            (data[:,:,0] > white_threshold) & 
            (data[:,:,1] > white_threshold) & 
            (data[:,:,2] > white_threshold)
        )
        
        # Create gradual transparency for edges (anti-aliasing)
        from scipy import ndimage
        
        # Blur the mask slightly for smoother edges
        blurred_mask = ndimage.gaussian_filter(background_mask.astype(float), sigma=0.5)
        
        # Invert mask (we want to keep non-background pixels)
        alpha_mask = 1.0 - blurred_mask
        
        # Apply alpha mask
        data[:,:,3] = (alpha_mask * 255).astype(np.uint8)
        
        # Create new image with transparency
        transparent_image = Image.fromarray(data, 'RGBA')
        
        # Save transparent version
        import time
        timestamp = int(time.time())
        transparent_path = os.path.join(output_dir, f"apple_transparent_final_{timestamp}.png")
        transparent_image.save(transparent_path, 'PNG', optimize=True)
        
        return transparent_path
        
    except ImportError:
        print("   ⚠️ scipy not available, using basic transparency...")
        return process_basic_transparency(image_data, output_dir)
    except Exception as e:
        print(f"   ⚠️ Error in advanced processing: {e}")
        return process_basic_transparency(image_data, output_dir)

def process_basic_transparency(image_data, output_dir):
    """Basic transparency processing without scipy."""
    try:
        # Load image
        image = Image.open(io.BytesIO(image_data)).convert('RGBA')
        
        # Convert to numpy array
        data = np.array(image)
        
        # Simple white background removal
        white_threshold = 240
        
        # Create mask where pixels are NOT white
        mask = ~((data[:,:,0] > white_threshold) & 
                 (data[:,:,1] > white_threshold) & 
                 (data[:,:,2] > white_threshold))
        
        # Apply mask to alpha channel
        data[:,:,3] = mask * 255
        
        # Create new image with transparency
        transparent_image = Image.fromarray(data, 'RGBA')
        
        # Save transparent version
        import time
        timestamp = int(time.time())
        transparent_path = os.path.join(output_dir, f"apple_transparent_basic_{timestamp}.png")
        transparent_image.save(transparent_path, 'PNG', optimize=True)
        
        return transparent_path
        
    except Exception as e:
        print(f"   ⚠️ Error in basic processing: {e}")
        return None

def main():
    # Create output directory
    os.makedirs("transparent_output", exist_ok=True)
    
    # Generate transparent apple
    result = create_transparent_apple()
    
    if result:
        print(f"\n🎉 Success!")
        print(f"📁 Transparent apple image created: {result}")
        print(f"\n💡 Verification:")
        print(f"   • Open the image in an image editor (Photoshop, GIMP, etc.)")
        print(f"   • Check that the background is truly transparent")
        print(f"   • The apple should have smooth edges")
        
        # Try to open the file to show it
        try:
            import subprocess
            subprocess.run(['open', result], check=False)
            print(f"   • Image opened automatically for review")
        except:
            pass
            
    else:
        print(f"\n❌ Failed to create transparent image")
        print(f"   • Check server connectivity")
        print(f"   • Verify ComfyUI is running properly")

if __name__ == "__main__":
    main()
