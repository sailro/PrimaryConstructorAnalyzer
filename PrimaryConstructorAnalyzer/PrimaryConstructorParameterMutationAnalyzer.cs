using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PrimaryConstructorAnalyzer;

#pragma warning disable RS2008

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PrimaryConstructorParameterMutationAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "PCA0001";

	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
	private const string Category = "Mutability";

	private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
	}

	private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
	{
		if (context.Node is not ClassDeclarationSyntax node)
			return;

		if (node.ParameterList == null)
			return;

		var model = context.SemanticModel;

		var symbols = node
			.ParameterList
			.Parameters
			.Select(p => model.GetDeclaredSymbol(p))
			.OfType<IParameterSymbol>()
			.ToArray();

		ReportMutations(context, model, symbols, node);
	}

	private static void ReportMutations(SyntaxNodeAnalysisContext context, SemanticModel model, IParameterSymbol[] symbols, SyntaxNode node)
	{
		ReportMutations(context, model, symbols, node.DescendantNodes().OfType<ExpressionSyntax>());
	}

	private static void ReportMutations(SyntaxNodeAnalysisContext context, SemanticModel model, IParameterSymbol[] symbols, IEnumerable<ExpressionSyntax> candidates)
	{
		var comparer = SymbolEqualityComparer.Default;

		foreach (var candidate in candidates)
		{
			foreach(var expression in ExpressionsSelector(candidate))
			{
				var candidateSymbol = model.GetSymbolInfo(expression).Symbol;
				if (candidateSymbol == null)
					continue;

				if (symbols.Any(symbol => comparer.Equals(candidateSymbol, symbol)))
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, expression.GetLocation(), candidateSymbol.Name));
				}
			}
		}
	}

	private static IEnumerable<ExpressionSyntax> ExpressionsSelector(ExpressionSyntax expression)
	{
		return expression switch
		{
			InvocationExpressionSyntax invocation => GetArgumentsWithRefOrOutKeyword(invocation),
			AssignmentExpressionSyntax assignment => [assignment.Left],
			PostfixUnaryExpressionSyntax postExpression => [postExpression.Operand],
			PrefixUnaryExpressionSyntax preExpression => [preExpression.Operand],
			_ => []
		};
	}

	private static IEnumerable<ExpressionSyntax> GetArgumentsWithRefOrOutKeyword(InvocationExpressionSyntax invocation)
	{
		foreach (var argument in invocation.ArgumentList.Arguments)
		{
			var keyword = argument.RefOrOutKeyword;
			if (keyword.IsKind(SyntaxKind.RefKeyword) || keyword.IsKind(SyntaxKind.OutKeyword))
				yield return argument.Expression;
		}
	}
}
