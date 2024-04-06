using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using NPC = Terraria.NPC;
namespace ExplosiveIdea.Plr
{
    public class ModifiedPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            var player = Main.LocalPlayer;

            if (player == null) return;
            
            // Drowning doesnt trigger OnHurt so we need to manually trigger it here
            if (player.breath <= 0)
            {
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " drowned!"), 0, 0);
            }
        }
        
        public override void OnHurt(Player.HurtInfo info)
        {
            // prevent chain reaction
            if (info.DamageSource.SourceCustomReason == "get rekt lol")
            {
                return;
            }
            
            var explosionPosition = Player.Center;
            int explosionRadius = ModContent.GetInstance<ExplosiveIdeaConfig>().ExplosionRadius * 16;
            bool instantKill = ModContent.GetInstance<ExplosiveIdeaConfig>().InstantKill;
            int explosionDamage = ModContent.GetInstance<ExplosiveIdeaConfig>().ExplosionDamage;
            
            
            // Handle damage to local player
            Player.statLife = instantKill ? -1 : Player.statLife - explosionDamage;
            
            // Handle damage to NPCs
            foreach (var npc in Main.npc)
            {
                if (!npc.active || !(Vector2.Distance(npc.Center, explosionPosition) <= explosionRadius)) continue;

                var newHitInfo = new NPC.HitInfo
                {
                    Damage = instantKill ? npc.lifeMax : explosionDamage
                };
                
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NetMessage.SendStrikeNPC(npc, newHitInfo);
                }
                npc.StrikeNPC(newHitInfo);
            }
            
            // Handle damage to tiles
            for (var x = (int)(explosionPosition.X / 16) - (int)(explosionRadius / 16);
                 x <= (int)(explosionPosition.X / 16) + (int)(explosionRadius / 16);
                 x++)
            {
                for (var y = (int)(explosionPosition.Y / 16) - (int)(explosionRadius / 16);
                     y <= (int)(explosionPosition.Y / 16) + (int)(explosionRadius / 16);
                     y++)
                {
                    if (Vector2.Distance(new Vector2(x * 16, y * 16), explosionPosition) <= explosionRadius)
                    {
                        Tile tile = Framing.GetTileSafely(x, y);
                        
                        // Check if the tile is a chest and skip destroying it if true
                        if (tile.TileType == TileID.Containers)
                        {
                            continue;
                        }
                        
                        WorldGen.KillTile(x, y, false, false, true); // Destroy foreground tiles
                        WorldGen.KillWall(x, y); // Destroy background tiles
                        
                        if (Main.netMode != NetmodeID.SinglePlayer)
                        {
                            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y, tile.TileType, 0, 0,
                                0); // KillTile
                            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 2, x, y, tile.TileType, 0, 0,
                                0); // KillWall
                        }
                    }
                }
            }
            
            // Handle damage to other players
            foreach (var player in Main.player)
            {
                if (!player.active || !(Vector2.Distance(player.Center, explosionPosition) <= explosionRadius)) continue;
                player.statLife = instantKill ? -1 : player.statLife - explosionDamage;
                
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    var hurtInfo = new Player.HurtInfo
                    {
                        Damage = instantKill ? player.statLifeMax2 : explosionDamage,
                        DamageSource = new PlayerDeathReason
                        {
                            SourceCustomReason = "get rekt lol"
                        }
                    };

                    NetMessage.SendPlayerHurt(player.whoAmI, hurtInfo);
                }
            }
        }
    }
}