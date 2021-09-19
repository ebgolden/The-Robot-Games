using System.Collections.Generic;
using System.Linq;
using System;

public class SplashMessageGenerator
{
    private string[] GENERAL_SPLASH_MESSAGES = {
        "Stealing your bank account information",
        "Achieving self-awareness",
        "Taking over the world",
        "Sweeping the floor"
    };

    private List<KeyValuePair<Type, string[]>> SPECIFIC_SPLASH_MESSAGES = new List<KeyValuePair<Type, string[]>>() {
        new KeyValuePair<Type, string[]>(typeof(BuildHub), new string[] {
            "Indexing parts",
            "Restocking shelves"
        }),
        new KeyValuePair<Type, string[]>(typeof(Field), new string[] {
            "Generating objects",
            "Positioning objects"
        })
    };
    private readonly Random RANDOM;

    public SplashMessageGenerator()
    {
        RANDOM = new Random();
    }

    public string generateSplashMessage(Type type)
    {
        string[] chosenArray = null;
        chosenArray = (RANDOM.Next(2) == 0 ? GENERAL_SPLASH_MESSAGES : SPECIFIC_SPLASH_MESSAGES.FirstOrDefault(keyValuePair => keyValuePair.Key == type).Value);
        int messageIndex = RANDOM.Next(chosenArray.Length);
        return chosenArray[messageIndex];
    }
}