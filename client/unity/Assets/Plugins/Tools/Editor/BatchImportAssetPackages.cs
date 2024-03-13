using UnityEngine;
using UnityEditor;
using System.IO;

namespace Wtf.Editor
{
	public class BatchImportAssetPackages
	{

		[MenuItem("Tools/Import Packages In Folder", false, 80)]
		public static void ImportPackagesInFolder()
		{
			string packagePath = EditorUtility.OpenFolderPanel("Import Packages In Folder", "", "");
			packagePath = packagePath.Replace("\\", "/");
			if (packagePath.EndsWith("/"))
			{
				packagePath = packagePath.Substring(0, packagePath.Length - 1);
			}

			Debug.Log("Importing packages in folder: " + packagePath);

			ImportPackagesInFolderPath(packagePath);
		}

		public static void ImportPackagesInFolderPath(string packagePath)
		{
			string[] allFilePaths = Directory.GetFiles(packagePath);

			try
			{
				foreach (string curPath in allFilePaths)
				{
					string fileToImport = curPath.Replace("\\", "/");
					if (Path.GetExtension(fileToImport).ToLower() == ".unitypackage")
					{
						Debug.Log("Importing: " + fileToImport);
						AssetDatabase.ImportPackage(fileToImport, false);
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.Log("Error: " + ex.Message);
			}
		}

		[MenuItem("Tools/Import Select Plugins", false, 81)]
		public static void ImportSelectPlugins()
		{
			var assetsPath = Application.dataPath;
			var projectPath = Path.GetDirectoryName(assetsPath);
			var rootPath = Path.GetDirectoryName(projectPath);
			var pluginPath = Path.Combine(rootPath, "plugins");
			Debug.Log("pluginPath: " + pluginPath);
			ImportPackagesInFolderPath(pluginPath);
		}
	}
}