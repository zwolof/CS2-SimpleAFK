using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AFKManager;

public sealed class Helpers {
	public static bool IsPlayerValid(CCSPlayerController player)
	{
		if (player is null || !player.IsValid || !player.PlayerPawn.IsValid || player.IsBot || player.IsHLTV)
		{
			return false;
		}
		return true;
	}

	public static void ChatMessageAll(string message)
	{
		Server.PrintToChatAll(Config.CHAT_PREFIX + message);
	}

	public static void ChatMessage(CCSPlayerController player, string message)
	{
		player.PrintToChat(Config.CHAT_PREFIX + message);
	}

	// Credits: killstr3ak
	public static CCSGameRules GetGameRules()
	{
		return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
	}

	public static bool IsWarmup()
	{
		var GameRules = GetGameRules();

		if(GameRules is null)
		{
			return false;
		}

		return GameRules.WarmupPeriod;
	}

	public static bool IsPlayerOnGround(CCSPlayerController player)
	{
		if(player.PlayerPawn.Value == null) 
		{
			return false;
		}
		var flags = (PlayerFlags)player.PlayerPawn.Value.Flags;

		return flags.HasFlag(PlayerFlags.FL_ONGROUND);
	}
}