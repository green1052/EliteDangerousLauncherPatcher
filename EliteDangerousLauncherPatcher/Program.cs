using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Resources;
using Newtonsoft.Json.Linq;

namespace EliteDangerousLauncherPatcher;

internal static class Program
{
    public static void Main()
    {
        Console.Write("input LocalResources.dll path: ");
        string dllPath = Console.ReadLine();

        Console.Write("input paratranz json path: ");
        string jsonPath = Console.ReadLine();

        if (string.IsNullOrEmpty(dllPath) || string.IsNullOrEmpty(jsonPath))
        {
            Console.WriteLine("path is null");
            return;
        }

        JArray json = JArray.Parse(File.ReadAllText(jsonPath));

        ModuleContext modCtx = ModuleDef.CreateModuleContext();
        ModuleDefMD module = ModuleDefMD.Load(dllPath, modCtx);

        module.Resources.Remove(module.Resources.First());

        ResourceElementSet element = new();

        foreach (JToken jToken in json.Children())
        {
            string key = jToken.Value<string>("key");
            string original = jToken.Value<string>("original");
            string translation = jToken.Value<string>("translation");

            element.Add(new ResourceElement
            {
                Name = key,
                ResourceData = new BuiltInResourceData(ResourceTypeCode.String,
                    string.IsNullOrEmpty(translation) ? original : translation)
            });
        }

        MemoryStream outStream = new();
        ResourceWriter.Write(module, outStream, element);

        module.Resources.Add(new EmbeddedResource("LocalResources.Properties.Resources.resources", outStream.ToArray(),
            ManifestResourceAttributes.Public));

        module.Write("LocalResources.dll");
    }
}