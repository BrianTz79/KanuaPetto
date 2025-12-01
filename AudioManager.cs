using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    private AudioStreamPlayer _musicPlayer;
    private AudioStreamPlayer _sfxPlayer;

    // Diccionario para guardar los sonidos cargados y no leerlos del disco cada vez
    private Dictionary<string, AudioStream> _sounds = new Dictionary<string, AudioStream>();

    public override void _Ready()
    {
        _musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
        _sfxPlayer = GetNode<AudioStreamPlayer>("SFXPlayer");
        
        // Cargar sonidos comunes al inicio (Asegúrate de tener los archivos o comenta estas líneas)
        // Ejemplo: LoadSound("click", "res://audio/click.wav");
        // LoadSound("eat", "res://audio/eat.wav");
    }

    // Función para cargar un sonido y guardarlo con un nombre clave
    public void LoadSound(string name, string path)
    {
        if (!_sounds.ContainsKey(name))
        {
            var stream = GD.Load<AudioStream>(path);
            _sounds.Add(name, stream);
        }
    }

    public void PlayMusic(string path)
    {
        // Si ya está sonando esa misma canción, no la reinicies
        if (_musicPlayer.Stream != null && _musicPlayer.Stream.ResourcePath == path && _musicPlayer.Playing)
            return;

        var stream = GD.Load<AudioStream>(path);
        _musicPlayer.Stream = stream;
        _musicPlayer.Play();
        // Hacemos que la música se repita (Loop)
        // Nota: En Godot 4, el loop se configura en el archivo de importación del .mp3/.ogg, 
        // no por código. (Doble clic al archivo -> Import -> Loop -> Reimport).
    }

    public void PlaySFX(string nameOrPath)
    {
        AudioStream stream = null;

        // 1. Buscamos si ya lo cargamos por nombre clave (ej: "click")
        if (_sounds.ContainsKey(nameOrPath))
        {
            stream = _sounds[nameOrPath];
        }
        // 2. Si no, intentamos cargarlo como ruta directa (ej: "res://audio/coin.wav")
        else 
        {
            stream = GD.Load<AudioStream>(nameOrPath);
        }

        if (stream != null)
        {
            // Truco: Para que los sonidos no se corten entre sí (ej. clickear rápido),
            // lo ideal es tener varios players, pero para empezar usaremos el principal.
            // Si quieres que se solapen, tendrías que instanciar un nuevo AudioStreamPlayer aquí.
            
            _sfxPlayer.Stream = stream;
            _sfxPlayer.Play();
        }
    }
    
    // Función avanzada: Reproducir SFX sin cortar el anterior (Creando nodos temporales)
    public void PlaySFXPoly(string path)
    {
        var tempPlayer = new AudioStreamPlayer();
        AddChild(tempPlayer);
        tempPlayer.Stream = GD.Load<AudioStream>(path);
        tempPlayer.Finished += tempPlayer.QueueFree; // Se autodestruye al terminar
        tempPlayer.Play();
    }
}