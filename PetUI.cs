using Godot;
using System;

public partial class PetUI : CanvasLayer
{
    #region Referencias de UI
    [Export] private ProgressBar _hungerBar;
    [Export] private ProgressBar _happinessBar;
    [Export] private ProgressBar _healthBar;
    [Export] private Label _coinsLabel;
    #endregion

    #region Estado y Referencias
    private PetState _petState;
    #endregion

    #region Ciclo de Vida
    public override void _Ready()
    {
        // Obtener referencia al estado global
        _petState = GetNode<PetState>("/root/PetState");

        // Suscribirse a cambios en las estadísticas
        _petState.StatsChanged += OnPetStatsChanged;

        // Inicializar la UI con los valores actuales
        OnPetStatsChanged(_petState.Hunger, _petState.Happiness, _petState.Health, _petState.Coins);

        // Configurar efectos de sonido para los botones de la interfaz
        SetupButtonSounds();
    }

    public override void _ExitTree()
    {
        // Desuscribirse para evitar memory leaks o llamadas a objetos destruidos
        if (_petState != null)
        {
            _petState.StatsChanged -= OnPetStatsChanged;
        }
    }
    #endregion

    #region Configuración
    private void SetupButtonSounds()
    {
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
            }
        }
    }
    #endregion

    #region Manejadores de Eventos
    /// <summary>
    /// Actualiza los elementos visuales cuando cambian las estadísticas de la mascota.
    /// </summary>
    public void OnPetStatsChanged(int hunger, int happiness, int health, int coins)
    {
        // Verificar validez de la instancia antes de intentar actualizar
        if (!IsInstanceValid(_hungerBar)) return;

        if (_hungerBar != null) _hungerBar.Value = hunger;
        if (_happinessBar != null) _happinessBar.Value = happiness;
        if (_healthBar != null) _healthBar.Value = health;
        
        if (_coinsLabel != null)
        {
            _coinsLabel.Text = $"Coins: {coins}";
        }
    }
    #endregion
}