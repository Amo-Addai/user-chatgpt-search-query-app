# user-gpt-search-query-app - front-end - flutter client - web, mobile (android & ios)

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes. 
See deployment for notes on how to deploy the project on a live system.

## Getting Started

This project is a starting point for a Flutter application.

A few resources to get you started if this is your first Flutter project:

- [Lab: Write your first Flutter app](https://docs.flutter.dev/get-started/codelab)
- [Cookbook: Useful Flutter samples](https://docs.flutter.dev/cookbook)

For help getting started with Flutter development, view the
[online documentation](https://docs.flutter.dev/), which offers tutorials,
samples, guidance on mobile development, and a full API reference.

Technologies Required:

- Chome WebDriver (comes in-built with Flutter, but if not with you device, you can download it online)
- Android Studio - with Android SDK and Device Manager emulators
- XCode 15 - with iOS Simulator
- Flutter - download it from the official [Flutter website](https://docs.flutter.dev/get-started/install)

<br />

# Installation Steps:

After cloning the mono-repo, and going into the base directory.

Install Flutter

```
Mac OS - brew install --cask flutter
Windows - 
```

Verify installation

```
flutter doctor
```

Go into the front-end directory

```
cd client/
```

Clean the project first, before (re-)building

```
flutter clean
```

Upgrade Flutter package-manager, pub

```
flutter pub upgrade --major-versions
```

Install all dependencies

```
flutter pub get
```

Test the project (Test scripts in-progress)

```
flutter test
```

Run the project in the Chromium browser

```
flutter run -d chrome
```

### Alternatives

- Run Android Studio and an Android Emulator
- Run XCode 15 and iOS Simulator

List all available devices on your machine, and record their IDs

```
flutter devices
```

Build the project for Android

```
flutter build apk
```

Build the project for iOS

```
flutter build ios
```

Run the project on the device, using its Device ID

```
flutter run -d <device_id>
```

Example:

```
flutter run -d emulator-5554 - example android emulator device id
```

