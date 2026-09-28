#import <GameKit/GameKit.h>
#import <UIKit/UIKit.h>
#include <stdlib.h>
#include <string.h>
extern "C" UIViewController *UnityGetGLViewController(void);
extern "C" void ShakoGCAuthenticate() {
 dispatch_async(dispatch_get_main_queue(), ^{
  GKLocalPlayer.localPlayer.authenticateHandler = ^(UIViewController *controller, NSError *error) {
   if (controller) {
    UIViewController *presenter=UnityGetGLViewController();
    while(presenter.presentedViewController) presenter=presenter.presentedViewController;
    [presenter presentViewController:controller animated:YES completion:nil];
   }
  };
 });
}
extern "C" char *ShakoGCDisplayName() {
 GKLocalPlayer *player=GKLocalPlayer.localPlayer;
 return strdup(player.isAuthenticated ? (player.displayName.UTF8String ?: "") : "");
}
extern "C" void ShakoGCFree(void *value) { free(value); }
