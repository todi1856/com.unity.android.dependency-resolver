//
//  iOSBridge.h
//  iOSBridge
//
//  Created by Supersonic.
//  Copyright (c) 2015 Supersonic. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <IronSource/IronSource.h>
#import "RewardedVideoLevelPlayCallbacksWrapper.h"
#import "InterstitialLevelPlayCallbacksWrapper.h"
#import "BannerLevelPlayCallbacksWrapper.h"

static NSString *  UnityGitHash = @"2ad9faf";
typedef void (*ISUnityBackgroundCallback)(const char* args);
typedef void (*ISUnityPauseGame)(const bool gamePause);

@interface iOSBridge : NSObject<ISRewardedVideoDelegate,
								ISDemandOnlyRewardedVideoDelegate, 
								ISInterstitialDelegate,
								ISDemandOnlyInterstitialDelegate,
								ISOfferwallDelegate,
								ISBannerDelegate,
								ISSegmentDelegate,
								ISImpressionDataDelegate,
								ISConsentViewDelegate,
								ISRewardedVideoManualDelegate,
								ISInitializationDelegate>

@end


