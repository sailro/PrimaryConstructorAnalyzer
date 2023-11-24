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

		context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ClassDeclaration);
	}

	private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
	{
		if (context.Node is not ClassDeclarationSyntax node)
			return;

		if (node.ParameterList == null)
			return;

		var model = context.SemanticModel;

		foreach (var parameter in node.ParameterList.Parameters)
		{
			var symbol = model.GetDeclaredSymbol(parameter);
			if (symbol is null)
				continue;

			ReportMutations(context, node, model, symbol);
		}
	}

	private static void ReportMutations(SyntaxNodeAnalysisContext context, SyntaxNode node, SemanticModel model, ISymbol symbol)
	{
		var assignments = node
			.DescendantNodes()
			.OfType<AssignmentExpressionSyntax>();

		foreach (var assignment in assignments)
		{
			var left = assignment.Left;
			var leftSymbol = model.GetSymbolInfo(left).Symbol;
			if (leftSymbol == null)
				continue;

			if (SymbolEqualityComparer.Default.Equals(leftSymbol, symbol))
				context.ReportDiagnostic(Diagnostic.Create(Rule, left.GetLocation(), leftSymbol.Name));
		}
	}
}