# ReBuzz
ReBuzz is a modular digital audio workstation (DAW) built upon the foundation of [Jeskola Buzz](https://jeskola.net/buzz/) software. Written in C#, ReBuzz combines modern features with the beloved workflow of its predecessor. While it’s still in development, users should exercise some caution regarding stability and other potential uncertainties. The primary focus is on providing a stable experience and robust VST support.

## Features
* 32 and 64 bit VST2/3 support
* 32 and 64 bit buzz machine support
* Modern Pattern Editor, Modern Sequence Editor, AudioBlock, EnvelopeBlock, CMC, TrackScript...
* Multi-process architecture
* Multi-io for native and managed machines
* Includes [NWaves](https://github.com/ar1st0crat/NWaves) .NET DSP library for audio processing
* bmx and bmxml file support
* ...

## Download
[ReBuzz Installer](https://github.com/wasteddesign/ReBuzz/releases/latest)

Requires:
1. [.NET 9.0 Desktop Runtime - Windows x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.100-windows-x64-installer)
2. [Latest Microsoft Visual C++ Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170)

## How to build?
1. Install [ReBuzz](https://github.com/wasteddesign/ReBuzz/releases/latest))
2. Clone/download this repo
3. Open the solution in Visual Studio
4. Update solution dependencies (Dependencies-->Assemblies) to reference assemblies included in the ReBuzz app folder.
5. Build ReBuzz and copy ReBuzz.exe, dll and pdb files to ReBuzz install folder
6. Open project preferences and in the Debug section click 'Open debug launch profiles UI'
   * Click 'Create a new profile'
   * Choose 'Executable' and select ReBuzz.exe in the ReBuzz install directory
7. Choose the new debug profile and run the app


## How can I help?
All the basic functionality is implemented but there many areas to improve. In general, contributions are needed in every part of the software, but here are few items to look into:

- [ ] Pick an [issue](https://github.com/wasteddesign/ReBuzz/issues) and start contributing to Rebuzz development today!
- [ ] Improve stability, fix bugs and issues
- [ ] Cleanup code and architecture
- [ ] Add comments and documentation
- [ ] Improve Audio wave handling (Wavetable)
- [ ] Improve file handling to support older songs
- [ ] Reduce latency, optimize code

You might want to improve also
- [ReBuzz GUI Components](https://github.com/wasteddesign/ReBuzzGUI)
- [ReBuzzEngine](https://github.com/wasteddesign/ReBuzzEngine)
- [ModernPatternEditor](https://github.com/wasteddesign/ModernPatternEditor)

Let's make this a good one.
