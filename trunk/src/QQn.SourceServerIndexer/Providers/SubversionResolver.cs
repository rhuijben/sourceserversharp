// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;
using System.IO;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using System.Xml;
using System.Xml.XPath;

namespace QQn.SourceServerIndexer.Providers
{
	/// <summary>
	/// Implements the <see cref="SourceProvider"/> class for subversion (http://subversion.tigris.org/)
	/// </summary>
	public class SubversionResolver : SourceResolver, ISourceProviderDetector
	{
		string _svnExePath;
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
			get { return (SvnExePath != null); }
		}

		bool _searchedPath;
		/// <summary>
		/// 
		/// </summary>
		protected string SvnExePath
		{
			get
			{
				if(_searchedPath || !string.IsNullOrEmpty(_svnExePath))
					return _svnExePath;

				string path = Environment.GetEnvironmentVariable("PATH");
				if(path == null)
					path = "";
				else
					path = path.ToUpperInvariant();

				string[] pathItems = path.Split(Path.PathSeparator);

				// First try to find some directory with subversion in its name (probably ok for 99% of the cases)
				foreach(string item in pathItems)
				{
					if(item.Contains("SUBVERSION"))
					{
						string file = Path.GetFullPath(Path.Combine(item.Trim(), "SVN.EXE"));

						if(File.Exists(file))
							return _svnExePath = file;							
					}
				}

				// Search whole path
				foreach(string item in pathItems)
				{
					string file = Path.GetFullPath(Path.Combine(item.Trim(), "SVN.EXE"));

					if(File.Exists(file))
						return _svnExePath = file;							
				}

				_searchedPath = true;
				return null;
			}
		}
		

		#region #### ISourceProviderDetector Members

		/// <summary>
		/// SourceIndex detector which tries to find valid providers
		/// </summary>
		/// <param name="state"></param>
		/// <returns>true if one or more files might be managed in subversion, otherwise false</returns>
		public bool CanProvideSources(QQn.SourceServerIndexer.Framework.IndexerState state)
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

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool ResolveFiles()
		{
			ProcessStartInfo psi = new ProcessStartInfo(SvnExePath);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			CommandLineBuilder cb = new CommandLineBuilder();

			cb.AppendSwitch("--non-interactive");
			//cb.AppendSwitch("--verbose");
			//cb.AppendSwitch("--non-recursive");
			cb.AppendSwitch("--xml");

			cb.AppendSwitch("info");

			foreach (SourceFile file in State.SourceFiles.Values)
			{
				if (file.IsResolved)
					continue;

				cb.AppendFileNameIfNotNull(file.FullName);				
			}

			psi.Arguments = cb.ToString();

			using (Process p = Process.Start(psi))
			{
				XPathDocument doc = new XPathDocument(p.StandardOutput);

				XPathNavigator nav = doc.CreateNavigator();

				foreach (XPathNavigator i in nav.Select("/info/entry[@path and url and repository/root and commit/@revision]"))
				{
					SourceFile file;

					string path = State.NormalizePath(i.GetAttribute("path", ""));

					if (!State.SourceFiles.TryGetValue(path, out file))
						continue;

					if (file.IsResolved)
						continue; // No need to resolve it again

					XPathNavigator urlNav = i.SelectSingleNode("url");
					XPathNavigator repositoryRootNav = i.SelectSingleNode("repository/root");
					XPathNavigator commit = i.SelectSingleNode("commit");

					if (urlNav == null || repositoryRootNav == null || commit == null)
						continue; // Not enough information to provide reference

					string itemPath = urlNav.Value;
					string reposRoot = repositoryRootNav.Value;

					if (!reposRoot.EndsWith("/"))
						reposRoot += '/';

					if (!itemPath.StartsWith(reposRoot, StringComparison.InvariantCultureIgnoreCase))
						continue;
					else
						itemPath = itemPath.Substring(reposRoot.Length);

					string commitRev = commit.GetAttribute("revision", "");
					string wcRev = i.GetAttribute("revision", "");
										
					file.SourceReference = new SubversionSourceReference(this, file, new Uri(reposRoot), new Uri(itemPath, UriKind.Relative), int.Parse(commitRev), int.Parse(wcRev));
				}
				

				p.WaitForExit();
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
			writer.WriteLine(@"__TRG=%targ%\%var6%%fnbksl%(%var4%)\%var5%\%fnfile%(%var4%)");
			writer.Write(Id);
			writer.Write("__CMD=svn.exe export \"%var3%%var4%@%var5%\" \"%");
			writer.Write(Id);
			writer.WriteLine("__TRG%\"");
			writer.WriteLine("__TRG%\" --non-interactive --quiet");
		}

		/// <summary>
		/// 
		/// </summary>
		public override int SourceEntryVariableCount
		{
			get { return 4; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class SubversionSourceReference : SourceReference
	{
		readonly Uri _reposRoot;
		readonly Uri _itemPath;
		readonly int _commitRev;
		readonly int _wcRev;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resolver"></param>
		/// <param name="sourceFile"></param>
		/// <param name="reposRoot"></param>
		/// <param name="itemPath"></param>
		/// <param name="commitRev"></param>
		/// <param name="wcRev"></param>
		public SubversionSourceReference(SubversionResolver resolver, SourceFile sourceFile, Uri reposRoot, Uri itemPath, int commitRev, int wcRev)
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
				ReposSubDir(_reposRoot)
			};
		}
	}
}
