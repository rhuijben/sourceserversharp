// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.ComponentModel;

namespace QQn.SourceServerIndexer
{
	/// <summary>
	/// MSBuild SourceServerIndex task implementation
	/// </summary>
	public class SourceServerIndexTask : Task
	{
		ITaskItem[] _symbolRoots = new ITaskItem[0];
		ITaskItem[] _sourceRoots = new ITaskItem[0];
		ITaskItem[] _symbolFiles = new ITaskItem[0];
		bool _includeHiddenDirectories;
		bool _includeDotDirs;
		bool _notRecursive;

		/// <summary>
		/// Gets or sets a list of symbol root directories
		/// </summary>
		/// <remarks>.pdf files in these directories are added to the <see cref="SymbolFiles"/></remarks>
		public ITaskItem[] SymbolRoots
		{
			get { return _symbolRoots; }
			set
			{
				if (value == null)
					_symbolRoots = new ITaskItem[0];
				else
					_symbolRoots = value;
			}
		}

		/// <summary>
		/// Gets or sets a list of source root directories
		/// </summary>
		public ITaskItem[] SourceRoots
		{
			get { return _sourceRoots; }
			set
			{
				if (value == null)
					_sourceRoots = new ITaskItem[0];
				else
					_sourceRoots = value;
			}
		}

		/// <summary>
		/// Gets or sets a list of symbol files
		/// </summary>
		public ITaskItem[] SymbolFiles
		{
			get { return _symbolFiles; }
			set
			{
				if (value == null)
					_symbolFiles = new ITaskItem[0];
				else
					_symbolFiles = value;
			}
		}

		/// <summary>
		/// Gets or sets a boolean indicating whether to index hidden directories (via <see cref="SymbolRoots"/>)
		/// </summary>
		[DefaultValue(false)]
		public bool IncludeHiddenDirectories
		{
			get { return _includeHiddenDirectories; }
			set { _includeHiddenDirectories = value; }
		}

		/// <summary>
		/// Gets or sets a boolean indicating whether to index directories starting with a dot or underscore (via <see cref="SymbolRoots"/>)
		/// </summary>
		[DefaultValue(false)]
		public bool IncludeDotDirs
		{
			get { return _includeDotDirs; }
			set { _includeDotDirs = value; }
		}

		/// <summary>
		/// Gets or sets a boolean indicating whether to recursively search for pdb files (via <see cref="SymbolRoots"/>)
		/// </summary>
		[DefaultValue(false)]
		public bool NotRecursive
		{
			get { return _notRecursive; }
			set { _notRecursive = value; }
		}

		/// <summary>
		/// Executes the indexing task
		/// </summary>
		/// <returns>true if the indexing succeeded, othewise false</returns>
		public override bool Execute()
		{
			SortedList<string, string> symbolFiles = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			SortedList<string, string> sourceRoots = new SortedList<string,string>(StringComparer.InvariantCultureIgnoreCase);

			foreach (ITaskItem item in SymbolFiles)
			{
				string pdb = Path.GetFullPath(item.ItemSpec);

				symbolFiles[pdb] = pdb;
			}

			foreach (ITaskItem item in SourceRoots)
			{
				string dir = Path.GetFullPath(item.ItemSpec);

				sourceRoots[dir] = dir;
			}

			Dictionary<string, string> searchedPaths = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			foreach(ITaskItem item in SymbolRoots)
			{
				RecursiveSearchSymbols(new DirectoryInfo(item.ItemSpec), searchedPaths, symbolFiles);
			}

			SourceServerIndexer indexer = new SourceServerIndexer();

			indexer.SymbolFiles = new List<string>(symbolFiles.Values);
			indexer.SourceRoots = new List<string>(sourceRoots.Values);

			return indexer.Exec();
		}

		void RecursiveSearchSymbols(DirectoryInfo dir, Dictionary<string, string> searchedPaths, SortedList<string, string> symbolFiles)
		{
			if (searchedPaths.ContainsKey(dir.FullName))
				return;

			string searchPath = dir.FullName;
			searchedPaths.Add(searchPath, searchPath);

			foreach (FileInfo file in dir.GetFiles("*.pdb"))
			{
				string fullPath = file.FullName;
				symbolFiles[fullPath] = fullPath;
			}

			foreach (DirectoryInfo subDir in dir.GetDirectories())
			{
				if (!IncludeHiddenDirectories && ((subDir.Attributes & FileAttributes.Hidden) != 0))
					continue;

				if(!IncludeDotDirs && ("._".IndexOf(subDir.Name[0]) >= 0))
					continue;
					
				RecursiveSearchSymbols(subDir, searchedPaths, symbolFiles);
			}
		}
	}
}
