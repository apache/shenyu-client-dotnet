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

using System;
using Apache.ShenYu.AspNetCore.Services;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Registers;
using Apache.ShenYu.Client.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apache.ShenYu.AspNetCore.Utils
{
    public static class ShenyuCollectionExtensions
    {
        /// <summary>
        /// Add Shenyu register
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddShenyuRegister(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var shenyuOptions = new ShenyuOptions();
            configuration.GetSection(ShenyuOptions.Shenyu).Bind(shenyuOptions);

            services.Configure<ShenyuOptions>(configuration.GetSection(ShenyuOptions.Shenyu));
            services.AddHostedService<ShenyuStartupService>();

            if (shenyuOptions.Register.RegisterType.Equals(""))
            {
                services.AddSingleton<IShenyuRegister, ShenyuHttpRegister>();
            }

            switch (shenyuOptions.Register.RegisterType)
            {
                case Constants.RegisterType.Http:
                {
                    services.AddSingleton<IShenyuRegister, ShenyuHttpRegister>();
                    break;
                }
                case Constants.RegisterType.Zookeeper:
                {
                    services.AddSingleton<IShenyuRegister, ShenyuZookeeperRegister>();
                    break;
                }
                case Constants.RegisterType.Consul:
                {
                    services.AddSingleton<IShenyuRegister, ShenyuConsulRegister>();
                    break;
                }
                default:
                    throw new Exception($"not supported type {shenyuOptions.Register.RegisterType}");
            }
        }
    }
}
