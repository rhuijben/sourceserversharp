using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using QQn.SourceServerSharp;
using QQn.SourceServerSharp.Framework;

namespace QQn.SourceServerSharp.Tasks
{
	[TaskName("SssIndex")]
	public class SssIndex : Task
	{
		SssDirSet[] _sourceRoots;
		DirSet[] _excludeSourceRoots;
		SssFileSet[] _sourceFiles;
		FileSet[] _excludeSourceFiles;

		FileSet[] _symbols;
		string _types;

		[BuildElementArray("Sources")]
		public SssDirSet[] Sources
		{
			get { return _sourceRoots; }
			set { _sourceRoots = value; }
		}

		[BuildElementArray("ExcludeSources")]
		public DirSet[] ExcludeSources
		{
			get { return _excludeSourceRoots; }
			set { _excludeSourceRoots = value; }
		}

		[BuildElementArray("SourceFiles")]
		public SssFileSet[] SourceFiles
		{
			get { return _sourceFiles; }
			set { _sourceFiles = value; }
		}

		[BuildElementArray("ExcludeSourceFiles")]
		public FileSet[] ExcludeSourceFiles
		{
			get { return _excludeSourceFiles; }
			set { _excludeSourceFiles = value; }
		}

		[BuildElementArray("Symbols", Required=true)]
		public FileSet[] Symbols
		{
			get { return _symbols; }
			set { _symbols = value; }
		}

		[TaskAttribute("Type")]
		public string Types
		{
			get { return _types; }
			set { _types = value; }
		}		

		protected override void ExecuteTask()
		{
			SourceServerIndexer indexer = new SourceServerIndexer();

			if(Sources != null)
				foreach(SssDirSet set in Sources)
				{
					bool hasData = !string.IsNullOrEmpty(set.Type);

					foreach(string dir in set.DirectoryNames)
					{
						indexer.SourceRoots.Add(dir);

						if(hasData)
							indexer.IndexerData[dir] = new IndexerTypeData(dir, set.Type, set.Info);
					}
				}

			if(SourceFiles != null)
				foreach(SssFileSet set in SourceFiles)
				{
					bool hasData = !string.IsNullOrEmpty(set.Type);

					foreach(string file in set.FileNames)
					{
						indexer.SourceRoots.Add(file);

						if (hasData)
							indexer.IndexerData[file] = new IndexerTypeData(file, set.Type, set.Info);
					}
				}

			if(ExcludeSources != null)
				foreach(DirSet set in ExcludeSources)
				{
					foreach(string dir in set.DirectoryNames)
					{
						indexer.ExcludeSourceRoots.Add(dir);
					}
				}

			foreach(FileSet set in Symbols)
			{
				foreach(string file in set.FileNames)
				{
					indexer.SymbolFiles.Add(file);
				}
			}
			
			if (!string.IsNullOrEmpty(Types))
			{
				indexer.Types.Clear();
				foreach (string type in Types.Split(';'))
				{
					string item = type.Trim();
					if (!string.IsNullOrEmpty(item))
						indexer.Types.Add(item);
				}
			}
			else if(Properties.Contains("SssIndex.Type"))
			{
				indexer.Types.Clear();
				foreach (string type in Properties["SssIndex.Type"].Split(';'))
				{
					string item = type.Trim();
					if (!string.IsNullOrEmpty(item))
						indexer.Types.Add(item);
				}
			}

			if (Properties.Contains("SssIndex.Providers"))
			{
				indexer.Providers.Clear();

				foreach (string type in Properties["SssIndex.Providers"].Split(';'))
				{
					string item = type.Trim();
					if (!string.IsNullOrEmpty(item))
						indexer.Providers.Add(item);
				}
			}

			if (Properties.Contains("SssIndex.SourceServerSdkDir"))
			{
				indexer.SourceServerSdkDir = Properties["SssIndex.SourceServerSdkDir"];
			}

			IndexerResult result = null;
			try
			{
				result = indexer.Exec();
			}
			catch (Exception e)
			{
				throw new BuildException("SssIndex: SourceServer indexing failed", e);
			}

			if (result == null)
				throw new BuildException("SssIndex: SourceServer indexing failed");
			else if (!result.Success)
				throw new BuildException("SssIndex: " + result.ErrorMessage);
			else
				Log(Level.Info, "SourceServer-annotated {0} symbolfile(s) from {1} sourcefile reference(s) with {2} provider(s)", result.IndexedSymbolFiles, result.IndexedSourceFiles, result.ProvidersUsed);
		}
	}
}
