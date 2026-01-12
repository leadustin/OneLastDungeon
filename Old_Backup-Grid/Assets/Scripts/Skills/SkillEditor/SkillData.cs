using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// --- DEFINITIONEN ---

public enum DamageType
{
    Physical, Magical, Fire, Ice, Poison, Holy
}

[System.Serializable]
public class SkillStep
{
    public SkillEffect effectLogic;
    public float baseAmount = 10;
    public float scalingFactor = 0.1f;

    // Diese Felder werden nun vom Custom Editor gesteuert
    public DamageType damageType = DamageType.Physical;
    public string statusName;
    public int durationRounds = 3;

    public int GetScaledValue(int playerLevel)
    {
        float multiplier = 1f + ((playerLevel - 1) * scalingFactor);
        return Mathf.RoundToInt(baseAmount * multiplier);
    }
}

// --- HAUPT CONTAINER ---

[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skill Container")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public float cooldownTime;
    public List<SkillStep> skillSteps;
    public GameObject visualEffectPrefab;
}

// --- AUTOMATISCHE AUSBLEND-LOGIK (NUR FÜR DEN EDITOR) ---
#if UNITY_EDITOR
[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Standard-Felder zeichnen (Name, Icon, etc.)
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldownTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualEffectPrefab"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Der Baukasten (Skill Steps)", EditorStyles.boldLabel);

        SerializedProperty steps = serializedObject.FindProperty("skillSteps");

        for (int i = 0; i < steps.arraySize; i++)
        {
            SerializedProperty step = steps.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical("helpBox");

            EditorGUILayout.PropertyField(step.FindPropertyRelative("effectLogic"));
            EditorGUILayout.PropertyField(step.FindPropertyRelative("baseAmount"));
            EditorGUILayout.PropertyField(step.FindPropertyRelative("scalingFactor"));

            // LOGIK ZUM AUSBLENDEN:
            SkillEffect logic = step.FindPropertyRelative("effectLogic").objectReferenceValue as SkillEffect;

            if (logic != null)
            {
                string typeName = logic.GetType().Name;

                // Wenn es ein Schadens-Effekt ist
                if (typeName == "DamageEffect")
                {
                    EditorGUILayout.LabelField("--- Schadens-Details ---", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(step.FindPropertyRelative("damageType"));
                }
                // Wenn es ein Heil- oder Status-Effekt ist
                else if (typeName == "HealEffect" || typeName == "StatusEffect")
                {
                    EditorGUILayout.LabelField("--- Status/Heal Details ---", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(step.FindPropertyRelative("statusName"), new GUIContent("Anzeige-Name"));

                    if (typeName == "StatusEffect")
                    {
                        EditorGUILayout.PropertyField(step.FindPropertyRelative("durationRounds"));
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Bitte einen Logik-Baustein zuweisen!", MessageType.Info);
            }

            if (GUILayout.Button("Schritt entfernen", GUILayout.Width(120)))
            {
                steps.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Neuen Skill-Schritt hinzufügen"))
        {
            steps.InsertArrayElementAtIndex(steps.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif