using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class AutoSetupComplete : MonoBehaviour
{
    [MenuItem("VR Runner/Complete Setup")]
    static void CompleteSetup()
    {
        // Step 1: Create required scene objects if missing
        CreateSceneObject("Ground", () =>
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 100f);
            CreateTagIfMissing("Ground");
            ground.tag = "Ground";
            return ground;
        });

        CreateSceneObject("Player", () =>
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            return player;
        });

        CreateSceneObject("LaneManager", () => new GameObject("LaneManager"));
        CreateSceneObject("ObstacleSpawner", () => new GameObject("ObstacleSpawner"));
        CreateSceneObject("GameManager", () => new GameObject("GameManager"));

        // Step 2: Create obstacle prefabs
        CreateObstaclePrefabs();

        // Step 3: Configure LaneManager
        GameObject lmObj = GameObject.Find("LaneManager");
        if (lmObj != null && lmObj.GetComponent<LaneManager>() == null)
            lmObj.AddComponent<LaneManager>();

        // Step 4: Configure ObstacleSpawner
        GameObject osObj = GameObject.Find("ObstacleSpawner");
        if (osObj != null)
        {
            ObstacleSpawner os = osObj.GetComponent<ObstacleSpawner>();
            if (os == null) os = osObj.AddComponent<ObstacleSpawner>();
            os.player = GameObject.Find("Player")?.transform;
            os.laneManager = lmObj?.GetComponent<LaneManager>();
            os.highObstacles = new[] { AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HighObstacle.prefab") };
            os.lowObstacles = new[] { AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LowObstacle.prefab") };
        }

        // Step 5: Configure GameManager
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null)
        {
            GameManager gm = gmObj.GetComponent<GameManager>();
            if (gm == null) gm = gmObj.AddComponent<GameManager>();
            gm.player = GameObject.Find("Player")?.transform;

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (gm.scoreText == null)
            {
                GameObject textObj = new GameObject("ScoreText");
                textObj.transform.SetParent(canvas.transform);
                RectTransform rect = textObj.AddComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(10, -10);
                rect.sizeDelta = new Vector2(300, 50);
                Text scoreText = textObj.AddComponent<Text>();
                scoreText.text = "Score: 0";
                scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                scoreText.alignment = TextAnchor.UpperLeft;
                scoreText.color = Color.black;
                gm.scoreText = scoreText;
            }
        }

        // Step 6: Configure Player and PlayerController
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc == null) pc = playerObj.AddComponent<PlayerController>();
            pc.laneManager = lmObj?.GetComponent<LaneManager>();

            Transform cameraRig = null;
            Transform eyeAnchor = null;
            GameObject xrRig = GameObject.Find("XR Rig") ?? GameObject.Find("OVRCameraRig");
            if (xrRig != null)
            {
                cameraRig = xrRig.transform;
                eyeAnchor = xrRig.transform.Find("CenterEyeAnchor") ?? xrRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            }
            else
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    eyeAnchor = mainCam.transform;
                    cameraRig = mainCam.transform.parent ?? mainCam.transform;
                }
            }

            if (cameraRig != null) pc.cameraRig = cameraRig;
            if (eyeAnchor != null) pc.centerEyeAnchor = eyeAnchor;
        }

        // Step 7: Ensure tags exist
        CreateTagIfMissing("Ground");
        CreateTagIfMissing("Obstacle");
        GameObject groundObj = GameObject.Find("Ground");
        if (groundObj != null) groundObj.tag = "Ground";

        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "Your VR Runner scene is now fully configured:\n\n" +
            "✓ Ground plane created and tagged\n" +
            "✓ Player capsule with Rigidbody\n" +
            "✓ Obstacle prefabs created\n" +
            "✓ GameManager with UI canvas\n" +
            "✓ All script references wired\n\n" +
            "Ready to play! Press Play button.",
            "Start Testing!");

        AssetDatabase.Refresh();
    }

    static void CreateSceneObject(string name, System.Func<GameObject> createFunc)
    {
        if (GameObject.Find(name) == null)
        {
            createFunc();
            Debug.Log($"✓ {name} created");
        }
    }

    static void CreateObstaclePrefabs()
    {
        string prefabPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // High Obstacle (must duck)
        string highPath = prefabPath + "/HighObstacle.prefab";
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(highPath))
        {
            GameObject highObs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highObs.name = "HighObstacle";
            highObs.transform.localScale = new Vector3(2f, 2f, 1f);
            highObs.transform.position = new Vector3(0, 2f, 50f);
            
            // Remove collider from primitive mesh
            DestroyImmediate(highObs.GetComponent<MeshCollider>());
            highObs.AddComponent<BoxCollider>();
            
            // Add rigidbody for collision detection
            Rigidbody rb = highObs.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            highObs.tag = "Obstacle";
            highObs.layer = LayerMask.NameToLayer("Default");
            
            PrefabUtility.SaveAsPrefabAsset(highObs, highPath);
            DestroyImmediate(highObs);
            Debug.Log("✓ HighObstacle prefab created");
        }

        // Low Obstacle (must jump)
        string lowPath = prefabPath + "/LowObstacle.prefab";
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(lowPath))
        {
            GameObject lowObs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowObs.name = "LowObstacle";
            lowObs.transform.localScale = new Vector3(2f, 0.5f, 1f);
            lowObs.transform.position = new Vector3(0, 0.25f, 50f);
            
            DestroyImmediate(lowObs.GetComponent<MeshCollider>());
            lowObs.AddComponent<BoxCollider>();
            
            Rigidbody rb = lowObs.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            lowObs.tag = "Obstacle";
            
            PrefabUtility.SaveAsPrefabAsset(lowObs, lowPath);
            DestroyImmediate(lowObs);
            Debug.Log("✓ LowObstacle prefab created");
        }
    }

    static void CreateTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(0);
        tagsProp.GetArrayElementAtIndex(0).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
