using System;
using JetBrains.Annotations;
using SQLite4Unity3d;

namespace CoreUtils.Editor.AssetUsages {
	public class UsageEntry {
		[PrimaryKey, UsedImplicitly]
		public string ComboGuid { get; set; }

		[UsedImplicitly]
		public Guid UserGuid { get; set; }

		[UsedImplicitly]
		public Guid ResourceGuid { get; set; }

		public UsageEntry() { }

		public UsageEntry(Guid user, Guid resource) {
			ComboGuid = user.ToString() + resource;
			UserGuid = user;
			ResourceGuid = resource;
		}
	}
}