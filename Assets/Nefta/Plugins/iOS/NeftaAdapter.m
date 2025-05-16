#import <Foundation/Foundation.h>
#import <WebKit/WebKit.h>
#import <ISNeftaCustomAdapter.h>
#import <NeftaSDK/NeftaSDK-Swift.h>

#ifdef __cplusplus
extern "C" {
#endif
    typedef void (*OnBehaviourInsight)(int requestId, const char *behaviourInsight);

    void EnableLogging(bool enable);
    void NeftaPlugin_Init(const char *appId, bool sendImpressions, OnBehaviourInsight onBehaviourInsight);
    void NeftaPlugin_Record(int type, int category, int subCategory, const char *name, long value, const char *customPayload);
    void NeftaPlugin_OnExternalMediationRequest(int adType, const char *recommendedAdUnitId, double requestedFloorPrice, double calculatedFloorPrice, const char *adUnitId, double revenue, const char *precision, int status);
    const char * NeftaPlugin_GetNuid(bool present);
    void NeftaPlugin_SetContentRating(const char *rating);
    void NeftaPlugin_GetBehaviourInsight(int requestId, const char *insights);
    void NeftaPlugin_SetOverride(const char *root);
#ifdef __cplusplus
}
#endif

NeftaPlugin *_plugin;

void NeftaPlugin_EnableLogging(bool enable) {
    [NeftaPlugin EnableLogging: enable];
}

void NeftaPlugin_Init(const char *appId, bool sendImpressions, OnBehaviourInsight onBehaviourInsight) {
    _plugin = [ISNeftaCustomAdapter initWithAppId: [NSString stringWithUTF8String: appId] sendImpressions: sendImpressions];
    _plugin.OnBehaviourInsightAsString = ^void(NSInteger requestId, NSString * _Nonnull behaviourInsight) {
        const char *cBI = [behaviourInsight UTF8String];
        onBehaviourInsight((int)requestId, cBI);
    };
}

void NeftaPlugin_Record(int type, int category, int subCategory, const char *name, long value, const char *customPayload) {
    NSString *n = name ? [NSString stringWithUTF8String: name] : nil;
    NSString *cp = customPayload ? [NSString stringWithUTF8String: customPayload] : nil;
    [_plugin RecordWithType: type category: category subCategory: subCategory name: n value: value customPayload: cp];
}

const char * NeftaPlugin_GetNuid(bool present) {
    const char *string = [[_plugin GetNuidWithPresent: present] UTF8String];
    char *returnString = (char *)malloc(strlen(string) + 1);
    strcpy(returnString, string);
    return returnString;
}

void NeftaPlugin_SetContentRating(const char *rating) {
    [_plugin SetContentRatingWithRating: [NSString stringWithUTF8String: rating]];
}

void NeftaPlugin_GetBehaviourInsight(int requestId, const char *insights) {
    [_plugin GetBehaviourInsightBridge: requestId string: [NSString stringWithUTF8String: insights]];
}

void NeftaPlugin_OnExternalMediationRequest(int adType, const char *recommendedAdUnitId, double requestedFloorPrice, double calculatedFloorPrice, const char *adUnitId, double revenue, const char *precision, int status) {
    NSString *r = recommendedAdUnitId ? [NSString stringWithUTF8String: recommendedAdUnitId] : nil;
    NSString *a = adUnitId ? [NSString stringWithUTF8String: adUnitId] : nil;
    NSString *p = precision ? [NSString stringWithUTF8String: precision] : nil;
    [NeftaPlugin OnExternalMediationRequest: @"ironsource-levelplay" adType: adType recommendedAdUnitId: r requestedFloorPrice: requestedFloorPrice calculatedFloorPrice: calculatedFloorPrice adUnitId: a revenue: revenue precision: p status: status];
}

void NeftaPlugin_SetOverride(const char *root) {
    [NeftaPlugin SetOverrideWithUrl: [NSString stringWithUTF8String: root]];
}
