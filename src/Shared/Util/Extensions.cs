// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Shared.Util
{
	public static class Extensions
	{
		/// <summary>
		/// Calculates differences between 2 strings.
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Levenshtein_distance
		/// </remarks>
		/// <example>
		/// <code>
		/// "test".LevenshteinDistance("test")       // == 0
		/// "test1".LevenshteinDistance("test2")     // == 1
		/// "test1".LevenshteinDistance("test1 asd") // == 4
		/// </code>
		/// </example>
		public static int LevenshteinDistance(this string str, string compare, bool caseSensitive = true)
		{
			if (!caseSensitive)
			{
				str = str.ToLower();
				compare = compare.ToLower();
			}

			var sLen = str.Length;
			var cLen = compare.Length;
			var result = new int[sLen + 1, cLen + 1];

			if (sLen == 0)
				return cLen;

			if (cLen == 0)
				return sLen;

			for (int i = 0; i <= sLen; result[i, 0] = i++) ;
			for (int i = 0; i <= cLen; result[0, i] = i++) ;

			for (int i = 1; i <= sLen; i++)
			{
				for (int j = 1; j <= cLen; j++)
				{
					var cost = (compare[j - 1] == str[i - 1]) ? 0 : 1;
					result[i, j] = Math.Min(Math.Min(result[i - 1, j] + 1, result[i, j - 1] + 1), result[i - 1, j - 1] + cost);
				}
			}

			return result[sLen, cLen];
		}

		/// <summary>
		/// Returns a random item from the given IList
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static T Random<T>(this IList<T> list)
		{
			return list[RandomProvider.Get().Next(list.Count)];
		}
	}
}
