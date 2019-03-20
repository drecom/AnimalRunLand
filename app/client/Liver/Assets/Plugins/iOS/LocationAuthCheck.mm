#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>

#ifdef __cplusplus
extern "C" {
#endif

const char* mallocString(const char* str)
{
    char* ret = (char*)malloc(strlen(str) + 1);
    strcpy(ret, str);
    ret[strlen(str)] = '\0';
    return ret;
}

const char* LocationAuthorizationStatusCheck()
{
    CLAuthorizationStatus status = [CLLocationManager authorizationStatus];
    
    switch (status) {
        case kCLAuthorizationStatusNotDetermined:
            return mallocString("kCLAuthorizationStatusNotDetermined");
            break;
        case kCLAuthorizationStatusRestricted:
            return mallocString("kCLAuthorizationStatusRestricted");
            break;
        case kCLAuthorizationStatusDenied:
            return mallocString("kCLAuthorizationStatusDenied");
            break;
        case kCLAuthorizationStatusAuthorizedAlways:
            return mallocString("kCLAuthorizationStatusAuthorizedAlways");
            break;
        case kCLAuthorizationStatusAuthorizedWhenInUse:
            return mallocString("kCLAuthorizationStatusAuthorizedWhenInUse");
            break;
    }
}

#ifdef __cplusplus
}
#endif
