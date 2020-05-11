# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## Added

* A `ClearInputImages` method to the `Gmic<TGmicBitmap>` class.
* A `GdiPlusGmicBitmap(Image)` constructor overload.
* A Changelog. 

## Changed

* Improved type-safety for the `Gmic` class, it now uses a generic parameter
  to specify the `GmicBitmap` class that is in use. **(breaking change)**
* The `IGmicOutputImageFactory` now uses a generic parameter
  to specify the `GmicBitmap` class that is in use.  **(breaking change)**
* The `GdiPlusGmicBitmap(int, int, PixelFormat)` constructor overload is now internal. **(breaking change)**
* GDI+ Bitmaps that use an unsupported `PixelFormat` are converted to a
  supported format instead of throwing an exception.
* Improved the exception documentation for multiple methods.
* Replaced `Delegate.BeginInvoke` because it is not supported on .NET Core.

## Fixed

* Dispose of the previous output bitmaps before setting new ones.
* A few race conditions related to `System.Threading.Timer`.
* The `GdiPlusGmicBitmap(Bitmap)` constructor overload now clones the input image.
* An issue with the `CancellationToken` when updating the G'MIC status.
* An issue where G'MIC would always be reported as running.
* An issue that would hang an application that waited for G'MIC to complete.

## Removed

The `OutputImageInfo` structure


## v0.5.0

### Added

First version
