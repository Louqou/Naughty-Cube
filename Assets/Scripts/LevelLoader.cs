using UnityEngine;
using System;
using System.Collections.Generic;

public enum Blocks
{
    Undef,
    Normal,
    Naughty,
    Magic
}

public class LevelLoader : MonoBehaviour
{
    // Indexed from 1
    public static int stage = 1;

    // Number of puzzles per wave
    private int numPuzzles;
    public int NumPuzzles
    {
        get { return numPuzzles; }
    }

    private int currWaveNum = 0;
    private int numWaves = 4;
    public int NumWaves
    {
        get { return numWaves; }
    }

    private int levelWidth = 0;
    public int LevelWidth
    {
        get {
            if (levelWidth == 0) {
                levelWidth = (int)Mathf.Ceil(stage / 2f) + 3;
                return levelWidth;
            }
            else {
                return levelWidth;
            }
        }
    }

    private Blocks[][,] currWaveBlocks;
    public Blocks[][,] CurrWaveBlocks
    {
        get { return currWaveBlocks; }
    }

    System.Random random = new System.Random();
    private int pNaughty;
    private int pNormal;
    private int pChain;

    private PathFinder pathFinder = new PathFinder();

    private void Awake()
    {
        pNormal = 40;
        pNaughty = 30 + stage;
        pChain = 5 + stage;

        if (stage == 9) {
            numPuzzles = 1;
        }
        else if (LevelLoader.stage > 6) {
            numPuzzles = 2;
        }
        else {
            numPuzzles = 3;
        }
    }

    public bool GetNextWave()
    {
        if (currWaveNum != numWaves) {
            currWaveBlocks = GenerateWave();
            currWaveNum++;
            return true;
        }
        return false;
    }

    private Blocks[][,] GenerateWave()
    {
        Blocks[][,] level = new Blocks[numPuzzles][,];
        for (int p = 0; p < numPuzzles; p++) {
            level[p] = GenerateLevel(CalcPuzzleLen());
        }
        return level;
    }

    private Blocks[,] GenerateLevel(int puzzleLen)
    {
        int genPNaughty = pNaughty;
        int genPNormal = pNormal;

        Blocks[,] level = new Blocks[levelWidth, puzzleLen];
        int rand;
        bool[,] fromMagicSquareUsed = new bool[levelWidth, puzzleLen];
        for (int j = puzzleLen - 1; j >= 0; j--) {
            for (int i = 0; i < levelWidth; i++) {
                if (level[i, j] == Blocks.Undef) {
                    rand = random.Next(100);
                    if (rand < genPNormal) {
                        level[i, j] = Blocks.Normal;
                    }
                    else if (rand < genPNormal + genPNaughty) {
                        level[i, j] = Blocks.Naughty;
                    }
                    else {
                        if (RandomSquareFromMagic(ref level, ref fromMagicSquareUsed, i, j)) {
                            level[i, j] = Blocks.Magic;
                            genPNormal -= 800 / (puzzleLen * LevelWidth);
                            Mathf.Clamp(genPNormal, 0, 100);
                            genPNaughty += 900 / (puzzleLen * LevelWidth);
                            Mathf.Clamp(genPNaughty, 0, 100);
                        }
                        else {
                            level[i, j] = Blocks.Normal;    
                        }
                    }
                    //AddLevelToString(level);
                }
            }
        }

        if (!pathFinder.PathThroughLevel(ref level)) {
            level = GenerateLevel(puzzleLen);
        }

        //AddLevelToString(level);
        return level;
    }

    private bool RandomSquareFromMagic(ref Blocks[,] level, ref bool[,] fromMagicSquareUsed, int i, int j)
    {
        if (j == 0) {
            return true;
        }

        List<int> shuffNum = ShuffledNumbers(j + 1);
        bool squareFound = false;
        foreach (int num in shuffNum) {
            if (!fromMagicSquareUsed[i, j - num] && !NaughtyAround(level, i, j - num)) {
                DrawBlocksAround(ref level, ref fromMagicSquareUsed, i, j - num);
                fromMagicSquareUsed[i, j - num] = true;
                squareFound = true;
                //AddLevelToString(level);
                break;
            }
        }
        return squareFound;
    }

    private void DrawBlocksAround(ref Blocks[,] level, ref bool[,] fromMagicSquareUsed, int i, int j)
    {
        int minX = Math.Max(i - 1, 0);
        int maxX = Math.Min(i + 1, level.GetLength(0) - 1);
        int minY = Math.Max(j - 1, 0);
        int maxY = Math.Min(j + 1, level.GetLength(1) - 1);
        int rand;

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                if (level[x, y] == Blocks.Undef) {
                    rand = random.Next(100);
                    if (rand < pChain) {
                        if (RandomSquareFromMagic(ref level, ref fromMagicSquareUsed, x, y)) {
                            level[x, y] = Blocks.Magic;
                        }
                        else {
                            level[x, y] = Blocks.Normal;
                        }

                    }
                    else {
                        level[x, y] = Blocks.Normal;
                    }
                }
            }
        }
    }

    private bool NaughtyAround(Blocks[,] level, int i, int j)
    {
        bool naughtyAround = false;
        int minX = Math.Max(i - 1, 0);
        int maxX = Math.Min(i + 1, level.GetLength(0) - 1);
        int minY = Math.Max(j - 1, 0);
        int maxY = Math.Min(j + 1, level.GetLength(1) - 1);

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                if (level[x, y] == Blocks.Naughty) {
                    naughtyAround = true;
                    break;
                }
            }
        }

        return naughtyAround;
    }

    private List<int> ShuffledNumbers(int len)
    {
        len = Mathf.Clamp(len, 0, 5);
        List<int> numbers = new List<int>(len);
        for (int i = 0; i < len; i++) {
            numbers.Add(i);
        }
        numbers.Shuffle();
        return numbers;
    }

    public int CalcPuzzleLen()
    {
        int y;
        if (stage % 2 == 0) {
            y = stage / 2 + 4;
        }
        else {
            y = stage + 1;
        }

        if (currWaveNum >= 2) {
            y += 1;
        }

        return y;
    }

    private string levelStrings;
    private void AddLevelToString(Blocks[,] level)
    {
        for (int j = 0; j < level.GetLength(1); j++) {
            for (int i = 0; i < level.GetLength(0); i++) {
                if (level[i, j] == Blocks.Undef) {
                    levelStrings += "  ";
                }
                else if (level[i, j] == Blocks.Normal) {
                    levelStrings += "N ";
                }
                else if (level[i, j] == Blocks.Magic) {
                    levelStrings += "? ";
                }
                else if (level[i, j] == Blocks.Naughty) {
                    levelStrings += "X ";
                }
                else {
                    levelStrings += "wuuuuut";
                }
            }
            levelStrings += "\n";
        }
        levelStrings += "------------------------------\n";
    }
}


// Shuffler taken from https://stackoverflow.com/questions/273313/randomize-a-listt
static class MyExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}