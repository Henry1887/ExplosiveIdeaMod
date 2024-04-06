using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ExplosiveIdea
{
	public class ExplosiveIdea : Mod
	{
		public override void Load()
		{
			Instance = this;
		}
		public static ExplosiveIdea Instance { get; private set; }
	}
	
	public class ExplosiveIdeaConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Range(1, 50)]
		public int ExplosionRadius { get; set; } = 10;
		
		public bool InstantKill { get; set; } = false;

		[Range(0, 999)]
		public int ExplosionDamage { get; set; } = 100;
	}
}