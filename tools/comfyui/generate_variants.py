#!/usr/bin/env python3
"""
Demonstration script showing how to generate multiple variants of images
with different sizes and transparency options using ComfyUI.
"""

from comfyui_client import ComfyUIClient
from config_loader import get_server_url
import time

def main():
    # Initialize client
    server_url = get_server_url()
    client = ComfyUIClient(server_url)
    
    prompt = "a simple red apple on a white background, clean product photo style"
    negative = "blurry, low quality, distorted, background clutter, multiple objects"
    
    print("🍎 Generating Red Apple Variants - Multiple Sizes & Transparency")
    print("=" * 60)
    
    # Generate small variants (256x256)
    print("\n📏 Generating Small Variants (256x256)...")
    for i in range(3):
        print(f"  Variant {i+1}/3...")
        try:
            saved_files = client.generate_image(
                positive_prompt=prompt,
                negative_prompt=negative,
                width=256,
                height=256,
                steps=25,
                cfg_scale=7.5,
                seed=12345 + i,  # Different seed for variety
                output_dir="apple_variants",
                filename_prefix=f"apple_small_{i+1}"
            )
            print(f"    ✓ Saved: {saved_files[0]}")
        except Exception as e:
            print(f"    ✗ Error: {e}")
    
    # Generate medium variants (512x512)
    print("\n📏 Generating Medium Variants (512x512)...")
    for i in range(2):
        print(f"  Variant {i+1}/2...")
        try:
            saved_files = client.generate_image(
                positive_prompt=prompt,
                negative_prompt=negative,
                width=512,
                height=512,
                steps=20,
                cfg_scale=8.0,
                seed=54321 + i,
                output_dir="apple_variants",
                filename_prefix=f"apple_medium_{i+1}"
            )
            print(f"    ✓ Saved: {saved_files[0]}")
        except Exception as e:
            print(f"    ✗ Error: {e}")
    
    # Generate large variants (768x768)
    print("\n📏 Generating Large Variants (768x768)...")
    for i in range(2):
        print(f"  Variant {i+1}/2...")
        try:
            saved_files = client.generate_image(
                positive_prompt=prompt,
                negative_prompt=negative,
                width=768,
                height=768,
                steps=30,
                cfg_scale=7.0,
                seed=98765 + i,
                output_dir="apple_variants",
                filename_prefix=f"apple_large_{i+1}"
            )
            print(f"    ✓ Saved: {saved_files[0]}")
        except Exception as e:
            print(f"    ✗ Error: {e}")
    
    # Generate transparent background variant
    print("\n🔍 Generating Transparent Background Variant...")
    transparent_prompt = "a simple red apple, isolated object, transparent background, product photo, clean cutout"
    try:
        saved_files = client.generate_image(
            positive_prompt=transparent_prompt,
            negative_prompt="background, white background, any background, blurry, low quality",
            width=512,
            height=512,
            steps=25,
            cfg_scale=7.5,
            seed=11111,
            output_dir="apple_variants",
            filename_prefix="apple_transparent"
        )
        print(f"    ✓ Saved: {saved_files[0]}")
        print("    💡 Note: For true transparency, you may need to use background removal tools")
        print("       or specialized ComfyUI workflows with alpha channel support.")
    except Exception as e:
        print(f"    ✗ Error: {e}")
    
    print(f"\n🎉 Generation Complete!")
    print(f"📁 Check the 'apple_variants' directory for all generated images")
    print(f"📊 Total variants generated across different sizes and styles")

if __name__ == "__main__":
    main()
