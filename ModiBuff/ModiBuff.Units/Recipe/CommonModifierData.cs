namespace ModiBuff.Core.Units
{
	public sealed record CommonModifierData
	{
		public RemoveApplierId? ApplierRemoverId { get; init; }
	}

	public readonly struct RemoveApplierId
	{
		public int Id { get; init; }

		public RemoveApplierId(int id) => Id = id;
	}


	public readonly struct RemoveApplierGenId
	{
		public int GenId { get; init; }

		public RemoveApplierGenId(int genId) => GenId = genId;
	}
}