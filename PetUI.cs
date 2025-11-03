using Godot;
using System;

public partial class PetUI : CanvasLayer
{
    [Export] 
    private ProgressBar _hungerBar;

    [Export] 
    private ProgressBar _happinessBar;

    [Export] 
    private ProgressBar _healthBar;
    
    [Export] 
    private Label _coinsLabel;

    // Esta es la línea clave que hay que corregir.
    // Asegúrate de que acepte los cuatro parámetros.
    public void OnPetStatsChanged(int hunger, int happiness, int health, int coins)
    {
        if (_hungerBar != null)
        {
            _hungerBar.Value = hunger;
        }
        if (_happinessBar != null)
        {
            _happinessBar.Value = happiness;
        }
        if (_healthBar != null)
        {
            _healthBar.Value = health;
        }
        if (_coinsLabel != null)
        {
            _coinsLabel.Text = coins.ToString();
        }
    }
}