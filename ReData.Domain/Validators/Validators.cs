using FluentValidation;
using ReData.Core;

namespace ReData.Domain.Validators;

public static class Validators
{
        public static IRuleBuilderOptions<Domain.DataSource, IDictionary<StringKey,string>>
            RequiredParameters(
                this IRuleBuilder<Domain.DataSource, IDictionary<StringKey,string>> ruleBuilder,
                IEnumerable<string> properties)
        {
            IRuleBuilderOptions<Domain.DataSource, IDictionary<StringKey, string>> options = ruleBuilder.NotNull();
            var props = properties.ToArray();
            foreach (var prop in props)
            {
                options = options.Must(d => Array.TrueForAll(props, p => d.ContainsKey(p)))
                    .WithMessage($"Parameter '{prop}' is required");
            }

            return options;
        }
    
}