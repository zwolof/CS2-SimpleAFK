using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AFKManager;

public static class Helpers
{
	public static bool IsPlayerValid(CCSPlayerController player)
	{
		return player is
		{
			IsValid: true, 
			PlayerPawn.IsValid: true, 
			IsBot: false,
			IsHLTV: false
		};
	}
	
	public static void ChatMessageAll(string message)
	{
		Server.PrintToChatAll(Config.ChatPrefix + message);
	}

	public static void ChatMessage(CCSPlayerController player, string message)
	{
		player.PrintToChat(Config.ChatPrefix + message);
	}

	public static bool IsWarmup()
	{
		var gameRules = GetGameRules();

		if (gameRules == null)
		{
			return false;
		}

		return gameRules.WarmupPeriod;
	}

	public static bool IsPlayerOnGround(CCSPlayerController player)
	{
		if (player.PlayerPawn.Value == null) 
		{
			return false;
		}
		
		var flags = (PlayerFlags)player.PlayerPawn.Value.Flags;

		return flags.HasFlag(PlayerFlags.FL_ONGROUND);
	}
	
	// Credits: killstr3ak
	private static CCSGameRules? GetGameRules()
	{
		return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
	}
}
