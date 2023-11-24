# Primary Constructor Analyzer
[![Build status](https://github.com/sailro/PrimaryConstructorAnalyzer/workflows/CI/badge.svg)](https://github.com/sailro/PrimaryConstructorAnalyzer/actions?query=workflow%3ACI)
[![NuGet](https://img.shields.io/nuget/v/PrimaryConstructorAnalyzer.svg)](https://www.nuget.org/packages/PrimaryConstructorAnalyzer/)

C# 12 introduced [primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/primary-constructors) but the only drawback at the moment is that we can't declare parameters as `readonly`.

I'm quite sure this will come later but here is a simple analyzer to detect mutations to those parameters.

Just reference the [nuget](https://www.nuget.org/packages/PrimaryConstructorAnalyzer) in your favorite compatible IDE, and it should work.

![Visual Studio](https://user-images.githubusercontent.com/638167/285432320-cf1fe20d-e3af-4682-baf8-e6e6a17a7c0e.png)

![Visual Studio Code](https://user-images.githubusercontent.com/638167/285432034-85b2a613-2313-405b-a371-0b2c86098465.png)
