using Godot;
using System.Collections.Generic;

/// <summary>
/// Clase estática que contiene la base de datos de textos y diálogos del juego.
/// </summary>
public static class DialogueData
{
    #region Diccionarios de Frases
    
    /// <summary>
    /// Frases para cuando la mascota está en reposo (Idle), categorizadas por personalidad.
    /// Incluye pensamientos generales, antojos de comida y sugerencias de minijuegos.
    /// </summary>
    public static Dictionary<PetState.PersonalityType, string[]> IdlePhrases = new()
    {
        { PetState.PersonalityType.Happy, new[] { 
            // Generales
            "¡Qué buen día!", "Me gusta estar contigo.", "¡Tengo mucha energía!", 
            "¿Viste esa nube? Parecía un gato.", "La vida es bella, ¿no?",
            
            // Antojos (Happy)
            "¡Tengo un antojo enorme de galletas!", "¡Unos tacos estarían deliciosos!", 
            "¿Me compras una leche con chocolate?", "¡Quiero birria! ¡Está buenísima!", 
            "¡Mmm, huele a tamales!", "¿Y si comemos un croissant?",
            
            // Minijuegos (Happy)
            "¡Vamos a jugar al Gato!", "¡Te desafío a una partida de Tic-Tac-Toe!", 
            "¡Juguemos para ganar muchas monedas!", "¿Una partidita rápida? ¡Ándale!"
        }},
        
        { PetState.PersonalityType.Normal, new[] { 
            // Generales
            "¿Qué haremos hoy?", "Tengo un poco de sueño...", "Mmm...", 
            "Todo tranquilo por aquí.", "Me pregunto qué hay en la tele.", 
            
            // Antojos (Normal)
            "Se me antoja un croissant.", "Creo que podría comer una Maruchan.", 
            "¿Un burrito para la cena?", "Tengo ganas de algo dulce, ¿quizás pastel?",
            "No me caería mal una manzana.", "¿Hace cuánto no comemos birria?",
            
            // Minijuegos (Normal)
            "¿Jugamos una partida rápida de Gato?", "Podríamos ganar monedas jugando.", 
            "¿Tic-Tac-Toe?", "Estoy un poco aburrido, ¿juguemos?"
        }},
        
        { PetState.PersonalityType.Grumpy, new[] { 
            // Generales
            "Hmpf.", "Déjame solo un rato.", "¿Ya terminaste?", 
            "Qué aburrido está esto.", "No me mires tanto.", "zzZZzz...",
            
            // Antojos (Grumpy)
            "Dame birria o nada.", "Tengo hambre. Tacos. Ahora.", 
            "¿Y mis galletas? No veo mis galletas.", "Esa Maruchan no se va a comprar sola.", 
            "Necesito comida real, como un buen tamal.", "El aire no alimenta, quiero un burrito.",
            
            // Minijuegos (Grumpy)
            "Supongo que podemos jugar Gato.", "Si jugamos, déjame ganar.", 
            "Haz algo útil y gana monedas en el Tic-Tac-Toe.", "Estoy aburrido. Juguemos, ya que insistes."
        }}
    };

    /// <summary>
    /// Frases para cuando la mascota recibe comida exitosamente.
    /// </summary>
    public static Dictionary<PetState.PersonalityType, string[]> EatingPhrases = new()
    {
        { PetState.PersonalityType.Happy, new[] { 
            "¡Delicioso! ¡Gracias!", "¡Ñam ñam! ¡Qué rico!", 
            "¡Eres el mejor cuidador!", "¡Sabe a gloria!", "¡Me encanta!" 
        }},
        { PetState.PersonalityType.Normal, new[] { 
            "Gracias por la comida.", "Estaba bueno.", 
            "Provecho.", "Satisfecho.", "No estaba mal." 
        }},
        { PetState.PersonalityType.Grumpy, new[] { 
            "Al fin comida.", "Ya era hora.", "Supongo que está comestible.", 
            "Podría estar mejor.", "Mastica, traga, repite." 
        }}
    };

    /// <summary>
    /// Frases para cuando se intenta alimentar pero el inventario está vacío.
    /// </summary>
    public static Dictionary<PetState.PersonalityType, string[]> NoFoodPhrases = new()
    {
        { PetState.PersonalityType.Happy, new[] { 
            "¡Ups! Parece que no queda nada.", "¿Podríamos ir a la tiendita?", 
            "Tengo un huequito en la panza...", "Creo que se acabó la comida, jijiji." 
        }},
        { PetState.PersonalityType.Normal, new[] { 
            "El inventario está vacío.", "Necesitamos comprar víveres.", 
            "No tengo nada para comer.", "¿Vamos a la tienda primero?" 
        }},
        { PetState.PersonalityType.Grumpy, new[] { 
            "¿Me piensas matar de hambre?", "¡Compra comida primero!", 
            "El aire no se come.", "¡La alacena está vacía! ¡Haz algo!" 
        }}
    };
    #endregion

    #region Utilidades
    /// <summary>
    /// Obtiene una frase aleatoria basada en la personalidad y la categoría seleccionada.
    /// </summary>
    /// <param name="personality">La personalidad actual de la mascota.</param>
    /// <param name="category">El diccionario de frases a utilizar (Idle, Eating, etc).</param>
    /// <returns>Una cadena de texto aleatoria o "..." si no se encuentra.</returns>
    public static string GetRandomPhrase(PetState.PersonalityType personality, Dictionary<PetState.PersonalityType, string[]> category)
    {
        if (category.TryGetValue(personality, out string[] phrases))
        {
            // Usar GD.Randi() para obtener un índice aleatorio dentro del array
            return phrases[GD.Randi() % phrases.Length];
        }
        return "...";
    }
    #endregion
}