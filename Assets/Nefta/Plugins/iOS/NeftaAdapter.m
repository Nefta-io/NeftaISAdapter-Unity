#import <Foundation/Foundation.h>
#import <WebKit/WebKit.h>
#import <ISNeftaCustomAdapter.h>
#import <NeftaSDK/NeftaSDK-Swift.h>

#ifdef __cplusplus
extern "C" {
#endif
    typedef void (*OnBehaviourInsight)(const char *behaviourInsight);

    void EnableLogging(bool enable);
    void NeftaPlugin_Init(const char *appId, bool sendImpressions, OnBehaviourInsight onBehaviourInsight);
    void NeftaPlugin_Record(int type, int category, int subCategory, const char *name, long value, const char *customPayload);
    void NeftaPlugin_OnExternalAdLoad(int adType, double unitFloorPrice, double calculatedFloorPrice, int status);
    const char * NeftaPlugin_GetNuid(bool present);
    void NeftaPlugin_GetBehaviourInsight(const char *insights);
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
    _plugin.OnBehaviourInsightAsString = ^void(NSString * _Nonnull behaviourInsight) {
        const char *cBI = [behaviourInsight UTF8String];
        onBehaviourInsight(cBI);
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

void NeftaPlugin_GetBehaviourInsight(const char *insights) {
    [_plugin GetBehaviourInsightWithString: [NSString stringWithUTF8String: insights]];
}

void NeftaPlugin_OnExternalAdLoad(int adType, double unitFloorPrice, double calculatedFloorPrice, int status) {
    [NeftaPlugin OnExternalAdLoad: @"is" adType: adType unitFloorPrice: unitFloorPrice calculatedFloorPrice: calculatedFloorPrice status: status];
}

void NeftaPlugin_SetOverride(const char *root) {
    [_plugin SetOverrideWithUrl: [NSString stringWithUTF8String: root]];
}
