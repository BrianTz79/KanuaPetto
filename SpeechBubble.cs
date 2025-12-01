using Godot;
using System;

public partial class SpeechBubble : PanelContainer
{
    #region Configuración
    // Tiempo en segundos que el mensaje permanecerá visible
    [Export] 
    public float DisplayDuration { get; set; } = 3.0f;
    #endregion

    #region Referencias
    private Label _label;
    private Timer _hideTimer;
    #endregion

    #region Métodos de Inicialización
    public override void _Ready()
    {
        // Obtener referencias a nodos hijos
        _label = GetNode<Label>("Label");
        
        // Estado inicial: Oculto
        Visible = false; 
        
        // Configurar el temporizador interno
        SetupHideTimer();
    }

    private void SetupHideTimer()
    {
        _hideTimer = new Timer();
        _hideTimer.WaitTime = DisplayDuration;
        _hideTimer.OneShot = true; // Solo se ejecuta una vez por llamada
        
        // Lambda: Al terminar el tiempo, ocultar la burbuja
        _hideTimer.Timeout += () => Visible = false;
        
        AddChild(_hideTimer);
    }
    #endregion

    #region Lógica Pública
    /// <summary>
    /// Muestra un mensaje de texto en la burbuja y reinicia el temporizador de ocultamiento.
    /// </summary>
    /// <param name="text">El mensaje a mostrar.</param>
    public void ShowMessage(string text)
    {
        if (_label != null)
        {
            _label.Text = text;
        }

        // Mostrar la burbuja
        Visible = true;
        
        // Aseguramos que el tiempo esté actualizado (por si se cambió en el editor)
        _hideTimer.WaitTime = DisplayDuration;
        _hideTimer.Start();
    }
    #endregion
}