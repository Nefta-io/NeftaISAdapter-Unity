#import <Foundation/Foundation.h>
#import <WebKit/WebKit.h>
#import <NeftaSDK/NeftaSDK-Swift.h>

#ifdef __cplusplus
extern "C" {
#endif
    void EnableLogging(bool enable);
    void * NeftaPlugin_Init(const char *appId);
    void NeftaPlugin_Record(void *instance, const char *event);
    const char * NeftaPlugin_ShowNuid(void *instance);
#ifdef __cplusplus
}
#endif

NeftaPlugin_iOS *_plugin;

void NeftaPlugin_EnableLogging(bool enable) {
    [NeftaPlugin_iOS EnableLogging: enable];
}

void * NeftaPlugin_Init(const char *appId) {
    _plugin = [NeftaPlugin_iOS InitWithAppId: [NSString stringWithUTF8String: appId]];
    return (__bridge_retained void *)_plugin;
}

void NeftaPlugin_Record(void *instance, const char *event) {
    [_plugin RecordWithEvent: [NSString stringWithUTF8String: event]];
}

const char * NeftaPlugin_ShowNuid(void *instance) {
    const char *string = [[_plugin ShowNuid] UTF8String];
    char *returnString = (char *)malloc(strlen(string) + 1);
    strcpy(returnString, string);
    return returnString;
}