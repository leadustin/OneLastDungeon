public enum ItemCategory
{
    None,
    All,        // Zeigt alles
    Weapon,     // Waffen (Nahkampf & Magie)
    Offhand,    // Schilde, Bücher, Relikte
    Armor,      // Rüstung
    Accessory,  // Schmuck
    Material,   // Crafting
    Consumable, // Tränke, Essen
    Quest       // Quest-Items
}

public enum ItemSubType
{
    None,

    // --- Physische Waffen ---
    Sword,
    Axe,
    Hammer,
    Spear,
    Dagger,
    Club,   // Keule

    // --- Fernkampf / Magische Waffen ---
    Bow,
    Staff,  // Großer Magierstab (z.B. Zweihand)
    Wand,   // Kleiner Zauberstab (z.B. Einhand)

    // --- Offhand ---
    Shield,
    Book,
    Relic,

    // --- Rüstung ---
    Head,
    Shoulder,
    Chest,
    Bracer,
    Gloves,
    Belt,
    Legs,
    Feet,

    // --- Schmuck ---
    Necklace,
    Ring,

    // --- Sonstiges ---
    Material,
    Potion,
    Food
}