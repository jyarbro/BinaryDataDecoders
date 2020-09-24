﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace BinaryDataDecoders.TestUtilities
{
    public static class TextContextExtensions
    {
        public static TestContext AddResult(this TestContext context, object value)
        {
            var fileName = $"{value.GetType().Name}_{DateTime.Now.Ticks}.json".Replace('`', '_').Replace(':', '_').Replace('<', '_').Replace('>', '_');

            if (value is byte[] data)
            {
                AddResultFile(context, Path.ChangeExtension(fileName, ".bin"), data);
            }
            else if (value is string content)
            {
                AddResultFile(context, Path.ChangeExtension(fileName, ".txt"), Encoding.UTF8.GetBytes(content));
            }
            else if (value != null)
            {
                var json = JsonConvert.SerializeObject(value, Formatting.Indented);
                AddResultFile(context, fileName, Encoding.UTF8.GetBytes(json));
            }
            else
            {
                context.WriteLine("No result");
            }
            return context;
        }
        public static TestContext AddResultFile(this TestContext context, string fileName, byte[] content)
        {
            var outFile = Path.Combine(context.TestRunResultsDirectory, fileName);
            var dir = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(outFile, content);
            context.AddResultFile(outFile);
            return context;
        }

        //public static async Task<T> GetTestDataAsync<T>(this TestContext context, string target = null)
        //{
        //    var typeQuery = from assembly in AppDomain.CurrentDomain.GetAssemblies()
        //                    from type in assembly.GetTypes()
        //                    select type;
        //    var testType = typeQuery.FirstOrDefault(t => t.FullName == context.FullyQualifiedTestClassName);

        //    var testClass = Activator.CreateInstance(testType);

        //    var targetName = string.IsNullOrWhiteSpace(target) ? context.TestName : target;

        //    if (string.IsNullOrWhiteSpace(target))
        //        target = null;

        //    var possibleResources = target != null ? new[] {
        //      $"{testType.Name}_{context.TestName}_{target}" ,
        //      $"{testType.Name}_{context.TestName}_{target}.json",
        //      $"{testType.Name}_{target}" ,
        //      $"{testType.Name}_{target}.json",

        //      $"{context.TestName}_{target}",
        //      $"{context.TestName}_{target}.json",
        //      $"{target}",
        //      $"{target}.json",
        //    } : new[]
        //    {
        //      $"{testType.Name}_{context.TestName}",
        //      $"{testType.Name}_{context.TestName}.json",
        //      $"{context.TestName}",
        //      $"{context.TestName}.json",
        //    }.Where(i => i != null);

        //    async Task<string> getValueAsync()
        //    {
        //        foreach (var possible in possibleResources)
        //        {
        //            var result = await testClass.GetResourceAsStringAsync(possible);
        //            if (!string.IsNullOrWhiteSpace(result))
        //                return result;
        //        }
        //        return default;
        //    }

        //    var content = await getValueAsync();

        //    if (string.IsNullOrWhiteSpace(content))
        //        return default;

        //    if (typeof(T) == typeof(string))
        //        return (T)(object)content;
        //    else if (typeof(T) == typeof(JToken))
        //        return (T)(object)JToken.Parse(content);
        //    else if (typeof(T) == typeof(JObject))
        //        return (T)(object)JObject.Parse(content);
        //    else if (typeof(T) == typeof(JArray))
        //        return (T)(object)JArray.Parse(content);

        //    var services = new ServiceCollection()
        //            .AddToolkitServices()
        //            .BuildServiceProvider();

        //    var deserializer = services.GetService<IObjectDeserializer>();

        //    var result = await deserializer.DeserializeAsync<T>(content);
        //    return result;


        //}
    }
}
