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

using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Utils;

namespace Apache.ShenYu.Client.Registers
{
    public abstract class ShenyuAbstractRegister : IShenyuRegister
    {
        protected string BuildMetadataNodeName(MetaDataRegisterDTO metadata)
        {
            string nodeName;
            string rpcType = metadata.rpcType;

            if (Constants.RegisterRpcType.Http.Equals(rpcType) || Constants.RegisterRpcType.SpringCloud.Equals(rpcType))
            {
                nodeName = string.Join(Constants.SelectorJoinRule, metadata.contextPath,
                    metadata.ruleName.Replace(Constants.PathSeparator, Constants.SelectorJoinRule));
            }
            else
            {
                nodeName = RegisterPathConstants.BuildNodeName(metadata.serviceName, metadata.methodName);
            }

            return nodeName.StartsWith(Constants.PathSeparator) ? nodeName.Substring(1) : nodeName;
        }

        protected string BuildContextNodePath(string contextPath, string appName)
        {
            return string.IsNullOrEmpty(contextPath)
                ? appName
                : (contextPath.StartsWith("/")
                    ? contextPath.Substring(1)
                    : contextPath);
        }

        public abstract Task Init(ShenyuOptions shenyuOptions);
        public abstract Task PersistInterface(MetaDataRegisterDTO metadata);
        public abstract Task PersistURI(URIRegisterDTO registerDTO);
        public abstract Task Close();
    }
}
