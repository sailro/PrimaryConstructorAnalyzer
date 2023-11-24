using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = PrimaryConstructorAnalyzer.Test.CSharpAnalyzerVerifier<PrimaryConstructorAnalyzer.PrimaryConstructorParameterMutationAnalyzer>;

namespace PrimaryConstructorAnalyzer.Test;

[TestClass]
public class PrimaryConstructorAnalyzerUnitTest
{
	[TestMethod]
	public async Task TestRegularCtorAsync()
	{
		const string test = """
		                    class Foo
		                    {
		                        public Foo(int i) {
		                            i = 0;
		                        }
		                    }
		                    """;

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[TestMethod]
	public async Task TestPrimaryCtorParameterReadAsync()
	{
		const string test = """
		                    class Foo(int i)
		                    {
		                        void Bar() {
		                            int j = i + 12;
		                        }
		                    }
		                    """;

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[TestMethod]
	public async Task TestPrimaryCtorParameterAssignmentAsync()
	{
		const string test = """
		                    class Foo(int i, string bar)
		                    {
		                        void Bar() {
		                            i = 42;
		                            bar = string.Empty;
		                        }
		                    }
		                    """;

		var diagnostics = new[]
		{
			VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(4, 9)
			.WithArguments("i"),

			VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(5, 9)
			.WithArguments("bar")
		};

		await VerifyCS.VerifyAnalyzerAsync(test, diagnostics);
	}

	[TestMethod]
	public async Task TestPrimaryCtorParameterIncrementAsync()
	{
		const string test = """
		                    class Foo(int i, string bar)
		                    {
		                        void Bar() {
		                            i += 1;
		                        }
		                    }
		                    """;

		var diagnostic = VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(4, 9)
			.WithArguments("i");

		await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}

	[TestMethod]
	public async Task TestPrimaryCtorParameterPostUnaryIncrementAsync()
	{
		const string test = """
		                    class Foo(int i, string bar)
		                    {
		                        void Bar() {
		                            i++;
		                        }
		                    }
		                    """;

		var diagnostic = VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(4, 9)
			.WithArguments("i");

		await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}

	[TestMethod]
	public async Task TestPrimaryCtorParameterPreUnaryDecrementAsync()
	{
		const string test = """
		                    class Foo(int i, string bar)
		                    {
		                        void Bar() {
		                            --i;
		                        }
		                    }
		                    """;

		var diagnostic = VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(4, 11)
			.WithArguments("i");

		await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}
}