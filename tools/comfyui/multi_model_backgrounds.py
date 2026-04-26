#!/usr/bin/env python3
"""
Multi-Model Card Background Generation
This script generates card backgrounds using DIFFERENT MODELS for each suit to find the best fit.
"""

import json
import random
import time
from comfyui_client import ComfyUIClient
from config_loader import get_server_url

def create_consistent_background_prompt(suit_name, suit_theme):
    """
    Create an improved prompt for consistent card backgrounds with proper layout structure.
    
    Args:
        suit_name: Name of the suit (Hearts, Diamonds, Clubs, Spades)
        suit_theme: Thematic description for the suit
    
    Returns:
        Dictionary with prompt and settings
    """
    
    # Base layout structure - ensures consistent card proportions
    base_layout = """
    playing card template design, vertical rectangular frame, 
    ornate decorative border frame, symmetrical layout,
    CLEAR EMPTY CORNERS for rank numbers, 
    LARGE CENTRAL AREA left completely empty for character art overlay,
    Renaissance medieval fantasy style,
    """
    
    # Suit-specific theming
    suit_specific = f"""
    {suit_theme} themed decorative elements,
    {suit_name.lower()} suit motifs integrated into border design,
    """
    
    # Layout requirements - critical for card assembly
    layout_requirements = """
    border decoration only around edges,
    top-left corner EMPTY space for rank symbol,
    top-right corner EMPTY space for suit symbol,
    bottom-left corner EMPTY space for inverted rank,
    bottom-right corner EMPTY space for inverted suit,
    center 60% of card COMPLETELY TRANSPARENT/EMPTY for character portrait,
    symmetrical ornate frame border,
    """
    
    # Quality and style modifiers
    quality_modifiers = """
    highly detailed, ornate Renaissance artwork,
    clean vector-style design, card game aesthetic,
    professional playing card design, elegant decorative flourishes,
    rich colors, medieval illuminated manuscript style,
    """
    
    # Negative prompt to avoid unwanted elements
    negative_prompt = """
    characters, people, faces, portraits, animals, text, numbers, letters,
    cluttered center, busy background, modern elements, photography,
    center artwork, filled corners, messy design, asymmetrical layout,
    low quality, blurry, pixelated, watermarks
    """
    
    full_prompt = f"{base_layout} {suit_specific} {layout_requirements} {quality_modifiers}".strip()
    
    return {
        "prompt": full_prompt,
        "negative_prompt": negative_prompt,
        "width": 512,
        "height": 768,
        "steps": 30,
        "cfg_scale": 7.5,
        "seed": random.randint(1000000, 9999999)
    }

