using Godot;
using System;
using System.Linq; // Necesario para revisar listas

public partial class TicTacToe : Control
{
    private Button[] _gridButtons = new Button[9];
    private Label _statusLabel;
    private bool _playerTurn = true;
    private bool _gameOver = false;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    // Referencia al estado para dar dinero
    private PetState _petState;

    public override void _Ready()
    {
        _petState = GetNode<PetState>("/root/PetState");
        
        // --- RUTAS ACTUALIZADAS ---
        // Buscamos dentro de MainContainer
        _statusLabel = GetNode<Label>("MainContainer/StatusLabel"); 
        var exitBtn = GetNode<Button>("MainContainer/ExitButton");
        var grid = GetNode<GridContainer>("MainContainer/BoardBackground/GridContainer");
        // ---------------------------

        exitBtn.Pressed += () => GetTree().ChangeSceneToFile("res://minigame_selection.tscn");

        for (int i = 0; i < 9; i++)
        {
            _gridButtons[i] = grid.GetChild<Button>(i);
            // Hacemos que los botones se expandan para verse bien
            _gridButtons[i].CustomMinimumSize = new Vector2(100, 100); 
            int index = i; 

            _gridButtons[i].Pressed += () => {
                GetNode<AudioManager>("/root/AudioManager").PlaySFX("res://audio/click.wav");
                OnCellPressed(index);
            };



        }

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

        ResetGame();
    }

    private void ResetGame()
    {
        _playerTurn = true;
        _gameOver = false;
        _statusLabel.Text = "¡Tu turno! (X)";
        
        foreach (var btn in _gridButtons)
        {
            btn.Text = "";
            btn.Disabled = false;
        }
    }

    private void OnCellPressed(int index)
    {
        if (_gameOver || !_playerTurn || _gridButtons[index].Text != "") return;

        // Turno del Jugador
        _gridButtons[index].Text = "X";
        
        if (CheckWin("X"))
        {
            EndGame(true);
            return;
        }
        
        if (IsDraw())
        {
            EndGame(false, true);
            return;
        }

        _playerTurn = false;
        _statusLabel.Text = "Pensando...";
        
        // Simular pensamiento de la IA (pequeña pausa)
        GetTree().CreateTimer(0.5f).Timeout += OmniAITurn;
    }

    private void OmniAITurn()
    {
        if (_gameOver) return;

        // IA simple: Elige una casilla vacía al azar
        var emptyIndices = _gridButtons
            .Select((btn, index) => new { btn, index })
            .Where(x => x.btn.Text == "")
            .Select(x => x.index)
            .ToList();

        if (emptyIndices.Count > 0)
        {
            int pick = emptyIndices[_rng.RandiRange(0, emptyIndices.Count - 1)];
            _gridButtons[pick].Text = "O";

            if (CheckWin("O"))
            {
                EndGame(false); // Perdió el jugador
            }
            else if (IsDraw())
            {
                EndGame(false, true); // Empate
            }
            else
            {
                _playerTurn = true;
                _statusLabel.Text = "¡Tu turno! (X)";
            }
        }
    }

    private bool CheckWin(string mark)
    {
        // Combinaciones ganadoras (índices 0-8)
        int[][] wins = new int[][] 
        {
            new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8}, // Horizontales
            new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8}, // Verticales
            new[] {0,4,8}, new[] {2,4,6}                 // Diagonales
        };

        foreach (var w in wins)
        {
            if (_gridButtons[w[0]].Text == mark &&
                _gridButtons[w[1]].Text == mark &&
                _gridButtons[w[2]].Text == mark)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsDraw()
    {
        return _gridButtons.All(b => b.Text != "");
    }

    private void EndGame(bool playerWon, bool draw = false)
    {
        _gameOver = true;
        var audio = GetNode<AudioManager>("/root/AudioManager");
        
        if (playerWon)
        {
            _statusLabel.Text = "¡GANASTE! +20 Monedas";
            audio.PlaySFX("res://audio/win.wav");
            audio.PlaySFXPoly("res://audio/coin.wav");
            // --- AQUÍ DAMOS LA RECOMPENSA ---
            _petState.Coins += 20;
            _petState.Happiness += 5; 
            
            // ¡Importante! Guardar en la nube para asegurar el dinero
            GetNode<NetworkManager>("/root/NetworkManager").SaveGame();
        }
        else if (draw)
        {
            _statusLabel.Text = "Empate. +5 Monedas";
            audio.PlaySFX("res://audio/coin.wav");
            _petState.Coins += 5;
            GetNode<NetworkManager>("/root/NetworkManager").SaveGame();
        }
        else
        {
            _statusLabel.Text = "Perdiste... Intenta de nuevo.";
        }
        
        // Reiniciar después de 2 segundos
        GetTree().CreateTimer(2.0f).Timeout += ResetGame;
    }
}