#!/usr/bin/env python3
"""
Improved Card Background Generation with Consistent Layout
This script generates card backgrounds with proper cutouts for corner elements and center art.
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

def test_hearts_background_multiple_variations():
    """Generate Hearts background with multiple variations for comparison."""
    
    client = ComfyUIClient(get_server_url())
    hearts_theme = "royal nobility, red and gold colors, crown and scepter motifs, regal elegance"
    
    results = []
    
    for i in range(5):  # Generate 5 variations
        print(f"\n🎨 Generating Variation {i+1}/5...")
        
        prompt_config = create_consistent_background_prompt("Hearts", hearts_theme)
        
        try:
            generated_files = client.generate_image(
                positive_prompt=prompt_config["prompt"],
                negative_prompt=prompt_config["negative_prompt"],
                width=prompt_config["width"],
                height=prompt_config["height"],
                steps=prompt_config["steps"],
                cfg_scale=prompt_config["cfg_scale"],
                seed=prompt_config["seed"],
                filename_prefix=f"Hearts_Background_Improved_Var_{i+1}"
            )
            
            if generated_files:
                print(f"✅ Generated: {generated_files[0]}")
                results.append({
                    "variation": i+1,
                    "filename": generated_files[0],
                    "seed": prompt_config["seed"],
                    "success": True
                })
            else:
                print(f"❌ Failed variation {i+1}: No files generated")
                results.append({
                    "variation": i+1,
                    "success": False,
                    "error": "No files generated"
                })
                
        except Exception as e:
            print(f"❌ Exception with variation {i+1}: {str(e)}")
            results.append({
                "variation": i+1,
                "success": False,
                "error": str(e)
            })
        
        # Small delay between generations
        time.sleep(1)
    
    return results

def main():
    """Main function to test improved card background generation."""
    print("🎯 Improved Card Background Generation")
    print("=" * 50)
    
    # Test Hearts background with multiple variations
    print("🃁 Testing Hearts Background with Multiple Variations...")
    results = test_hearts_background_multiple_variations()
    
    # Print summary
    print("\n📊 Generation Summary:")
    print("=" * 30)
    
    successful = [r for r in results if r["success"]]
    failed = [r for r in results if not r["success"]]
    
    print(f"✅ Successful generations: {len(successful)}")
    print(f"❌ Failed generations: {len(failed)}")
    
    if successful:
        print("\n🎉 Successful Variations:")
        for i, result in enumerate(successful, 1):
            print(f"  {i}. Variation {result['variation']} (seed: {result.get('seed', 'N/A')}) → {result['filename']}")
    
    if failed:
        print("\n⚠️ Failed Variations:")
        for i, result in enumerate(failed, 1):
            print(f"  {i}. Variation {result['variation']} → {result['error']}")
    
    print("\n🔍 Review generated images to select the best variation for consistent layouts!")

if __name__ == "__main__":
    main()
