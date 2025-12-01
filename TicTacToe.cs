using Godot;
using System;
using System.Linq;

public partial class TicTacToe : Control
{
    #region Variables de Estado y Referencias
    // Referencias a nodos de la UI y Estado Global
    private PetState _petState;
    private Label _statusLabel;
    
    // Variables lógicas del juego
    private Button[] _gridButtons = new Button[9];
    private bool _playerTurn = true;
    private bool _gameOver = false;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    #endregion

    #region Métodos de Inicialización
    public override void _Ready()
    {
        // Inicializar referencias globales y de UI
        _petState = GetNode<PetState>("/root/PetState");
        _statusLabel = GetNode<Label>("MainContainer/StatusLabel");
        
        var exitBtn = GetNode<Button>("MainContainer/ExitButton");
        var grid = GetNode<GridContainer>("MainContainer/BoardBackground/GridContainer");

        // Configurar botón de salida
        exitBtn.Pressed += () => GetTree().ChangeSceneToFile("res://minigame_selection.tscn");

        // Configurar la cuadrícula de botones (Tablero)
        for (int i = 0; i < 9; i++)
        {
            _gridButtons[i] = grid.GetChild<Button>(i);
            _gridButtons[i].CustomMinimumSize = new Vector2(100, 100); 
            
            int index = i; // Copia local para la lambda
            _gridButtons[i].Pressed += () => 
            {
                GetNode<AudioManager>("/root/AudioManager").PlaySFX("res://audio/click.wav");
                OnCellPressed(index);
            };
        }

        // Configurar sonidos genéricos para botones restantes (como ExitButton)
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        foreach (var node in FindChildren("*", "Button", true, false))
        {
            if (node is Button btn)
            {
                // Evitar duplicar la señal si ya fue conectada manualmente
                if (!btn.IsConnected(Button.SignalName.Pressed, Callable.From(() => audioManager.PlaySFX("res://audio/click.wav"))))
                {
                    btn.Pressed += () => audioManager.PlaySFX("res://audio/click.wav");
                }
            }
        }

        ResetGame();
    }
    #endregion

    #region Lógica del Juego (Core)
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
        // Validar si la celda es válida
        if (_gameOver || !_playerTurn || _gridButtons[index].Text != "") return;

        // Turno del Jugador (X)
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

        // Turno de la IA (O)
        _playerTurn = false;
        _statusLabel.Text = "Pensando...";
        
        // Pequeño retraso para simular pensamiento
        GetTree().CreateTimer(0.5f).Timeout += OmniAITurn;
    }

    private void OmniAITurn()
    {
        if (_gameOver) return;

        // IA: Selecciona una casilla vacía al azar
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
    #endregion

    #region Verificación de Estado (Victoria/Empate)
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
        // Si no hay botones vacíos, es empate
        return _gridButtons.All(b => b.Text != "");
    }
    #endregion

    #region Finalización y Recompensas
    private void EndGame(bool playerWon, bool draw = false)
    {
        _gameOver = true;
        var audio = GetNode<AudioManager>("/root/AudioManager");
        
        if (playerWon)
        {
            _statusLabel.Text = "¡GANASTE! +20 Monedas";
            audio.PlaySFX("res://audio/win.wav");
            audio.PlaySFXPoly("res://audio/coin.wav");
            
            // Otorgar recompensas
            _petState.Coins += 20;
            _petState.Happiness += 5; 
            
            // Guardar progreso en la nube
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
        
        // Reiniciar el juego después de 2 segundos
        GetTree().CreateTimer(2.0f).Timeout += ResetGame;
    }
    #endregion
}