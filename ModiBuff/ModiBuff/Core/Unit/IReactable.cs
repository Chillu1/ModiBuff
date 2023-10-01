namespace ModiBuff.Core
{
	public interface IReactable<TReact>
	{
		void RegisterReact(ReactCallback<TReact>[] reactCallbacks);
		void UnRegisterReact(ReactCallback<TReact>[] reactCallbacks);
	}
}