// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerSharp.Framework;
using System.IO;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using System.Xml;
using System.Xml.XPath;
using SharpSvn;

namespace QQn.SourceServerSharp.Providers
{
	/// <summary>
	/// Implements the <see cref="SourceProvider"/> class for subversion (http://subversion.tigris.org/)
	/// </summary>
	public class SubversionResolver : SourceResolver, ISourceProviderDetector
	{
		/// <summary>
		/// 
		/// </summary>
		public SubversionResolver(IndexerState state)
			: base(state, "Subversion")
		{
			//GC.KeepAlive(null);
		}

		#region ### Availability
		/// <summary>
		/// 
		/// </summary>
		public override bool Available
		{
            get { return true; }
		}

		#region #### ISourceProviderDetector Members

		/// <summary>
		/// SourceIndex detector which tries to find valid providers
		/// </summary>
		/// <param name="state"></param>
		/// <returns>true if one or more files might be managed in subversion, otherwise false</returns>
		public bool CanProvideSources(QQn.SourceServerSharp.Framework.IndexerState state)
		{
			SortedList<string, string> directories = new SortedList<string,string>(StringComparer.InvariantCultureIgnoreCase);

			foreach(SourceFile file in state.SourceFiles.Values)
			{
				string dir = file.File.DirectoryName;

				if (directories.ContainsKey(dir))
					continue;

				directories.Add(dir, dir);

				if (Directory.Exists(Path.Combine(dir, ".svn")))
					return true;
				else if (Directory.Exists(Path.Combine(dir, "_svn")))
					return true; // Might not work; requires environment variable
			}

			return false;
		}

		#endregion #### ISourceProviderDetector Members
		#endregion ### Availability

		static bool ContainsAt(string hayStack, int index, string needle)
		{
			if (hayStack == null)
				throw new ArgumentNullException("hayStack");
			else if (index < 0)
				throw new ArgumentException("offset out of range", "index");
			else if (string.IsNullOrEmpty(needle))
				throw new ArgumentNullException("needle");

			if (hayStack.Length < index + needle.Length)
				return false;

			return 0 == string.Compare(hayStack, index, needle, 0, needle.Length);
		}

        void OnSvnInfo(object sender, SvnInfoEventArgs e)
        {
            SourceFile file;

            string path = State.NormalizePath(e.Path);

            if (!State.SourceFiles.TryGetValue(path, out file) || file.IsResolved)
                return; 

            file.SourceReference = new SubversionSourceReference(this, file, e.RepositoryRoot, e.RepositoryRoot.MakeRelativeUri(e.Uri), e.LastChangeRevision, e.Revision);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool ResolveFiles()
		{
            SvnClient client = new SvnClient();
            SvnInfoArgs infoArgs = new SvnInfoArgs();

            infoArgs.ThrowOnError = false;
            infoArgs.Depth = SvnDepth.Empty;
            infoArgs.Info += new EventHandler<SvnInfoEventArgs>(OnSvnInfo);

			foreach (KeyValuePair<string, SourceFile> file in State.SourceFiles)
			{
				if (file.Value.IsResolved)
					continue;

                if (client.Info(new SvnPathTarget(file.Key), infoArgs, new EventHandler<SvnInfoEventArgs>(OnSvnInfo)))
                {
                    // Info set in OnSvnInfo
                }               
			}

            return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void WriteEnvironment(StreamWriter writer)
		{
			writer.Write(Id);
			writer.WriteLine(@"__TRG=%targ%\%var7%%fnbksl%(%var4%)\%var5%\%fnfile%(%var4%)");
			writer.Write(Id);
			writer.Write("__CMD=svn.exe export \"%var3%%var4%@%var6%\" \"%");
			writer.Write(Id);
			writer.WriteLine("__TRG%\" --non-interactive --quiet");
		}

		/// <summary>
		/// 
		/// </summary>
		public override int SourceEntryVariableCount
		{
			get { return 5; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class SubversionSourceReference : SourceReference
	{
		readonly Uri _reposRoot;
		readonly Uri _itemPath;
		readonly long _commitRev;
		readonly long _wcRev;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resolver"></param>
		/// <param name="sourceFile"></param>
		/// <param name="reposRoot"></param>
		/// <param name="itemPath"></param>
		/// <param name="commitRev"></param>
		/// <param name="wcRev"></param>
		public SubversionSourceReference(SourceProvider resolver, SourceFile sourceFile, Uri reposRoot, Uri itemPath, long commitRev, long wcRev)
			: base(resolver, sourceFile)
		{
			if (reposRoot == null)
				throw new ArgumentNullException("reposRoot");
			else if (itemPath == null)
				throw new ArgumentNullException("itemPath");

			_reposRoot = reposRoot;
			_itemPath = itemPath;

			_commitRev = commitRev;
			_wcRev = wcRev;
		}

		static string ReposSubDir(Uri reposUri)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("svn-");
			sb.Append(reposUri.Scheme);
			sb.Append("\\");
			if(!string.IsNullOrEmpty(reposUri.Host))
			{
				foreach(char c in reposUri.Host)
					if(char.IsLetterOrDigit(c) || (".-".IndexOf(c) >= 0))
						sb.Append(c);
					else
						sb.Append('_');

				if(reposUri.Port >= 1)
					sb.AppendFormat("_{0}", reposUri.Port);
			}

			if(!string.IsNullOrEmpty(reposUri.AbsolutePath))
			{
				sb.Append(reposUri.AbsolutePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
			}
			
			if(sb[sb.Length-1] != Path.DirectorySeparatorChar)
				sb.Append(Path.DirectorySeparatorChar);

			return sb.ToString();
		}

		/// <summary>
		/// Gets a list of entries for the sourcefiles
		/// </summary>
		/// <returns></returns>
		public override string[] GetSourceEntries()
		{
			return new string[]
			{
				_reposRoot.ToString(),
				_itemPath.ToString(),
				_commitRev.ToString(),
				_wcRev.ToString(),				
				ReposSubDir(_reposRoot)
			};
		}
	}
}
