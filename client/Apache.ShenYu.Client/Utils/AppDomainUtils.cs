/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;

namespace Apache.ShenYu.Client.Utils
{
    /// <summary>
    /// appdomain utils
    /// </summary>
    public class AppDomainUtils
    {
        static AppDomainUtils()
        {
            EntryAssemblyName = Assembly.GetEntryAssembly().GetName().Name;
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            LocalAssemblies = LoadAssemblies();
        }

        public static List<Assembly> LocalAssemblies { get; private set; }

        public static string EntryAssemblyName { get; private set; }

        public static bool IsWindows { get; }

        private static List<Assembly> LoadAssemblies()
        {
            var list = new List<Assembly>();
            var deps = DependencyContext.Default;
            var libs = deps.CompileLibraries.Where(lib => !lib.Name.StartsWith("System.") && !lib.Name.StartsWith("Microsoft."));
            foreach (var lib in libs)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                    list.Add(assembly);
                }
                catch
                {
                }
            }
            return list;
        }
    }
}
