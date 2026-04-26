#!/usr/bin/env python3
"""
Example usage of the ComfyUI client for generating images from text prompts.
"""

from comfyui_client import ComfyUIClient
from config_loader import get_server_url

def main():
    # Your ComfyUI server URL
    server_url = get_server_url()
    
    # Initialize the client
    client = ComfyUIClient(server_url)
    
    # Example prompts
    prompts = [
        "a beautiful landscape with mountains and a lake, digital art",
        "a cute cat sitting on a windowsill, photorealistic",
        "futuristic city skyline at sunset, cyberpunk style"
    ]
    
    # Generate images for each prompt
    for i, prompt in enumerate(prompts):
        print(f"\n=== Generating image {i+1}/3 ===")
        print(f"Prompt: {prompt}")
        
        try:
            saved_files = client.generate_image(
                positive_prompt=prompt,
                negative_prompt="blurry, low quality, distorted",
                width=512,
                height=512,
                steps=20,
                cfg_scale=7.5,
                output_dir="example_outputs",
                filename_prefix=f"example_{i+1}"
            )
            
            print(f"Success! Generated {len(saved_files)} image(s):")
            for filepath in saved_files:
                print(f"  - {filepath}")
                
        except Exception as e:
            print(f"Error generating image: {e}")
    
    print("\nAll done! Check the 'example_outputs' directory for your generated images.")

if __name__ == "__main__":
    main()
