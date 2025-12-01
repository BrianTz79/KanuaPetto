using Godot;
using System;

public partial class MinigameSelection : Control
{
    #region Ciclo de Vida
    public override void _Ready()
    {
        SetupButtonSounds();
    }

    private void SetupButtonSounds()
    {
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                // Evitar duplicar conexiones
                if (!btn.IsConnected(Button.SignalName.Pressed, Callable.From(() => audioManager.PlaySFX("res://audio/click.wav"))))
                {
                    btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
                }
            }
        }
    }
    #endregion

    #region Navegación
    public void _on_back_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
    }

    public void _on_tic_tac_toe_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://TicTacToe.tscn");
    }
    #endregion
}