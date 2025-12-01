using Godot;
using System;

public partial class FoodCard : PanelContainer
{
    #region Señales
    [Signal]
    public delegate void CardPressedEventHandler(string itemID);
    #endregion

    #region Referencias de UI
    private TextureRect _icon;
    private Label _nameLabel;
    private Label _infoLabel;
    private Button _actionButton;
    #endregion

    #region Datos
    private string _itemID;
    private FoodItem _foodItem;
    #endregion

    #region Configuración
    /// <summary>
    /// Configura la tarjeta con los datos del ítem y el modo de visualización (Tienda o Inventario).
    /// </summary>
    public void Setup(FoodItem foodItem, bool isShop, int quantity = 0)
    {
        // Inicializar referencias a nodos hijos
        _icon = GetNode<TextureRect>("VBoxContainer/Icon");
        _nameLabel = GetNode<Label>("VBoxContainer/NameLabel");
        _infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");
        _actionButton = GetNode<Button>("VBoxContainer/ActionButton");

        // Asignar datos internos
        _itemID = foodItem.ItemID;
        _foodItem = foodItem;

        // Configurar elementos visuales básicos
        _icon.Texture = foodItem.Texture;
        _nameLabel.Text = foodItem.ItemName;

        // Conectar señal del botón (evitando duplicados)
        if (!_actionButton.IsConnected(Button.SignalName.Pressed, Callable.From(OnButtonPressed)))
        {
            _actionButton.Pressed += OnButtonPressed;
        }

        // Configurar textos según el contexto
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
    #endregion

    #region Manejadores de Eventos
    private void OnButtonPressed()
    {
        EmitSignal(SignalName.CardPressed, _itemID);
    }
    #endregion
}