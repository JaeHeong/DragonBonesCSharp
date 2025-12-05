using UnityEngine;
using UnityEditor;
using System.IO;

namespace DragonBones
{
    public class DragonBonesMaterialGenerator
    {
        [MenuItem("Assets/Create/DragonBones/Generate Materials for Selected Texture")]
        private static void GenerateMaterialsForSelectedTexture()
        {
            var selectedObjects = Selection.objects;

            foreach (var obj in selectedObjects)
            {
                if (obj is Texture2D texture)
                {
                    GenerateMaterials(texture);
                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Create/DragonBones/Generate Materials for Selected Texture", true)]
        private static bool ValidateGenerateMaterialsForSelectedTexture()
        {
            var selectedObjects = Selection.objects;
            foreach (var obj in selectedObjects)
            {
                if (obj is Texture2D)
                {
                    return true;
                }
            }
            return false;
        }

        [MenuItem("Assets/Create/DragonBones/Generate All Missing Materials")]
        private static void GenerateAllMissingMaterials()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Only process DragonBones texture files (ending with _tex.png)
                if (!path.EndsWith("_tex.png"))
                    continue;

                string basePath = path.Substring(0, path.Length - 4); // Remove .png
                string matPath = basePath + "_Mat.mat";
                string uiMatPath = basePath + "_UI_Mat.mat";

                // Check if materials already exist
                if (File.Exists(matPath) && File.Exists(uiMatPath))
                    continue;

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    GenerateMaterials(texture);
                    count++;
                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            Debug.Log($"[DragonBones] Generated materials for {count} textures.");
        }

        private static void GenerateMaterials(Texture2D texture)
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            string basePath = texturePath.Substring(0, texturePath.LastIndexOf('.'));

            // Generate normal material
            string matPath = basePath + "_Mat.mat";
            if (!File.Exists(matPath))
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    Debug.LogError("[DragonBones] Could not find Sprites/Default shader!");
                    return;
                }

                Material mat = new Material(shader);
                mat.name = texture.name + "_Mat";
                mat.mainTexture = texture;

                AssetDatabase.CreateAsset(mat, matPath);
                Debug.Log($"[DragonBones] Created material: {matPath}");
            }

            // Generate UI material
            string uiMatPath = basePath + "_UI_Mat.mat";
            if (!File.Exists(uiMatPath))
            {
                Shader uiShader = Shader.Find("UI/Default");
                if (uiShader == null)
                {
                    Debug.LogError("[DragonBones] Could not find UI/Default shader!");
                    return;
                }

                Material uiMat = new Material(uiShader);
                uiMat.name = texture.name + "_UI_Mat";
                uiMat.mainTexture = texture;

                AssetDatabase.CreateAsset(uiMat, uiMatPath);
                Debug.Log($"[DragonBones] Created UI material: {uiMatPath}");
            }
        }
    }
}
