using Godot;
using System;

public partial class LoginScreen : Control
{
    private LineEdit _usernameInput;
    private LineEdit _passwordInput;
    private Label _statusLabel;
    private NetworkManager _networkManager;

    public override void _Ready()
    {
        // Ahora la ruta incluye el contenedor padre LoginBox
        _usernameInput = GetNode<LineEdit>("LoginBox/VBoxContainer/UsernameInput");
        _passwordInput = GetNode<LineEdit>("LoginBox/VBoxContainer/PasswordInput");
        _statusLabel = GetNode<Label>("LoginBox/VBoxContainer/StatusLabel");

        GetNode<Button>("LoginBox/VBoxContainer/LoginButton").Pressed += OnLoginPressed;
        GetNode<Button>("LoginBox/VBoxContainer/RegisterButton").Pressed += OnRegisterPressed;
        
        // Obtenemos el Singleton de Red
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");

        // Escuchamos las señales del NetworkManager
        _networkManager.LoginSuccess += OnLoginSuccess;
        _networkManager.LoginFailed += OnLoginFailed;

        GetNode<AudioManager>("/root/AudioManager").PlayMusic("res://audio/3-01. Main Theme.mp3");

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

        _statusLabel.Text = "Verificando sesión guardada...";
        bool haySesion = _networkManager.CheckAutoLogin();

        if (!haySesion)
        {
            // Si NO encontró archivo, borramos el mensaje de "Verificando..." inmediatamente
            // para que el usuario sepa que puede escribir.
            _statusLabel.Text = "Inicia sesión o regístrate.";
        }

    }

    private void OnLoginPressed()
    {
        _statusLabel.Text = "Conectando...";
        string user = _usernameInput.Text;
        string pass = _passwordInput.Text;

        // Llamamos a la función de Login que creamos antes
        _networkManager.Login(user, pass);
    }

    private void OnRegisterPressed()
    {
        _statusLabel.Text = "Registrando...";
        string user = _usernameInput.Text;
        string pass = _passwordInput.Text;
        
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            _statusLabel.Text = "Escribe usuario y contraseña";
            return;
        }

        // Llamamos a la nueva función
        _networkManager.Register(user, pass);
    }

    private void OnLoginSuccess()
    {
        _statusLabel.Text = "¡Éxito!";
        // Cambiar a la escena del juego principal
        GetTree().ChangeSceneToFile("res://main.tscn");
    }

    private void OnLoginFailed(string message)
    {
        _statusLabel.Text = $"Error: {message}";
    }
}