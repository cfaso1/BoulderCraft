using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public static UndoRedoManager Instance { get; private set; }

    readonly Stack<ICommand> undo = new();
    readonly Stack<ICommand> redo = new();

    void Awake() { Instance = this; }

    // Execute a command and push it to the undo stack.
    public void Do(ICommand cmd)
    {
        cmd.Execute();
        undo.Push(cmd);
        redo.Clear();
    }

    // Push a command that has already been executed (e.g. drag/rotate finalized after the fact).
    public void PushToUndo(ICommand cmd)
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
