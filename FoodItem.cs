using Godot;

/// <summary>
/// Recurso que define los datos y estadísticas de un alimento en el juego.
/// </summary>
[GlobalClass] 
public partial class FoodItem : Resource
{
    #region Identificación y Descripción
    [Export] public string ItemID { get; set; }
    [Export] public string ItemName { get; set; }
    
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; }
    #endregion

    #region Economía y Visuales
    [Export] public int Price { get; set; }
    [Export] public Texture2D Texture { get; set; }
    #endregion

    #region Estadísticas de Efecto
    [ExportGroup("Efectos en Estadísticas")]
    [Export] public int HungerRestore { get; set; }
    [Export] public int HappinessRestore { get; set; }
    [Export] public int HealthRestore { get; set; }
    #endregion
}