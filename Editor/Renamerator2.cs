using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using System.Linq;

public class Renamerator2 : EditorWindow
{
    Button renameBtn;
    Label numSelectedLbl;
    Label previewLbl;
    TextField searchTxt;
    TextField replaceTxt;
    Toggle useRegex;

    [MenuItem("Custom Tools/The Renamerator II %#t")]
    public static void OpenWindow()
    {
        Renamerator2 wnd = GetWindow<Renamerator2>();
        var icon = (Texture2D)EditorGUIUtility.Load("Assets/Editor/RenameratorIcon.png");
        wnd.titleContent = new GUIContent("Renamerator\u00B2", icon);
    }

    public void OnEnable()
    {
        var uxmlTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Renamerator2.uxml");
        var ui = uxmlTemplate.Instantiate();
        rootVisualElement.Add(ui);
        searchTxt = ui.Q<TextField>("searchTxt");
        replaceTxt = ui.Q<TextField>("replaceTxt");
        useRegex = ui.Q<Toggle>("useRegex");
        renameBtn = ui.Q<Button>("renameBtn");
        renameBtn.clicked += RenameSelected;
        numSelectedLbl = ui.Q<Label>("numSelectedLbl");
        previewLbl = ui.Q<Label>("previewLbl");
        SyncUIWithSelection();
        Selection.selectionChanged += SyncUIWithSelection;
        searchTxt.RegisterValueChangedCallback(x => UpdatePreview());
        replaceTxt.RegisterValueChangedCallback(x => UpdatePreview());
    }
    public void OnDisable()
    {
        Selection.selectionChanged -= SyncUIWithSelection;
    }

    void SyncUIWithSelection()
    {
        UpdatePreview();
        var numSelected = Selection.gameObjects.Length;
        numSelectedLbl.text = $"# Selected: {numSelected}";
        renameBtn.SetEnabled(numSelected > 0);
    }

    void UpdatePreview()
    {
        var search = searchTxt.value;
        var replace = replaceTxt.value;
        var gameObjs = Selection.gameObjects;
        if (search == "") { previewLbl.text = ""; return; }
        var oldNames = gameObjs.Select(go => go.name);
        System.Collections.Generic.IEnumerable<string> newNames;
        if (useRegex.value)
        {
            newNames = gameObjs.Select(go =>
            {
                try { return Regex.Replace(go.name, search, replace); }
                catch { return "REGEX ERROR"; }
            });
        }
        else
        {
            newNames = gameObjs.Select(go => go.name.Replace(search, replace));
        }
        var nameChanges = oldNames.Zip(newNames, (x, y) => $"{x} -> {y}");
        previewLbl.text = string.Join("\n", nameChanges);
    }

    void RenameSelected()
    {
        var search = searchTxt.value;
        var replace = replaceTxt.value;
        var numSelected = Selection.gameObjects.Length;

        const string title = "Rename Selected GameObjects";
        var msg = $"Are you sure you want to rename the {numSelected} selected GameObjects?";
        var doIt = EditorUtility.DisplayDialog(title, msg, "Rename", "Cancel");
        if (!doIt) { return; }

        var logMsg = $"Renaming { numSelected } objs: { search} -> { replace }";
        Debug.Log($"[Renamerator2] " + logMsg);
        Undo.RecordObjects(Selection.gameObjects, "Rename Selected GameObjects");

        foreach (var gameObj in Selection.gameObjects)
        {
            if (useRegex.value)
            {
                gameObj.name = Regex.Replace(gameObj.name, search, replace);
            }
            else
            {
                gameObj.name = gameObj.name.Replace(search, replace);
            }
        }
    }
}
