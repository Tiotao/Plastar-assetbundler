using System.Collections;
using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Bundler : MonoBehaviour {

	// Use this for initialization

	static GameObject marker, markerModel, building, buildingModel, anim, cast, castModel, hotspot2DContainer, story, wrapper, defaultSprites;

	static Hashtable markerInfo = new Hashtable();

	static Hashtable[] postcardsInfo;

	static Hashtable castInfo = new Hashtable();

	static Shader buildingShader;

	static int castType;


	void Start () {
		Init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	static void Init() {
		buildingShader = Shader.Find("Custom/Clip");
#if UNITY_EDITOR
		InitSprites();
#endif
		InitData();
		InitGameObjectStructure();
		InitBuildingModel();
		InitMarkerInfo();
		InitStory();
		InitCastInfo();
		InitCastModel();
		Debug.Log("model initiated");
#if UNITY_EDITOR
		AssetManager.CreatePrefab(marker.transform);
#endif
		
		
	}

	static void InitGameObjectStructure() {
		// wrapper = CreateEmptyGameObject("Markers", null);

		marker = CreateEmptyGameObject("Marker", null);

		building = CreateEmptyGameObject("Building", marker);
		buildingModel = CreateEmptyGameObject("BuildingModel", building);

		anim = CreateEmptyGameObject("Animation", building);

		cast = CreateEmptyGameObject("Cast", marker);
		castModel = CreateEmptyGameObject("CastModel", cast);
		defaultSprites = CreateEmptyGameObject("DefaultSprites", cast);
		hotspot2DContainer = CreateEmptyGameObject("HotSpot2DContainter", cast);

		story = CreateEmptyGameObject("Story", marker);
	}

	static void InitBuildingModel() {
		GameObject restInBuidling = CreateEmptyGameObject("rest", buildingModel);
		GameObject castInBuilding = CreateEmptyGameObject("cast", buildingModel);

		Mesh[] buildingMeshes = Resources.LoadAll<Mesh>("cast/building/buildingModel");
		Material castMaterial = Resources.Load<Material>("cast/building/materials/cast/cast");
		Material[] buildingMaterials = Resources.LoadAll<Material>("cast/building/materials/building");

		AddMesh(restInBuidling, buildingMeshes[0], buildingMaterials);
		AddMesh(castInBuilding, buildingMeshes[1], castMaterial);
		
	}

	static void InitCastModel() {
		Mesh castMesh = Resources.Load<Mesh>("cast/model/castModel");
		GameObject castModelObj = CreateEmptyGameObject("Model", castModel);
		AddMesh(castModelObj, castMesh);
		if (castType == 0) {
			castModelObj.transform.localScale = new Vector3(4f, 4f, 4f);
		} else {
			castModelObj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
		}
	}

	static void InitCastInfo() {
		
		// defaultSprites = CreateEmptyGameObject("DefaultSprites", cast);

		Sprite[] defaultSp = Resources.LoadAll<Sprite>("cast/model/rotatedView");

		Hotspot2D defaultHotspot = defaultSprites.AddComponent<Hotspot2D>();

		defaultHotspot._description = "Tap on the hotspots to learn about this cast.";
		defaultHotspot._sprites = defaultSp;

		foreach(Hashtable hotspotInfo in (Hashtable[]) castInfo["_hotspots"]) {
			GameObject hotspot = CreateEmptyGameObject("Hotspot", hotspot2DContainer);
			hotspot.transform.localPosition = (Vector3) hotspotInfo["_position"];
			hotspotInfo.Remove("_position");
			ApplyScript(hotspot, typeof(Hotspot2D), hotspotInfo);
		}


	}

	static void InitMarkerInfo() {
		ApplyScript(marker, typeof(Marker), markerInfo);
	}

	static void InitStory() {
		int storyCount = postcardsInfo.Length;

		for (int i = 0; i < storyCount; i++) {
			GameObject postcard = CreateEmptyGameObject("Postcard", story);
			Postcard pComp = postcard.AddComponent<Postcard>();
			pComp._year = (string) postcardsInfo[i]["_year"];
			pComp._frontImage = (Sprite) postcardsInfo[i]["_frontImage"];
			
			foreach (Hashtable hotspotInfo in (Hashtable[]) postcardsInfo[i]["_hotspots"]) {
				GameObject hotspot = CreateEmptyGameObject("Hotspot", postcard);
				ApplyScript(hotspot, typeof(HotspotStory), hotspotInfo);
			}

		}
	}

	static XmlDocument ReadXML(string path) {
		Debug.Log(path);
		TextAsset textAsset = Resources.Load<TextAsset>(path);  
		XmlDocument xmldoc = new XmlDocument ();
		xmldoc.LoadXml(textAsset.text);
		return xmldoc;

	}

	static void InitData() {
		// cast data
		XmlDocument xmldoc;
		
		xmldoc = ReadXML("cast/castInfo");
		XmlNode cast =  xmldoc.SelectSingleNode("cast");

		castType = int.Parse(cast.SelectSingleNode("castType").InnerText);
		
		// Tower of the Winds
		markerInfo.Add("_castName", cast.SelectSingleNode("name").InnerText);
		
		
		// I have some description
		markerInfo.Add("_castDescription", cast.SelectSingleNode("description").InnerText);
		
		// Athens, Greece (c. 100 - 37 BC)
		string locationTime = cast.SelectSingleNode("location").InnerText + " (c. " + cast.SelectSingleNode("time").InnerText+ ")";
		markerInfo.Add("_castLocationTime", locationTime);

		// castMap.png
		markerInfo.Add("_icon", Resources.Load<Sprite>("cast/castIcon"));
		// castIcon.png
		markerInfo.Add("_buildingMap", Resources.Load<Sprite>("cast/castMap"));

		// casts
		xmldoc = ReadXML("cast/model/hotspots/hotspotsInfo");
		XmlNode model =  xmldoc.SelectSingleNode("cast");
		XmlNodeList castHotspotList = model.SelectNodes("hotspot");
		int castHotspotCount = castHotspotList.Count;
		
		Hashtable[] castHotspots = new Hashtable[castHotspotCount];
		
		for (int i = 0; i < castHotspotCount; i++) {
			XmlNode hotspotNode = castHotspotList[i];
			XmlNode posNode = hotspotNode.SelectSingleNode("position");
			Hashtable hotspot = new Hashtable();
			Vector3 pos = new Vector3(
				float.Parse(posNode.SelectSingleNode("x").InnerText),
				float.Parse(posNode.SelectSingleNode("y").InnerText),
				float.Parse(posNode.SelectSingleNode("z").InnerText)
			);
			string id = hotspotNode.SelectSingleNode("id").InnerText;
			hotspot.Add("_position", pos);
			hotspot.Add("_description", hotspotNode.SelectSingleNode("description").InnerText);
			hotspot.Add("_sprites", Resources.LoadAll<Sprite>("cast/model/hotspots/hotspot" + id));
			castHotspots[i] = hotspot;
		}

		castInfo.Add("_hotspots", castHotspots);


		// stories
		int postcardCount = Convert.ToInt32(cast.SelectSingleNode("storiesCount").InnerText);
		postcardsInfo = new Hashtable[postcardCount];
		
		for (int i = 0; i < postcardCount; i++) {

			// each postcard
			
			string folderName = "postcard" + i.ToString().PadLeft(3, '0');
			string folderPath = "cast/story/" + folderName;
			
			xmldoc = ReadXML("cast/story/" + folderName + "/postcardInfo");
			XmlNode postcardNode =  xmldoc.SelectSingleNode("postcard");
			XmlNodeList storyHotspotList =  postcardNode.SelectNodes("hotspot");
			
			postcardsInfo[i] = new Hashtable();

			postcardsInfo[i].Add("_year", postcardNode.SelectSingleNode("year").InnerText);

			postcardsInfo[i].Add("_frontImage", Resources.Load<Sprite>(folderPath + "/front"));

			Hashtable[] storyHotspots = new Hashtable[storyHotspotList.Count];

			for (int j = 0; j < storyHotspotList.Count; j++) {
				XmlNode hotspotNode = storyHotspotList[j];
				string id = hotspotNode.SelectSingleNode("id").InnerText;
				XmlNode posNode = hotspotNode.SelectSingleNode("position");
				Vector2 pos = new Vector2(
					float.Parse(posNode.SelectSingleNode("x").InnerText),
					float.Parse(posNode.SelectSingleNode("y").InnerText)
					);
				storyHotspots[j] = new Hashtable();
				storyHotspots[j].Add("_coolFact", hotspotNode.SelectSingleNode("coolfact").InnerText);
				storyHotspots[j].Add("_description", hotspotNode.SelectSingleNode("description").InnerText);
				storyHotspots[j].Add("_sprite", Resources.Load<Sprite>(folderPath + "/" + id));
				storyHotspots[j].Add("_position", pos);
			}

			postcardsInfo[i].Add("_hotspots", storyHotspots);

		}


	}

	static void ApplyScript(GameObject obj, Type script, Hashtable param)  {
		var scr = obj.AddComponent(script);
		foreach(string key in param.Keys) {
			var val = param[key];
			Debug.Log(key);
			Debug.Log(val);
			FieldInfo fieldInfo = scr.GetType().GetField (key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			fieldInfo.SetValue(scr, val);
		}
	}

	static void AddMesh(GameObject obj, Mesh mesh) {
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		// MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
		obj.AddComponent<MeshCollider>();
	}

	
	static void AddMesh(GameObject obj, Mesh mesh, Material mat) {
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
		mat.shader = buildingShader;
		renderer.material = mat;
		
	}

	static void AddMesh(GameObject obj, Mesh mesh, Material[] mats) {
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
		foreach (Material m in mats) {
			m.shader = buildingShader;
		}
		renderer.materials = mats;
	}

	static GameObject CreateEmptyGameObject(string name, GameObject parent) {
		GameObject g = new GameObject(name);
		if (parent) {
			g.transform.parent = parent.transform;
		}
		return g;
	}

	// helpers

	static void DirSearch(string sDir, List<string> pathList){
		try
		{
			foreach (string d in Directory.GetDirectories(sDir))
			{
				foreach (string f in Directory.GetFiles(d, "*.png"))
				{
					Debug.Log(f);
					pathList.Add(f);
				}
				DirSearch(d, pathList);
			}
		}
		catch (System.Exception excpt)
		{
			Console.WriteLine(excpt.Message);
		}

	}
#if UNITY_EDITOR
	static public void InitSprites() {
		List<string> filePaths = new List<string>();

		DirSearch("Assets/Resources/cast", filePaths);

		Debug.Log(filePaths.Count);

		string currentDir = Environment.CurrentDirectory;
		DirectoryInfo directory = new DirectoryInfo(currentDir);
		string fullDirectory = directory.FullName;



		
		foreach (string file in filePaths) {
			string path = file;
			Debug.Log("convert texture: " + path);

			AssetManager.ImportTextureByPath(path);

			// filePaths.Add(file.DirectoryName);
		}
		

	}
#endif

	// public void ImportTextureByPath(string path) {
	// 	TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
	// 	tImporter.textureType = TextureImporterType.Sprite;
	// 	AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
	// }

	// void CreatePrefab(Transform marker) {
	// 	GameObject prefab = PrefabUtility.CreatePrefab("Assets/Prefabs/Markers.prefab", (GameObject)marker.gameObject, ReplacePrefabOptions.ReplaceNameBased);
	// 	string assetPath = AssetDatabase.GetAssetPath(prefab.GetInstanceID());
	// 	AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("Markers", "");
	// }

	// void Bundle(string assetPath) {
	// 	BuildPipeline.BuildAssetBundles ("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.Android);
	// }


}
