#!/usr/bin/env python3
"""
Quick script to check available models on the ComfyUI server.
"""

import urllib.request
import json
from config_loader import get_server_url

def check_available_models():
    server_url = get_server_url()
    
    print("🔍 Checking Available Models...")
    print("=" * 50)
    
    try:
        with urllib.request.urlopen(f"{server_url}/object_info", timeout=10) as response:
            object_info = json.loads(response.read())
        
        if "CheckpointLoaderSimple" in object_info:
            checkpoint_info = object_info["CheckpointLoaderSimple"]
            if "input" in checkpoint_info and "required" in checkpoint_info["input"]:
                if "ckpt_name" in checkpoint_info["input"]["required"]:
                    available_models = checkpoint_info["input"]["required"]["ckpt_name"][0]
                    
                    print(f"📋 Found {len(available_models)} models:")
                    print()
                    
                    # Categorize models for better selection
                    realistic_models = []
                    product_models = []
                    artistic_models = []
                    other_models = []
                    
                    for model in available_models:
                        model_lower = model.lower()
                        if any(keyword in model_lower for keyword in ['realistic', 'photo', 'real', 'vision']):
                            realistic_models.append(model)
                        elif any(keyword in model_lower for keyword in ['product', 'commercial', 'studio']):
                            product_models.append(model)
                        elif any(keyword in model_lower for keyword in ['disney', 'cartoon', 'anime', 'art', 'dream']):
                            artistic_models.append(model)
                        else:
                            other_models.append(model)
                    
                    if realistic_models:
                        print("🎯 REALISTIC/PHOTO MODELS (Best for clean backgrounds):")
                        for model in realistic_models[:10]:  # Show top 10
                            print(f"   • {model}")
                        print()
                    
                    if product_models:
                        print("📸 PRODUCT PHOTOGRAPHY MODELS:")
                        for model in product_models[:5]:
                            print(f"   • {model}")
                        print()
                    
                    if artistic_models:
                        print("🎨 ARTISTIC/STYLIZED MODELS:")
                        for model in artistic_models[:5]:
                            print(f"   • {model}")
                        print()
                    
                    if other_models:
                        print("📦 OTHER MODELS:")
                        for model in other_models[:5]:
                            print(f"   • {model}")
                        print()
                    
                    # Recommend best models for transparency
                    print("💡 RECOMMENDED FOR TRANSPARENCY:")
                    recommendations = []
                    
                    for model in available_models:
                        model_lower = model.lower()
                        if 'realistic' in model_lower or 'photo' in model_lower:
                            recommendations.append(model)
                    
                    if not recommendations:
                        # Fallback recommendations
                        for model in available_models:
                            if any(keyword in model.lower() for keyword in ['v1-5', 'stable', 'base']):
                                recommendations.append(model)
                    
                    for i, model in enumerate(recommendations[:3]):
                        print(f"   {i+1}. {model}")
                    
                    return available_models, recommendations[:3]
                    
    except Exception as e:
        print(f"❌ Error: {e}")
        return [], []

if __name__ == "__main__":
    models, recommendations = check_available_models()
    
    if recommendations:
        print(f"\n🎯 Current model: v1-5-pruned-emaonly.ckpt")
        print(f"🔄 Try these instead: {recommendations[0]}")
