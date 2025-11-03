using Godot;
using System;

public partial class KanuaPet : Node2D
{
    [Signal]
    public delegate void StatsChangedEventHandler(int hunger, int happiness, int health, int coins);

    [Export] public float HungerDecreaseRate { get; set; } = 5.0f;
    [Export] public float HappinessDecreaseRate { get; set; } = 10.0f;
    [Export] public float HealthDecreaseRate { get; set; } = 8.0f;

    private double _hungerTimer = 0.0;
    private double _happinessTimer = 0.0;
    private double _healthTimer = 0.0;

    private int _hunger = 100;
    private int _happiness = 100;
    private int _health = 100;
    private int _coins = 0;

    [Export(PropertyHint.Range, "0,100,1")]
    public int Hunger
    {
        get => _hunger;
        set
        {
            _hunger = Mathf.Clamp(value, 0, 100);
            // ¡Llamada corregida aquí!
            EmitSignal(SignalName.StatsChanged, _hunger, _happiness, _health, _coins);
        }
    }

    [Export(PropertyHint.Range, "0,100,1")]
    public int Happiness
    {
        get => _happiness;
        set
        {
            _happiness = Mathf.Clamp(value, 0, 100);
            // ¡Llamada corregida aquí!
            EmitSignal(SignalName.StatsChanged, _hunger, _happiness, _health, _coins);
        }
    }

    [Export(PropertyHint.Range, "0,100,1")]
    public int Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, 100);
            // ¡Llamada corregida aquí!
            EmitSignal(SignalName.StatsChanged, _hunger, _happiness, _health, _coins);
        }
    }

    [Export]
    public int Coins
    {
        get => _coins;
        set
        {
            _coins = value;
            // Esta llamada ya estaba correcta.
            EmitSignal(SignalName.StatsChanged, _hunger, _happiness, _health, _coins);
        }
    }

    private AnimatedSprite2D _petAnimation;

    public override void _Ready()
    {
        _petAnimation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        EmitSignal(SignalName.StatsChanged, Hunger, Happiness, Health, Coins);
    }

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
        _petAnimation.Play("eat");
        Hunger += 15;
        Happiness += 5;
        GD.Print("¡Kanua Pet ha sido alimentado!");
    }
    
    public void _on_play_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://minigame_selection.tscn");
    }

    private void _on_animated_sprite_2d_animation_finished()
    {
        _petAnimation.Play("idle");
    }
}