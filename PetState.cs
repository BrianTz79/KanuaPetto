using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PetState : Node
{
    #region Señales
    [Signal]
    public delegate void StatsChangedEventHandler(int hunger, int happiness, int health, int coins);
    #endregion

    #region Propiedades de Estado (Stats)
    private int _hunger = 80;
    public int Hunger
    {
        get => _hunger;
        set { _hunger = Mathf.Clamp(value, 0, 100); EmitStatsChanged(); }
    }

    private int _happiness = 80;
    public int Happiness
    {
        get => _happiness;
        set { _happiness = Mathf.Clamp(value, 0, 100); EmitStatsChanged(); }
    }

    private int _health = 80;
    public int Health
    {
        get => _health;
        set { _health = Mathf.Clamp(value, 0, 100); EmitStatsChanged(); }
    }

    private int _coins = 150;
    public int Coins
    {
        get => _coins;
        set { _coins = value; EmitStatsChanged(); }
    }

    // Afinidad y Personalidad
    private int _affinity = 0;
    public int Affinity
    {
        get => _affinity;
        set { _affinity = Mathf.Clamp(value, -100, 100); EmitStatsChanged(); }
    }

    public enum PersonalityType { Grumpy, Normal, Happy }
    public PersonalityType CurrentPersonality
    {
        get
        {
            if (_affinity < -30) return PersonalityType.Grumpy;
            if (_affinity > 30) return PersonalityType.Happy;
            return PersonalityType.Normal;
        }
    }
    #endregion

    #region Configuración Global
    // Tasas de disminución de stats
    public float HungerDecreaseRate { get; set; } = 10.0f;
    public float HappinessDecreaseRate { get; set; } = 10.0f;
    public float HealthDecreaseRate { get; set; } = 8.0f;
    #endregion

    #region Inventario y Base de Datos
    public Dictionary<string, FoodItem> AllFoodItems { get; private set; } = new();
    public Dictionary<string, int> PlayerInventory { get; private set; } = new();
    #endregion

    #region Inicialización
    public override void _Ready()
    {
        LoadAllFoodItems();
    }

    private void LoadAllFoodItems()
    {
        // --- Carga de Base de Datos de Ítems ---
        
        // Ítems Originales
        var apple = new FoodItem { ItemID = "apple", ItemName = "Manzana", Description = "Roja y crujiente.", Price = 10, Texture = GD.Load<Texture2D>("res://sprites/food/manzana.png"), HungerRestore = 15, HappinessRestore = 5, HealthRestore = 1 };
        var cake = new FoodItem { ItemID = "cake", ItemName = "Pastel", Description = "Delicioso.", Price = 25, Texture = GD.Load<Texture2D>("res://sprites/food/pastel.png"), HungerRestore = 30, HappinessRestore = 20, HealthRestore = -5 };
        
        // Ítems Nuevos
        var cookie = new FoodItem { ItemID = "cookie", ItemName = "Galletas", Description = "Con chispas.", Price = 8, Texture = GD.Load<Texture2D>("res://sprites/food/cookie.png"), HungerRestore = 10, HappinessRestore = 8, HealthRestore = -1 };
        var chocoMilk = new FoodItem { ItemID = "chocomilk", ItemName = "Leche Cho.", Description = "Refrescante.", Price = 12, Texture = GD.Load<Texture2D>("res://sprites/food/chocomilk.png"), HungerRestore = 10, HappinessRestore = 15, HealthRestore = 5 };
        var croissant = new FoodItem { ItemID = "croissant", ItemName = "Croissant", Description = "Suave y francés.", Price = 15, Texture = GD.Load<Texture2D>("res://sprites/food/croissant.png"), HungerRestore = 20, HappinessRestore = 10, HealthRestore = 2 };
        var birria = new FoodItem { ItemID = "birria", ItemName = "Birria", Description = "Levanta muertos.", Price = 45, Texture = GD.Load<Texture2D>("res://sprites/food/birria.png"), HungerRestore = 50, HappinessRestore = 25, HealthRestore = 10 };
        var tamal = new FoodItem { ItemID = "tamal", ItemName = "Tamal", Description = "Calientito.", Price = 18, Texture = GD.Load<Texture2D>("res://sprites/food/tamal.png"), HungerRestore = 35, HappinessRestore = 10, HealthRestore = 5 };
        var tacos = new FoodItem { ItemID = "tacos", ItemName = "Tacos", Description = "Con todo.", Price = 30, Texture = GD.Load<Texture2D>("res://sprites/food/tacos.png"), HungerRestore = 40, HappinessRestore = 20, HealthRestore = 5 };
        var burrito = new FoodItem { ItemID = "burro", ItemName = "Burrito", Description = "Gigante.", Price = 28, Texture = GD.Load<Texture2D>("res://sprites/food/burro.png"), HungerRestore = 45, HappinessRestore = 15, HealthRestore = 3 };
        var maruchan = new FoodItem { ItemID = "maruchan", ItemName = "Sopa Inst.", Description = "Rápida.", Price = 5, Texture = GD.Load<Texture2D>("res://sprites/food/maruchan.png"), HungerRestore = 25, HappinessRestore = 2, HealthRestore = -5 };

        // Añadir al diccionario maestro
        AllFoodItems.Add(apple.ItemID, apple);
        AllFoodItems.Add(cake.ItemID, cake);
        AllFoodItems.Add(cookie.ItemID, cookie);
        AllFoodItems.Add(chocoMilk.ItemID, chocoMilk);
        AllFoodItems.Add(croissant.ItemID, croissant);
        AllFoodItems.Add(birria.ItemID, birria);
        AllFoodItems.Add(tamal.ItemID, tamal);
        AllFoodItems.Add(tacos.ItemID, tacos);
        AllFoodItems.Add(burrito.ItemID, burrito);
        AllFoodItems.Add(maruchan.ItemID, maruchan);
    }
    #endregion

    #region Lógica de Juego (Acciones)
    public bool BuyFood(string itemID)
    {
        if (!AllFoodItems.TryGetValue(itemID, out FoodItem itemToBuy))
        {
            GD.PrintErr($"Error: Item '{itemID}' no encontrado.");
            return false;
        }

        if (Coins >= itemToBuy.Price)
        {
            Coins -= itemToBuy.Price;

            if (PlayerInventory.ContainsKey(itemID)) PlayerInventory[itemID]++;
            else PlayerInventory.Add(itemID, 1);

            GD.Print($"Compra exitosa: {itemToBuy.ItemName}");
            GetNode<NetworkManager>("/root/NetworkManager").SaveGame();
            return true;
        }
        
        GD.Print("Monedas insuficientes.");
        return false;
    }

    public bool ConsumeFood(string itemID)
    {
        if (!PlayerInventory.TryGetValue(itemID, out int quantity) || quantity <= 0)
        {
            return false; // No hay inventario
        }

        if (!AllFoodItems.TryGetValue(itemID, out FoodItem itemToEat))
        {
            return false; // Item no existe en BD
        }

        PlayerInventory[itemID]--;

        Hunger += itemToEat.HungerRestore;
        Happiness += itemToEat.HappinessRestore;
        Health += itemToEat.HealthRestore;

        GD.Print($"Consumido: {itemToEat.ItemName}");
        GetNode<NetworkManager>("/root/NetworkManager").SaveGame();

        return true;
    }

    public string GetFirstFoodItemID()
    {
        return PlayerInventory.FirstOrDefault(item => item.Value > 0).Key;
    }

    public void ChangeAffinity(int amount)
    {
        Affinity += amount;
        GD.Print($"Afinidad actual: {Affinity} ({CurrentPersonality})");

        GetNode<NetworkManager>("/root/NetworkManager").SaveGame();

        if (amount > 0)
        {
            GetNode<AudioManager>("/root/AudioManager").PlaySFXPoly("res://audio/levelup.wav");
        }
    }
    #endregion

    #region Utilidades
    public void EmitStatsChanged()
    {
        CallDeferred(MethodName.EmitSignal, SignalName.StatsChanged, Hunger, Happiness, Health, Coins);
    }
    #endregion
}