using System;
using System.Collections.Generic;
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
internal sealed class AutoTradeOffer : IPlugin, IASF, IBot, IBotConnection {
	private uint SteamAppID = 753;
	private ulong SteamCommunityContextID = 6;
	private bool TradeOfferAfterBotConnection = false;

	public string Name => nameof(AutoTradeOffer);
	public Version Version => typeof(AutoTradeOffer).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	public Task OnLoaded() => Task.CompletedTask;
	public Task OnBotDestroy(Bot bot) => Task.CompletedTask;
	public Task OnBotInit(Bot bot) => Task.CompletedTask;

	public Task OnASFInit(IReadOnlyDictionary<string, JToken>? additionalConfigProperties = null) {
		// Login master account if exist

		// Loading config
		if (additionalConfigProperties == null)
			return Task.CompletedTask;

		foreach (string? configProperty in additionalConfigProperties.Keys) {
			if (configProperty == "AutoTradeOffer") {
				if (additionalConfigProperties.TryGetValue(configProperty, out JToken? pluginConfig)) {

					TradeOfferAfterBotConnection = pluginConfig.Value<bool?>("TradeOfferAfterBotConnection") ?? false;
					SteamAppID = pluginConfig.Value<uint?>("SteamAppID") ?? 753;
					SteamCommunityContextID = pluginConfig.Value<ulong?>("SteamCommunityContextID") ?? 6;
				}
			}
		}

		return Task.CompletedTask;
	}

	public Task OnBotLoggedOn(Bot bot) {
		if (TradeOfferAfterBotConnection) {
			_ = Task.Run(async () => {
				(bool success, string message) = await bot.Actions.SendInventory(730, 2).ConfigureAwait(false);

				ASF.ArchiLogger.LogGenericInfo($"AutoTradeOffer: {success}, {message}!");
			});
		}

		return Task.CompletedTask;
	}

	public Task OnBotDisconnected(Bot bot, EResult reason) => Task.CompletedTask;
}
#pragma warning restore CA1812 // ASF uses this class during runtime
