# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

* A `Name` property to the `GmicBitmap` class.
  * This replaces the `AddInputImage(TGmicBitmap, string)` method in the `Gmic<TGmicBitmap>` class.

### Changed

* The `Gmic<TGmicBitmap>` class no longer implements `IDisposable` **(breaking change)**.
* The `Gmic<TGmicBitmap>` class no longer implements  the [Event-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-overview) **(breaking change)**.
  * The Event-based Asynchronous Pattern was replaced by the Task-based Asynchronous Pattern in .NET 4.0.
* Renamed the `RunGmicTaskAsync` methods to `RunGmicAsync` in the `Gmic<TGmicBitmap>` class **(breaking change)**.
* All of the `RunGmic*` methods in the `Gmic<TGmicBitmap>` class now take the input images as a parameter **(breaking change)**.
  * This allows the `RunGmicAsync` methods to execute G'MIC commands in parallel.

### Fixed

The return value documentation for `RunGmic`.

### Removed

* The `AddInputImage` methods in the `Gmic<TGmicBitmap>` class.
* The `ClearInputImages` method in the `Gmic<TGmicBitmap>` class.
* The `void RunGmicAsync(string)` and `RunGmicAsyncCancel` methods in the `Gmic<TGmicBitmap>` class.
* The `RunGmicCompleted` and `RunGmicProgressChanged` events in the `Gmic<TGmicBitmap>` class.

## [v0.7.0](https://github.com/0xC0000054/gmic-sharp/compare/v0.6.0...v0.7.0) - 2020-07-10

### Added

* Support for changing the host application name seen by G'MIC scripts through the `HostName` property in `Gmic<TGmicBitmap>`.
* `RunGmicTaskAsync` methods for [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap) support.
* A `GrayAlpha16` value to the `GmicPixelFormat` enumeration **(breaking change)**.
* Support for running on BSD, Linux and macOS when using .NET Standard 2.1.

### Changed

* The `CustomResourcePath` property now supports paths non-ASCII characters.
* Throw an exception if `AddInputImage` is called while G'MIC is running.
* Throw an exception if `ClearInputImages` is called while G'MIC is running.
* Updated `Gmic<TGmicBitmap>` to conform to the [Event-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-overview) **(breaking change)**.
* `RunGmic(string)` is now a synchronous call, use `RunGmicAsync` or `RunGmicTaskAsync` for asynchronous calls **(breaking change)**.
* Raise the `RunGmicCompleted` and `RunGmicProgressChanged` events on the thread that called `RunGmicAsync`.
* `GdiPlusGmicBitmap` now checks if the class has been disposed before using the bitmap.
* Changed the native library names and deployment location to work with NuGet.
* Custom `GmicBitmap` classes now have to perform their own conversion to/from the G'MIC image format **(breaking change)**.
* Rename the `Gray` value in the `GmicPixelFormat` enumeration to `Gray8` **(breaking change)**.
* Throw an exception if `GetEnumerator` is called after the `OutputImageCollection<TGmicBitmap>` class has been disposed.

### Fixed

* The progress update event will only be fired when the value changes.

### Removed

* The `CustomUserFilePath` property in `Gmic<TGmicBitmap>`.
* The `RunGmic(string, CancellationToken)` method.
* The `GmicBitmapLock` structure.
* The `Bgr24`, `Bgr32`, `Bgra32` and `Rgb32` values from the `GmicPixelFormat` enumeration.

## v0.6.0

### Added

* A `ClearInputImages` method to the `Gmic<TGmicBitmap>` class.
* A `GdiPlusGmicBitmap(Image)` constructor overload.
* A Changelog. 

### Changed

* Improved type-safety for the `Gmic` class, it now uses a generic parameter
  to specify the `GmicBitmap` class that is in use. **(breaking change)**
* The `IGmicOutputImageFactory` now uses a generic parameter
  to specify the `GmicBitmap` class that is in use.  **(breaking change)**
* The `GdiPlusGmicBitmap(int, int, PixelFormat)` constructor overload is now internal. **(breaking change)**
* GDI+ Bitmaps that use an unsupported `PixelFormat` are converted to a
  supported format instead of throwing an exception.
* Improved the exception documentation for multiple methods.
* Replaced `Delegate.BeginInvoke` because it is not supported on .NET Core.

### Fixed

* Dispose of the previous output bitmaps before setting new ones.
* A few race conditions related to `System.Threading.Timer`.
* The `GdiPlusGmicBitmap(Bitmap)` constructor overload now clones the input image.
* An issue with the `CancellationToken` when updating the G'MIC status.
* An issue where G'MIC would always be reported as running.
* An issue that would hang an application that waited for G'MIC to complete.

### Removed

The `OutputImageInfo` structure

## v0.5.0

### Added

First version

