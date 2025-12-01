using Godot;
using System;

public partial class LoginScreen : Control
{
    #region Referencias de UI
    private LineEdit _usernameInput;
    private LineEdit _passwordInput;
    private Label _statusLabel;
    #endregion

    #region Referencias de Sistema
    private NetworkManager _networkManager;
    #endregion

    #region Ciclo de Vida
    public override void _Ready()
    {
        InitializeReferences();
        SetupUIEvents();
        SetupAudio();
        
        // Iniciar proceso de autenticación automática
        PerformAutoLoginCheck();
    }
    #endregion

    #region Configuración e Inicialización
    private void InitializeReferences()
    {
        // Obtener referencias a nodos de UI dentro del contenedor "LoginBox"
        _usernameInput = GetNode<LineEdit>("LoginBox/VBoxContainer/UsernameInput");
        _passwordInput = GetNode<LineEdit>("LoginBox/VBoxContainer/PasswordInput");
        _statusLabel = GetNode<Label>("LoginBox/VBoxContainer/StatusLabel");

        // Obtener referencia al gestor de red global
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");
        
        // Suscribirse a eventos de red
        _networkManager.LoginSuccess += OnLoginSuccess;
        _networkManager.LoginFailed += OnLoginFailed;
    }

    private void SetupUIEvents()
    {
        GetNode<Button>("LoginBox/VBoxContainer/LoginButton").Pressed += OnLoginPressed;
        GetNode<Button>("LoginBox/VBoxContainer/RegisterButton").Pressed += OnRegisterPressed;
    }

    private void SetupAudio()
    {
        var audioManager = GetNode<AudioManager>("/root/AudioManager");
        
        // Iniciar música de fondo
        audioManager.PlayMusic("res://audio/3-01. Main Theme.mp3");

        // Configurar efectos de sonido para todos los botones de la escena
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

    private void PerformAutoLoginCheck()
    {
        _statusLabel.Text = "Verificando sesión guardada...";
        
        // Intentar loguear automáticamente con token guardado
        bool haySesion = _networkManager.CheckAutoLogin();

        if (!haySesion)
        {
            // Si no hay sesión previa, invitar al usuario a interactuar
            _statusLabel.Text = "Inicia sesión o regístrate.";
        }
    }
    #endregion

    #region Manejadores de Eventos de UI
    private void OnLoginPressed()
    {
        _statusLabel.Text = "Conectando...";
        string user = _usernameInput.Text;
        string pass = _passwordInput.Text;

        // Validar campos vacíos si es necesario, o dejar que el servidor responda
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

        _networkManager.Register(user, pass);
    }
    #endregion

    #region Callbacks de Red
    private void OnLoginSuccess()
    {
        _statusLabel.Text = "¡Éxito!";
        // Transición a la escena principal del juego
        GetTree().ChangeSceneToFile("res://main.tscn");
    }

    private void OnLoginFailed(string message)
    {
        _statusLabel.Text = $"Error: {message}";
    }
    #endregion
}