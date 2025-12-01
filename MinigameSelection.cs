using Godot;
using System;

public partial class MinigameSelection : Control
{

    public override void _Ready()
    {
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        // Busca todos los botones de ESTA escena y conéctales el sonido
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                // Desconectamos primero por seguridad para no tener doble sonido
                if (btn.IsConnected(Button.SignalName.Pressed, Callable.From(() => audioManager.PlaySFX("res://audio/click.wav"))))
                    continue;
                    
                btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
            }
        }
    }




    // Esta función la conectaremos a la señal "pressed" del botón "Volver".
    public void _on_back_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://main.tscn");
        
    }


    public void _on_tic_tac_toe_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://TicTacToe.tscn");
    }

}