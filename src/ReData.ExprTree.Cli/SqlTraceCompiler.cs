using ReData.Query.Core.Template;
using ReData.Query.Lang.Expressions;

namespace ReData.ExprTree.Cli;

public sealed record SqlTokenTrace(string Token, Expr? SourceExpr);

public sealed record SqlTraceResult(string Sql, IReadOnlyList<SqlTokenTrace> Tokens);

public static class SqlTraceCompiler
{
    public static SqlTraceResult Compile(ResolvedExpr resolvedExpr)
    {
        var segments = new List<SqlSegment>();
        Append(resolvedExpr, segments);

        var sql = string.Concat(segments.Select(s => s.Text));
        var tokens = BuildTokensFromSegments(segments);
        tokens = SimplifyQualifiedFieldTokens(tokens);

        return new SqlTraceResult(sql, tokens);
    }

    private static void Append(ResolvedExpr resolvedExpr, List<SqlSegment> segments)
    {
        foreach (var token in resolvedExpr.Template.Tokens)
        {
            switch (token)
            {
                case ConstToken(var text):
                    segments.Add(new SqlSegment(text, resolvedExpr.Expression));
                    break;
                case ArgToken(var index):
                    if (resolvedExpr.Arguments is null || index >= resolvedExpr.Arguments.Count)
                    {
                        throw new InvalidOperationException("Invalid argument index during SQL tracing.");
                    }

                    Append(resolvedExpr.Arguments[index], segments);
                    break;
            }
        }
    }

    private static List<SqlTokenTrace> BuildTokensFromSegments(IEnumerable<SqlSegment> segments)
    {
        var result = new List<SqlTokenTrace>();
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment.Text))
            {
                continue;
            }

            result.Add(new SqlTokenTrace(segment.Text, segment.SourceExpr));
        }

        return result;
    }

    private static List<SqlTokenTrace> SimplifyQualifiedFieldTokens(IReadOnlyList<SqlTokenTrace> tokens)
    {
        var result = new List<SqlTokenTrace>(tokens.Count);
        var i = 0;
        while (i < tokens.Count)
        {
            if (i + 6 < tokens.Count
                && IsNameToken(tokens[i], "\"")
                && IsNameToken(tokens[i + 1], "src")
                && IsNameToken(tokens[i + 2], "\"")
                && IsNameToken(tokens[i + 3], ".")
                && IsNameToken(tokens[i + 4], "\"")
                && IsNameToken(tokens[i + 6], "\""))
            {
                var fieldName = tokens[i + 5].Token;
                result.Add(new SqlTokenTrace($"\"{fieldName}\"", tokens[i + 5].SourceExpr));
                i += 7;
                continue;
            }

            if (TryStripSourceQualifier(tokens[i], out var strippedSingle))
            {
                result.Add(new SqlTokenTrace(strippedSingle, tokens[i].SourceExpr));
                i++;
                continue;
            }

            if (i + 1 < tokens.Count
                && IsSourceToken(tokens[i])
                && TryStripDotQualifiedIdentifier(tokens[i + 1], out var strippedFromDotPrefixed))
            {
                result.Add(new SqlTokenTrace(strippedFromDotPrefixed, tokens[i + 1].SourceExpr));
                i += 2;
                continue;
            }

            if (i + 2 < tokens.Count
                && IsSourceToken(tokens[i])
                && tokens[i + 1].Token == "."
                && IsQuotedIdentifier(tokens[i + 2].Token))
            {
                result.Add(new SqlTokenTrace(tokens[i + 2].Token, tokens[i + 2].SourceExpr));
                i += 3;
                continue;
            }

            result.Add(tokens[i]);
            i++;
        }

        return result;
    }

    private static bool IsSourceToken(SqlTokenTrace token)
    {
        if (token.SourceExpr is not NameExpr)
        {
            return false;
        }

        return string.Equals(token.Token, "\"src\"", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNameToken(SqlTokenTrace token, string value)
    {
        return token.SourceExpr is NameExpr
               && string.Equals(token.Token, value, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryStripSourceQualifier(SqlTokenTrace token, out string strippedToken)
    {
        strippedToken = string.Empty;
        if (token.SourceExpr is not NameExpr)
        {
            return false;
        }

        var prefix = "\"src\".";
        if (!token.Token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        strippedToken = token.Token[prefix.Length..];
        return strippedToken.Length > 0;
    }

    private static bool TryStripDotQualifiedIdentifier(SqlTokenTrace token, out string strippedToken)
    {
        strippedToken = string.Empty;
        if (token.SourceExpr is not NameExpr)
        {
            return false;
        }

        if (!token.Token.StartsWith('.'))
        {
            return false;
        }

        var candidate = token.Token[1..];
        if (!IsQuotedIdentifier(candidate))
        {
            return false;
        }

        strippedToken = candidate;
        return true;
    }

    private static bool IsQuotedIdentifier(string token)
    {
        return token.Length >= 2 && token[0] == '"' && token[^1] == '"';
    }

    private sealed record SqlSegment(string Text, Expr SourceExpr);
}
