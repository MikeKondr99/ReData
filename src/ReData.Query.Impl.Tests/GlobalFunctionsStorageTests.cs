using FluentAssertions;
using ReData.Query.Core.Types;
using ReData.Query.Impl.Functions;

namespace ReData.Query.Impl.Tests;

public class GlobalFunctionsStorageTests
{
    [Fact]
    public void GetFunctions_DoesNotThrow_WhenDatabaseHasNoTemplateForParticularFunction()
    {
        var act = () => GlobalFunctionsStorage.GetFunctions(DatabaseTypes.SqlServer);

        act.Should().NotThrow();
    }

    [Fact]
    public void GetFunctions_SkipsDatabaseSpecificFunctions_ForUnsupportedDatabases()
    {
        var sqlServerFunctions = GlobalFunctionsStorage.GetFunctions(DatabaseTypes.SqlServer);

        var resolution = sqlServerFunctions.ResolveFunction(new FunctionSignature
        {
            Name = "ExcludeChars",
            Kind = FunctionKind.Default,
            ArgumentTypes = [ExprType.Text(), ExprType.Text()]
        });

        resolution.IsError().Should().BeTrue();
        resolution.UnwrapErr().Message.Should().Contain("Функция ExcludeChars");
    }

    [Fact]
    public void GetFunctions_KeepsDatabaseSpecificFunctions_ForSupportedDatabases()
    {
        var postgresFunctions = GlobalFunctionsStorage.GetFunctions(DatabaseTypes.PostgreSql);

        var resolution = postgresFunctions.ResolveFunction(new FunctionSignature
        {
            Name = "ExcludeChars",
            Kind = FunctionKind.Default,
            ArgumentTypes = [ExprType.Text(), ExprType.Text()]
        });

        resolution.IsError().Should().BeFalse();
    }
}
