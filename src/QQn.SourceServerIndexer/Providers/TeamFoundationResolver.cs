// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;
using System.IO;

namespace QQn.SourceServerIndexer.Providers
{
	/// <summary>
	/// 
	/// </summary>
	public class TeamFoundationResolver : SourceResolver
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public TeamFoundationResolver(IndexerState state)
			: base(state, "TF")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool Available
		{
			get { return TFExePath != null; }
		}

		bool _searchedPath;
		string _tfExePath;
		public string TFExePath
		{
			get
			{
				if (_searchedPath || !string.IsNullOrEmpty(_tfExePath))
					return _tfExePath;

				string path = Environment.GetEnvironmentVariable("PATH");
				if (path == null)
					path = "";
				else
					path = path.ToUpperInvariant();

				string[] pathItems = path.Split(Path.PathSeparator);

				// First try to find via some smart way (registry, path with the right name) (probably ok for 99% of the cases)
				//foreach (string item in pathItems)
				//{
				//    if (item.Contains("TEAM"))
				//    {
				//        string file = Path.GetFullPath(Path.Combine(item.Trim(), "TF.EXE"));

				//        if (File.Exists(file))
				//            return _svnExePath = file;
				//    }
				//}

				// Search whole path
				foreach (string item in pathItems)
				{
					string file = Path.GetFullPath(Path.Combine(item.Trim(), "TF.EXE"));

					if (File.Exists(file))
						return _tfExePath = file;
				}

				_searchedPath = true;
				return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool ResolveFiles()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void WriteEnvironment(System.IO.StreamWriter writer)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}
	}
}
