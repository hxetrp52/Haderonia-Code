using UnityEditor;
using UnityEngine;
using System.IO;

public class OutfitCreatorEditor : EditorWindow
{
    private CharacterOutfitData previewData;
    private SerializedObject serializedData;

    private string assetName = "KnightOutfit";
    private string combatClassName = "Knight";

    private const string ASSET_FOLDER = "Assets/Game_InGame/SO/OutFit/";
    private const string COMBAT_TEMPLATE_PATH = "Assets/Editor/OutfitCombatTemplate.txt";
    private const string COMBAT_SCRIPT_FOLDER = "Assets/Game_InGame/Script/Combat/";

    [MenuItem("Tools/Outfit Creator")]
    static void Open()
    {
        GetWindow<OutfitCreatorEditor>("Outfit Creator");
    }

    private void OnEnable()
    {
        previewData = ScriptableObject.CreateInstance<CharacterOutfitData>();
        serializedData = new SerializedObject(previewData);
    }

    private void OnGUI()
    {
        serializedData.Update();

        GUILayout.Label("Outfit Data", EditorStyles.boldLabel);

        DrawAllProperties(serializedData);

        GUILayout.Space(10);

        assetName = EditorGUILayout.TextField("Asset Name", assetName);
        combatClassName = EditorGUILayout.TextField("Combat Class Name", combatClassName);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Outfit Asset & Combat"))
        {
            CreateOutfit();
        }

        serializedData.ApplyModifiedProperties();
    }

    private void DrawAllProperties(SerializedObject so)
    {
        SerializedProperty prop = so.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            if (prop.name == "m_Script") continue;
            EditorGUILayout.PropertyField(prop, true);
            enterChildren = false;
        }
    }

    private void CreateOutfit() // outfit SO 및 스크립트 제작
    {
        Directory.CreateDirectory(ASSET_FOLDER);
        Directory.CreateDirectory(COMBAT_SCRIPT_FOLDER);

        CharacterOutfitData asset =
            ScriptableObject.CreateInstance<CharacterOutfitData>();

        EditorUtility.CopySerialized(previewData, asset);

        asset.outfitName = assetName;
        asset.combatClassName = combatClassName + "CombatData"; // 핵심

        string assetPath = ASSET_FOLDER + assetName + ".asset";
        AssetDatabase.CreateAsset(asset, assetPath);

        string combatScriptPath =
            COMBAT_SCRIPT_FOLDER + combatClassName + "CombatData.cs";

        if (!File.Exists(combatScriptPath))
        {
            string template = File.ReadAllText(COMBAT_TEMPLATE_PATH)
                .Replace("##CLASSNAME##", combatClassName)
                .Replace("##OUTFITNAME##", asset.outfitName);

            File.WriteAllText(combatScriptPath, template);
        }

        AssetDatabase.Refresh();
        EditorUtility.SetDirty(asset);

        Debug.Log("Outfit + Combat 생성 완료");
    }

}
