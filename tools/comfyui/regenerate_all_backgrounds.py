#!/usr/bin/env python3
"""
Regenerate All Card Backgrounds with Improved Consistent Layout
This script generates all 4 suit backgrounds with proper cutouts for corner elements and center art.
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

def generate_all_backgrounds():
    """Generate all 4 suit backgrounds with improved consistent layout."""
    
    # Suit definitions with themes
    suits = {
        "Hearts": "royal nobility, red and gold colors, crown and scepter motifs, regal elegance",
        "Diamonds": "merchant wealth, gold and blue colors, coin and gem motifs, prosperity symbols",
        "Clubs": "nature and war, green and brown colors, tree and weapon motifs, natural elements",
        "Spades": "military power, black and silver colors, sword and shield motifs, martial strength"
    }
    
    client = ComfyUIClient(get_server_url())
    results = []
    
    for suit_name, suit_theme in suits.items():
        print(f"\n🎨 Generating {suit_name} Background...")
        
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
                filename_prefix=f"Card_Background_{suit_name}_Improved"
            )
            
            if generated_files:
                print(f"✅ Generated: {generated_files[0]}")
                results.append({
                    "suit": suit_name,
                    "filename": generated_files[0],
                    "seed": prompt_config["seed"],
                    "success": True
                })
            else:
                print(f"❌ Failed {suit_name}: No files generated")
                results.append({
                    "suit": suit_name,
                    "success": False,
                    "error": "No files generated"
                })
                
        except Exception as e:
            print(f"❌ Exception with {suit_name}: {str(e)}")
            results.append({
                "suit": suit_name,
                "success": False,
                "error": str(e)
            })
        
        # Small delay between generations
        time.sleep(1)
    
    return results

def main():
    """Main function to regenerate all card backgrounds."""
    print("🎯 Regenerating All Card Backgrounds with Improved Layout")
    print("=" * 60)
    
    # Generate all backgrounds
    results = generate_all_backgrounds()
    
    # Print summary
    print("\n📊 Generation Summary:")
    print("=" * 30)
    
    successful = [r for r in results if r["success"]]
    failed = [r for r in results if not r["success"]]
    
    print(f"✅ Successful generations: {len(successful)}")
    print(f"❌ Failed generations: {len(failed)}")
    
    if successful:
        print("\n🎉 Successfully Generated Backgrounds:")
        for i, result in enumerate(successful, 1):
            print(f"  {i}. {result['suit']} (seed: {result.get('seed', 'N/A')}) → {result['filename']}")
    
    if failed:
        print("\n⚠️ Failed Backgrounds:")
        for i, result in enumerate(failed, 1):
            print(f"  {i}. {result['suit']} → {result['error']}")
    
    print("\n🔍 All backgrounds generated with consistent layouts and proper cutouts!")
    print("📁 Check the generated_images/ folder for the new background files.")

if __name__ == "__main__":
    main()
