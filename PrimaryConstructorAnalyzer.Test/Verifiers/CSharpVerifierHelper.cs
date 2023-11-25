using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace PrimaryConstructorAnalyzer.Test;

internal static class CSharpVerifierHelper
{
	internal static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

	private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
	{
		var args = new[] { "/warnaserror:nullable" };
		var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
		var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

		nullableWarnings = nullableWarnings
			.SetItem("CS8632", ReportDiagnostic.Error)
			.SetItem("CS8669", ReportDiagnostic.Error);

		return nullableWarnings;
	}
}
