using Godot; // <-- Para Node, GD, Mathf, [Signal], Texture2D, etc.
using System; // <-- Para funcionalidad básica de C#
using System.Collections.Generic; // <-- Para Dictionary<,>
using System.Linq; // <-- Para .FirstOrDefault()

public partial class PetState : Node
{
// Señal para notificar a la UI
    [Signal]
    public delegate void StatsChangedEventHandler(int hunger, int happiness, int health, int coins);

    // --- ¡CAMBIO IMPORTANTE! ---
    // Convertimos las estadísticas en propiedades completas
    // para que emitan la señal cuando cambien.
    
    private int _hunger = 80;
    public int Hunger
    {
        get => _hunger;
        set
        {
            _hunger = Mathf.Clamp(value, 0, 100);
            EmitStatsChanged(); // ¡Emitir señal!
        }
    }

    private int _happiness = 80;
    public int Happiness
    {
        get => _happiness;
        set
        {
            _happiness = Mathf.Clamp(value, 0, 100);
            EmitStatsChanged(); // ¡Emitir señal!
        }
    }

    private int _health = 80;
    public int Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, 100);
            EmitStatsChanged(); // ¡Emitir señal!
        }
    }

    private int _coins = 150;
    public int Coins
    {
        get => _coins;
        set
        {
            _coins = value;
            EmitStatsChanged(); // ¡Emitir señal!
        }
    }

    private int _affinity = 0;
    public int Affinity
    {
        get => _affinity;
        set
        {
            _affinity = Mathf.Clamp(value, -100, 100);
            EmitStatsChanged(); 
        }
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
    
    // Función para modificar afinidad (usar esta en lugar de set directo)
    public void ChangeAffinity(int amount)
    {
        Affinity += amount;
        GD.Print($"Afinidad cambiada: {amount}. Total: {Affinity} ({CurrentPersonality})");

        GetNode<NetworkManager>("/root/NetworkManager").SaveGame();
    }



    // Tasas de disminución (como antes)
    public float HungerDecreaseRate { get; set; } = 10.0f;
    public float HappinessDecreaseRate { get; set; } = 10.0f;
    public float HealthDecreaseRate { get; set; } = 8.0f;

    // --- ¡NUEVA SECCIÓN DE INVENTARIO Y TIENDA! ---

    // 1. La "Base de Datos" de todos los items que existen en el juego.
    // La clave (string) es el ItemID ("apple", "cake").
    public Dictionary<string, FoodItem> AllFoodItems { get; private set; } = new();

    // 2. El inventario REAL del jugador.
    // La clave (string) es el ItemID, el valor (int) es la CANTIDAD que tiene.
    // ¡ESTO es lo que guardarás en tu base de datos online!
    public Dictionary<string, int> PlayerInventory { get; private set; } = new();

    public override void _Ready()
    {
        // Cargamos la "base de datos" de items.
        // Más adelante, puedes cargar esto desde archivos .tres en lugar de codificarlo.
        LoadAllFoodItems();
    }

    private void LoadAllFoodItems()
    {
        // Vamos a crear 2 comidas de ejemplo.
        // (¡Necesitarás crear las imágenes para "res://sprites/food/apple.png"!)

        var apple = new FoodItem
        {
            ItemID = "apple",
            ItemName = "Manzana",
            Description = "Una manzana roja y crujiente.",
            Price = 10,
            Texture = GD.Load<Texture2D>("res://sprites/food/apple.png"), // ¡Asegúrate de tener esta imagen!
            HungerRestore = 15,
            HappinessRestore = 5,
            HealthRestore = 1
        };

        var cake = new FoodItem
        {
            ItemID = "cake",
            ItemName = "Pastel",
            Description = "Un delicioso pedazo de pastel.",
            Price = 25,
            Texture = GD.Load<Texture2D>("res://sprites/food/cake.png"), // ¡Asegúrate de tener esta imagen!
            HungerRestore = 30,
            HappinessRestore = 20,
            HealthRestore = -5 // ¡El azúcar no es saludable!
        };

        AllFoodItems.Add(apple.ItemID, apple);
        AllFoodItems.Add(cake.ItemID, cake);
    }

    // --- Métodos para interactuar con la Tienda y el Inventario ---

    public bool BuyFood(string itemID)
    {
        if (!AllFoodItems.TryGetValue(itemID, out FoodItem itemToBuy))
        {
            GD.PrintErr($"Error: Item ID '{itemID}' no encontrado.");
            return false;
        }

        if (Coins >= itemToBuy.Price)
        {
            // Restar monedas
            Coins -= itemToBuy.Price;

            // Añadir al inventario
            if (PlayerInventory.ContainsKey(itemID))
            {
                PlayerInventory[itemID]++; // Aumenta la cantidad
            }
            else
            {
                PlayerInventory.Add(itemID, 1); // Añade el item por primera vez
            }

            GD.Print($"¡Comprado: {itemToBuy.ItemName}! Tienes {PlayerInventory[itemID]}.");
            GetNode<NetworkManager>("/root/NetworkManager").SaveGame();
            // Notificar a la UI que las monedas cambiaron
            return true;
        }
        else
        {
            GD.Print("¡No tienes suficientes monedas!");
            return false;
        }
    }

    public string GetFirstFoodItemID()
    {
        // Busca el primer item en el inventario que tenga cantidad > 0
        return PlayerInventory.FirstOrDefault(item => item.Value > 0).Key;
        // Devolverá null si el inventario está vacío
    }

    public bool ConsumeFood(string itemID)
    {
        if (!PlayerInventory.TryGetValue(itemID, out int quantity) || quantity <= 0)
        {
            GD.PrintErr($"Error: No tienes '{itemID}' en tu inventario.");
            return false;
        }

        if (!AllFoodItems.TryGetValue(itemID, out FoodItem itemToEat))
        {
            GD.PrintErr($"Error: Item ID '{itemID}' no se pudo comer.");
            return false;
        }

        // Restar del inventario
        PlayerInventory[itemID]--;

        // Aplicar estadísticas
        Hunger += itemToEat.HungerRestore;
        Happiness += itemToEat.HappinessRestore;
        Health += itemToEat.HealthRestore;

        GD.Print($"¡Consumido: {itemToEat.ItemName}! Quedan {PlayerInventory[itemID]}.");
        GetNode<NetworkManager>("/root/NetworkManager").SaveGame();

        // ¡No es necesario emitir la señal aquí!
        // Los 'setters' de Hunger, Happiness, etc., en KanuaPet.cs
        // ya llaman a EmitStatsChanged() automáticamente.

        return true;
    }

    // Un método simple para emitir la señal con los valores actuales
    public void EmitStatsChanged()
    {
        // Usamos CallDeferred para evitar errores si la señal se emite
        // durante un proceso físico o de inicialización.
        CallDeferred(MethodName.EmitSignal, SignalName.StatsChanged, Hunger, Happiness, Health, Coins);
    }
}