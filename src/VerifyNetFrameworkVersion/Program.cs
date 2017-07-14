using System;
using System.IO;
using System.Reflection;
#if NET462
using System.Runtime.Versioning;
#endif

namespace VerifyNetFrameworkVersion
{

	class Program
	{


		static int Main(string[] args)
		{

			string myName = "VerifyNetFrameworkVersion";
			string assemblyDir;

			if (args.Length < 1)
			{
				assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				Console.Error.WriteLine($"{myName}: no directory passed, using current directory: '{assemblyDir}'");
			}
			else
			{
				assemblyDir = args[0];
			}
			if (!Directory.Exists(assemblyDir))
			{
				Console.Error.WriteLine($"{myName}: directory does not exist: {assemblyDir}");
				return 1;
			}

			string[] dlls = Directory.GetFiles(assemblyDir, "*.dll");
			if (dlls.Length < 1)
			{
				Console.Error.WriteLine($"{myName}: no dlls found in {assemblyDir}");
				return 1;
			}

			Console.WriteLine($"{myName} analyzing assemblies:");
			foreach (var dll in dlls)
			{
				Assembly assembly = Assembly.LoadFrom(dll);
				string frameworkName = "NA";
				string frameworkDisplayName = "NA";
#if NET462
				TargetFrameworkAttribute targetFWA = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
				 frameworkName = targetFWA.FrameworkName;
				 frameworkDisplayName = targetFWA.FrameworkDisplayName;
#endif
				Console.WriteLine($"* {Path.GetFileName(dll)}, ImageRuntimeVersion:{assembly.ImageRuntimeVersion}, FrameworkName:{frameworkName}, FrameworkDisplayName:{frameworkDisplayName}");
			}
			return 0;
		}




	}
}
