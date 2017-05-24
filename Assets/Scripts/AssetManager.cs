#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class AssetManager {

#if UNITY_EDITOR
    public static void ImportTextureByPath(string path) {
		TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		if (tImporter.textureType == TextureImporterType.Sprite) {
			return;
		}
		tImporter.textureType = TextureImporterType.Sprite;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
	}

	public static void CreatePrefab(Transform marker, string castId) {
		GameObject prefab = PrefabUtility.CreatePrefab("Assets/Prefabs/Marker" + castId + ".prefab", (GameObject)marker.gameObject, ReplacePrefabOptions.ReplaceNameBased);
		string assetPath = AssetDatabase.GetAssetPath(prefab.GetInstanceID());
		AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("Marker", "");
	}
#endif

}