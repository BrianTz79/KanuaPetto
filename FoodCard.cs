using Godot;
using System;

public partial class FoodCard : PanelContainer
{
    [Signal]
    public delegate void CardPressedEventHandler(string itemID);

    private TextureRect _icon;
    private Label _nameLabel;
    private Label _infoLabel;
    private Button _actionButton;

    private string _itemID;
    private FoodItem _foodItem;

    // Este es el método principal para configurar la tarjeta
    public void Setup(FoodItem foodItem, bool isShop, int quantity = 0)
    {
        // Obtenemos las referencias de los nodos AHORA,
        _icon = GetNode<TextureRect>("VBoxContainer/Icon");
        _nameLabel = GetNode<Label>("VBoxContainer/NameLabel");
        _infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");
        _actionButton = GetNode<Button>("VBoxContainer/ActionButton");

        _itemID = foodItem.ItemID;
        _foodItem = foodItem;

        _icon.Texture = foodItem.Texture;
        _nameLabel.Text = foodItem.ItemName;
        
        // --- ¡ARREGLO AQUÍ! ---
        // Simplemente eliminamos la línea de desconexión.
        // Solo conectamos la señal una vez.
        _actionButton.Pressed += OnButtonPressed;

        if (isShop)
        {
            _infoLabel.Text = $"Precio: {foodItem.Price} monedas";
            _actionButton.Text = "Comprar";
        }
        else
        {
            _infoLabel.Text = $"H: +{foodItem.HungerRestore} | F: +{foodItem.HappinessRestore}\n" +
                              $"S: +{foodItem.HealthRestore}\n" +
                              $"Tienes: {quantity}";
            _actionButton.Text = "Usar";
        }
    }

    private void OnButtonPressed()
    {
        EmitSignal(SignalName.CardPressed, _itemID);
    }
}