# Primary Constructor Analyzer
[![NuGet](https://img.shields.io/nuget/v/PrimaryConstructorAnalyzer.svg)](https://www.nuget.org/packages/PrimaryConstructorAnalyzer/)

C# 12 introduced [primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/primary-constructors) but the only drawback at the moment is that we can't declare parameters as `readonly`.

I'm quite sure this will come later but here is a simple analyzer to detect mutations to those parameters.

Just reference the [nuget](https://www.nuget.org/packages/PrimaryConstructorAnalyzer) in your favorite compatible IDE, and it should work.

![Visual Studio](https://github.com/sailro/PrimaryConstructorAnalyzer/assets/638167/cf1fe20d-e3af-4682-baf8-e6e6a17a7c0e)

![Visual Studio Code](https://github.com/sailro/PrimaryConstructorAnalyzer/assets/638167/85b2a613-2313-405b-a371-0b2c86098465)
