using Godot;
using System;

public partial class KanuaPet : Node2D
{
    private PetState _petState;

    private double _hungerTimer = 0.0;
    private double _happinessTimer = 0.0;
    private double _healthTimer = 0.0;

    [Export] private SpeechBubble _speechBubble;

    private Timer _idleTalkTimer;

    // --- ¡CAMBIO AQUÍ! ---
    // Ya no es [Export]. Godot ya no intentará ponerle
    // el valor "90" del archivo .tscn antes de _Ready().
    public int Hunger
    {
        get => _petState.Hunger;
        set => _petState.Hunger = value; // ¡Así de simple!
    }

    public int Happiness
    {
        get => _petState.Happiness;
        set => _petState.Happiness = value; // ¡Así de simple!
    }

    public int Health
    {
        get => _petState.Health;
        set => _petState.Health = value; // ¡Así de simple!
    }

    public int Coins
    {
        get => _petState.Coins;
        set => _petState.Coins = value; // ¡Así de simple!
    }
    
    // Estos están bien, los leemos del Singleton
    public float HungerDecreaseRate { get => _petState.HungerDecreaseRate; }
    public float HappinessDecreaseRate { get => _petState.HappinessDecreaseRate; }
    public float HealthDecreaseRate { get => _petState.HealthDecreaseRate; }


    private AnimatedSprite2D _petAnimation;

    public override void _Ready()
    {
        _petAnimation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        
        // 1. AHORA _Ready() se ejecuta PRIMERO
        _petState = GetNode<PetState>("/root/PetState");

        // --- ¡AGREGA ESTA LÍNEA! ---
        // Buscamos el nodo hijo llamado "SpeechBubble" automáticamente
        _speechBubble = GetNode<SpeechBubble>("SpeechBubble");

        base._Ready(); // Llamar al base si es necesario
        
        // Timer para hablar aleatoriamente cada 10-20 segundos
        _idleTalkTimer = new Timer();
        _idleTalkTimer.WaitTime = 15.0f;
        _idleTalkTimer.Timeout += OnIdleTalk;
        AddChild(_idleTalkTimer);
        _idleTalkTimer.Start();
    
    }

    private void OnIdleTalk()
    {
        // 30% de probabilidad de hablar cuando está quieto
        if (GD.Randf() < 0.3f)
        {
            var personality = _petState.CurrentPersonality;
            string msg = DialogueData.GetRandomPhrase(personality, DialogueData.IdlePhrases);
            _speechBubble.ShowMessage(msg);
        }
    }
    
    // ... (El resto del script _Process, Feed, etc. está perfecto) ...
    
    public override void _Process(double delta)
    {
        _hungerTimer += delta;
        if (_hungerTimer >= HungerDecreaseRate)
        {
            _hungerTimer = 0.0;
            Hunger -= 1; 
        }

        _happinessTimer += delta;
        if (_happinessTimer >= HappinessDecreaseRate)
        {
            _happinessTimer = 0.0;
            Happiness -= 1; 
        }

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

    public void Feed()
    {
        // 1. Preguntamos al Singleton cuál es la primera comida que tenemos
        string firstFoodID = _petState.GetFirstFoodItemID();

        if (string.IsNullOrEmpty(firstFoodID))
        {
            GD.Print("¡No hay comida en el inventario! Visita la tienda.");
            // (Aquí podrías mostrar un mensaje en la UI)
            return;
        }
        
        // 2. Le decimos al Singleton que consuma esa comida
        bool success = _petState.ConsumeFood(firstFoodID);

        if (success)
        {
            // 3. Si se pudo consumir, reproducimos la animación
            _petAnimation.Play("eat");

            _petState.ChangeAffinity(1);

            // Decir frase de comer
             var personality = _petState.CurrentPersonality;
             string msg = DialogueData.GetRandomPhrase(personality, DialogueData.EatingPhrases);
             _speechBubble.ShowMessage(msg);
            
            // Las estadísticas se actualizan solas porque ConsumeFood()
            // cambia los valores en PetState, lo que dispara
            // las propiedades 'set' que llaman a EmitSignal().
        }
        else
        {
            // Esto no debería pasar si GetFirstFoodItemID() funcionó,
            // pero es bueno tenerlo.
            GD.PrintErr("Error al intentar consumir la comida.");
        }
    }
    
    public void _on_play_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://minigame_selection.tscn");
    }

    private void _on_animated_sprite_2d_animation_finished()
    {
        _petAnimation.Play("idle");
    }

    public void _on_shop_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://ShopScreen.tscn");
    }

    public void _on_inventory_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://InventoryScreen.tscn");
    }
}