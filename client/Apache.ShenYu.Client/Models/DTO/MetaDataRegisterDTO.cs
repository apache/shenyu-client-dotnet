/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace Apache.ShenYu.Client.Models.DTO
{
    public class MetaDataRegisterDTO
    {
        public String appName { get; set; }

        public String contextPath { get; set; }

        public String path { get; set; }

        public String pathDesc { get; set; }

        public String rpcType { get; set; }

        public String serviceName { get; set; }

        public String methodName { get; set; }

        public String ruleName { get; set; }

        public String parameterTypes { get; set; }

        public String rpcExt { get; set; }

        public bool enabled { get; set; }

        public String host { get; set; }

        public int port { get; set; }

        public List<String> pluginNames { get; set; }

        public bool registerMetaData { get; set; }

        public long timeMillis { get; set; }
    }
}
