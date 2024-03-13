using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Wtf
{
	public static class CsvUtils
	{
		private static List<string> MatchList = new List<string>();

		public static string[,] Deserialize(string csvText)
		{
			string[] array = csvText.Split("\n"[0]);
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				List<string> list = SplitCsvLine(array[i]);
				num = Mathf.Max(num, list.Count);
			}
			string[,] array2 = new string[num + 1, array.Length + 1];
			for (int j = 0; j < array.Length; j++)
			{
				List<string> list2 = SplitCsvLine(array[j]);
				for (int k = 0; k < list2.Count; k++)
				{
					array2[k, j] = list2[k];
					array2[k, j] = array2[k, j].Replace("\"\"", "\"");
				}
			}
			return array2;
		}

		public static List<string> SplitCsvLine(string line)
		{
			MatchList.Clear();
			MatchCollection matchCollection = Regex.Matches(line, "(((?<x>(?=[,\\r\\n]+))|\"(?<x>([^\"]|\"\")+)\"|(?<x>[^,\\r\\n]+)),?)", RegexOptions.ExplicitCapture);
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match match = matchCollection[i];
				MatchList.Add(match.Groups[1].Value);
			}
			return MatchList;
		}
	}
}