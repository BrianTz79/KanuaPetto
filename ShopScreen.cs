using Godot;
using System;

public partial class ShopScreen : CanvasLayer
{
    #region Referencias
    private GridContainer _itemGrid;
    private PetState _petState;
    private PackedScene _foodCardScene;
    #endregion

    #region Inicialización
    public override void _Ready()
    {
        // Inicializar referencias
        _petState = GetNode<PetState>("/root/PetState");
        _foodCardScene = GD.Load<PackedScene>("res://FoodCard.tscn");
        
        _itemGrid = GetNode<GridContainer>("ShopWindow/VBoxContainer/ScrollContainer/ItemGrid");
        var backButton = GetNode<Button>("ShopWindow/VBoxContainer/BackButton");
        
        // Conectar señales
        backButton.Pressed += OnBackButtonPressed;

        // Configurar audio para todos los botones de la escena
        SetupButtonSounds();

        // Generar el contenido de la tienda
        PopulateShop();
    }

    private void SetupButtonSounds()
    {
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                // Evitar duplicar la conexión si ya existe
                if (!btn.IsConnected(Button.SignalName.Pressed, Callable.From(() => audioManager.PlaySFX("res://audio/click.wav"))))
                {
                    btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
                }
            }
        }
    }
    #endregion

    #region Lógica de la Tienda
    /// <summary>
    /// Limpia el grid actual e instancia una tarjeta por cada ítem disponible en PetState.
    /// </summary>
    private void PopulateShop()
    {
        // Limpiar elementos existentes
        foreach (Node child in _itemGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Instanciar tarjetas de productos
        foreach (var foodItem in _petState.AllFoodItems.Values)
        {
            var card = _foodCardScene.Instantiate<FoodCard>();
            
            // true indica que es modo tienda (mostrar precio y botón comprar)
            card.Setup(foodItem, true); 
            
            card.CardPressed += OnFoodCardPressed;
            
            _itemGrid.AddChild(card);
        }
    }

    private void OnFoodCardPressed(string itemID)
    {
        // Intentar realizar la compra a través del estado global
        bool success = _petState.BuyFood(itemID);
        
        if (success)
        {
            GD.Print($"Compra exitosa del item: {itemID}");
            // Aquí se podría añadir un feedback visual o sonoro adicional de "Caja Registradora"
        }
        else
        {
            GD.Print("Fondos insuficientes para realizar la compra.");
            // Aquí se podría añadir un sonido de error
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