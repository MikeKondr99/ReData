using FluentAssertions;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Tests.Queries;

public abstract partial class Сommon
{
    [Fact(DisplayName = "Переменная из блока Where должна быть доступна в последующих блоках")]
    public async Task WhereConstantShouldBeAvailableInNextBlocks()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const threshold = 5; UserId > threshold")
            .Select(new()
            {
                ["UserId"] = "UserId",
                ["ThresholdPlusOne"] = "threshold + 1",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Where(u => u.UserId > 5)
            .Select(u => new
            {
                u.UserId,
                ThresholdPlusOne = 6,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "inline const должен работать в Where без сохранения в следующий блок")]
    public async Task InlineConstInWhere_ShouldNotLeakToNextBlocks()
    {
        // Arrange
        var withInlineConst = (await CreateUsersQueryWithRuntimeAsync())
            .Where("UserId > const(5)")
            .Expect("Valid query");

        // Act
        var failed = withInlineConst.Where("avgAge > 0");
        var errors = failed.UnwrapErr();

        // Assert
        errors
            .SelectMany(e => e)
            .Should()
            .Contain(e => e.Message.Contains("'avgAge'"));
    }

    [Fact(DisplayName = "inline const с агрегатом должен вычисляться в Select")]
    public async Task InlineConstWithAggregateInSelect_ShouldWork()
    {
        // Arrange
        var selectResult = (await CreateUsersQueryWithRuntimeAsync())
            .Select(new()
            {
                ["TotalAge"] = "const(SUM(Age))",
            });

        if (selectResult.UnwrapErr(out var selectErrors, out var qbBase))
        {
            var message = string.Join(" | ", selectErrors.SelectMany(e => e).Select(e => e.Message));
            throw new InvalidOperationException(message);
        }

        var qb = qbBase.Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expected = assets.UsersDynamicArray.Sum(u => (double)u.Age);
        dynamic[] expectData =
        [
            new
            {
                TotalAge = expected
            }
        ];

        result.Should().BeEquivalentTo(expectData.PrepareRecords(), o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная со средним значением должна фильтровать записи в блоке Where")]
    public async Task WhereConstantWithAvgAggregationFiltersRows()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgAge = AVG(Age); Age >= avgAge")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgAge = assets.UsersDynamicArray.Average(u => (double)u.Age);
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Age >= avgAge)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с суммой должна использоваться в блоке Select")]
    public async Task WhereConstantWithSumAggregationCanBeUsedInSelect()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const totalSalary = SUM(Salary); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["TotalSalary"] = "totalSalary",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var totalSalary = assets.UsersDynamicArray.Sum(u => (double)u.Salary);
        dynamic[] expectData =
        [
            new
            {
                TotalSalary = totalSalary
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с количеством записей должна использоваться в блоке Select")]
    public async Task WhereConstantWithCountAggregationCanBeUsedInSelect()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const userCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["UserCount"] = "userCount",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] expectData =
        [
            new
            {
                UserCount = assets.UsersDynamicArray.Length
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с минимальным значением должна фильтровать записи в блоке Where")]
    public async Task WhereConstantWithMinAggregationFiltersRows()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const minUserId = MIN(UserId); UserId = minUserId")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var minUserId = assets.UsersDynamicArray.Min(u => (int)u.UserId);
        var expect = assets.UsersDynamicArray
            .Where(u => (int)u.UserId == minUserId)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с максимальным значением должна фильтровать записи в блоке Where")]
    public async Task WhereConstantWithMaxAggregationFiltersRows()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const maxSalary = MAX(Salary); Salary = maxSalary")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var maxSalary = assets.UsersDynamicArray.Max(u => (double)u.Salary);
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Salary == maxSalary)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Цепочка агрегатных переменных должна корректно работать в блоке Where")]
    public async Task WhereConstantChainedAggregationsCanBeUsedInWhere()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgAge = AVG(Age); const threshold = avgAge + 5; Age >= threshold")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgAge = assets.UsersDynamicArray.Average(u => (double)u.Age);
        var threshold = avgAge + 5;
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Age >= threshold)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Цепочка агрегатных переменных должна корректно работать в блоке Select")]
    public async Task WhereConstantChainedAggregationsCanBeUsedInSelect()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgSalary = AVG(Salary); const adjusted = avgSalary - 1000; true")
            .Expect("Valid query")
            .Select(new()
            {
                ["Adjusted"] = "adjusted",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var adjusted = assets.UsersDynamicArray.Average(u => (double)u.Salary) - 1000;
        dynamic[] expectData =
        [
            new
            {
                Adjusted = adjusted
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная должна вычисляться в контексте после предыдущего блока Where")]
    public async Task WhereConstantUsesContextAfterPreviousWhere()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("Age >= 30")
            .Where("const avgAge = AVG(Age); Age >= avgAge")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var filtered = assets.UsersDynamicArray.Where(u => (int)u.Age >= 30).ToArray();
        var avgAge = filtered.Average(u => (double)u.Age);
        var expect = filtered
            .Where(u => (double)u.Age >= avgAge)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная, объявленная в одном Where, должна использоваться в следующем Where")]
    public async Task WhereConstantCanBeUsedAcrossWhereBlocks()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgAge = AVG(Age); true")
            .Where("Age >= avgAge")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgAge = assets.UsersDynamicArray.Average(u => (double)u.Age);
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Age >= avgAge)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная из блока Where должна использоваться в блоке OrderBy")]
    public async Task WhereConstantCanBeUsedInOrderBy()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgAge = AVG(Age); true")
            .Select(new()
            {
                ["UserId"] = "UserId",
                ["Age"] = "Age",
                ["AgeDiff"] = "Abs(Age - avgAge)",
            })
            .OrderBy([
                ("AgeDiff", OrderItem.Type.Asc),
                ("UserId", OrderItem.Type.Asc),
            ])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgAge = assets.UsersDynamicArray.Average(u => (double)u.Age);
        var expect = assets.UsersDynamicArray
            .Select(u => new
            {
                u.UserId,
                u.Age,
                AgeDiff = Math.Abs((double)u.Age - avgAge),
            })
            .OrderBy(u => u.AgeDiff)
            .ThenBy(u => u.UserId)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная со средним значением через сумму и количество должна вычисляться корректно")]
    public async Task WhereConstantManualAverageViaSumAndCount()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const totalAge = SUM(Age); const cnt = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["ManualAvg"] = "totalAge / cnt",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var manualAvg = assets.UsersDynamicArray.Sum(u => (double)u.Age) / assets.UsersDynamicArray.Length;
        dynamic[] expectData =
        [
            new
            {
                ManualAvg = manualAvg
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Смешивание литеральной и агрегатной констант из разных блоков должно работать")]
    public async Task WhereConstantCanMixLiteralAndAggregation()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const extra = 10; true")
            .Expect("Valid query")
            .Where("const avgAge = AVG(Age); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["AvgPlusExtra"] = "avgAge + extra",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expected = assets.UsersDynamicArray.Average(u => (double)u.Age) + 10;
        dynamic[] expectData =
        [
            new
            {
                AvgPlusExtra = expected
            }
        ];

        result.Should().BeEquivalentTo(expectData.PrepareRecords(), o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с количеством записей должна учитывать уже отфильтрованный контекст")]
    public async Task WhereConstantCountUsesFilteredContext()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("Age > 30")
            .Where("const adultsCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["AdultsCount"] = "adultsCount",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var adultsCount = assets.UsersDynamicArray.Count(u => (int)u.Age > 30);
        dynamic[] expectData =
        [
            new
            {
                AdultsCount = adultsCount
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с минимальной зарплатой должна использоваться в строгом сравнении")]
    public async Task WhereConstantMinSalaryCanBeUsedForInequality()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const minSalary = MIN(Salary); Salary > minSalary")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var minSalary = assets.UsersDynamicArray.Min(u => (double)u.Salary);
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Salary > minSalary)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с максимальной зарплатой должна использоваться в строгом сравнении")]
    public async Task WhereConstantMaxSalaryCanBeUsedForInequality()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const maxSalary = MAX(Salary); Salary < maxSalary")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var maxSalary = assets.UsersDynamicArray.Max(u => (double)u.Salary);
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Salary < maxSalary)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с минимальным значением должна использоваться в выражении блока Select")]
    public async Task WhereConstantCanUseMinAggregationInSelect()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const minUserId = MIN(UserId); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["MinUserIdPlusOne"] = "minUserId + 1",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var minUserIdPlusOne = assets.UsersDynamicArray.Min(u => (int)u.UserId) + 1;
        dynamic[] expectData =
        [
            new
            {
                MinUserIdPlusOne = minUserIdPlusOne
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная из блока Where должна использоваться в выборке после GroupBy")]
    public async Task WhereConstantCanBeUsedInsideGroupBySelect()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgAge = AVG(Age); true")
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["UserCount"] = "COUNT()",
                ["AvgAge"] = "avgAge",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgAge = assets.UsersDynamicArray.Average(u => (double)u.Age);
        var expect = assets.UsersDynamicArray
            .GroupBy(u => (string)u.Notes, (k, v) => new
            {
                Note = k,
                UserCount = v.Count(),
                AvgAge = avgAge,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная должна вычисляться по полям после промежуточной проекции Select")]
    public async Task WhereConstantCanBeUsedAfterSelectProjection()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Select(new()
            {
                ["AgePlusOne"] = "Age + 1",
                ["UserId"] = "UserId",
            })
            .Where("const avgAgePlusOne = AVG(AgePlusOne); AgePlusOne >= avgAgePlusOne")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var projected = assets.UsersDynamicArray.Select(u => new
        {
            AgePlusOne = (int)u.Age + 1,
            u.UserId,
        }).ToArray();
        var avgAgePlusOne = projected.Average(u => (double)u.AgePlusOne);
        var expect = projected
            .Where(u => (double)u.AgePlusOne >= avgAgePlusOne)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная со средним значением должна использоваться в фильтре по отклонению")]
    public async Task WhereConstantCanBeUsedForDistanceToAverageFilter()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgSalary = AVG(Salary); Abs(Salary - avgSalary) < 10000")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgSalary = assets.UsersDynamicArray.Average(u => (double)u.Salary);
        var expect = assets.UsersDynamicArray
            .Where(u => Math.Abs((double)u.Salary - avgSalary) < 10000)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с количеством записей должна корректно работать вместе с ограничением Take")]
    public async Task WhereConstantCountCanDriveTakeBoundedByDataset()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const userCount = COUNT(); true")
            .Expect("Valid query")
            .Take((uint)assets.UsersDynamicArray.Length)
            .Select(new()
            {
                ["UserId"] = "UserId",
                ["UserCount"] = "userCount",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var userCount = assets.UsersDynamicArray.Length;
        var expect = assets.UsersDynamicArray
            .Select(u => new
            {
                u.UserId,
                UserCount = userCount,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с количеством записей должна корректно преобразовываться в текст")]
    public async Task WhereConstantCountCanBeConvertedToText()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const userCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["UserCountText"] = "userCount.Text()",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] expectData =
        [
            new
            {
                UserCountText =
                    assets.UsersDynamicArray.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменные с минимумом и максимумом должны задавать внутренний диапазон фильтра")]
    public async Task WhereConstantMinAndMaxCanDefineInnerRange()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const minAge = MIN(Age); const maxAge = MAX(Age); Age > minAge and Age < maxAge")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var minAge = assets.UsersDynamicArray.Min(u => (int)u.Age);
        var maxAge = assets.UsersDynamicArray.Max(u => (int)u.Age);
        var expect = assets.UsersDynamicArray
            .Where(u => (int)u.Age > minAge && (int)u.Age < maxAge)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Одна переменная должна использоваться сразу в нескольких колонках блока Select")]
    public async Task WhereConstantCanBeUsedInMultipleSelectColumns()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const userCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["CountA"] = "userCount",
                ["CountB"] = "userCount + 1",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var userCount = assets.UsersDynamicArray.Length;
        dynamic[] expectData =
        [
            new
            {
                CountA = userCount,
                CountB = userCount + 1
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная должна подставляться в строковую интерполяцию")]
    public async Task WhereConstantCanBeUsedInStringInterpolation()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const userCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["Summary"] = "'Users: {userCount}'",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] expectData =
        [
            new
            {
                Summary = $"USERS: {assets.UsersDynamicArray.Length}"
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная должна использоваться в математических функциях")]
    public async Task WhereConstantCanBeUsedInMathFunctions()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgAge = AVG(Age); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["RoundedAvg"] = "Floor(avgAge)",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var roundedAvg = Math.Floor(assets.UsersDynamicArray.Average(u => (double)u.Age));
        dynamic[] expectData =
        [
            new
            {
                RoundedAvg = roundedAvg
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с количеством записей должна учитывать ограниченный контекст выборки")]
    public async Task WhereConstantCountRespectsLimitedContext()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Take(5)
            .Where("const limitedCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["LimitedCount"] = "limitedCount",
            })
            .Expect("Valid query")
            .Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] expectData =
        [
            new
            {
                LimitedCount = 5
            }
        ];
        var expect = expectData.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная должна использоваться в нескольких последовательных блоках Where")]
    public async Task WhereConstantCanBeUsedAcrossMultipleWhereBlocksAfterDeclaration()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const avgSalary = AVG(Salary); true")
            .Where("Salary >= avgSalary")
            .Where("Age >= 30")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var avgSalary = assets.UsersDynamicArray.Average(u => (double)u.Salary);
        var expect = assets.UsersDynamicArray
            .Where(u => (double)u.Salary >= avgSalary)
            .Where(u => (int)u.Age >= 30)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная из уже отфильтрованного контекста должна использоваться в последующем Where")]
    public async Task WhereConstantFromFilteredScopeCanBeUsedInLaterWhere()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("Age > 30")
            .Where("const avgSalary = AVG(Salary); true")
            .Where("Salary >= avgSalary")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var filtered = assets.UsersDynamicArray.Where(u => (int)u.Age > 30).ToArray();
        var avgSalary = filtered.Average(u => (double)u.Salary);
        var expect = filtered
            .Where(u => (double)u.Salary >= avgSalary)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Переменная с количеством записей должна использоваться в проекции перед сортировкой")]
    public async Task WhereConstantCountCanBeUsedInOrderByProjection()
    {
        // Arrange
        var qb = (await CreateUsersQueryWithRuntimeAsync())
            .Where("const userCount = COUNT(); true")
            .Expect("Valid query")
            .Select(new()
            {
                ["UserId"] = "UserId",
                ["SortWeight"] = "userCount - UserId",
            })
            .Expect("Valid query")
            .OrderBy([("SortWeight", OrderItem.Type.Desc)])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var userCount = assets.UsersDynamicArray.Length;
        var expect = assets.UsersDynamicArray
            .Select(u => new
            {
                u.UserId,
                SortWeight = userCount - (int)u.UserId,
            })
            .OrderByDescending(u => u.SortWeight)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Локальная константная переменная должна использоваться в том же выражении Select")]
    public async Task SelectConstantCanBeDeclaredAndUsedInSameExpression()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Select(new()
            {
                ["ShiftedAge"] = "const shift = 2; Age + shift",
                ["UserId"] = "UserId",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Select(u => new
            {
                ShiftedAge = (int)u.Age + 2,
                u.UserId,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Локальная константная переменная должна влиять на сортировку в блоке OrderBy")]
    public async Task OrderByConstantCanBeDeclaredAndUsedInSameExpression()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy(
            [
                ("const m = 3; Mod(UserId, m)", OrderItem.Type.Asc),
                ("UserId", OrderItem.Type.Asc),
            ])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .OrderBy(u => (int)u.UserId % 3)
            .ThenBy(u => (int)u.UserId)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Локальная константная переменная должна работать в ключе группы и в выборке GroupBy")]
    public async Task GroupByConstantCanBeDeclaredAndUsedInSameExpression()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["const shift = 1; Age + shift"], new()
            {
                ["ShiftedAge"] = "const shift = 1; Age + shift",
                ["UserCount"] = "COUNT()",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(
                u => (int)u.Age + 1,
                (k, v) => new
                {
                    ShiftedAge = k,
                    UserCount = v.Count(),
                })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
}



