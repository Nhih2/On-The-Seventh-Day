using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AdditionalFunction
{
    public static string FloatDisplay(this float value)
    {
        return value.ToString("0.##");
    }

    public static Color ChangeTransparency(this Color color, float a)
    {
        return new Color(color.r, color.g, color.b, a);
    }

    public static int CountUniqueIDs(this List<int> ids)
    {
        return ids.Distinct().Count();
    }

    public static bool IsFirstOccurrence(this List<int> ids, int index)
    {
        int current = ids[index];
        for (int i = 0; i < index; i++)
            if (ids[i] == current)
                return false;
        return true;
    }
    public static int CountPreviousOccurrences(this List<int> ids, int index)
    {
        int target = ids[index], count = 0;
        for (int i = 0; i < index; i++)
            if (ids[i] == target)
                count++;
        return count;
    }

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static T GetRandom<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static void LogDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        foreach (KeyValuePair<TKey, TValue> entry in dict)
        {
            Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
        }
    }

    public static Emotion GetEmotion(this Emo emo)
    {
        foreach (Emotion emotion in GameSetting.emotions) if (emo == emotion.emotion) return emotion;
        return null;
    }

    public static Sprite GetSprite(this Emo emo)
    {
        if (!GameManager.Instance) return null;
        for (int i = 0; i < GameManager.Instance.emoticons.Count; i++)
        {
            if (GameSetting.emotions[i].emotion == emo) return GameManager.Instance.emoticons[i];
        }
        return null;
    }

    public static Scenery GetScenery(this Sce sce)
    {
        foreach (Scenery scenery in GameSetting.scenes) if (sce == scenery.sce) return scenery;
        return null;
    }

    public static Sprite GetSprite(this Sce sce)
    {
        if (!GameManager.Instance) return null;
        for (int i = 0; i < GameManager.Instance.sceneries.Count; i++)
        {
            if (GameSetting.scenes[i].sce == sce) return GameManager.Instance.sceneries[i];
        }
        return null;
    }
}

public static class GameSetting
{
    public static bool hasJob = true;
    public static float MASTER_SFX_VOLUME = 1f, MASTER_BG_VOLUME = 1f;
    public static Color MULT_COLOR = Color.red, SCORE_COLOR = Color.blue;
    public static int SAD_INC = 10;
    public static float HAP_MUL = 0.5f, ANG_MUL = 1f, SAS_MUL = 2f, EXC_MUL = 2f, CUT_MUL = 1f;
    public static int MAX_HEART = 5, MAX_ENERGY = 7, BASE_HEART = 3, BASE_ENERGY = 5, MAX_REDRAW = 7;
    public static int STATS_THRESHOLD = 4;
    public static float DON_MUL = 2f;
    public static List<string> STATS_NAME = new()
    {
        "STR", "DEX", "INT", "SYM", "EX", "EX"
    };
    public static List<string> STATS_FULLNAME = new()
    {
        "Strength", "Dexterity", "Intelligence", "Sympathy", "Double or Nothing", "Blessing"
    };
    public static List<string> STATS_FULLDESCRIPTION = new()
    {
        "+10", "+10", "+10", "+10", "+2 Mult", "Triple next card's score."
    };
    public static List<string> SORTING_LAYER = new()
    {
        "BG", "Fore_BG", "Stage", "Back_FG", "FG"
    };

    public static List<int> ScenariosSuccess = new();
    public static List<int> ScenariosFailure = new();

