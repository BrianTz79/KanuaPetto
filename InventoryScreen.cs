using Godot;

public partial class InventoryScreen : CanvasLayer
{
    private GridContainer _itemGrid;
    private PetState _petState;
    private PackedScene _foodCardScene;

    public override void _Ready()
    {
        _petState = GetNode<PetState>("/root/PetState");
        _foodCardScene = GD.Load<PackedScene>("res://FoodCard.tscn");
        
        _itemGrid = GetNode<GridContainer>("ShopWindow/VBoxContainer/ScrollContainer/ItemGrid"); // El nombre sigue siendo ShopWindow porque duplicamos
        var backButton = GetNode<Button>("ShopWindow/VBoxContainer/BackButton");
        
        backButton.Pressed += OnBackButtonPressed;

        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        // Busca todos los botones de ESTA escena y conéctales el sonido
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                // Desconectamos primero por seguridad para no tener doble sonido
                if (btn.IsConnected(Button.SignalName.Pressed, Callable.From(() => audioManager.PlaySFX("res://audio/click.wav"))))
                    continue;
                    
                btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
            }
        }

        PopulateInventory();
    }

    private void PopulateInventory()
    {
        foreach (Node child in _itemGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Instanciamos una tarjeta SOLO por los items que el jugador TIENE
        foreach (var itemEntry in _petState.PlayerInventory)
        {
            string itemID = itemEntry.Key;
            int quantity = itemEntry.Value;

            // Solo mostrar si tiene 1 o más
            if (quantity > 0)
            {
                FoodItem foodItem = _petState.AllFoodItems[itemID];
                var card = _foodCardScene.Instantiate<FoodCard>();

                // false = no es de tienda, y pasamos la cantidad
                card.Setup(foodItem, false, quantity);
                card.CardPressed += OnFoodCardPressed;

                // No conectamos la señal "CardPressed",
                // porque el inventario es de solo lectura.

                _itemGrid.AddChild(card);
            }
        }
    }
    
    private void OnFoodCardPressed(string itemID)
    {
        // Le decimos al Singleton que consuma ESE item
        bool success = _petState.ConsumeFood(itemID);

        if (success)
        {
            GD.Print($"Consumido {itemID} desde el inventario.");
            
            // ¡Importante!
            // Volvemos a dibujar el inventario para
            // que se actualice la cantidad (o desaparezca el item).
            PopulateInventory();
        }
    }

    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
    }
}