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
	public class SubversionProvider : SourceProvider, ISourceProviderDetector
	{
		string _svnExePath;
		/// <summary>
		/// 
		/// </summary>
		public SubversionProvider(IndexerState state)
			: base(state)
		{
			//GC.KeepAlive(null);
		}

		/// <summary>
		/// Returns "Subversion"
		/// </summary>
		public override string Name
		{
			get { return "Subversion"; }
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
					return true;
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
			cb.AppendSwitch("--verbose");
			cb.AppendSwitch("--non-recursive");
			cb.AppendSwitch("--xml");

			cb.AppendSwitch("status");

			foreach (SourceFile file in State.SourceFiles.Values)
			{
				if (file.IsResolved)
					continue;

				cb.AppendFileNameIfNotNull(file.FullName);				
			}

			psi.Arguments = cb.ToString();

			using (Process p = Process.Start(psi))
			{
				string output = p.StandardOutput.ReadToEnd();

				if (output.Length > 0)
				{
					XPathDocument doc = new XPathDocument(new StringReader(output));

					XPathNavigator nav = doc.CreateNavigator();

					foreach (XPathNavigator i in nav.Select("/status/target/entry[@path and wc-status]"))
					{
						SourceFile file;

						string path = State.NormalizePath(i.GetAttribute("path", ""));

						if(!State.SourceFiles.TryGetValue(path, out file))
							continue;

						XPathNavigator wcStatus = i.SelectSingleNode("wc-status");
						XPathNavigator commit = (wcStatus != null) ? wcStatus.SelectSingleNode("commit") : null;

						GC.KeepAlive(commit);
					}					
				}				

				p.WaitForExit();
			}
			
			return true;
		}
	}
}
