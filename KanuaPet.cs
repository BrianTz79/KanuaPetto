using Godot;
using System;

public partial class KanuaPet : Node2D
{
    #region Referencias y Variables
    // Referencias a Nodos
    private PetState _petState;
    private AnimatedSprite2D _petAnimation;
    private Timer _idleTalkTimer;
    
    [Export] private SpeechBubble _speechBubble;

    // Temporizadores internos para la degradación de estadísticas
    private double _hungerTimer = 0.0;
    private double _happinessTimer = 0.0;
    private double _healthTimer = 0.0;
    #endregion

    #region Propiedades (Wrappers de Estado)
    // Estas propiedades actúan como puente directo al Singleton PetState
    public int Hunger
    {
        get => _petState.Hunger;
        set => _petState.Hunger = value;
    }

    public int Happiness
    {
        get => _petState.Happiness;
        set => _petState.Happiness = value;
    }

    public int Health
    {
        get => _petState.Health;
        set => _petState.Health = value;
    }

    public int Coins
    {
        get => _petState.Coins;
        set => _petState.Coins = value;
    }
    
    // Propiedades de solo lectura para tasas de disminución
    public float HungerDecreaseRate => _petState.HungerDecreaseRate;
    public float HappinessDecreaseRate => _petState.HappinessDecreaseRate;
    public float HealthDecreaseRate => _petState.HealthDecreaseRate;
    #endregion

    #region Ciclo de Vida
    public override void _Ready()
    {
        base._Ready();

        // Inicializar referencias
        _petAnimation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _petState = GetNode<PetState>("/root/PetState");
        _speechBubble = GetNode<SpeechBubble>("SpeechBubble"); // Referencia automática al nodo hijo

        // Configurar temporizador para diálogos aleatorios
        SetupIdleTalkTimer();
    }

    public override void _Process(double delta)
    {
        HandleStatDegradation(delta);
    }
    #endregion

    #region Lógica Interna
    private void SetupIdleTalkTimer()
    {
        _idleTalkTimer = new Timer();
        _idleTalkTimer.WaitTime = 15.0f; // Intervalo de intento de habla
        _idleTalkTimer.Timeout += OnIdleTalk;
        AddChild(_idleTalkTimer);
        _idleTalkTimer.Start();
    }

    private void HandleStatDegradation(double delta)
    {
        // Reducción de Hambre
        _hungerTimer += delta;
        if (_hungerTimer >= HungerDecreaseRate)
        {
            _hungerTimer = 0.0;
            Hunger -= 1; 
        }

        // Reducción de Felicidad
        _happinessTimer += delta;
        if (_happinessTimer >= HappinessDecreaseRate)
        {
            _happinessTimer = 0.0;
            Happiness -= 1; 
        }

        // Reducción de Salud (consecuencia del hambre extrema)
        if (Hunger == 0)
        {
            _healthTimer += delta;
            if (_healthTimer >= HealthDecreaseRate)
            {
                _healthTimer = 0.0;
                Health -= 1; 
                GD.PrintErr("¡La salud está bajando por hambre!");
            }
        }
    }

    private void OnIdleTalk()
    {
        // 30% de probabilidad de hablar cuando el timer se activa
        if (GD.Randf() < 0.3f)
        {
            var personality = _petState.CurrentPersonality;
            string msg = DialogueData.GetRandomPhrase(personality, DialogueData.IdlePhrases);
            _speechBubble.ShowMessage(msg);
        }
    }
    #endregion

    #region Interacciones (Alimentar)
    public void Feed()
    {
        // 1. Verificar inventario
        string firstFoodID = _petState.GetFirstFoodItemID();

        if (string.IsNullOrEmpty(firstFoodID))
        {
            // Feedback visual: No hay comida
            var personality = _petState.CurrentPersonality;
            string msg = DialogueData.GetRandomPhrase(personality, DialogueData.NoFoodPhrases);
            _speechBubble.ShowMessage(msg);
            return;
        }
        
        // 2. Intentar consumir
        bool success = _petState.ConsumeFood(firstFoodID);

        if (success)
        {
            // Visuales y Audio
            _petAnimation.Play("eat");
            GetNode<AudioManager>("/root/AudioManager").PlaySFXPoly("res://audio/eat.wav");

            // Lógica de juego
            _petState.ChangeAffinity(1);

            // Diálogo de reacción
             var personality = _petState.CurrentPersonality;
             string msg = DialogueData.GetRandomPhrase(personality, DialogueData.EatingPhrases);
             _speechBubble.ShowMessage(msg);
        }
        else
        {
            GD.PrintErr("Error inesperado al intentar consumir la comida.");
        }
    }
    #endregion

    #region Manejadores de Eventos (UI y Animación)
    public void _on_play_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://minigame_selection.tscn");
    }

    public void _on_shop_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://ShopScreen.tscn");
    }

    public void _on_inventory_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://InventoryScreen.tscn");
    }

    private void _on_animated_sprite_2d_animation_finished()
    {
        // Volver a la animación de reposo cuando termina de comer
        _petAnimation.Play("idle");
    }
    #endregion
}