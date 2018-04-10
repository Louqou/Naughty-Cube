using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathFinder
{
    private struct Coord
    {
        public int x, y;
        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private Stack<Coord> stack = new Stack<Coord>();
    private Blocks[,] level;
    private bool[,] visited;

    private void Setup()
    {
        stack.Clear();
        int y = level.GetLength(1);
        visited = new bool[level.GetLength(0), level.GetLength(1)];
        for (int x = 0; x < level.GetLength(0); x++) {
            if (!Blocked(x, y - 1)) {
                stack.Push(new Coord(x, y - 1));
                visited[x, y - 1] = true;
            }
        }
    }

    private void PushPossibleMoves(int x, int y)
    {
        for (int i = 1; i <= 3; i++) {
            if (x + i < level.GetLength(0) && !visited[x + i, y] && !Blocked(x + i, y)) {
                stack.Push(new Coord(x + i, y));
                visited[x + i, y] = true;
            }
            else {
                break;
            }
        }

        for (int i = 1; i <= 3; i++) {
            if (x - i >= 0 && !visited[x - i, y] && !Blocked(x - i, y)) {
                stack.Push(new Coord(x - i, y));
                visited[x - i, y] = true;
            }
            else {
                break;
            }
        }

        if (!Blocked(x, y - 1)) {
            stack.Push(new Coord(x, y - 1));
            visited[x, y - 1] = true;
        }
    }

    private bool Blocked(int x, int y)
    {
        return level[x, y] == Blocks.Naughty;
    }

    private bool GoalInFront(Coord pos)
    {
        return !Blocked(pos.x, pos.y - 1) && pos.y - 1 == 0;
    }

    public bool PathThroughLevel(ref Blocks[,] iLevel)
    {
        level = iLevel;
        Setup();
        Coord pos;
        bool pathFound = false;

        while (stack.Count != 0) {
            pos = stack.Pop();
            if (GoalInFront(pos)) {
                pathFound = true;
                break;
            }
            else {
                PushPossibleMoves(pos.x, pos.y);
            }
        }

        return pathFound;
    }
}