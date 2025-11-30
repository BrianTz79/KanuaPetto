using Godot;
using System;
using Godot.Collections; // Necesario para Diccionarios de Godot y JSON

public partial class NetworkManager : Node
{
    // URL de tu servidor
    private const string BASE_URL = "https://kp.stellarbanana.com";
    
    // Señales
    [Signal] public delegate void LoginSuccessEventHandler();
    [Signal] public delegate void LoginFailedEventHandler(string message);

    private string _sessionToken = "";
    private PetState _petState;

    private const string SESSION_FILE = "user://session.save";

    public override void _Ready()
    {
        _petState = GetNode<PetState>("/root/PetState");
    }

    // --- FUNCIÓN 1: INICIAR SESIÓN ---
    public void Login(string username, string password)
    {
        var loginData = new Dictionary<string, Variant>
        {
            { "username", username },
            { "password", password }
        };

        string jsonString = Json.Stringify(loginData);

        SendRequest("/login", jsonString, (result, body) => 
        {
            // ÉXITO
            if (result.ContainsKey("token"))
            {
                _sessionToken = (string)result["token"];
                SaveSessionToken(_sessionToken);
                GD.Print("¡Login exitoso! Token recibido.");
                
                // Cargar datos si existen
                if (result.ContainsKey("petState") && result["petState"].Obj != null)
                {
                    var cloudState = result["petState"].AsGodotDictionary();
                    ApplyCloudData(cloudState);
                }
                
                EmitSignal(SignalName.LoginSuccess);
            }
            else
            {
                EmitSignal(SignalName.LoginFailed, "Respuesta inválida del servidor");
            }
        }, 
        (errorMsg) => 
        {
            EmitSignal(SignalName.LoginFailed, errorMsg);
        });
    }

    // --- FUNCIÓN 2: REGISTRARSE (¡Esta faltaba!) ---
    public void Register(string username, string password)
    {
        var registerData = new Dictionary<string, Variant>
        {
            { "username", username },
            { "password", password }
        };

        SendRequest("/register", Json.Stringify(registerData), (result, body) => 
        {
            // Reutilizamos la señal de fallo para mandar el mensaje de éxito (un truco rápido)
            // o simplemente imprimimos.
            GD.Print("Registro exitoso");
            // Para que la UI se entere, emitimos Failed con un mensaje positivo, 
            // o podrías crear una señal nueva RegisterSuccess.
            EmitSignal(SignalName.LoginFailed, "¡Registro exitoso! Por favor inicia sesión.");
        }, 
        (errorMsg) => 
        {
            EmitSignal(SignalName.LoginFailed, "Error registro: " + errorMsg);
        });
    }

