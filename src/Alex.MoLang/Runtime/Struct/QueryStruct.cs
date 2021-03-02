using System;
using System.Collections.Generic;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public class QueryStruct : IMoStruct
	{
		private Dictionary<string, Func<MoParams, object>> _funcs = new Dictionary<string, Func<MoParams, object>>();

		/// <inheritdoc />
		public object Value => this;

		public QueryStruct()
		{
			
		}

		public QueryStruct(IEnumerable<KeyValuePair<string, Func<MoParams, object>>> parameters)
		{
			_funcs = new Dictionary<string, Func<MoParams, object>>(parameters);
		}

		/// <inheritdoc />
		public void Set(string key, IMoValue value)
		{
			throw new NotSupportedException("Cannot set a value in a query struct.");
		}

		/// <inheritdoc />
		public IMoValue Get(string key, MoParams parameters)
		{
			try
			{
				if (_funcs.TryGetValue(key, out var func))
				{
					return MoValue.FromObject(func(parameters));
				}

				return DoubleValue.Zero;
			}
			catch (Exception exception)
			{
				throw new MoLangRuntimeException($"Query request failed for requested key \'{key}\'", exception);
			}
		}

		/// <inheritdoc />
		public void Clear()
		{
			_funcs.Clear();
		}
	}
}