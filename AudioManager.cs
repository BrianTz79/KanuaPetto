using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    #region Referencias y Estado
    private AudioStreamPlayer _musicPlayer;
    private AudioStreamPlayer _sfxPlayer;

    // Caché de sonidos para evitar cargas repetitivas desde disco
    private Dictionary<string, AudioStream> _sounds = new Dictionary<string, AudioStream>();
    #endregion

    #region Inicialización
    public override void _Ready()
    {
        _musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
        _sfxPlayer = GetNode<AudioStreamPlayer>("SFXPlayer");
    }
    #endregion

    #region Gestión de Recursos
    /// <summary>
    /// Pre-carga un sonido en memoria y lo asocia a una clave.
    /// </summary>
    public void LoadSound(string name, string path)
    {
        if (!_sounds.ContainsKey(name))
        {
            var stream = GD.Load<AudioStream>(path);
            _sounds.Add(name, stream);
        }
    }
    #endregion

    #region Reproducción
    public void PlayMusic(string path)
    {
        // Evitar reiniciar la música si ya está sonando la misma pista
        if (_musicPlayer.Stream != null && 
            _musicPlayer.Stream.ResourcePath == path && 
            _musicPlayer.Playing)
        {
            return;
        }

        var stream = GD.Load<AudioStream>(path);
        _musicPlayer.Stream = stream;
        _musicPlayer.Play();
    }

    /// <summary>
    /// Reproduce un efecto de sonido en el canal principal de SFX.
    /// </summary>
    public void PlaySFX(string nameOrPath)
    {
        AudioStream stream = null;

        // Intentar obtener de la caché o cargar directamente
        if (_sounds.ContainsKey(nameOrPath))
        {
            stream = _sounds[nameOrPath];
        }
        else 
        {
            stream = GD.Load<AudioStream>(nameOrPath);
        }

        if (stream != null)
        {
            _sfxPlayer.Stream = stream;
            _sfxPlayer.Play();
        }
    }
    
    /// <summary>
    /// Reproduce un sonido creando un reproductor temporal. 
    /// Útil para sonidos que se solapan (ej. monedas rápidas, comer).
    /// </summary>
    public void PlaySFXPoly(string path)
    {
        var tempPlayer = new AudioStreamPlayer();
        AddChild(tempPlayer);
        
        tempPlayer.Stream = GD.Load<AudioStream>(path);
        tempPlayer.Finished += tempPlayer.QueueFree; // Autodestrucción al terminar
        
        tempPlayer.Play();
    }
    #endregion
}