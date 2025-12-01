using Godot;
using System;
using Godot.Collections; // Necesario para Diccionarios de Godot y JSON

public partial class NetworkManager : Node
{
    #region Configuración y Eventos
    private const string BASE_URL = "https://kp.stellarbanana.com";
    private const string SESSION_FILE = "user://session.save";

    [Signal] public delegate void LoginSuccessEventHandler();
    [Signal] public delegate void LoginFailedEventHandler(string message);
    #endregion

    #region Estado
    private string _sessionToken = "";
    private PetState _petState;
    #endregion

    #region Inicialización
    public override void _Ready()
    {
        _petState = GetNode<PetState>("/root/PetState");
    }
    #endregion

    #region Autenticación (Login/Registro)
    public void Login(string username, string password)
    {
        var loginData = new Dictionary<string, Variant>
        {
            { "username", username },
            { "password", password }
        };

        SendRequest("/login", Json.Stringify(loginData), (result, body) => 
        {
            if (result.ContainsKey("token"))
            {
                // Guardar token en memoria y disco
                _sessionToken = (string)result["token"];
                SaveSessionToken(_sessionToken);
                
                GD.Print("¡Login exitoso! Token recibido.");
                
                // Cargar datos de la mascota si existen en la respuesta
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
        (errorMsg) => EmitSignal(SignalName.LoginFailed, errorMsg));
    }

    public void Register(string username, string password)
    {
        var registerData = new Dictionary<string, Variant>
        {
            { "username", username },
            { "password", password }
        };

        SendRequest("/register", Json.Stringify(registerData), (result, body) => 
        {
            GD.Print("Registro exitoso");
            EmitSignal(SignalName.LoginFailed, "¡Registro exitoso! Por favor inicia sesión.");
        }, 
        (errorMsg) => EmitSignal(SignalName.LoginFailed, "Error registro: " + errorMsg));
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
                return true; // Se encontró archivo, esperando respuesta del servidor
            }
        }
        return false; // No hay sesión guardada
    }
    #endregion

    #region Sincronización de Datos (Guardar/Cargar)
    public void SaveGame()
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            GD.PrintErr("No se puede guardar: No hay sesión iniciada.");
            return;
        }

        // Convertir inventario a formato compatible con JSON
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
            { "affinity", _petState.Affinity }, // Minúscula para coincidir con backend
            { "Inventory", godotInventory } 
        };

        var payload = new Dictionary<string, Variant>
        {
            { "token", _sessionToken },
            { "petState", stateData }
        };

        SendRequest("/sync", Json.Stringify(payload), 
            (result, body) => GD.Print("☁️ Partida guardada en la nube exitosamente."),
            (error) => GD.PrintErr($"Error al guardar: {error}")
        );
    }

    private void LoadGameFromToken()
    {
        // Petición GET para recuperar datos usando solo el token
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
                GD.Print("Token expirado o inválido.");
                if (FileAccess.FileExists(SESSION_FILE))
                {
                     DirAccess.RemoveAbsolute(SESSION_FILE);
                }
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
    #endregion

    #region Utilidades Internas
    private void ApplyCloudData(Godot.Collections.Dictionary data)
    {
        // Deserialización segura de datos (Variant -> Tipos C#)
        if (data.ContainsKey("hunger")) _petState.Hunger = (int)data["hunger"].AsSingle();
        if (data.ContainsKey("happiness")) _petState.Happiness = (int)data["happiness"].AsSingle();
        if (data.ContainsKey("health")) _petState.Health = (int)data["health"].AsSingle();
        if (data.ContainsKey("coins")) _petState.Coins = (int)data["coins"].AsSingle();
        if (data.ContainsKey("affinity")) _petState.Affinity = (int)data["affinity"].AsSingle();

        if (data.ContainsKey("inventory") && data["inventory"].Obj != null)
        {
            _petState.PlayerInventory.Clear();
            var cloudInv = data["inventory"].AsGodotDictionary();
            
            foreach (var item in cloudInv)
            {
                _petState.PlayerInventory.Add((string)item.Key, (int)item.Value.AsSingle());
            }
        }
        
        _petState.EmitStatsChanged();
    }

    private void SendRequest(string endpoint, string jsonBody, Action<Godot.Collections.Dictionary, byte[]> onSuccess, Action<string> onFailure = null)
    {
        var httpRequest = new HttpRequest(); 
        AddChild(httpRequest);

        httpRequest.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) =>
        {
            // Códigos 200-299 son exitosos
            if (responseCode >= 200 && responseCode < 300)
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
            httpRequest.QueueFree();
        };

        string[] requestHeaders = { "Content-Type: application/json" };
        
        // Determinar método (GET si no hay cuerpo, POST si hay cuerpo)
        var method = string.IsNullOrEmpty(jsonBody) ? HttpClient.Method.Get : HttpClient.Method.Post;
        
        httpRequest.Request(BASE_URL + endpoint, requestHeaders, method, jsonBody);
    }

    private void SaveSessionToken(string token)
    {
        using var file = FileAccess.Open(SESSION_FILE, FileAccess.ModeFlags.Write);
        file.StoreString(token);
    }
    #endregion
}