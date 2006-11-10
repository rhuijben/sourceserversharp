using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace QQn.SourceServerIndexer
{
	public class SourceServerIndexTask : Task
	{
		ITaskItem[] _symbolRoots = new ITaskItem[0];
		ITaskItem[] _sourceRoots = new ITaskItem[0];
		ITaskItem[] _symbolFiles = new ITaskItem[0];
		bool _includeHiddenDirectories;

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

		public bool IncludeHiddenDirectories
		{
			get { return _includeHiddenDirectories; }
			set { _includeHiddenDirectories = value; }
		}

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
				if (IncludeHiddenDirectories || ((subDir.Attributes & FileAttributes.Hidden) != 0))
				{
					RecursiveSearchSymbols(subDir, searchedPaths, symbolFiles);
				}
			}
		}
	}
}
