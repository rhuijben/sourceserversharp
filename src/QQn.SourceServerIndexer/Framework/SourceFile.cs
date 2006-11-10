// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using QQn.SourceServerIndexer.Providers;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	public class SourceFile : SourceFileBase, IComparable<SourceFile>, IEquatable<SourceFile>
	{
		readonly SortedList<string, SymbolFile> _symbolFiles = new SortedList<string, SymbolFile>(StringComparer.InvariantCultureIgnoreCase);
		SourceReference _sourceReference;
		bool _noSourceAvailable;

		/// <summary>
		/// Creates a new SourceFile object for the specified file
		/// </summary>
		/// <param name="filename"></param>
		public SourceFile(string filename)
			: base(filename)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public IDictionary<string,SymbolFile> Containers
		{
			get { return _symbolFiles; }
		}

		/// <summary>
		/// 
		/// </summary>
		public SourceReference SourceReference
		{
			get { return _sourceReference; }
			set { _sourceReference = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsResolved
		{
			get { return (SourceReference != null) || NoSourceAvailable; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool NoSourceAvailable
		{
			get { return _noSourceAvailable; }
			set { _noSourceAvailable = value; }
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="symbolFile"></param>
		internal void AddContainer(SymbolFile symbolFile)
		{
			if (symbolFile == null)
				throw new ArgumentNullException("symbolFile");

			if (!_symbolFiles.ContainsKey(symbolFile.FullName))
				_symbolFiles.Add(symbolFile.FullName, symbolFile);
		}

		#region ## IComparable<SourceFile>, IEquatable<SourceFile>
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(SourceFile other)
		{
			return base.CompareTo(other);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(SourceFile other)
		{
			return base.Equals(other);
		}
		#endregion ## IComparable<SourceFile>, IEquatable<SourceFile>
	}
}