def generate_multi_model_backgrounds():
    """Generate card backgrounds using DIFFERENT MODELS for each suit."""
    
    # Suit definitions with themes AND specific models
    suit_model_combinations = {
        "Hearts": {
            "theme": "royal nobility, red and gold colors, crown and scepter motifs, regal elegance",
            "model": "sd_xl_base_1.0.safetensors"  # SDXL for architectural precision
        },
        "Diamonds": {
            "theme": "merchant wealth, gold and blue colors, coin and gem motifs, prosperity symbols", 
            "model": "fantasy_card_diffusion.safetensors"  # Fantasy Card model for game aesthetics
        },
        "Clubs": {
            "theme": "nature and war, green and brown colors, tree and weapon motifs, natural elements",
            "model": "fantasy_art_style.ckpt"  # Fantasy Art for medieval themes
        },
        "Spades": {
            "theme": "military power, black and silver colors, sword and shield motifs, martial strength",
            "model": "v1-5-pruned-emaonly.ckpt"  # SD 1.5 for comparison
        }
    }
    
    client = ComfyUIClient(get_server_url())
    results = []
    
    print("🎯 Multi-Model Card Background Generation")
    print("=" * 60)
    print("Each suit will use a DIFFERENT MODEL for comparison:")
    for suit, info in suit_model_combinations.items():
        print(f"  • {suit}: {info['model']}")
    print("=" * 60)
    
    for suit_name, suit_info in suit_model_combinations.items():
        suit_theme = suit_info["theme"]
        model_name = suit_info["model"]
        
        print(f"\n🎨 Generating {suit_name} Background...")
        print(f"   📦 Using Model: {model_name}")
        print(f"   🎭 Theme: {suit_theme}")
        
        prompt_config = create_consistent_background_prompt(suit_name, suit_theme)
        
        try:
            generated_files = client.generate_image(
                positive_prompt=prompt_config["prompt"],
                negative_prompt=prompt_config["negative_prompt"],
                width=prompt_config["width"],
                height=prompt_config["height"],
                steps=prompt_config["steps"],
                cfg_scale=prompt_config["cfg_scale"],
                seed=prompt_config["seed"],
                checkpoint=model_name,  # 🔥 THIS IS THE KEY - DIFFERENT MODEL PER SUIT
                filename_prefix=f"Card_Background_{suit_name}_Model_{model_name.split('.')[0]}"
            )
            
            if generated_files:
                print(f"   ✅ Generated: {generated_files[0]}")
                results.append({
                    "suit": suit_name,
                    "model": model_name,
                    "filename": generated_files[0],
                    "seed": prompt_config["seed"],
                    "success": True
                })
            else:
                print(f"   ❌ Failed {suit_name} with {model_name}: No files generated")
                results.append({
                    "suit": suit_name,
                    "model": model_name,
                    "success": False,
                    "error": "No files generated"
                })
                
        except Exception as e:
            print(f"   ❌ Exception with {suit_name} using {model_name}: {str(e)}")
            results.append({
                "suit": suit_name,
                "model": model_name,
                "success": False,
                "error": str(e)
            })
        
        # Small delay between generations
        time.sleep(2)
    
    return results

def main():
    """Main function to test multi-model card background generation."""
    print("🎯 Multi-Model Card Background Generation Test")
    print("=" * 60)
    
    # Generate all backgrounds with different models
    results = generate_multi_model_backgrounds()
    
    # Print detailed summary
    print("\n📊 Multi-Model Generation Summary:")
    print("=" * 40)
    
    successful = [r for r in results if r["success"]]
    failed = [r for r in results if not r["success"]]
    
    print(f"✅ Successful generations: {len(successful)}")
    print(f"❌ Failed generations: {len(failed)}")
    
    if successful:
        print("\n🎉 Successfully Generated Backgrounds (Model-Specific):")
        for i, result in enumerate(successful, 1):
            print(f"  {i}. {result['suit']}")
            print(f"     📦 Model: {result['model']}")
            print(f"     🌱 Seed: {result.get('seed', 'N/A')}")
            print(f"     📁 File: {result['filename']}")
            print()
    
    if failed:
        print("\n⚠️ Failed Backgrounds:")
        for i, result in enumerate(failed, 1):
            print(f"  {i}. {result['suit']} (Model: {result['model']})")
            print(f"     ❌ Error: {result['error']}")
            print()
    
    print("🔍 IMPORTANT: Each background was generated with a DIFFERENT MODEL!")
    print("📁 Check the generated_images/ folder to compare model results.")
    print("🎯 Model Performance Analysis:")
    
    model_performance = {}
    for result in results:
        model = result["model"]
        if model not in model_performance:
            model_performance[model] = {"success": 0, "failed": 0}
        
        if result["success"]:
            model_performance[model]["success"] += 1
        else:
            model_performance[model]["failed"] += 1
    
    for model, stats in model_performance.items():
        total = stats["success"] + stats["failed"]
        success_rate = (stats["success"] / total * 100) if total > 0 else 0
        print(f"  • {model}: {stats['success']}/{total} success ({success_rate:.1f}%)")

if __name__ == "__main__":
    main()
