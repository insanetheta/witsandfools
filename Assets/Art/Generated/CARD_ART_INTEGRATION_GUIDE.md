# Wits and Fools - Card Art Integration Guide

## Generated Card Artwork Summary

This directory contains 11 AI-generated card artworks created using ComfyUI for the Wits and Fools Renaissance card game. All images are generated in **512x768 pixels** (portrait format) with a Renaissance theme inspired by Queen's Blood and Tetra Master aesthetics.

## Special Ability Cards (10)

### 1. Double_Defense.png

-   **Ability:** Allows the defender to counter two attacking cards with one if they match rank or suit
-   **Visual:** Twin shields overlapping with defensive magical aura, ornate medieval armor protection with golden reinforcement

### 2. Double_Trouble.png

-   **Ability:** Allows the attacker to play two additional cards of any rank
-   **Visual:** Twin swords crossed with magical duplicating effect, ornate medieval weapons with sparkling aura

### 3. Extra_Draw.png

-   **Ability:** Forces the defender to draw an extra card before defending
-   **Visual:** Magical quill pen drawing cards from the air, ornate feathered quill with mystical ink creating floating playing cards

### 4. Shield_Card.png

-   **Ability:** The defender can skip their turn, passing the attack to the next player
-   **Visual:** Ornate medieval shield with protective magical aura, golden heraldic design

### 5. Skip_Turn.png

-   **Ability:** The next player is skipped
-   **Visual:** Ornate medieval hourglass with sand frozen in time, magical stasis field with glowing runes

### 6. The_Blocker.png

-   **Ability:** Prevents the defender from adding any more cards to the attack
-   **Visual:** Massive stone wall with magical barrier glowing runes, medieval fortress defense mechanism

### 7. The_Magnet.png

-   **Ability:** Attracts all cards of the same rank from the deck
-   **Visual:** Powerful magical lodestone attracting floating playing cards, mystical magnetic field with swirling energy

### 8. The_Reverser.png

-   **Ability:** Reverses turn order
-   **Visual:** Mystical hourglass with swirling temporal magic reversing time, ornate clockwork mechanism with magical gears

### 9. Trump_Changer.png

-   **Ability:** Changes the trump suit to the suit of the played card
-   **Visual:** Magical crown with swirling suit symbols (hearts, diamonds, clubs, spades), ornate royal regalia with transformative magic

### 10. Wildcard.png

-   **Ability:** Can be played as any rank or suit
-   **Visual:** Mystical chameleon card shifting between all four suits, transformative magical aura with rainbow colors

## Standard Playing Cards (1)

### 11. King_of_Hearts.png

-   **Card Type:** Standard playing card (King of Hearts)
-   **Visual:** Ornate royal portrait with crown and scepter, medieval noble character

## Unity Integration Instructions

### Step 1: Import Images to Unity

1. Copy all PNG files from `Assets/Art/Generated/Cards/` to your Unity project
2. Select all images in Unity Project window
3. In Inspector, set:
    - **Texture Type:** Sprite (2D and UI)
    - **Sprite Mode:** Single
    - **Pixels Per Unit:** 100 (adjust based on your card size requirements)
    - **Filter Mode:** Bilinear
    - **Compression:** Normal Quality

### Step 2: Create Card Art Assets

1. Create a new folder: `Assets/Art/CardArt/`
2. Move the imported sprites to this folder
3. Create ScriptableObject instances for each card linking to the appropriate sprite

### Step 3: Update CardData ScriptableObjects

For special ability cards, create new CardData instances:

```csharp
// Example for Shield Card
[CreateAssetMenu(fileName = "ShieldCard", menuName = "Cards/Special Ability Card")]
public class ShieldCardData : CardData
{
    public override CardAbilityType AbilityType => CardAbilityType.Shield;
    public override string CardName => "Shield Card";
    public override string Description => "The defender can skip their turn, passing the attack to the next player";
    // Set cardArt field to Shield_Card sprite
}
```

### Step 4: Update CardRenderer System

Modify `CardRenderer.cs` to use the new artwork:

```csharp
public class CardRenderer : MonoBehaviour
{
    [SerializeField] private Image cardArtImage;
    [SerializeField] private Image backgroundImage;

    public void SetCardArt(Sprite artwork)
    {
        if (cardArtImage != null && artwork != null)
        {
            cardArtImage.sprite = artwork;
            cardArtImage.preserveAspect = true;
        }
    }

    public void RenderCard(CardData cardData)
    {
        if (cardData != null && cardData.cardArt != null)
        {
            SetCardArt(cardData.cardArt);
        }
        // ... rest of rendering logic
    }
}
```

### Step 5: Card Prefab Updates

1. Open your Card prefab
2. Add an Image component for card artwork (child of main card)
3. Set appropriate size and positioning for the artwork
4. Ensure artwork appears behind text elements but above background

### Step 6: Testing the Integration

1. Create test CardData assets using the new artwork
2. Update DemoCardCreator to use the new cards
3. Test in Play Mode to ensure artwork displays correctly
4. Adjust sizing and positioning as needed

## Art Style Consistency

All generated artwork follows these design principles:

-   **Renaissance Theme:** Medieval/Renaissance era aesthetic
-   **Rich Textures:** Detailed fantasy illustrations with ornate elements
-   **Political Intrigue:** Tavern/court setting backgrounds
-   **Queen's Blood Style:** Inspired by FF7 Rebirth card game aesthetics
-   **Consistent Lighting:** Warm, ambient lighting suitable for tavern settings

## File Specifications

-   **Format:** PNG with transparency support
-   **Resolution:** 512x768 pixels (portrait orientation)
-   **Aspect Ratio:** 2:3 (standard playing card ratio)
-   **Color Space:** sRGB
-   **Compression:** Lossless PNG

## Future Expansion

When the ComfyUI server becomes available again, additional cards can be generated:

### Remaining Standard Cards Needed:

-   Queen of Spades
-   Jack of Clubs
-   Ace of Diamonds
-   Additional face cards and number cards as needed

### Suggested Generation Prompts:

```
"Renaissance era Queen of Spades, elegant noble lady with ornate crown and royal dress, card game artwork style like Queen's Blood, detailed fantasy illustration, rich textures, political intrigue theme, tavern setting background"

"Renaissance era Jack of Clubs, young knight with club symbol weapon, medieval courtier character, card game artwork style like Queen's Blood, detailed fantasy illustration, rich textures, political intrigue theme, tavern setting background"

"Renaissance era Ace of Diamonds, magnificent jeweled diamond centerpiece with magical sparkles, ornate gemstone design, card game artwork style like Queen's Blood, detailed fantasy illustration, rich textures, political intrigue theme, tavern setting background"
```

## Technical Notes

-   All images are ready for Unity import without additional processing
-   Artwork is designed to work with both light and dark card backgrounds
-   Images include sufficient contrast for readability of overlay text
-   Art style is consistent across all special ability cards
-   Standard cards follow traditional playing card character archetypes

## Attribution

-   Generated using ComfyUI AI image generation
-   Prompts designed for Wits and Fools Renaissance card game
-   Style inspiration: Queen's Blood (FF7 Rebirth), Tetra Master (FF9)
-   Created: August 16, 2025

---

**Ready for Integration:** All 10 special ability cards + 1 standard card are complete and ready for Unity implementation!
