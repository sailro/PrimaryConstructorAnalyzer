using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = PrimaryConstructorAnalyzer.Test.Verifiers.CSharpAnalyzerVerifier<PrimaryConstructorAnalyzer.PrimaryConstructorParameterMutationAnalyzer>;

namespace PrimaryConstructorAnalyzer.Test;

[TestClass]
public class PrimaryConstructorAnalyzerUnitTest
{
	[TestMethod]
	public Task TestRegularCtorAsync()
	{
		const string test = """
		                    class Foo
		                    {
		                        public Foo(int i) {
		                            i = 0;
		                        }
		                    }
		                    """;

		return VerifyCS.VerifyAnalyzerAsync(test);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterReadAsync()
	{
		const string test = """
		                    class Foo(int i)
		                    {
		                        void Bar() {
		                            int j = i + 12;
		                        }
		                    }
		                    """;

		return VerifyCS.VerifyAnalyzerAsync(test);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterAssignmentAsync()
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

		return VerifyCS.VerifyAnalyzerAsync(test, diagnostics);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterIncrementAsync()
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

		return VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterPostUnaryIncrementAsync()
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

		return VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterPreUnaryDecrementAsync()
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

		return VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterWithOutArgumentInvocationAsync()
	{
		const string test = """
		                    class Foo(int c)
		                    {
		                        void Bar(out int x) => x = 1;

		                        public void Baz() {
		                            Bar(out c);
		                        }
		                    }
		                    """;

		var diagnostic = VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(6, 17)
			.WithArguments("c");

		return VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}

	[TestMethod]
	public Task TestPrimaryCtorParameterWithRefArgumentInvocationAsync()
	{
		const string test = """
		                    class Foo(int c)
		                    {
		                        void Bar(int y, ref int x, int z) => x = 1;

		                        public void Baz() {
		                            Bar(c, ref c, c);
		                        }
		                    }
		                    """;

		var diagnostic = VerifyCS
			.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
			.WithLocation(6, 20)
			.WithArguments("c");

		return VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
	}
}
