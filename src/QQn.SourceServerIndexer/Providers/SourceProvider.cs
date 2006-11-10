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
	public abstract class SourceProvider
	{
		readonly IndexerState _state;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		protected SourceProvider(IndexerState state)
		{
			if(state == null)
				throw new ArgumentNullException("state");

			_state = state;
		}

		/// <summary>
		/// 
		/// </summary>
		public abstract string Name
		{
			get;
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
		protected IndexerState State
		{
			get { return _state; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public abstract bool ResolveFiles();
	}
}
