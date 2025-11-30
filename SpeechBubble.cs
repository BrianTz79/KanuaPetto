using Godot;

public partial class SpeechBubble : PanelContainer
{
    private Label _label;
    private Timer _hideTimer;

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
        Visible = false; // Oculto por defecto
        
        // Timer para que el texto desaparezca solo a los 3 segundos
        _hideTimer = new Timer();
        _hideTimer.WaitTime = 3.0f;
        _hideTimer.OneShot = true;
        _hideTimer.Timeout += () => Visible = false;
        AddChild(_hideTimer);
    }

    public void ShowMessage(string text)
    {
        _label.Text = text;
        Visible = true;
        _hideTimer.Start();
    }
}