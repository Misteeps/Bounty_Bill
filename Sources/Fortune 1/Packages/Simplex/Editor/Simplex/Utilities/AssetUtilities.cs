using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

using UnityEditor;


namespace Simplex.Editor
{
	public static class AssetUtilities
	{
		#region Asset
		public readonly struct Asset
		{
			public readonly string name;
			public readonly string path;
			public readonly string guid;
			public readonly long localID;
			public readonly int instanceID;

			public readonly Type type;
			public readonly Object asset;


			public Asset(int instanceID)
			{
				this.instanceID = instanceID;
				this.path = AssetDatabase.GetAssetPath(instanceID);
				AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceID, out guid, out localID);

				asset = AssetDatabase.LoadAssetAtPath<Object>(path);
				type = asset.GetType();
				name = asset.name;
			}

			public override string ToString() => ConsoleUtilities.Format($"{type:type} {name}\n    Path: {path:info}\n    GUID: {guid:info}  |  Local ID: {localID:info}  |  Instance ID: {instanceID:info}");
		}
		#endregion Asset


		[MenuItem("Assets/Print Info", false, 34)]
		public static void PrintInfo()
		{
			IEnumerable<Asset> assets = Selection.instanceIDs.Select(id => new Asset(id));
			assets.LogCollection($"Asset Info");
		}

		[MenuItem("Assets/Print USS URL", false, 34)]
		public static void PrintUSSURL()
		{
			IEnumerable<Asset> assets = Selection.instanceIDs.Select(id => new Asset(id));
			assets.LogCollection($"Asset USS URLs", asset => $"url('project://database/{asset.path}?fileID={asset.localID}&guid={asset.guid}&type=3#{asset.name}');");
		}

		public static string GetDirectory(this Object obj) => System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));
		public static string GetPath(this Object obj) => AssetDatabase.GetAssetPath(obj);
		public static string GetGuid(this Object obj) => AssetDatabase.AssetPathToGUID(obj.GetPath());
		public static Texture2D GetIcon<T>(this T obj) where T : Object => (Texture2D)EditorGUIUtility.ObjectContent(obj, typeof(T)).image;

		public static T Load<T>(string path, string guid) where T : Object
		{
			T pathObj = (path.Empty()) ? null : LoadPath<T>(path);
			T guidObj = (guid.Empty()) ? null : LoadGuid<T>(guid);

			if (pathObj != null && guidObj != null && pathObj != guidObj)
				ConsoleUtilities.Warn($"Loading asset path and guid do not match\nPath: {path:info} = {pathObj:ref}\nGuid: {guid:info} = {guidObj:ref}");

			return pathObj ?? guidObj;
		}
		public static T LoadPath<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
		public static T LoadGuid<T>(string guid) where T : Object => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
		public static bool TryLoad<T>(string path, string guid, out T obj) where T : Object
		{
			obj = Load<T>(path, guid);
			return obj != null;
		}
		public static bool TryLoadPath<T>(string path, out T obj) where T : Object
		{
			obj = LoadPath<T>(path);
			return obj != null;
		}
		public static bool TryLoadGuid<T>(string guid, out T obj) where T : Object
		{
			obj = LoadGuid<T>(guid);
			return obj != null;
		}

		public static T[] Find<T>(string filter, params string[] folders) where T : Object => AssetDatabase.FindAssets(filter, folders).Select(guid => LoadGuid<T>(guid)).Where(asset => asset != null).ToArray();
		public static string[] FindPaths(string filter, params string[] folders) => AssetDatabase.FindAssets(filter, folders).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
		public static string[] FindGuids(string filter, params string[] folders) => AssetDatabase.FindAssets(filter, folders);
	}
}