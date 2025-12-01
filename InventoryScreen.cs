using Godot;
using System;

public partial class InventoryScreen : CanvasLayer
{
    #region Referencias
    private GridContainer _itemGrid;
    private PetState _petState;
    private PackedScene _foodCardScene;
    #endregion

    #region Inicialización
    public override void _Ready()
    {
        // Inicializar referencias globales y recursos
        _petState = GetNode<PetState>("/root/PetState");
        _foodCardScene = GD.Load<PackedScene>("res://FoodCard.tscn");
        
        // Inicializar referencias de UI
        // Nota: La ruta sigue siendo ShopWindow porque reutilizamos la estructura de la escena
        _itemGrid = GetNode<GridContainer>("ShopWindow/VBoxContainer/ScrollContainer/ItemGrid");
        var backButton = GetNode<Button>("ShopWindow/VBoxContainer/BackButton");
        
        backButton.Pressed += OnBackButtonPressed;

        // Configurar efectos de sonido para botones
        SetupButtonSounds();

        // Cargar el inventario visualmente
        PopulateInventory();
    }

    private void SetupButtonSounds()
    {
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                // Evitar duplicar conexiones
                if (!btn.IsConnected(Button.SignalName.Pressed, Callable.From(() => audioManager.PlaySFX("res://audio/click.wav"))))
                {
                    btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
                }
            }
        }
    }
    #endregion

    #region Lógica de Inventario
    /// <summary>
    /// Limpia y regenera la lista de ítems basada en el inventario actual del jugador.
    /// </summary>
    private void PopulateInventory()
    {
        // Limpiar elementos existentes
        foreach (Node child in _itemGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Instanciar tarjetas solo para ítems que el jugador posee (cantidad > 0)
        foreach (var itemEntry in _petState.PlayerInventory)
        {
            string itemID = itemEntry.Key;
            int quantity = itemEntry.Value;

            if (quantity > 0)
            {
                // Obtener datos del ítem desde la base de datos maestra
                FoodItem foodItem = _petState.AllFoodItems[itemID];
                
                var card = _foodCardScene.Instantiate<FoodCard>();

                // false indica modo inventario (mostrar botón "Usar" y cantidad)
                card.Setup(foodItem, false, quantity);
                
                card.CardPressed += OnFoodCardPressed;

                _itemGrid.AddChild(card);
            }
        }
    }
    
    private void OnFoodCardPressed(string itemID)
    {
        // Intentar consumir el ítem a través del estado global
        bool success = _petState.ConsumeFood(itemID);

        if (success)
        {
            GD.Print($"Consumido ítem: {itemID}");
            
            // Actualizar la visualización para reflejar la nueva cantidad
            // o eliminar el ítem si se acabó.
            PopulateInventory();
        }
    }
    #endregion

    #region Navegación
    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
    }
    #endregion
}