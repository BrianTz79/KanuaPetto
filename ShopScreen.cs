using Godot;

public partial class ShopScreen : CanvasLayer
{
    private GridContainer _itemGrid;
    private PetState _petState;
    private PackedScene _foodCardScene;

    public override void _Ready()
    {
        // Cargar recursos
        _petState = GetNode<PetState>("/root/PetState");
        _foodCardScene = GD.Load<PackedScene>("res://FoodCard.tscn");
        
        // Referencias de nodos
        _itemGrid = GetNode<GridContainer>("ShopWindow/VBoxContainer/ScrollContainer/ItemGrid");
        var backButton = GetNode<Button>("ShopWindow/VBoxContainer/BackButton");
        
        // Conectar señales
        backButton.Pressed += OnBackButtonPressed;

        // Generar la tienda
        PopulateShop();
    }

    private void PopulateShop()
    {
        // Limpiamos la tienda por si acaso
        foreach (Node child in _itemGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Instanciamos una tarjeta por cada item en la "base de datos"
        foreach (var foodItem in _petState.AllFoodItems.Values)
        {
            var card = _foodCardScene.Instantiate<FoodCard>();
            card.Setup(foodItem, true); // true = es tarjeta de tienda
            
            // Conectamos la señal de la tarjeta a nuestra función
            card.CardPressed += OnFoodCardPressed;
            
            _itemGrid.AddChild(card);
        }
    }

    private void OnFoodCardPressed(string itemID)
    {
        // La lógica de compra está en el Singleton,
        // nosotros solo le decimos QUÉ comprar.
        bool success = _petState.BuyFood(itemID);
        
        if (success)
        {
            GD.Print("¡Compra exitosa!");
            // (Aquí podrías poner un sonido de "caja registradora")
        }
        else
        {
            GD.Print("¡Fondos insuficientes!");
            // (Aquí podrías poner un sonido de "error")
        }
    }

    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
    }
}