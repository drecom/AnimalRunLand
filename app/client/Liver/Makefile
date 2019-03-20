UnityCommand := /Applications/Unity/Hub/Editor/2017.4.5f1/Unity.app/Contents/MacOS/Unity
LogFile := "logFile_`date +%y%m%d_%H%M`.txt"

prebuild_android:
	$(UnityCommand) \
	  -batchmode \
	  -nographics \
	  -quit \
	  -projectPath $(PWD) \
	  -executeMethod Editor.LiverBuildScripts.PreBuildSettingsAndroid \
	  -logFile /dev/stdout \
	  -bundleVersion ${BUNDLE_VERSION} \
	  -bundleVersionCode ${BUNDLE_VERSION_CODE} \
	  -keystoreName ${KEYSTORE_NAME} -keystorePass ${KEYSTORE_PASS} -keyaliasName ${KEYALIAS_NAME} -keyaliasPass ${KEYALIAS_PASS} \

prebuild_ios:
	$(UnityCommand) \
	  -batchmode \
	  -nographics \
	  -quit \
	  -projectPath $(PWD) \
	  -executeMethod Editor.LiverBuildScripts.PreBuildSettingsIos \
	  -logFile /dev/stdout \
	  -bundleVersion ${BUNDLE_VERSION} \
	  -bundleVersionCode ${BUNDLE_VERSION_CODE} \
	  -teamId ${TEAM_ID}

build_android: prebuild_android
	# BUNDLE_VERSION='' BUNDLE_VERSION_CODE='' KEYSTORE_NAME='' KEYSTORE_PASS='' KEYALIAS_NAME='' KEYALIAS_PASS='' make build_android
	$(UnityCommand) \
	  -batchmode \
	  -nographics \
	  -quit \
	  -projectPath $(PWD) \
	  -executeMethod Editor.LiverBuildScripts.ReleaseBuildAndroid \
	  -logFile $(LogFile) \
	  -bundleVersion ${BUNDLE_VERSION} \
	  -bundleVersionCode ${BUNDLE_VERSION_CODE} \
	  -keystoreName ${KEYSTORE_NAME} -keystorePass ${KEYSTORE_PASS} -keyaliasName ${KEYALIAS_NAME} -keyaliasPass ${KEYALIAS_PASS} \

build_gradle: build_android
	cd 'build/android/アニマルラン'; gradle wrapper; ./gradlew assembleRelease

build_ios: prebuild_ios
	# BUNDLE_VERSION='' BUNDLE_VERSION_CODE='' TEAM_ID='' make build_ios
	$(UnityCommand) \
	  -batchmode \
	  -nographics \
	  -quit \
	  -projectPath $(PWD) \
	  -executeMethod Editor.LiverBuildScripts.ReleaseBuildIos \
	  -logFile $(LogFile) \
	  -bundleVersion ${BUNDLE_VERSION} \
	  -bundleVersionCode ${BUNDLE_VERSION_CODE} \
	  -teamId ${TEAM_ID}

clean:
	if test -e build; then rm -rf build; fi
