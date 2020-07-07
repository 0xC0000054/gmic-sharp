# gmic-sharp

A .NET wrapper for [G'MIC](https://gmic.eu/).

## Features

* Allows .NET applications to execute G'MIC commands.
* Supports user-defined custom image types, the default image type uses the GDI+ `Bitmap` class. 
* Supports progress reporting and cancellation.
* Supports using a custom resource directory.

### Native libraries

This project depends on the native libraries in [gmic-sharp-native](https://github.com/0xC0000054/gmic-sharp-native).   
These libraries provide the native interface between gmic-sharp and [libgmic](https://github.com/dtschump/gmic).

### Example application

A Windows Forms-based example application is located at [gmic-sharp-example](https://github.com/0xC0000054/gmic-sharp-example).

## License

This project is licensed under the terms of the MIT License.   
See [License.txt](License.txt) for more information.

### Native libraries

The gmic-sharp native libraries (libGmicSharpNative*) are dual-licensed under the terms of the either the [CeCILL v2.1](https://cecill.info/licences/Licence_CeCILL_V2.1-en.html) (GPL-compatible) or [CeCILL-C v1](https://cecill.info/licences/Licence_CeCILL-C_V1-en.html) (similar to the LGPL).  
Pick the one you want to use.

This was done to match the licenses used by [libgmic](https://github.com/dtschump/gmic).
