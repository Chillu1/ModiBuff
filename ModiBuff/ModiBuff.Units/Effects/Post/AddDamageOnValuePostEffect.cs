namespace ModiBuff.Core.Units
{
	//Might be a few issues with saving/loading damage, or reverting it if wanted
	public sealed class AddDamageOnValuePostEffect : IPostEffect<float>
	{
		private readonly Targeting _targeting;

		public AddDamageOnValuePostEffect(Targeting targeting) => _targeting = targeting;

		public void Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			if (!(target is IAddDamage<float> addDamageTarget))
				return;

			addDamageTarget.AddDamage(value);
		}
	}
}