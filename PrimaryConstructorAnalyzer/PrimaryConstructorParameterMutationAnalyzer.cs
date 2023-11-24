using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection.Metadata;

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

		context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ClassDeclaration);
	}

	private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
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
			.OfType<IParameterSymbol>();

		ReportMutations(context, model, symbols, node);
	}

	private static void ReportMutations(SyntaxNodeAnalysisContext context, SemanticModel model, IEnumerable<ISymbol> symbols, SyntaxNode node)
	{
		ReportMutations(context, model, symbols, node.DescendantNodes().OfType<AssignmentExpressionSyntax>(), node => node.Left);
		ReportMutations(context, model, symbols, node.DescendantNodes().OfType<PostfixUnaryExpressionSyntax>(), node => node.Operand);
		ReportMutations(context, model, symbols, node.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>(), node => node.Operand);
	}

	private static void ReportMutations<T>(SyntaxNodeAnalysisContext context, SemanticModel model, IEnumerable<ISymbol> symbols, IEnumerable<T> candidates, Func<T, SyntaxNode> selector) where T : ExpressionSyntax
	{
		foreach (var candidate in candidates)
		{
			var candidateNode = selector(candidate);
			var candidateSymbol = model.GetSymbolInfo(candidateNode).Symbol;
			if (candidateSymbol == null)
				continue;

			foreach(var symbol in symbols)
			{
				if (SymbolEqualityComparer.Default.Equals(candidateSymbol, symbol))
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, candidateNode.GetLocation(), candidateSymbol.Name));
					break;
				}
			}
		}
	}
}