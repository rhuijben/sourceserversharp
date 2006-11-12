// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;
using QQn.SourceServerIndexer.Providers;
using System.IO;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class IndexerState
	{
		readonly SortedList<string, SymbolFile> _symbolFiles = new SortedList<string,SymbolFile>(StringComparer.InvariantCultureIgnoreCase);
		readonly SortedList<string, SourceFile> _sourceFiles = new SortedList<string, SourceFile>(StringComparer.InvariantCultureIgnoreCase);
		readonly SortedList<string, SourceProvider> _srcProviders = new SortedList<string, SourceProvider>(StringComparer.InvariantCultureIgnoreCase);
		readonly List<SourceResolver> _resolvers = new List<SourceResolver>();

		/// <summary>
		/// 
		/// </summary>
		public IndexerState()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public SortedList<string, SymbolFile> SymbolFiles
		{
			get { return _symbolFiles; }
		}

		/// <summary>
		/// 
		/// </summary>
		public SortedList<string, SourceFile> SourceFiles
		{
			get { return _sourceFiles; }
		}

		/// <summary>
		/// 
		/// </summary>
		public List<SourceResolver> Resolvers
		{
			get { return _resolvers; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string NormalizePath(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			if (!Path.IsPathRooted(path))
				path = Path.GetFullPath(path);

			return path;
		}

		static string SafeId(string name)
		{
			StringBuilder sb = new StringBuilder();

			if (name.Length > 0)
			{
				if (!char.IsLetter(name, 0))
					sb.Append("PRV");
				else
					sb.Append(name);

				for (int i = 1; i < name.Length; i++)
					if (char.IsLetterOrDigit(name, i))
						sb.Append(name[i]);
			}
			else
				return "PRV";
			
			return sb.ToString();
		}

		internal string AssignId(SourceProvider sp, string name)
		{
			if (_srcProviders.ContainsValue(sp))
				return sp.Id;

			string id = SafeId(name);

			if (_srcProviders.ContainsKey(name))
			{
				string tmpId;
				int i=0;
				do
				{
					tmpId = string.Format("{0}{1:X}", id, i++);
				}
				while (_srcProviders.ContainsKey(tmpId));

				id = tmpId;
			}

			_srcProviders.Add(id, sp);

			return id;
		}
	}
}
