#!/usr/bin/env python3
"""
Advanced demonstration of ComfyUI client capabilities showing:
- Multiple image sizes
- Batch generation
- Different models
- Transparency effects
- Custom parameters
"""

from comfyui_client import ComfyUIClient
import time

def demonstrate_capabilities():
    """Show all the customization options available with the ComfyUI client."""
    
    print("🎨 ComfyUI Client - Advanced Capabilities Demo")
    print("=" * 60)
    
    # Your server URL (update when your server is running)
    server_url = "http://your-comfyui-server:7860"
    
    print(f"📋 Demonstration of available customization options:")
    print(f"   (Update server_url when your ComfyUI server is running)")
    print()
    
    # Available customization options
    customizations = {
        "Image Sizes": [
            "Small: 256x256 (fast generation)",
            "Medium: 512x512 (balanced)",
            "Large: 768x768 or 1024x1024 (high quality)",
            "Custom: any width/height combination"
        ],
        "Generation Parameters": [
            "Steps: 10-50+ (quality vs speed tradeoff)",
            "CFG Scale: 1-20 (prompt adherence)",
            "Seed: Fixed for reproducible results",
            "Sampler: euler, dpm++, etc."
        ],
        "Batch Generation": [
            "Multiple variants with different seeds",
            "Systematic parameter sweeps",
            "Size variations of same prompt"
        ],
        "Models Available": [
            "Realistic: realisticVisionV60B1_v60B1VAE.safetensors",
            "Cartoon: disneyPixarCartoon_v10.safetensors",
            "Anime: anything_v30.ckpt",
            "Art: dreamShaper_v40.safetensors",
            "And 70+ other models on your server"
        ],
        "Transparency Options": [
            "Background removal prompts",
            "Isolated object generation",
            "Product photo style prompts",
            "Note: True alpha channel requires specialized workflows"
        ]
    }
    
    for category, options in customizations.items():
        print(f"🔧 {category}:")
        for option in options:
            print(f"   • {option}")
        print()

def example_variant_generation():
    """Example code for generating multiple variants."""
    
    print("💡 Example Code for Multi-Variant Generation:")
    print("-" * 50)
    
    code_example = '''
# Initialize client
client = ComfyUIClient("http://your-server:7860")

# Generate small variants
for i in range(3):
    client.generate_image(
        positive_prompt="red apple on white background",
        width=256, height=256,
        seed=12345 + i,  # Different seed for variety
        filename_prefix=f"apple_small_{i+1}"
    )

# Generate large high-quality version
client.generate_image(
    positive_prompt="red apple, professional product photo, studio lighting",
    width=1024, height=1024,
    steps=30, cfg_scale=7.5,
    filename_prefix="apple_large_hq"
)

# Generate transparent background version
client.generate_image(
    positive_prompt="red apple isolated object transparent background",
    negative_prompt="background, any background, white background",
    filename_prefix="apple_transparent"
)

# Use different model for artistic style
client.create_text2img_workflow(
    positive_prompt="red apple, disney pixar style",
    checkpoint="disneyPixarCartoon_v10.safetensors"
)
'''
    
    print(code_example)

def show_command_line_examples():
    """Show command line usage examples."""
    
    print("🖥️  Command Line Usage Examples:")
    print("-" * 40)
    
    examples = [
        "# Small variant",
        "python comfyui_client.py \"red apple\" --width 256 --height 256 --output apple_variants",
        "",
        "# Large high-quality variant",
        "python comfyui_client.py \"red apple product photo\" --width 1024 --height 1024 --steps 30",
        "",
        "# Transparent background attempt",
        "python comfyui_client.py \"red apple isolated transparent\" --negative \"background\"",
        "",
        "# Fixed seed for reproducible results", 
        "python comfyui_client.py \"red apple\" --seed 12345",
        "",
        "# Multiple variants with different seeds",
        "for i in {1..5}; do",
        "  python comfyui_client.py \"red apple\" --seed $((12345 + i)) --output variants",
        "done"
    ]
    
    for example in examples:
        print(example)
    print()

def main():
    demonstrate_capabilities()
    example_variant_generation()
    show_command_line_examples()
    
    print("📝 Summary:")
    print("✅ ComfyUI client supports extensive customization")
    print("✅ Multiple sizes, models, and generation parameters")
    print("✅ Batch generation capabilities")
    print("✅ Both Python API and command-line interface")
    print("⚠️  Update server URL when your ComfyUI instance is running")
    print()
    print("🚀 Ready to generate images when server is accessible!")

if __name__ == "__main__":
    main()
