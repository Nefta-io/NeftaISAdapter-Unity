//
//  ISNeftaCustomRewardedVideo.m
//  NeftaISAdapter
//
//  Created by Tomaz Treven on 14/11/2023.
//

#import <Foundation/Foundation.h>

#import "ISNeftaCustomRewardedVideo.h"

@implementation ISNeftaCustomRewardedVideo

- (void)loadAdWithAdData:(nonnull ISAdData *)adData delegate:(nonnull id<ISRewardedVideoAdDelegate>)delegate {
    [self trySetAdapter];
    
    NSString *placementId = [adData getString: @"placementId"];
    if (placementId != nil && placementId.length > 0) {
        [_adapter Load: placementId delgate: delegate];
    }
}

- (BOOL)isAdAvailableWithAdData:(nonnull ISAdData *)adData {
    [self trySetAdapter];
    return [_adapter IsReady: [adData getString: @"placementId"]] == NeftaPlugin.PlacementReady;
}

- (void)showAdWithViewController:(nonnull UIViewController *)viewController adData:(nonnull ISAdData *)adData delegate:(nonnull id<ISRewardedVideoAdDelegate>)delegate {
    [self trySetAdapter];
    [ISNeftaCustomAdapter ApplyRenderer: viewController];
    
    NSString *placementId = [adData getString: @"placementId"];
    if (placementId != nil && placementId.length > 0) {
        [_adapter Show: placementId];
    }
}

- (void)trySetAdapter {
    if (_adapter == nil) {
        _adapter = (ISNeftaCustomAdapter *)[self getNetworkAdapter];
    }
}

@end
