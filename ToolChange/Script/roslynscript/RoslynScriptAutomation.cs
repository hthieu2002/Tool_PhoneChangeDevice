using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;

namespace WindowsFormsApp.Script.RoslynScript
{
    public static class RoslynScriptAutomation
    {
        public static void Run(string commandsFile, string deviceID, CancellationToken token)
        {
            var commandLines = File.ReadAllLines(commandsFile)
                                   .Where(l => !string.IsNullOrWhiteSpace(l))
                                   .ToArray();

            var statements = CommandParser.ParseCommandsToStatements(commandLines);
            var runMethod = MethodFactory.CreateRunMethod(statements);
            var commandExecutorClass = MethodFactory.CreateCommandExecutorClass(runMethod, deviceID);
            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddMembers(commandExecutorClass)
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Services")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("WindowsFormsApp")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.IO"))
                //   SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Tesseract"))
                )
                .NormalizeWhitespace();

            System.Diagnostics.Debug.WriteLine(compilationUnit.ToFullString());
            //  MessageBox.Show(compilationUnit.ToFullString());
            var assembly = CompilerRunner.CompileAndLoadAssembly(compilationUnit, deviceID);
            if (assembly == null)
            {
                return;
            }
            var commandExecutorType = assembly.GetType("CommandExecutor");
            var executorInstance = Activator.CreateInstance(commandExecutorType, deviceID);
            var runMethodInfo = commandExecutorType.GetMethod("Run");
            runMethodInfo.Invoke(executorInstance, null);
        }

    }
}