    // --- FUNCIÓN 3: GUARDAR PARTIDA ---
    public void SaveGame()
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            GD.PrintErr("No se puede guardar: No hay sesión iniciada.");
            return;
        }

        // Convertimos el inventario de C# a Godot Dictionary manualmente
        // para evitar errores de conversión de tipos
        var godotInventory = new Godot.Collections.Dictionary();
        foreach(var item in _petState.PlayerInventory)
        {
            godotInventory.Add(item.Key, item.Value);
        }

        var stateData = new Dictionary<string, Variant>
        {
            { "Hunger", _petState.Hunger },
            { "Happiness", _petState.Happiness },
            { "Health", _petState.Health },
            { "Coins", _petState.Coins },
            { "Affinity", _petState.Affinity },
            { "Inventory", godotInventory } 

        };

        var payload = new Dictionary<string, Variant>
        {
            { "token", _sessionToken },
            { "petState", stateData }
        };

        SendRequest("/sync", Json.Stringify(payload), (result, body) => 
        {
            GD.Print("☁️ Partida guardada en la nube exitosamente.");
        });
    }

    // --- AYUDANTES ---
    private void ApplyCloudData(Godot.Collections.Dictionary data)
    {
        // CORRECCIÓN: Usamos .AsSingle() (float) directamente del Variant
        // Casteamos a (int) porque nuestras stats son enteros, pero JSON suele enviar floats (ej: 80.0)
        
        if (data.ContainsKey("hunger")) _petState.Hunger = (int)data["hunger"].AsSingle();
        if (data.ContainsKey("happiness")) _petState.Happiness = (int)data["happiness"].AsSingle();
        if (data.ContainsKey("health")) _petState.Health = (int)data["health"].AsSingle();
        if (data.ContainsKey("coins")) _petState.Coins = (int)data["coins"].AsSingle();
        if (data.ContainsKey("affinity")) _petState.Affinity = (int)data["affinity"].AsSingle();

        // Cargar inventario
        if (data.ContainsKey("inventory"))
        {
            _petState.PlayerInventory.Clear();
            
            // Verificamos que no sea nulo antes de convertir
            if (data["inventory"].Obj != null)
            {
                var cloudInv = data["inventory"].AsGodotDictionary();
                
                foreach (var item in cloudInv)
                {
                    // item.Value también es un Variant, así que usamos .AsInt32() o .AsSingle()
                    // Usamos AsSingle por seguridad por si viene como 5.0
                    _petState.PlayerInventory.Add((string)item.Key, (int)item.Value.AsSingle());
                }
            }
        }
        
        // Forzamos actualización de UI
        _petState.EmitStatsChanged();
    }

    private void SendRequest(string endpoint, string jsonBody, Action<Godot.Collections.Dictionary, byte[]> onSuccess, Action<string> onFailure = null)
    {
        // CORRECCIÓN AQUÍ: HttpRequest en lugar de HTTPRequest
        var httpRequest = new HttpRequest(); 
        AddChild(httpRequest);

        httpRequest.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) =>
        {
            if (responseCode == 200 || responseCode == 201)
            {
                var json = new Json();
                var parseResult = json.Parse(System.Text.Encoding.UTF8.GetString(body));
                
                if (parseResult == Error.Ok)
                {
                    var responseDict = json.Data.AsGodotDictionary();
                    onSuccess?.Invoke(responseDict, body);
                }
                else
                {
                    onFailure?.Invoke("Error al procesar respuesta del servidor");
                }
            }
            else
            {
                string errorMsg = $"Error {responseCode}";
                try {
                     var json = new Json();
                     json.Parse(System.Text.Encoding.UTF8.GetString(body));
                     var responseDict = json.Data.AsGodotDictionary();
                     if(responseDict.ContainsKey("error")) errorMsg = (string)responseDict["error"];
                } catch {}
                
                GD.PrintErr($"API Error: {errorMsg}");
                onFailure?.Invoke(errorMsg);
            }
            httpRequest.QueueFree(); // Limpieza
        };

        string[] requestHeaders = { "Content-Type: application/json" };
        
        // CORRECCIÓN AQUÍ TAMBIÉN (Si te da error en HttpClient): 
        // Asegúrate de que HttpClient sea 'Godot.HttpClient' si hay conflicto, 
        // pero generalmente 'HttpClient.Method.Post' funciona bien si tienes 'using Godot;'
        httpRequest.Request(BASE_URL + endpoint, requestHeaders, HttpClient.Method.Post, jsonBody);
    }


    public bool CheckAutoLogin()
    {
        if (FileAccess.FileExists(SESSION_FILE))
        {
            using var file = FileAccess.Open(SESSION_FILE, FileAccess.ModeFlags.Read);
            string savedToken = file.GetAsText();
            
            if (!string.IsNullOrEmpty(savedToken))
            {
                GD.Print("Token encontrado, intentando auto-login...");
                _sessionToken = savedToken;
                LoadGameFromToken();
                return true; // ¡Sí encontramos archivo, espera la respuesta HTTP!
            }
        }
        
        // Si llegamos aquí, es que no hay archivo o está vacío.
        return false; 
    }


    private void LoadGameFromToken()
    {
        var httpRequest = new HttpRequest();
        AddChild(httpRequest);
        
        httpRequest.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) =>
        {
            if (responseCode == 200)
            {
                var json = new Json();
                json.Parse(System.Text.Encoding.UTF8.GetString(body));
                var responseDict = json.Data.AsGodotDictionary();
                
                if (responseDict.ContainsKey("petState"))
                {
                    ApplyCloudData(responseDict["petState"].AsGodotDictionary());
                    EmitSignal(SignalName.LoginSuccess);
                }
            }
            else
            {
                // EL TOKEN YA NO SIRVE (O expiró)
                GD.Print("Token expirado o inválido.");
                
                // Borramos el archivo para no intentarlo de nuevo la próxima vez
                if (FileAccess.FileExists(SESSION_FILE))
                {
                     DirAccess.RemoveAbsolute(SESSION_FILE);
                }

                // ¡IMPORTANTE! Avisamos a la UI que falló el auto-login
                // para que quite el texto "Verificando..."
                EmitSignal(SignalName.LoginFailed, "Sesión caducada, inicia de nuevo.");
            }
            httpRequest.QueueFree();
        };

        string[] requestHeaders = { 
            "Content-Type: application/json",
            $"Authorization: Bearer {_sessionToken}" 
        };
        httpRequest.Request(BASE_URL + "/load", requestHeaders, HttpClient.Method.Get);
    }

    private void SaveSessionToken(string token)
    {
        using var file = FileAccess.Open(SESSION_FILE, FileAccess.ModeFlags.Write);
        file.StoreString(token);
    }




}