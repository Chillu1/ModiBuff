#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER || NET47_OR_GREATER
#define VALUE_TUPLE
#endif

using System;

namespace ModiBuff.Core
{
	public sealed class ReactCallbackRegisterEffect<TReact> : IRevertEffect, IEffect, IShallowClone<ReactCallbackRegisterEffect<TReact>>
	{
		public bool IsRevertible => true;

		private readonly ReactCallback<TReact>[] _reactCallbacks;

#if VALUE_TUPLE
		public ReactCallbackRegisterEffect(params (TReact reactType, object action)[] reactCallbacks) : this(
			Array.ConvertAll(reactCallbacks, x => new ReactCallback<TReact>(x.reactType, x.action)))
		{
		}
#endif

		public ReactCallbackRegisterEffect(params ReactCallback<TReact>[] reactCallbacks)
		{
			_reactCallbacks = reactCallbacks;
		}

		public void Effect(IUnit target, IUnit source)
		{
			((IReactable<TReact>)target).RegisterReact(_reactCallbacks);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((IReactable<TReact>)target).UnRegisterReact(_reactCallbacks);
		}

		public ReactCallbackRegisterEffect<TReact> ShallowClone() => new ReactCallbackRegisterEffect<TReact>(_reactCallbacks);
		object IShallowClone.ShallowClone() => ShallowClone();
	}

	public readonly struct ReactCallback<TReact>
	{
		public readonly TReact ReactType;
		public readonly object Action;

		public ReactCallback(TReact reactType, object action)
		{
			ReactType = reactType;
			Action = action;
		}
	}
}