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
	public abstract class SourceProvider
	{
		readonly IndexerState _state;
		readonly string _id;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		/// <param name="name"></param>
		protected SourceProvider(IndexerState state, string name)
		{
			if (state == null)
				throw new ArgumentNullException("state");

			_state = state;
			_id = state.AssignId(this, name);
		}

		/// <summary>
		/// Gets the <see cref="SourceProvider"/> id; used for creating references
		/// </summary>
		public string Id
		{
			get { return _id; }
		}

		/// <summary>
		/// 
		/// </summary>
		protected IndexerState State
		{
			get { return _state; }
		}
	}
}
