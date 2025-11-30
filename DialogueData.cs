using Godot;
using System.Collections.Generic;

public static class DialogueData
{
    // Diccionarios de frases según personalidad y evento
    public static Dictionary<PetState.PersonalityType, string[]> IdlePhrases = new()
    {
        { PetState.PersonalityType.Happy, new[] { "¡Qué buen día!", "Me gusta estar contigo.", "¡Juguemos!" } },
        { PetState.PersonalityType.Normal, new[] { "¿Qué haremos hoy?", "Tengo un poco de sueño...", "Mmm..." } },
        { PetState.PersonalityType.Grumpy, new[] { "Hmpf.", "Déjame solo.", "¿Ya terminaste?", "Tengo hambre..." } }
    };

    public static Dictionary<PetState.PersonalityType, string[]> EatingPhrases = new()
    {
        { PetState.PersonalityType.Happy, new[] { "¡Delicioso! ¡Gracias!", "¡Ñam ñam! ¡Qué rico!" } },
        { PetState.PersonalityType.Normal, new[] { "Gracias por la comida.", "Estaba bueno." } },
        { PetState.PersonalityType.Grumpy, new[] { "Al fin comida.", "Ya era hora.", "Supongo que está bien." } }
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