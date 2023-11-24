using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace PrimaryConstructorAnalyzer.Test
{
	public static partial class CSharpAnalyzerVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
	{
		public static DiagnosticResult Diagnostic() => CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic();
		public static DiagnosticResult Diagnostic(string diagnosticId) => CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(diagnosticId);
		public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(descriptor);

		public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
		{
			var test = new Test
			{
				TestCode = source,
			};

			test.ExpectedDiagnostics.AddRange(expected);
			await test.RunAsync(CancellationToken.None);
		}
	}
}
