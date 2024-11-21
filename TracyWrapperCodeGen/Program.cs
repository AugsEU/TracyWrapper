using System;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using System.ComponentModel.Design.Serialization;
using Newtonsoft.Json.Linq;

static class Program
{
	const string INPUT_PROJ = @"TracyWrapper\TracyWrapper.csproj";
	const string OUTPUT_FOLDER = @"TracyWrapperStubs\";
	const string METHOD_NULL_COMMENT = "/// <summary> This method is a stub that does nothing. </summary>";

	static async Task Main(string[] args)
	{
		Console.WriteLine("=========STUBS CODE GEN START=========");

		// Initialize MSBuild (required for Roslyn to work with Visual Studio projects)
		MSBuildLocator.RegisterDefaults();

		// Path to the TracyWrapper project file
		string projectPath = GetInputProjPath();
		string outputPath = Path.Combine(FindSolutionFile(), OUTPUT_FOLDER);

		using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
		{
			// Load the TracyWrapper project
			var project = await workspace.OpenProjectAsync(projectPath);
			Console.WriteLine($"Loaded project: {project.Name}");

			// Process each document (source file) in the project
			foreach (Document? document in project.Documents)
			{
				SyntaxNode? root = await document.GetSyntaxRootAsync();
				if (root is null)
				{
					continue;
				}

				string outCode = "";
				Console.WriteLine($"Processing file: {document.Name}");

				// Generate stubs from the syntax tree
				GenerateStubs(root, document.Name, out outCode);

				if (outCode.Length > 0)
				{
					string outputFileDir = Path.Combine(outputPath, document.Name);
					Console.WriteLine($"Saving to: {outputFileDir}");
					WriteCodeToFile(outputFileDir, outCode);
				}
			}
		}
	}

	static void WriteCodeToFile(string outputPath, string code)
	{
		// Ensure the directory exists
		string? directory = Path.GetDirectoryName(outputPath);
		if(directory is null)
		{
			return;
		}

		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		// Write the code to the file
		File.WriteAllText(outputPath, code);
	}

	static void GenerateStubs(SyntaxNode root, string fileName, out string outCode)
	{
		outCode = "";

		bool hasClasses = false;

		var usingDeclarations = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

		foreach (UsingDirectiveSyntax? usingDeclaration in usingDeclarations)
		{
			if(!IsUsingAllowed(usingDeclaration))
			{ 
				continue;
			}

			outCode += usingDeclaration.ToFullString();
		}

		// Traverse the syntax tree to find classes and methods
		var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();

		foreach (NamespaceDeclarationSyntax? _namespace in namespaces)
		{
			outCode += string.Format("namespace {0} \n{{\n", _namespace.Name);

			// Traverse the syntax tree to find classes and methods
			var classDeclarations = _namespace.DescendantNodes().OfType<ClassDeclarationSyntax>();

			foreach (ClassDeclarationSyntax? classDeclaration in classDeclarations)
			{
				hasClasses = true;

				Console.WriteLine($"Class: {classDeclaration.Identifier.Text}");

				// Class header
				outCode += string.Format("\n\t{0}\n\t{{", GenerateClassDec(classDeclaration));

				// Constructors
				var constructors = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
				foreach (ConstructorDeclarationSyntax? constructor in constructors)
				{
					Console.WriteLine($"  Constructor: {constructor.Identifier.Text}");

					// Generate the stub signature (with no implementation)
					string stub = GenerateConstructorStub(constructor);

					outCode += string.Format("\n\t\t{0}\n\t\t{1}\n", METHOD_NULL_COMMENT, stub);
				}

				// Methods
				var methods = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
				foreach (MethodDeclarationSyntax? method in methods)
				{
					if (!IsMethodPublic(method))
					{
						continue;
					}

					Console.WriteLine($"  Method: {method.Identifier.Text}");

					// Generate the stub signature (with no implementation)
					string stub = GenerateMethodStub(method);

					outCode += string.Format("\n\t\t{0}\n\t\t{1}\n", METHOD_NULL_COMMENT, stub);
				}

				outCode += "\n\t}\n";
			}

			outCode += "}";
		}

		if(!hasClasses)
		{
			outCode = "";
		}
	}

	static bool IsUsingAllowed(UsingDirectiveSyntax usingDirective)
	{
		if(usingDirective.Name is null)
		{
			return false;
		}

		// Only allow "System." usings
		return usingDirective.Name.ToString().StartsWith("System", StringComparison.Ordinal);
	}

	static string GenerateClassDec(ClassDeclarationSyntax classDeclaration)
	{
		// Extract the signature
		string modifiers = string.Join(" ", classDeclaration.Modifiers.Select(m => m.Text));
		string className = classDeclaration.Identifier.Text;
		string baseTypes = classDeclaration.BaseList?.ToString() ?? string.Empty;

		return $"{modifiers} class {className} {baseTypes}".Trim();
	}

	static string GenerateMethodStub(MethodDeclarationSyntax method)
	{
		// Extract the signature and return it with an empty body
		var modifiers = string.Join(" ", method.Modifiers.Select(m => m.Text));
		var returnType = method.ReturnType.ToString();
		var methodName = method.Identifier.Text;
		var parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => p.ToString()));

		return $"{modifiers} {returnType} {methodName}({parameters}) {{ }}";
	}

	static bool IsMethodPublic(MethodDeclarationSyntax method)
	{
		return method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword));
	}

	static string GenerateConstructorStub(ConstructorDeclarationSyntax constructor)
	{
		// Extract the constructor signature
		var modifiers = string.Join(" ", constructor.Modifiers.Select(m => m.Text));
		var constructorName = constructor.Identifier.Text;
		var parameters = string.Join(", ", constructor.ParameterList.Parameters.Select(p => p.ToString()));

		return $"{modifiers} {constructorName}({parameters}) {{ }}";
	}

	static string GetInputProjPath()
	{
		// Get solution path
		string slnDir = FindSolutionFile();

		string projectPath = Path.Combine(slnDir, INPUT_PROJ);

		return Path.GetFullPath(projectPath);
	}


	static string FindSolutionFile()
	{
		string? directory = AppContext.BaseDirectory;

		while (!string.IsNullOrEmpty(directory))
		{
			// Look for .sln files in the current directory
			var solutionFiles = Directory.GetFiles(directory, "*.sln");
			if (solutionFiles.Length > 0)
			{
				return directory; // Return the first .sln file found
			}

			// Move up one directory
			directory = Directory.GetParent(directory)?.FullName;
		}

		throw new Exception("Couldn't locate sln file");
	}
}
