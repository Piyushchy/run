// ================================================================
// VRRunnerSetup.cs  —  place this in Assets/Editor/
// In Unity: top menu → VR Runner → Setup Scene
// Builds your entire scene in one click.
// ================================================================

using UnityEngine;
using UnityEditor;
using System.IO;

public class VRRunnerSetup : MonoBehaviour
{
    [MenuItem("VR Runner/Setup Scene")]
    static void SetupScene()
    {
        // ── 1. GROUND ────────────────────────────────────────────
        // Check if Ground already exists
        if (GameObject.Find("Ground") == null)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 100f); // wide short runway

            // Create and assign Ground tag
            CreateTagIfMissing("Ground");
            ground.tag = "Ground";

            Debug.Log("✓ Ground created");
        }
        else { Debug.Log("Ground already exists — skipped"); }

        // ── 2. PLAYER CAPSULE ────────────────────────────────────
        if (GameObject.Find("Player") == null)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f); // just above ground

            // Add Rigidbody
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationY
                           | RigidbodyConstraints.FreezeRotationZ;

            Debug.Log("✓ Player capsule created with Rigidbody (rotation frozen)");
        }
        else { Debug.Log("Player already exists — skipped"); }

        // ── 3. LANE MANAGER OBJECT ───────────────────────────────
        CreateEmptyIfMissing("LaneManager");

        // ── 4. OBSTACLE SPAWNER OBJECT ───────────────────────────
        CreateEmptyIfMissing("ObstacleSpawner");

        // ── 5. GAME MANAGER OBJECT ───────────────────────────────
        CreateEmptyIfMissing("GameManager");

        // ── 6. CREATE OBSTACLE TAG ───────────────────────────────
        CreateTagIfMissing("Obstacle");

        // ── 7. CREATE SCRIPTS FOLDER ────────────────────────────
        string scriptsPath = "Assets/Scripts";
        if (!AssetDatabase.IsValidFolder(scriptsPath))
        {
            AssetDatabase.CreateFolder("Assets", "Scripts");
            Debug.Log("✓ Assets/Scripts folder created");
        }

        // Create empty placeholder script files so you can paste code in
        string[] scriptNames = { "PlayerController", "LaneManager", "ObstacleSpawner", "GameManager" };
        foreach (string scriptName in scriptNames)
        {
            string filePath = scriptsPath + "/" + scriptName + ".cs";
            if (!File.Exists(Application.dataPath + "/Scripts/" + scriptName + ".cs"))
            {
                string template =
                    "using UnityEngine;\n\n" +
                    "// Paste your " + scriptName + " code here\n" +
                    "public class " + scriptName + " : MonoBehaviour\n" +
                    "{\n" +
                    "    void Start() { }\n" +
                    "    void Update() { }\n" +
                    "}\n";

                File.WriteAllText(Application.dataPath + "/Scripts/" + scriptName + ".cs", template);
                Debug.Log("✓ " + scriptName + ".cs created in Assets/Scripts");
            }
        }

        // ── 8. ADJUST DIRECTIONAL LIGHT ─────────────────────────
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                l.intensity = 1.2f;
                Debug.Log("✓ Directional light adjusted");
                break;
            }
        }

        // ── 9. REFRESH ASSET DATABASE ────────────────────────────
        AssetDatabase.Refresh();

        // ── DONE ─────────────────────────────────────────────────
        EditorUtility.DisplayDialog(
            "VR Runner Setup Complete!",
            "Your scene has been built:\n\n" +
            "✓ Ground plane (tagged, scaled)\n" +
            "✓ Player capsule (Rigidbody, frozen rotation)\n" +
            "✓ LaneManager, ObstacleSpawner, GameManager objects\n" +
            "✓ Ground + Obstacle tags created\n" +
            "✓ Assets/Scripts folder with 4 script files\n\n" +
            "Next steps:\n" +
            "1. Paste your script code into each .cs file\n" +
            "2. Import Meta XR SDK and drag OVRCameraRig onto Player\n" +
            "3. Wire the Inspector references on PlayerController",
            "Let's go!"
        );
    }

    // ── Helper: create an empty GameObject if it doesn't exist
    static void CreateEmptyIfMissing(string name)
    {
        if (GameObject.Find(name) == null)
        {
            new GameObject(name);
            Debug.Log("✓ " + name + " object created");
        }
        else { Debug.Log(name + " already exists — skipped"); }
    }

    // ── Helper: create a tag if it doesn't already exist
    static void CreateTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                found = true; break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log("✓ Tag '" + tag + "' created");
        }
    }
}