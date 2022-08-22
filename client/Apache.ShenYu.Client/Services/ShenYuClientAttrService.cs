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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Apache.ShenYu.Client.Attributes;
using Apache.ShenYu.Client.Utils;

namespace Apache.ShenYu.Client.Services
{
    public class ShenYuClientAttrService
    {
        /// <summary>
        /// get all have ShenyuClientAttribute info by class + method
        /// </summary>
        /// <returns></returns>
        public static List<ShenYuClientAttrModel> GetShenYuClientAttrClassList()
        {
            List<ShenYuClientAttrModel> list = new List<ShenYuClientAttrModel>();
            var typeArray = AppDomainUtils.LocalAssemblies.Select(m => m.GetTypes().Where(t =>
                t.IsClass
                && !t.IsAbstract
                && !t.GetTypeInfo().IsGenericTypeDefinition
                && t.GetCustomAttributes<ShenyuClientAttribute>().Any()))
                .Where(m => m.Count() > 0);
            foreach (var types in typeArray)
            {
                //select all methods
                foreach (var type in types)
                {
                    ShenYuClientAttrModel shenYuClientAttrModel = new ShenYuClientAttrModel();
                    shenYuClientAttrModel.ClientAttrClass = type;
                    shenYuClientAttrModel.ClientAttr = type.GetCustomAttribute<ShenyuClientAttribute>();
                    var actions = type.GetMethods().Where(method => method.IsPublic
                    && method.GetCustomAttributes(typeof(ShenyuClientAttribute), true).Any());
                    if (actions.Any())
                    {
                        shenYuClientAttrModel.MethodClientAttrList = new List<ShenyuClientAttribute>();
                        foreach (var method in actions)
                        {
                            var attrShenYuMethod = method.GetCustomAttribute<ShenyuClientAttribute>();
                            shenYuClientAttrModel.MethodClientAttrList.Add(attrShenYuMethod);
                        }
                    }
                    list.Add(shenYuClientAttrModel);
                }
            }
            return list;
        }

        public class ShenYuClientAttrModel
        {
            public Type ClientAttrClass { get; set; }

            public ShenyuClientAttribute ClientAttr { get; set; }

            public List<ShenyuClientAttribute> MethodClientAttrList { get; set; }
        }
    }
}
