using Godot;
using System.Collections.Generic;

public static class DialogueData
{
    // Diccionarios de frases según personalidad y evento

    // --- IDLE (ESTAR QUIETO) ---
    public static Dictionary<PetState.PersonalityType, string[]> IdlePhrases = new()
    {
        { PetState.PersonalityType.Happy, new[] { 
            "¡Qué buen día!", "Me gusta estar contigo.", "¡Juguemos a algo!", 
            "La vida es bella, ¿no?", "¡Tengo mucha energía!", 
            "¿Viste esa nube? Parecía un gato." 
        }},
        { PetState.PersonalityType.Normal, new[] { 
            "¿Qué haremos hoy?", "Tengo un poco de sueño...", "Mmm...", 
            "Todo tranquilo por aquí.", "Me pregunto qué hay en la tele.", 
            "..." 
        }},
        { PetState.PersonalityType.Grumpy, new[] { 
            "Hmpf.", "Déjame solo un rato.", "¿Ya terminaste?", "Tengo hambre...", 
            "Qué aburrido está esto.", "No me mires tanto.", "zzZZzz..." 
        }}
    };

    // --- COMER (ALIMENTACIÓN EXITOSA) ---
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

    // --- SIN COMIDA (INTENTO FALLIDO) ---
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

    public static string GetRandomPhrase(PetState.PersonalityType personality, Dictionary<PetState.PersonalityType, string[]> category)
    {
        if (category.TryGetValue(personality, out string[] phrases))
        {
            return phrases[GD.Randi() % phrases.Length];
        }
        return "...";
    }
}