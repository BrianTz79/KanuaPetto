using Godot;
using System;

public partial class MinigameSelection : Control
{
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