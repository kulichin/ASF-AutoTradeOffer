# ASF-AutoTradeOffer
The purpose of that plugin is to send automatically trade offers. The plugin can:
- immediately send trade offers right after connecting the bot.
- periodically send trading offers after authorization.

## Installation
1. Download the compiled plugin from **[latest releases](https://github.com/kulichin/ASF-AutoTradeOffer/releases).**
2. Unpack the .zip into **'plugins'** directory inside your ASF folder.

## Plugin configuration
Edit the **ASF.json** config and paste the following lines:
```
"AutoTradeOffer": {
    "SteamAppID": 730,
    "SteamCommunityContextID": 2,
    "TradeOfferAfterBotConnection": true,
    "WaitForOwnerAccount": true,
    "PeriodicallySendTradeOffers": true,
    "PeriodicallySendTradeOffersTimer": 7200
}
```

- **SteamAppID** - id of the game whose inventory we want to send.
- **SteamCommunityContextID** - context id.
- **TradeOfferAfterBotConnection** - send a trade offer immediately after connection the bot.
- **WaitForOwnerAccount** - 
- **PeriodicallySendTradeOffers** - periodically send trade offers.
- **PeriodicallySendTradeOffersTimer** - periodically trade offers delay (in seconds).

