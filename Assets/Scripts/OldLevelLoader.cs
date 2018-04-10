using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class OldLevelLoader : MonoBehaviour
{
    //private int stage = 2;
    // Number of puzzles per wave
    private int numPuzzles = 3;
    public int NumPuzzles
    {
        get {
            return numPuzzles;
        }
    }

    private int currWaveNum = 0;
    private int numWaves = 4;
    public int NumWaves
    {
        get {
            return numWaves;
        }
    }

    private Blocks[][,] currWaveBlocks;
    public Blocks[][,] CurrWaveBlocks
    {
        get {
            return currWaveBlocks;
        }
    }

    int[][,] levels;
    int[] seenLevels;
    int numLevels;
    bool secondSet = false;

    System.Random random = new System.Random();

    public TextAsset stage2;

    private void Awake()
    {
        LoadStageLevels();
        InitSeenLevels();
    }

    private void InitSeenLevels()
    {
        seenLevels = new int[numPuzzles * 2];
        for (int i = 0; i < numPuzzles * 2; i++) {
            seenLevels[i] = numLevels;
        }
    }

    private void LoadStageLevels()
    {
        string text = stage2.text;
        StringReader stringReader = new StringReader(text);

        // First line is the x and y lengh of waves 1 and 2
        // waves 3 and 4 will increase by on in the y direction
        // x and y have are reversed in the arrays
        string line = stringReader.ReadLine();
        int j = (int)char.GetNumericValue(line[0]);
        int i = (int)char.GetNumericValue(line[1]);
        // Lines 2 and 3 are the number of levels
        numLevels = (int)char.GetNumericValue(stringReader.ReadLine()[0]);
        int numLevels2 = (int)char.GetNumericValue(stringReader.ReadLine()[0]);

        levels = new int[numLevels + numLevels2][,];

        int level = 0;
        int x = 0;
        levels[0] = new int[i, j];
        while ((line = stringReader.ReadLine()) != null) {
            if (line.Length == 0) {
                if (++level == numLevels) {
                    i++;
                }
                x = 0;
                levels[level] = new int[i, j];
            }
            else {
                for (int y = 0; y < j; y++) {
                    levels[level][x, y] = (int)char.GetNumericValue(line[y]);
                }
                x++;
            }
        }
    }

    public bool GetNextWave()
    {
        if (currWaveNum != numWaves) {
            if (currWaveNum == 2) {
                secondSet = true;
                numLevels = levels.Length - numLevels;
                InitSeenLevels();
            }
            int[][,] levelInt = LoadWaveData();
            currWaveNum++;
            currWaveBlocks = MakeLevelBlocks(levelInt);
            return true;
        }
        return false;
    }

    private int[][,] LoadWaveData()
    {
        int[][,] levelInt = new int[numPuzzles][,];

        for (int p = 0; p < numPuzzles; p++) {
            if (secondSet) {
                levelInt[p] = levels[(levels.Length - numLevels) + NotSeenRanNum()];
            }
            else {
                levelInt[p] = levels[NotSeenRanNum()];
            }
        }
        return levelInt;
    }

    private int NotSeenRanNum()
    {
        int randomNum = random.Next(numLevels);
        while (Array.Exists(seenLevels, e => e == randomNum)) {
            randomNum = random.Next(numLevels);
        }
        seenLevels[Array.IndexOf(seenLevels, numLevels)] = randomNum;
        return randomNum;
    }

    private static Blocks[][,] MakeLevelBlocks(int[][,] levelInt)
    {
        Blocks[][,] levelBlocks = new Blocks[levelInt.GetLength(0)][,];

        for (int p = 0; p < levelInt.GetLength(0); p++) {
            levelBlocks[p] = new Blocks[levelInt[p].GetLength(0), levelInt[p].GetLength(1)];
            for (int i = 0; i < levelInt[p].GetLength(0); i++) {
                for (int j = 0; j < levelInt[p].GetLength(1); j++) {
                    if (levelInt[p][i, j] == 0) {
                        levelBlocks[p][i, j] = Blocks.Normal;
                    }
                    else if (levelInt[p][i, j] == 2) {
                        levelBlocks[p][i, j] = Blocks.Naughty;
                    }
                    else if (levelInt[p][i, j] == 1) {
                        levelBlocks[p][i, j] = Blocks.Magic;
                    }
                }
            }
        }
        return levelBlocks;
    }
}
