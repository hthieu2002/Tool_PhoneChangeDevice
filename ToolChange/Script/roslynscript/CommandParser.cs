using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace WindowsFormsApp.Script.RoslynScript
{
    static class CommandParser
    {
        public static List<StatementSyntax> ParseCommandsToStatements(string[] commandLines)
        {
            var statements = new List<StatementSyntax>();
            for (int i = 0; i < commandLines.Length; i++)
            {
                string line = commandLines[i].Trim();

                if (line.StartsWith("for="))
                {
                    // Xử lý for có block { ... }
                    if (i + 1 < commandLines.Length && commandLines[i + 1].Trim() == "{")
                    {
                        int blockStart = i + 2;
                        int blockEnd = FindBlockEnd(commandLines, blockStart);

                        var blockLines = new string[blockEnd - blockStart];
                        Array.Copy(commandLines, blockStart, blockLines, 0, blockEnd - blockStart);

                        var forStatement = ParseCustomForLoop(line);
                        var bodyStatements = ParseCommandsToStatements(blockLines);
                        var bodyBlock = SyntaxFactory.Block(bodyStatements);

                        forStatement = forStatement.WithStatement(bodyBlock);
                        statements.Add(forStatement);

                        i = blockEnd;
                    }
                    else
                    {
                        statements.Add(ParseCustomForLoop(line));
                    }
                }
                else if (line.StartsWith("if="))
                {
                    // Xử lý if có block { ... }
                    if (i + 1 < commandLines.Length && commandLines[i + 1].Trim() == "{")
                    {
                        int blockStart = i + 2;
                        int blockEnd = FindBlockEnd(commandLines, blockStart);

                        var blockLines = new string[blockEnd - blockStart];
                        Array.Copy(commandLines, blockStart, blockLines, 0, blockEnd - blockStart);

                        var conditionExpr = ParseConditionExpression(line);
                        var bodyStatements = ParseCommandsToStatements(blockLines);
                        var bodyBlock = SyntaxFactory.Block(bodyStatements);

                        var ifStatement = SyntaxFactory.IfStatement(conditionExpr, bodyBlock);
                        statements.Add(ifStatement);

                        i = blockEnd;
                    }
                    else
                    {
                        var conditionExpr = ParseConditionExpression(line);
                        var ifStatement = SyntaxFactory.IfStatement(conditionExpr, SyntaxFactory.Block());
                        statements.Add(ifStatement);
                    }
                }
                else
                {
                    var stmt = ParseLineToStatement(line);
                    if (stmt != null)
                        statements.Add(stmt);
                }
            }
            return statements;
        }

        private static int FindBlockEnd(string[] lines, int startIndex)
        {
            int braceCount = 1;
            for (int j = startIndex; j < lines.Length; j++)
            {
                var l = lines[j].Trim();
                if (l == "{") braceCount++;
                else if (l == "}") braceCount--;

                if (braceCount == 0)
                    return j;
            }
            throw new Exception("Không tìm thấy đóng ngoặc } cho block.");
        }

        private static ExpressionSyntax ParseConditionExpression(string line)
        {
            // Bỏ "if=" hoặc "if =" nếu có khoảng trắng
            int equalIndex = line.IndexOf('=');
            if (equalIndex < 0 || equalIndex == line.Length - 1)
                throw new ArgumentException("Câu lệnh if không đúng định dạng.");

            var exprStr = line.Substring(equalIndex + 1).Trim();
            var expr = SyntaxFactory.ParseExpression(exprStr);
            return expr;
        }

        private static StatementSyntax ParseLineToStatement(string line)
        {
            line = line.Trim();
            if (line.StartsWith("Log(") || line.StartsWith("SendText(") || line.StartsWith("RunCommandShell("))
            {
                var expr = SyntaxFactory.ParseExpression(line);
                return SyntaxFactory.ExpressionStatement(expr);
            }
            if (line.Equals("break", StringComparison.OrdinalIgnoreCase))
            {
                return SyntaxFactory.BreakStatement();
            }
            else if (line.Equals("continue", StringComparison.OrdinalIgnoreCase))
            {
                return SyntaxFactory.ContinueStatement();
            }
            else if (line.StartsWith("return", StringComparison.OrdinalIgnoreCase))
            {
                string returnExpr = line.Length > 6 ? line.Substring(6).Trim() : null;
                if (string.IsNullOrEmpty(returnExpr))
                    return SyntaxFactory.ReturnStatement();
                else
                    return SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression(returnExpr));
            }
            else if (line.StartsWith("for="))
            {
                return ParseCustomForLoop(line);
            }
            else
            {
                return ParseLineToExpressionStatement(line);
            }
        }


        private static ExpressionStatementSyntax ParseLineToExpressionStatement(string line)
        {
            int idxOpen = line.IndexOf('(');
            int idxClose = line.IndexOf(')');
            if (idxOpen < 0 || idxClose < 0) return null;

            string methodName = line.Substring(0, idxOpen);
            string argsStr = line.Substring(idxOpen + 1, idxClose - idxOpen - 1);

            string[] argsParts = argsStr.Replace(",", " ").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var arguments = new List<ArgumentSyntax>();
            foreach (var arg in argsParts)
            {
                if (arg.StartsWith("\"") && arg.EndsWith("\""))
                {
                    string stringValue = arg.Substring(1, arg.Length - 2);
                    arguments.Add(
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(stringValue)
                            )
                        )
                    );
                }
                else if (int.TryParse(arg, out int val))
                {
                    arguments.Add(
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(val)
                            )
                        )
                    );
                }
                else
                {
                    arguments.Add(
                        SyntaxFactory.Argument(
                            SyntaxFactory.IdentifierName(arg)
                        )
                    );
                }
            }

            var invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(methodName),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)));

            return SyntaxFactory.ExpressionStatement(invocation);
        }

        private static ForStatementSyntax ParseCustomForLoop(string line)
        {
            var parts = line.Split(',');
            string varName = null;
            int endValue = 0;

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length != 2) continue;
                var key = kv[0].Trim();
                var value = kv[1].Trim();

                if (key == "for")
                    varName = value;
                else if (key == "end" && int.TryParse(value, out int val))
                    endValue = val;
            }

            if (string.IsNullOrEmpty(varName))
                throw new ArgumentException("Không tìm thấy tên biến for.");

            var initializer = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(varName))
                        .WithInitializer(SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))))));

            var condition = SyntaxFactory.BinaryExpression(
                SyntaxKind.LessThanExpression,
                SyntaxFactory.IdentifierName(varName),
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(endValue)));

            var incrementor = SyntaxFactory.PostfixUnaryExpression(
                SyntaxKind.PostIncrementExpression,
                SyntaxFactory.IdentifierName(varName));

            var body = SyntaxFactory.Block();

            var forStatement = SyntaxFactory.ForStatement(body)
                .WithDeclaration(initializer)
                .WithCondition(condition)
                .WithIncrementors(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(incrementor));

            return forStatement;
        }
    }
}
