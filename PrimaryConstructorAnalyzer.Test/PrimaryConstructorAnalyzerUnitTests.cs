﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = PrimaryConstructorAnalyzer.Test.CSharpAnalyzerVerifier<PrimaryConstructorAnalyzer.PrimaryConstructorParameterMutationAnalyzer>;

namespace PrimaryConstructorAnalyzer.Test
{
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
		public async Task TestPrimaryCtorParameterMutateAsync()
		{
			const string test = """
			                    class Foo(int i)
			                    {
			                        void Bar() {
			                            i = 42;
			                        }
			                    }
			                    """;

			var diagnostic = VerifyCS
				.Diagnostic(PrimaryConstructorParameterMutationAnalyzer.DiagnosticId)
				.WithLocation(4, 9)
				.WithArguments("i");

			await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
		}
	}
}
