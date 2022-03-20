using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SteamKit2;

namespace AutoTradeOffer;

#pragma warning disable CA1812 // ASF uses this class during runtime
[UsedImplicitly]
internal sealed class AutoTradeOffer : IASF, IBotConnection {
	private uint SteamAppID = 753;
	private ulong SteamCommunityContextID = 6;
	private bool TradeOfferAfterBotConnection;
	private bool OwnerAccountLoggedIn;
	private bool WaitForOwnerAccount;
	private bool PeriodicallySendTradeOffers;
	private int PeriodicallySendTradeOffersTimer;

	private int TimerInMS;
	private int CurrentLoggedInBot;
	private IEnumerable<Bot>? Bots;
	private Timer? TimerHandle;

	private readonly List<Bot> WaitingBots = new();

	public string Name => nameof(AutoTradeOffer);
	public Version Version => typeof(AutoTradeOffer).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	public Task OnLoaded() => Task.CompletedTask;
	public Task OnASFInit(IReadOnlyDictionary<string, JToken>? additionalConfigProperties = null) {
		// Loading plugin config
		if (additionalConfigProperties == null)
			return Task.CompletedTask;

		foreach (string? configProperty in additionalConfigProperties.Keys) {
			if (configProperty == "AutoTradeOffer") {
				if (additionalConfigProperties.TryGetValue(configProperty, out JToken? pluginConfig)) {
					SteamAppID = pluginConfig.Value<uint?>("SteamAppID") ?? 753;
					SteamCommunityContextID = pluginConfig.Value<ulong?>("SteamCommunityContextID") ?? 6;
					TradeOfferAfterBotConnection = pluginConfig.Value<bool?>("TradeOfferAfterBotConnection") ?? false;
					WaitForOwnerAccount = pluginConfig.Value<bool?>("WaitForOwnerAccount") ?? false;
					PeriodicallySendTradeOffers = pluginConfig.Value<bool?>("PeriodicallySendTradeOffers") ?? false;
					PeriodicallySendTradeOffersTimer = pluginConfig.Value<int?>("PeriodicallySendTradeOffersTimer") ?? 3600;
					TimerInMS = PeriodicallySendTradeOffersTimer * 1000;
				}
			}
		}

		return Task.CompletedTask;
	}

	public void PeriodicallySendInventory() {
		if (Bot.BotsReadOnly == null)
			return;

		IEnumerable<Bot>? bots = Bot.BotsReadOnly.Values;
		foreach (Bot? bot in bots) {
			_ = SendBotInventory(bot);
		}
	}

	public Task SendBotInventory(Bot bot) => Task.Run(async () => {
		(bool success, string message) = await bot.Actions.SendInventory(SteamAppID, SteamCommunityContextID).ConfigureAwait(false);

		ASF.ArchiLogger.LogGenericInfo($"AutoTradeOffer: Bot: {bot.BotName} Success: {success}; Message: {message}!");
	});

	public Task OnBotLoggedOn(Bot bot) {
		CurrentLoggedInBot++;

		// Initialize array of bots references
		if (Bots == null && Bot.BotsReadOnly != null) {
			Bots = Bot.BotsReadOnly.Values;
		}

		if (WaitForOwnerAccount && ASF.IsOwner(bot.SteamID)) {
			OwnerAccountLoggedIn = true;

			// Sending previous trade offers
			if (TradeOfferAfterBotConnection) {
				foreach (Bot? waitingBot in WaitingBots) {
					_ = SendBotInventory(waitingBot);
				}

				WaitingBots.Clear();
			}
		}

		// Skip trade offer if owner account is not logged in
		if (WaitForOwnerAccount && !OwnerAccountLoggedIn) {
			WaitingBots.Add(bot);
			return Task.CompletedTask;
		}

		if (TradeOfferAfterBotConnection) {
			_ = SendBotInventory(bot);
		}

		// Starting trade offers timer after authorization of all bots
		if (PeriodicallySendTradeOffers && Bots != null && CurrentLoggedInBot == Enumerable.Count(Bots)) {
			TimerHandle = new(x => PeriodicallySendInventory(), null, TimerInMS, TimerInMS);
		}

		return Task.CompletedTask;
	}

	public Task OnBotDisconnected(Bot bot, EResult reason) => Task.CompletedTask;
}
#pragma warning restore CA1812 // ASF uses this class during runtime
