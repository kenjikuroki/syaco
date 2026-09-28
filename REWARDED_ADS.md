# Rewarded ads integration

## Implemented
- Google Mobile Ads Unity plugin 11.5.0 via the official documented OpenUPM registry; EDM4U dependency pinned by the package lock.
- iOS AdMob app ID: ca-app-pub-3331079517737737~9150305046
- iOS production rewarded unit: ca-app-pub-3331079517737737/2487770330
- iOS rewarded TEST unit: ca-app-pub-3940256099942544/1712485313
- Android remains TEST ONLY because no Android production IDs were provided. Test rewarded unit: ca-app-pub-3940256099942544/5224354917.
- Only opt-in RewardedAd is used. No banner, interstitial, rewarded interstitial, or app-open ads.
- Shop > Shells and completed-match results share 30 shells per earned-reward callback, twice per UTC day. No cooldown. The UI states the amount and remaining attempts; reset is 00:00 UTC (09:00 JST).
- Cancellation/load failure/show failure does not decrement quota. Duplicate receipts are rejected; quota and currency are saved in the same MissionProfile record. Time/audio restore after the ad closes.
- Under the current offline prototype, balance/quota are local PlayerPrefs, not server-authoritative. Use backend SSV and server time when the online economy is implemented.

## Testing / hidden command
- Editor and Development Build always use test mode. On Windows/Editor, AdMob device delivery is unavailable; a visibly labeled simulation allows completion after 3 seconds or immediate cancellation. This is not a Google-served ad.
- On iOS, development builds and persistent test mode use the official Google rewarded TEST unit, never the production rewarded ID.
- Home > Settings: tap the SETTINGS heading 50 times. Each tap must be within 8 seconds of the last. The heading area covers the title. A TEST ADS · SAVED indicator confirms activation.
- Test mode is one-way in the app, persists across restart/update on that installation, and is reset by app-data deletion/uninstall. It is not an account-wide or reinstall-proof setting.
- Activate before opening the ad entry point on a release build. A mode switch destroys a cached ad and rejects stale load completions before requesting the test ID.
- QA tests use a separate preference key ending .QA and isolated MissionStore so tests do not alter player progress.
- Include the hidden test-mode steps in App Review notes; this is a tester configuration, not reviewer-detection behavior.

## Provisional audience / privacy
The target age is undecided. Requests currently use AgeRestrictedTreatment.Child and maximum ad content rating G, set before SDK initialization. UMP is updated before initializing Mobile Ads; requests proceed only when consent permits. UMP privacy-options entry is shown in Settings when required. No ATT permission prompt or custom IDFA collection is added.
This provisional setting does not constitute completed age-rating, Kids Category, consent-message, privacy-policy or App Store disclosure setup. Decide the public audience before release and review the AdMob console/UMP configuration and App Store privacy information accordingly.

## Native iOS verification still required
This Windows environment has no iOS build module, Xcode, CocoaPods or connected iPhone. Native linking, device delivery and consent forms cannot be verified here.
On a Mac, resolve the SDK's iOS CocoaPods (Google-Mobile-Ads-SDK ~>13.9 plus UMP) and build with the SDK-supported Xcode/iOS versions. The official plugin adds GADApplicationIdentifier and SKAdNetwork items from GoogleMobileAdsSettings.asset. The RewardAdsBuild preprocessor sets the supplied iOS app ID and test Android app ID.
Verify on iPhone: development test creative, cancel and completion, shared two/day limit, background/foreground, no-fill retry, settings privacy entry if required, release build switched with 50 taps, restart remains TEST. Confirm the supplied unit is an iOS rewarded unit for this application in AdMob and align its reward display with 30 shells. Do not click live ads during testing.

## Official references
- https://developers.google.com/admob/unity/quick-start
- https://developers.google.com/admob/unity/test-ads (format-specific table is used for rewarded IDs)
- https://developers.google.com/admob/unity/rewarded
- https://developers.google.com/admob/unity/privacy
- https://developers.google.com/admob/unity/targeting
