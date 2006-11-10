// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class SourceIndexToolException : SourceIndexException
	{
		/// <summary>
		/// 
		/// </summary>
		public SourceIndexToolException()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="toolname"></param>
		/// <param name="message"></param>
		public SourceIndexToolException(string toolname, string message)
			: base(string.Format("{0} failed: {1}", toolname, message))
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="toolname"></param>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public SourceIndexToolException(string toolname, string message, Exception inner)
			: base(string.Format("{0} failed: {1}", toolname, message), inner)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected SourceIndexToolException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