    public static List<Scenario> Scenarios = new()
    {
        new() {
            name = "Brush your teeth", id = 0,
            stat_dex = 1,
            success = new() {energy = 2, multiplier = -1f}},
        new() {
            name = "Make your bed", id = 1,
            stat_dex = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Sweep the floor", id = 2,
            stat_dex = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Morning Excercises", id = 3,
            stat_str = 1,
            success = new() {heart = 1, multiplier = -0.5f}},
        new() {
            name = "Morning Jogging", id = 4,
            stat_str = 1,
            success = new() {multiplier = 0.5f}},
        new() { // 5
            name = "Cook rice for breakfast", id = 5,
            stat_str = 1, stat_dex = 1,
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Eat rice for breakfast", id = 6,
            stat_sym = 1,
            success = new() {energy = 1, heart = 1, multiplier = 0.5f}},
        new() {
            name = "Clean the rice bowls", id = 7,
            stat_dex = 2,
            required = new() {energy = -1},
            failure = new() {heart = -1, multiplier = -0.5f},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Play Wordle", id = 8,
            stat_int = 1,
            success = new() {multiplier = 0.5f}},
        new() { // 9
            name = "Call your friend", id = 9,
            stat_sym = 2, stat_int = 1,
            required = new () {energy = -2},
            failure = new () {heart = -2, multiplier = -2f},
            success = new() { multiplier = 3f }},
        new() { // 10
            name = "Take a morning shower", id = 10,
            stat_sym = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Drying your hair after a shower", id = 11,
            stat_sym = 1, stat_dex = 1,
            failure = new() { energy = -1 },
            success = new() { heart = 1, multiplier = -0.5f }},
        new() {
            name = "Read the news", id = 12,
            stat_int = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Listen to radio", id = 13,
            stat_sym = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Reviewing the codes", id = 14,
            stat_int = 2,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() { // 15
            name = "Ironing the clothes", id = 15,
            stat_dex = 3,
            required = new() {energy = -1},
            failure = new() {heart = -2, multiplier = -1f},
            success = new() {multiplier = 3.5f}},
        new() {
            name = "Doomscrolling", id = 16,
            stat_sym = 1,
            required = new() {energy = -1},
            failure = new() {energy = 1},
            success = new() {multiplier = 1}},
        new() {
            name = "Checking emails", id = 17,
            stat_int = 1,
            failure = new() {heart = -1},
            success = new() {multiplier = 1, heart = 1}},
        new() {
            name = "Emptying the trash", id = 18,
            stat_str = 1, stat_sym = 1,
            success = new() {multiplier = -0.5f, heart = 1, energy = 1}},
        new() {
            name = "Organizing the house", id = 19,
            stat_str = 1, stat_dex = 1,
            failure = new() {multiplier = 1.5f, heart = -1},
            success = new() {multiplier = 1.5f, energy = -1}},
        new() { // 20
            name = "Brew coffee", id = 20,
            stat_sym = 1, stat_int = 1,
            failure = new() {multiplier = -0.5f},
            success = new() {multiplier = 1f, energy = 1}},
        new() {
            name = "Drink coffee", id = 21,
            stat_dex = 2,
            failure = new() {multiplier = 1f, energy = -1},
            success = new() {multiplier = -1.5f, energy = 2}},
        new() {
            name = "Dust your TV", id = 22,
            stat_dex = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Do the laundry", id = 23,
            stat_dex = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Charge your phone", id = 24,
            stat_int = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Charge your laptop", id = 25,
            stat_int = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Call your family", id = 26,
            stat_sym = 2,
            success = new() {multiplier = 1.5f, heart = -1}},
        //
        //
        //
        new() {
            name = "Attend meetings", id = 27,
            stat_sym = 1,
            success = new() {multiplier = 0.5f, energy = 1}},
        new() {
            name = "Review assigned task", id = 28,
            stat_int = 1,
            success = new() {multiplier = 0.5f, energy = 1}},
        new() {
            name = "Sync with Git", id = 29,
            stat_dex = 1,
            success = new() {multiplier = 0.5f, energy = 1}},
        new() {
            name = "Stretch your body", id = 30,
            stat_str = 1,
            success = new() {multiplier = 0.5f, energy = 1}},
        new() {
            name = "Converse with coworkers", id = 31,
            stat_sym = 1, stat_int = 1,
            success = new() {multiplier = 1f, energy = 2}},
        new() {
            name = "Drink coffee", id = 32,
            stat_str = 1, stat_dex = 1,
            success = new() {multiplier = 1f, energy = 2}},
        new() { // 33
            name = "Review bugs report", id = 33,
            stat_int = 2,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() { //34
            name = "Talk to boss", id = 34,
            stat_int = 3, stat_sym = 2,
            required = new() {energy = -2, multiplier = -2},
            failure = new() {heart = -2, multiplier = -2},
            success = new() {multiplier = 5f}},
        new() { //35
            name = "Read client request", id = 35,
            stat_int = 1, stat_sym = 1,
            success = new() {energy = 2}},
        new() {
            name = "Brainstorm code for the request", id = 36,
            stat_int = 2,
            success = new() {energy = 3}},
        new() {
            name = "Implement requested feature", id = 37,
            stat_int = 2, stat_dex = 2,
            failure = new() {multiplier = -2f, heart = -2},
            required = new() {energy = -2},
            success = new() {multiplier = 10f}},
        new() {
            name = "Refracture code", id = 38,
            stat_dex = 2,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Improve Performance Issues", id = 39,
            stat_int = 2,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Intergrate Audio", id = 40,
            stat_int = 1, stat_dex = 1,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Playtesting", id = 41,
            stat_sym = 2, stat_dex = 1,
            required = new() {energy = -2},
            success = new() {multiplier = 2.5f}},
        new() { // 42
            name = "Review junior codes", id = 42,
            stat_dex = 1, stat_sym = 2,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Give criticisms", id = 43,
            stat_int = 1, stat_dex = 1, stat_sym = 2,
            required = new() {energy = -2},
            success = new() {multiplier = 4f}},
        new() {
            name = "Pacify an argument", id = 44,
            stat_int = 1, stat_dex = 1, stat_sym = 1, stat_str = 1,
            required = new() {energy = -2},
            success = new() {multiplier = 6f}},
        new() {
            name = "Handling stress", id = 45,
            stat_sym = 1, stat_str = 1,
            required = new() {energy = -1},
            success = new() {multiplier = 1.5f}},
        new() { // 46
            name = "Deliver Game", id = 46,
            stat_sym = 1, stat_int = 3, stat_dex = 1,
            required = new() {energy = -3},
            failure = new() {heart = -4},
            success = new() {multiplier = 12f}},
        new() {
            name = "Fix bugs", id = 47,
            stat_int = 1, stat_dex = 2,
            required = new() {energy = -1},
            success = new() {multiplier = 2.5f}},
        //47 above
        //
        //
        new() {
            name = "Take a bath", id = 48,
            stat_dex = 1,
            required = new() {},
            failure = new() {multiplier = -1f},
            success = new() {heart = 1, energy = 1}},
        new() {
            name = "Lift weights", id = 49,
            stat_str = 1,
            required = new() {energy = -1},
            failure = new() {heart = -1, multiplier = -0.5f},
            success = new() {heart = 2, multiplier = 0.5f}},
        new() {
            name = "Read a book", id = 50,
            stat_int = 1,
            required = new() {energy = -1},
            failure = new() {multiplier = 0f},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Write your diary", id = 51,
            stat_sym = 1,
            required = new() {heart = -1},
            failure = new() {energy = -1, multiplier = -0.5f},
            success = new() {energy = 2, multiplier = 0.5f}},
        new() {
            name = "Brush your teeth", id = 52,
            stat_dex = 1,
            required = new() {},
            failure = new() {multiplier = -1f},
            success = new() {heart = 1, energy = 1}},
        new() {
            name = "Do push up", id = 53,
            stat_str = 1,
            required = new() {energy = -1},
            failure = new() {heart = -1, multiplier = -0.5f},
            success = new() {heart = 2, multiplier = 0.5f}},
        new() {
            name = "Playing video games", id = 54,
            stat_int = 1,
            required = new() {energy = -1},
            failure = new() {multiplier = 0f},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Call your parents", id = 55,
            stat_sym = 1,
            required = new() {heart = -1},
            failure = new() {energy = -1, multiplier = -0.5f},
            success = new() {energy = 2, multiplier = 0.5f}},
        new() {
            name = "Cook and eat dinner", id = 56,
            stat_dex = 2,
            required = new() {},
            failure = new() {multiplier = -2f},
            success = new() {heart = 2, energy = 2}},
        new() {
            name = "Flex your muscles", id = 57,
            stat_str = 2,
            required = new() {energy = -2},
            failure = new() {heart = -2, multiplier = -1f},
            success = new() {heart = 4, multiplier = 1f}},
        new() {
            name = "Fight over an argument on Reddit", id = 58,
            stat_int = 2,
            required = new() {energy = -2},
            failure = new() {multiplier = 1f},
            success = new() {multiplier = 2.5f}},
        new() {
            name = "Scroll social media", id = 59,
            stat_sym = 2,
            required = new() {heart = -2},
            failure = new() {energy = -2, multiplier = -1f},
            success = new() {energy = 4, multiplier = 1f}},
        new() { // 60
            name = "Take clothes from washing machine", id = 60,
            stat_str = 2, stat_dex = 1,
            required = new() {energy = -1},
            failure = new() {energy = -1, heart = -1, multiplier = -1.5f},
            success = new() {heart = 1, energy = 1, multiplier = 2.5f}},
        new() {
            name = "Dry the wet clothes", id = 61,
            stat_int = 1, stat_dex = 2,
            required = new() {energy = -2},
            failure = new() {energy = 2, heart = 2, multiplier = 0f},
            success = new() {heart = 1, energy = 1, multiplier = 3f}},
        new() {
            name = "Fold the dried clothes", id = 62,
            stat_int = 1, stat_sym = 1, stat_dex = 1, stat_str = 1,
            required = new() {heart = -1, energy = -1},
            failure = new() {multiplier = 0f},
            success = new() {multiplier = 3.5f}},
        new() { // 63
            name = "Open Github of your Indie Game", id = 63,
            stat_int = 2, stat_sym = 2,
            required = new() {},
            failure = new() {energy = 1, heart = 1},
            success = new() {energy = 3, heart = 3}},
        new() {
            name = "Working on your Indie Game", id = 64,
            stat_int = 2, stat_dex = 2,
            required = new() {energy = -2, heart = -1},
            failure = new() {multiplier = -2f},
            success = new() {multiplier = 4f}},
        new() {
            name = "Git Push", id = 65,
            stat_int = 4,
            required = new() {},
            failure = new() {heart = -2, multiplier = 5f},
            success = new() {multiplier = 3f}},
        new() { // 66
            name = "Prepare for GTMK Game Jam", id = 66,
            stat_sym = 2, stat_int = 3,
            required = new() {heart = -2, energy = -2},
            failure = new() {heart = -1, energy = -1},
            success = new() {multiplier = 8f}},
        new() { // 67
            name = "Sleep peacefully", id = 67,
            stat_dex = 3, stat_str = 3,
            required = new() {},
            failure = new() {heart = 3, energy = 3},
            success = new() {multiplier = 10f}},
        new() {
            name = "Practice Speedtyping", id = 68,
            stat_dex = 1,
            required = new() {},
            failure = new() {multiplier = -1f},
            success = new() {heart = 1, energy = 1}},
        new() {
            name = "Go night jogging", id = 69,
            stat_str = 1,
            required = new() {energy = -1},
            failure = new() {heart = -1, multiplier = -0.5f},
            success = new() {heart = 2, multiplier = 0.5f}},
        new() {
            name = "Plan for tomorrow", id = 70,
            stat_int = 1,
            required = new() {energy = -1},
            failure = new() {multiplier = 0f},
            success = new() {multiplier = 1.5f}},
        new() {
            name = "Surf Facebook", id = 71,
            stat_sym = 1,
            required = new() {heart = -1},
            failure = new() {energy = -1, multiplier = -0.5f},
            success = new() {energy = 2, multiplier = 0.5f}},
        //
        // 
        //
        new() {
            name = "Update CV", id = 72,
            stat_dex = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Search Job Market", id = 73,
            stat_str = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Practice Leetcode problem", id = 74,
            stat_int = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Practice Presentating", id = 75,
            stat_sym = 1,
            success = new() {multiplier = 0.5f}},
        new() {
            name = "Prepare clothes", id = 76,
            stat_dex = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Browsing LinkedIn", id = 77,
            stat_str = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Researching Salary", id = 78,
            stat_int = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Practice Interviewing", id = 79,
            stat_sym = 2,
            success = new() {multiplier = 1f}},
        new() {
            name = "Write customized cover letter", id = 80,
            stat_int = 1, stat_sym = 1,
            success = new() {multiplier = 1f}},
        new() {
            name = "Filling online application form", id = 81,
            stat_str = 1, stat_dex = 1,
            success = new() {multiplier = 1f}},
        new() {
            name = "Do mock interview with AI", id = 82,
            stat_int = 1, stat_str = 1,
            success = new() {multiplier = 1f}},
        new() {
            name = "Discuss with friends", id = 83,
            stat_dex = 1, stat_sym = 1,
            success = new() {multiplier = 1f}},
        new() { // 84
            name = "Ask previos bosses for cover letters", id = 84,
            stat_sym = 2, stat_int = 2,
            required = new() {energy = -1, heart = -1},
            success = new() {multiplier = 5f}},
        new() {
            name = "Do free works to get cover letters", id = 85,
            stat_str = 2, stat_dex = 2,
            required = new() {energy = -1, heart = -1},
            success = new() {multiplier = 5f}},
        new() {
            name = "Received the cover letters", id = 86,
            success = new() {multiplier = 7.5f, heart = 2, energy = 2}},
        new() { // 87
            name = "Go to hairdresser", id = 87,
            stat_dex = 1,
            required = new() {heart = -1},
            success = new() {energy = 1}},
        new() {
            name = "Buy a new suit", id = 88,
            stat_int = 1,
            required = new() {heart = -1},
            success = new() {energy = 1}},
        new() {
            name = "Dress up formally", id = 89,
            stat_sym = 1, stat_str = 1,
            success = new() {heart = 2, multiplier = 7.5f}},
        new() { // 90
            name = "Go to Interview", id = 90,
            stat_sym = 2, stat_int = 2, stat_dex = 2,
            failure = new() {heart = -2, energy = -2, multiplier = -3f},
            success = new() {multiplier = 12.5f}},
        new() { // 91
            name = "Land a Job", id = 91,
            stat_str = 2, stat_dex = 2, stat_sym = 2,
            failure = new() {heart = -3, energy = -3, multiplier = -4f},
            success = new() {multiplier = 15f}},

    };

    public static List<Emotion> emotions = new()
    {
        new() {
            name = "Happy",
            des = "For every identical cards played,\n+1 Mult.",
            emotion = Emo.Happy
        },
        new() {
            name = "Sad",
            des = "For every different cards played,\n+10 Base Score.",
            emotion = Emo.Sad
        },
        new() {
            name = "Angry",
            des = "+1 Mult",
            emotion = Emo.Angry
        },
        new() {
            name = "Sastisfied",
            des = "When STR and DEX is played,\n+2 Mult",
            emotion = Emo.Sastisfied
        },
        new() {
            name = "Excited",
            des = "When INT and SYM is played,\n+2 Mult",
            emotion = Emo.Excited
        },
         new() {
            name = "Cute",
            des = "When EX card is played,\n+1 Mult",
            emotion = Emo.Cute
        },
        new() {
            name = "Cool",
            des = "When at least 5 cards are played,\n+2 Mult",
            emotion = Emo.Cool
        }
    };

    public static List<Scenery> scenes = new()
    {
        new() {
            name = "Home (Morning)",
            des = "A casual morning at home.",
            sce = Sce.House_Morning,
            actionsStart = 0, actionsEnd = 27
        },
        new() {
            name = "Office",
            des = "Working at the office\n(Hard Difficulty. INT Focus).",
            sce = Sce.Office,
            actionsStart = 27, actionsEnd = 48
        },
        new() {
            name = "Home (Night)",
            des = "A cozy night at home\n.",
            sce = Sce.House_Night,
            actionsStart = 48, actionsEnd = 72
        },
        new() {
            name = "Job Hunting",
            des = "A search for job\n.",
            sce = Sce.Job_Hunting,
            actionsStart = 72, actionsEnd = 92
        },
    };
}

public enum Emo
{
    Happy, Sad, Angry, Sastisfied, Excited, Cute, Cool
}

public enum Sce
{
    House_Morning, Office, House_Night, Job_Hunting
}

public class Scenario
{
    public string name = "null";
    public int stat_dex
    {
        private get;
        set;
    } = 0;
    public int stat_int = 0, stat_str = 0, stat_sym = 0, id;
    public Outcome success = new() { heart = 0, energy = 0, multiplier = 0 }, failure = new() { heart = 0, energy = 0, multiplier = 0 }, required = new() { heart = 0, energy = 0 };
    public struct Outcome
    {
        public int heart, energy;
        public float multiplier;
    }
    public int GetDex()
    {
        return stat_dex;
    }
}

public class Emotion
{
    public string name = "null", des = "null";
    public Emo emotion;
}

public class PlayCard
{
    public int type1 = -1, type2 = -1;
}

public class Scenery
{
    public string name = "null", des = "null";
    public Sce sce;
    public int actionsStart, actionsEnd;
}

public class Reward
{
    public PlayCard playCard;
    public Emotion emotion;
    public Scenery scenery;
}