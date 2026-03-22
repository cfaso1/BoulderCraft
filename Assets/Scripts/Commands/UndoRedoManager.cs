using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager Instance { get; private set; }

    readonly Stack<ICommand> undo = new();
    readonly Stack<ICommand> redo = new();

    void Awake() { Instance = this; }

    public void Do(ICommand cmd)
    {
        cmd.Execute();
        undo.Push(cmd);
        redo.Clear();
    }

    public void Record(ICommand cmd)
    {
        undo.Push(cmd);
        redo.Clear();
    }

    public void Undo()
    {
        if (undo.Count == 0) return;
        var cmd = undo.Pop();
        cmd.Undo();
        redo.Push(cmd);
    }

    public void Redo()
    {
        if (redo.Count == 0) return;
        var cmd = redo.Pop();
        cmd.Execute();
        undo.Push(cmd);
    }

    public void Clear()
    {
        undo.Clear();
        redo.Clear();
    }
}
