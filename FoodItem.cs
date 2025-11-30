using Godot;

[GlobalClass] // Esto permite a Godot reconocerlo como un tipo de Recurso
public partial class FoodItem : Resource
{
    [Export]
    public string ItemID { get; set; } // "apple", "cake", etc.

    [Export]
    public string ItemName { get; set; } // "Manzana", "Pastel"

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; }

    [Export]
    public int Price { get; set; }

    [Export]
    public Texture2D Texture { get; set; }

    [ExportGroup("Estadísticas")]
    [Export]
    public int HungerRestore { get; set; }
    [Export]
    public int HappinessRestore { get; set; }
    [Export]
    public int HealthRestore { get; set; }
}