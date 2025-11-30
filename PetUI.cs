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

    private PetState _petState; // <-- ¡Guarda la referencia!

    public override void _Ready()
    {
        // 1. Obtenemos el Singleton y LO GUARDAMOS
        _petState = GetNode<PetState>("/root/PetState");

        // 2. Nos conectamos a SU señal
        _petState.StatsChanged += OnPetStatsChanged;

        // 3. Forzamos una actualización inicial
        OnPetStatsChanged(_petState.Hunger, _petState.Happiness, _petState.Health, _petState.Coins);
    }

    // --- ¡¡AQUÍ ESTÁ LA SOLUCIÓN!! ---
    public override void _ExitTree()
    {
        // Esta función se llama justo antes de que el nodo sea destruido
        // (por ejemplo, al cambiar de escena).
        
        // Nos aseguramos de que _petState no sea nulo y nos desconectamos.
        if (_petState != null)
        {
            _petState.StatsChanged -= OnPetStatsChanged;
        }
    }
    // --- Fin de la Solución ---


    public void OnPetStatsChanged(int hunger, int happiness, int health, int coins)
    {
        // Esta comprobación es buena, pero la verdadera solución está en _ExitTree
        if (!IsInstanceValid(_hungerBar))
        {
             // Si la barra ya no existe (porque estamos saliendo de la escena),
             // no intentes actualizarla.
             return;
        }

        // La línea 38, que daba error, está aquí.
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
            _coinsLabel.Text = "Coins: " + coins.ToString();
        }
    }
}