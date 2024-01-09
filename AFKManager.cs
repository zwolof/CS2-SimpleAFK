using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

using CSSTimer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace AFKManager;

public sealed class Config 
{
	public const string CHAT_PREFIX = " \u000fAFKManager \u0010›\b ";
}

public sealed class AFKData
{
	public bool IsAFK { get; set; }
	public uint Time { get; set; }
	public CSSTimer? Timer { get; set; }
	public int Limit { get; set; }
}

[MinimumApiVersion(110)]
public partial class AFKManager : BasePlugin
{
	public override string ModuleName => "AFKManager";
	public override string ModuleAuthor => "zwolof";
	public override string ModuleDescription => "Manages players who are AFK.";
	public override string ModuleVersion => "1.0.0";

	#region local variables
	private Dictionary<CCSPlayerController, AFKData> _AFKPlayers { get; set; } = new();
	private const uint AFK_TIME = 30;
	#endregion

	public override void Load(bool hotReload)
	{	
		// Events
		RegisterEventHandler<EventRoundFreezeEnd>(OnFreezeTimeEnd);

		// Listeners
		RegisterListener<Listeners.OnTick>(OnTick);
		RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
		RegisterListener<Listeners.OnClientDisconnectPost>(OnClientDisconnectPost);
	}

	public void OnClientPutInServer(int playerSlot)
	{
		var player = Utilities.GetPlayerFromSlot(playerSlot);

		if (player == null)
		{
			return;
		}

		AddClientEntry(player);
	}

	public void OnClientDisconnectPost(int playerSlot)
	{
		var player = Utilities.GetPlayerFromSlot(playerSlot);

		if(player == null)
		{
			return;
		}

		if(!_AFKPlayers.ContainsKey(player))
		{
			return;
		}
		// kill timer if exists so we don't have a memory leak
		_AFKPlayers[player].Timer?.Kill();

		_AFKPlayers.Remove(player);
	}

	public bool AddClientEntry(CCSPlayerController player)
	{
		if (_AFKPlayers.ContainsKey(player))
		{
			return false;
		}

		_AFKPlayers.Add(
			player,
			new AFKData {
				IsAFK = false,
				Time = AFK_TIME,
				Limit = 0,
				Timer = new CSSTimer(1.0f, () => 
				{
					HandleAFK(player);
				})
			}
		);

		return true;
	}

	public HookResult OnFreezeTimeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
	{
		var players = Utilities.GetPlayers();

		if(!players.Any())
		{
			return HookResult.Continue;
		}

		foreach(var player in players)
		{
			if(!_AFKPlayers.ContainsKey(player))
			{
				AddClientEntry(player);
			}
			_AFKPlayers[player].Time = AFK_TIME;
			_AFKPlayers[player].IsAFK = true;
		}
		return HookResult.Continue;
	}

	public void OnTick()
	{
		var players = Utilities.GetPlayers();

		if(!players.Any())
		{
			return;
		}

		foreach(var player in players)
		{
			if(player.PlayerPawn.Value!.MovementServices is null)
			{
				continue;
			}

			if(!_AFKPlayers.ContainsKey(player))
			{
				AddClientEntry(player);
			}

			if(_AFKPlayers[player].IsAFK && player.PawnIsAlive && !player.IsBot)
			{
				foreach(var btnState in player.PlayerPawn.Value.MovementServices.Buttons.ButtonStates)
				{
					if(btnState > 0)
					{
						_AFKPlayers[player].Time = 0;
						_AFKPlayers[player].IsAFK = false;
					}
				}
			}
				
		}
	}

	public void HandleAFK(CCSPlayerController player)
	{
		if(player == null || Helpers.IsWarmup())
		{
			return;
		}

		if(!_AFKPlayers.ContainsKey(player))
		{
			AddClientEntry(player);
		}

		if(player.PawnIsAlive && !player.IsBot && _AFKPlayers[player].IsAFK)
		{
			if(--_AFKPlayers[player].Time > 0)
			{
				// kill timer?
				return;
			}
			player.CommitSuicide(true, true);
			_AFKPlayers[player].Limit++;

			if(_AFKPlayers[player].Limit >= 3)
			{
				Helpers.ChatMessage(player, "You have been kicked for being AFK too long.");
				// kick player
			}
		}

		_AFKPlayers[player].Time = AFK_TIME;
		_AFKPlayers[player].IsAFK = true;
	}
}