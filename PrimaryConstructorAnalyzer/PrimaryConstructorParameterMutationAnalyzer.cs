using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
		foreach (var candidate in candidates)
		{
			var candidateNode = ExpressionSelector(candidate);
			if (candidateNode == null)
				continue;

			var candidateSymbol = model.GetSymbolInfo(candidateNode).Symbol;
			if (candidateSymbol == null)
				continue;

			if (symbols.Any(symbol => SymbolEqualityComparer.Default.Equals(candidateSymbol, symbol)))
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, candidateNode.GetLocation(), candidateSymbol.Name));
			}
		}
	}

	private static ExpressionSyntax? ExpressionSelector(ExpressionSyntax expression)
	{
		return expression switch
		{
			AssignmentExpressionSyntax assignment => assignment.Left,
			PostfixUnaryExpressionSyntax postExpression => postExpression.Operand,
			PrefixUnaryExpressionSyntax preExpression => preExpression.Operand,
			_ => null
		};
	}
}