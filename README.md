# Primary Constructor Analyzer
[![NuGet](https://img.shields.io/nuget/v/PrimaryConstructorAnalyzer.svg)](https://www.nuget.org/packages/PrimaryConstructorAnalyzer/)

C# 12 introduced [primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/primary-constructors) but the only drawback at the moment is that we can't declare parameters as `readonly`.

I'm quite sure this will come later but here is a simple analyzer to detect mutations to those parameters.

Just reference the nuget in your favorite compatible IDE, and it should work.