// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerSharp.Framework;
using System.IO;
using Microsoft.Win32;

namespace QQn.SourceServerSharp.Providers
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
		/// <summary>
		/// 
		/// </summary>
		public string TFExePath
		{
			get
			{
				if (_searchedPath || !string.IsNullOrEmpty(_tfExePath))
					return _tfExePath;

				string path = null;
				using(RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\8.0", false))
				{
					if(rk != null)
						path = rk.GetValue("InstallDir") as string;
				}

				if(!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, "tf.exe")))
				{
					return _tfExePath = Path.GetFullPath(path);
				}
				
				_searchedPath = true;
				return _tfExePath = SssUtils.FindExecutable("tf.exe");
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
