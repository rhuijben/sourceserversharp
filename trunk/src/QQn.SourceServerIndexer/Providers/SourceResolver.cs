// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;

namespace QQn.SourceServerIndexer.Providers
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISourceProviderDetector
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		bool CanProvideSources(IndexerState state);
	}
	
	/// <summary>
	/// Base class of sourceproviders
	/// </summary>
	public abstract class SourceResolver : SourceProvider
	{
		readonly string _name;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		/// <param name="name"></param>
		protected SourceResolver(IndexerState state, string name)
			: base(state, name)
		{
			_name = name;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// 
		/// </summary>
		public abstract bool Available
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public abstract bool ResolveFiles();
	}
}
