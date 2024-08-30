using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Utilities;

namespace Emscripten.Build.CppTasks
{
    public class StartAssemblyResolveInjection : Task
    {
        public override bool Execute()
        {
            AssemblyResolver.StartInject();
            return true;
        }
    }

    public class EndAssemblyResolveInjection : Task
    {
        public override bool Execute()
        {
            AssemblyResolver.EndInject();
            return true;
        }
    }

    public static class AssemblyResolver
    {
        static Assembly ResolveAssembly(object sender, ResolveEventArgs e) 
        {
            var assembly = new AssemblyName(e.Name);

            //var allowedAssemblyNames = new List<string>
            //{
            //    "Microsoft.VisualStudio.CPPTasks.Common"
            //};
    
            //if (!allowedAssemblyNames.Contains(assembly.Name))
            //{
            //    return null;
            //}

            // compare except for version
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var candicate = loadedAssemblies.Where(a =>
            {
                var name = a.GetName();

                return name.Name == assembly.Name
                    && name.GetPublicKeyToken().SequenceEqual(assembly.GetPublicKeyToken())
                    && name.CultureName == assembly.CultureName;
            }).ToArray();

            if (candicate.Length > 0)
            {
                return candicate[0];
            }

            return null;
        }

        public static void StartInject()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public static void EndInject()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
        }
    }
}
