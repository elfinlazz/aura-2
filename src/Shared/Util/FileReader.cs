// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Aura.Shared.Util
{
	/// <summary>
	/// General use class to read files line for line.
	/// </summary>
	/// <remarks>
	/// Ignores empty lines and lines prefixed with '!', ';', '#', '//', or '--'.
	/// Supports including of files via 'include {file}' and 'require {file}'.
	/// Require raises an exception if the file couldn't be found.
	/// </remarks>
	/// <example>
	/// <code>
	///	using (var fr = new FileReader(filePath))
	///	{
	///		foreach (string line in fr)
	///		{
	///			// Do something with line
	///		}
	///	}
	///	</code>
	///	</example>
	public class FileReader : IEnumerable<string>, IDisposable
	{
		private string _filePath;
		private string _relativePath;
		private StreamReader _streamReader;

		public int CurrentLine { get; protected set; }

		public FileReader(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File '" + filePath + "' not found.");

			_filePath = filePath;
			_relativePath = Path.GetDirectoryName(Path.GetFullPath(filePath));

			_streamReader = new StreamReader(filePath);
		}

		public IEnumerator<string> GetEnumerator()
		{
			string line;

			// Until EOF
			while ((line = _streamReader.ReadLine()) != null)
			{
				this.CurrentLine++;

				line = line.Trim();

				if (string.IsNullOrWhiteSpace(line))
					continue;

				// Ignore very short or commented lines
				if (line.Length < 2 || line[0] == '!' || line[0] == ';' || line[0] == '#' || line.StartsWith("//") || line.StartsWith("--"))
					continue;

				// Include files
				bool require = false, divert = false;
				if (line.StartsWith("include ") || (require = line.StartsWith("require ")) || (divert = line.StartsWith("divert ")))
				{
					var fileName = line.Substring(line.IndexOf(' ')).Trim(' ', '"');
					var includeFilePath = Path.Combine((!fileName.StartsWith("/") ? _relativePath : ""), fileName.TrimStart('/'));

					// Prevent rekursive including
					if (includeFilePath != _filePath)
					{
						// Silently ignore failed includes, only raise an
						// exception on require.
						if (File.Exists(includeFilePath))
						{
							using (var fr = new FileReader(includeFilePath))
							{
								foreach (var incLine in fr)
									yield return incLine;
							}

							// Stop reading current file if divert was successful
							if (divert)
								yield break;
						}
						else if (require)
						{
							throw new FileNotFoundException("Required file '" + includeFilePath + "' not found.");
						}
					}

					continue;
				}

				yield return line;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Dispose()
		{
			_streamReader.Close();
		}
	}
}
