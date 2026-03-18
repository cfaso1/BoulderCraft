using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ── Scene setup required ─────────────────────────────────────────────────────
// 1. Create a new Canvas (or add to HoldToolbarCanvas) with a child Panel named
//    "SaveLoadPanel" (toggled by this script).
// 2. Add the following children to the panel:
//      - TMP_InputField  → filenameInput  (placeholder: "route")
//      - Button          → saveButton     (label: "Save")
//      - Button          → loadButton     (label: "Load")
//      - Button          → newButton      (label: "New")
//      - Button          → closeButton    (label: "✕")
// 3. Attach this script to any GameObject and wire the Inspector references.
// 4. Add a new empty GameObject "SaveLoadManager" and attach SaveManager to it.
// 5. Add a new empty GameObject "UndoRedoManager" and attach UndoRedoManager to it.
// ─────────────────────────────────────────────────────────────────────────────

public class SaveLoadUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] TMP_InputField filenameInput;
    [SerializeField] Button saveButton;
    [SerializeField] Button loadButton;
    [SerializeField] Button newButton;
    [SerializeField] Button closeButton;

    string SaveDir => Application.persistentDataPath;

    void Awake()
    {
        saveButton.onClick.AddListener(OnSave);
        loadButton.onClick.AddListener(OnLoad);
        newButton.onClick.AddListener(OnNew);
        closeButton.onClick.AddListener(() => SetPanel(false));
        SetPanel(false);
    }

    void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // Ctrl+S: quick-save if a path exists, otherwise open the panel.
        if (ctrl && Input.GetKeyDown(KeyCode.S))
        {
            if (!string.IsNullOrEmpty(SaveManager.Instance.LastSavedPath))
                SaveManager.Instance.SaveRoute(SaveManager.Instance.LastSavedPath);
            else
                SetPanel(true);
            return;
        }

        // Ctrl+O: toggle the save/load panel.
        if (ctrl && Input.GetKeyDown(KeyCode.O))
            SetPanel(!panel.activeSelf);
    }

    void SetPanel(bool open)
    {
        IsOpen = open;
        panel.SetActive(open);
    }

    void OnSave()
    {
        string name = filenameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "route";
        if (!name.EndsWith(".json")) name += ".json";
        SaveManager.Instance.SaveRoute(Path.Combine(SaveDir, name));
        SetPanel(false);
    }

    void OnLoad()
    {
        string name = filenameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "route";
        if (!name.EndsWith(".json")) name += ".json";
        SaveManager.Instance.LoadRoute(Path.Combine(SaveDir, name));
        SetPanel(false);
    }

    void OnNew()
    {
        SaveManager.Instance.NewRoute();
        SetPanel(false);
    }
}
