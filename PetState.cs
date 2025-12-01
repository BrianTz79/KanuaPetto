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

        if (amount > 0)
        {
            GetNode<AudioManager>("/root/AudioManager").PlaySFXPoly("res://audio/levelup.wav");
        }
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
        // --- Ítems Originales ---
        var apple = new FoodItem
        {
            ItemID = "apple",
            ItemName = "Manzana",
            Description = "Una manzana roja y crujiente.",
            Price = 10,
            Texture = GD.Load<Texture2D>("res://sprites/food/manzana.png"),
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
            Texture = GD.Load<Texture2D>("res://sprites/food/pastel.png"),
            HungerRestore = 30,
            HappinessRestore = 20,
            HealthRestore = -5
        };

        // --- Nuevos Ítems ---

        var cookie = new FoodItem
        {
            ItemID = "cookie",
            ItemName = "Galletas",
            Description = "Crujientes y con chispas de chocolate.",
            Price = 8,
            // ¡Recuerda añadir esta imagen a tu carpeta!
            Texture = GD.Load<Texture2D>("res://sprites/food/cookie.png"), 
            HungerRestore = 10,
            HappinessRestore = 8,
            HealthRestore = -1
        };

        var chocoMilk = new FoodItem
        {
            ItemID = "chocomilk",
            ItemName = "Leche con Chocolate",
            Description = "Bebida fría y refrescante.",
            Price = 12,
            Texture = GD.Load<Texture2D>("res://sprites/food/chocomilk.png"),
            HungerRestore = 10,
            HappinessRestore = 15,
            HealthRestore = 5
        };

        var croissant = new FoodItem
        {
            ItemID = "croissant",
            ItemName = "Croissant",
            Description = "Panecillo francés suave y mantequilloso.",
            Price = 15,
            Texture = GD.Load<Texture2D>("res://sprites/food/croissant.png"),
            HungerRestore = 20,
            HappinessRestore = 10,
            HealthRestore = 2
        };

        var birria = new FoodItem
        {
            ItemID = "birria",
            ItemName = "Birria",
            Description = "Un caldito levanta muertos. ¡Delicioso!",
            Price = 45,
            Texture = GD.Load<Texture2D>("res://sprites/food/birria.png"),
            HungerRestore = 50,
            HappinessRestore = 25,
            HealthRestore = 10
        };

        var tamal = new FoodItem
        {
            ItemID = "tamal",
            ItemName = "Tamal",
            Description = "Calientito y envuelto en hoja de maíz.",
            Price = 18,
            Texture = GD.Load<Texture2D>("res://sprites/food/tamal.png"),
            HungerRestore = 35,
            HappinessRestore = 10,
            HealthRestore = 5
        };

        var tacos = new FoodItem
        {
            ItemID = "tacos",
            ItemName = "Orden de Tacos",
            Description = "Con todo y mucha salsa.",
            Price = 30,
            Texture = GD.Load<Texture2D>("res://sprites/food/tacos.png"),
            HungerRestore = 40,
            HappinessRestore = 20,
            HealthRestore = 5
        };

        var burrito = new FoodItem
        {
            ItemID = "burro",
            ItemName = "Burrito",
            Description = "Tortilla de harina gigante rellena de felicidad.",
            Price = 28,
            Texture = GD.Load<Texture2D>("res://sprites/food/burro.png"),
            HungerRestore = 45,
            HappinessRestore = 15,
            HealthRestore = 3
        };

        var maruchan = new FoodItem
        {
            ItemID = "maruchan",
            ItemName = "Sopa Instantánea",
            Description = "Barata y rápida. No muy nutritiva.",
            Price = 5,
            Texture = GD.Load<Texture2D>("res://sprites/food/maruchan.png"),
            HungerRestore = 25,
            HappinessRestore = 2,
            HealthRestore = -5 // ¡Cuidado con la salud!
        };

        // --- Añadir al Diccionario ---
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