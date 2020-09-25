using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Arh
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "RedundantBaseConstructorCall")]
    public class ArhSnakeCaseWrapper : AdvancedRestHandler
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly OverrideDirection _direction;

        public ArhSnakeCaseWrapper(OverrideDirection direction = OverrideDirection.Both) : base()
        {
            _direction = direction;

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver {NamingStrategy = new SnakeCaseNamingStrategy()}
            };
        }

        public ArhSnakeCaseWrapper(string baseUrl, bool fixEndOfUrl = true,
            OverrideDirection direction = OverrideDirection.Both) : base(baseUrl, fixEndOfUrl)
        {
            _direction = direction;

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver {NamingStrategy = new SnakeCaseNamingStrategy()}
            };
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        protected override string JsonSerializeObject<TRequest>(TRequest req, RestHandlerRequestOptions options)
        {
            if (_direction.HasFlag(OverrideDirection.Serialization))
            {
                return JsonConvert.SerializeObject(req, _serializerSettings);
            }

            return base.JsonSerializeObject(req, options);

        }

        [SuppressMessage("ReSharper", "ReplaceWithSingleCallToSingle")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "InvertIf")]
        protected override object DeserializeToType(string jsonString, Type genericTypeArgument)
        {
            if (_direction.HasFlag(OverrideDirection.Deserialization))
            {
                var parameterTypes = new[] {typeof(string), typeof(JsonSerializerSettings)};
                return typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(i => i.Name.Equals(nameof(JsonConvert.DeserializeObject), StringComparison.InvariantCulture))
                    .Where(i => i.IsGenericMethod)
                    .Where(i => i.GetParameters().Select(a => a.ParameterType).SequenceEqual(parameterTypes))
                    .Single()

                    .MakeGenericMethod(genericTypeArgument)
                    .Invoke(null, new object[] {jsonString, _serializerSettings});
            }

            return base.DeserializeToType(jsonString, genericTypeArgument);
        }

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        protected override TResponse DeserializeToObject<TResponse>(string jsonString)
        {
            if (_direction.HasFlag(OverrideDirection.Deserialization))
            {
                //Faster Than Reflection
                return JsonConvert.DeserializeObject<TResponse>(jsonString, _serializerSettings);
                //var parameterTypes = new[] { typeof(string), typeof(JsonSerializerSettings) };
                //return (TResponse) typeof(JsonConvert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                //    .Where(i => i.Name.Equals(nameof(JsonConvert.DeserializeObject), StringComparison.InvariantCulture))
                //    .Where(i => i.IsGenericMethod)
                //    .Where(i => i.GetParameters().Select(a => a.ParameterType).SequenceEqual(parameterTypes))
                //    .Single()

                //    .MakeGenericMethod(typeof(TResponse))
                //    .Invoke(null, new object[] {jsonString, _serializerSettings});
            }

            return base.DeserializeToObject<TResponse>(jsonString);
        }
    }
}
