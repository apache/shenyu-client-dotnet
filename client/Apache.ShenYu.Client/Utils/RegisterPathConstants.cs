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

namespace Apache.ShenYu.Client.Utils
{
    /// <summary>
    /// zookeeper register center.
    /// </summary>
    public class RegisterPathConstants
    {
        /// <summary>
        /// uri register path pattern.
        /// e.g. /shenyu/register/uri/{rpcType}/{context}/{urlInstance}
        /// </summary>
        public const string REGISTER_URI_INSTANCE_PATH = "/shenyu/register/uri/*/*/*";

        /// <summary>
        /// metadata register path pattern.
        /// e.g. /shenyu/register/metadata/{rpcType}/{context}/{metadata}
        /// </summary>
        public const string REGISTER_METADATA_INSTANCE_PATH = "/shenyu/register/metadata/*/*/*";

        /// <summary>
        /// root path of zookeeper register center.
        /// </summary>
        public const string ROOT_PATH = "/shenyu/register";

        /// <summary>
        /// constants of separator.
        /// </summary>
        private const string SEPARATOR = "/";

        /// <summary>
        /// Dot separator.
        /// </summary>
        private const string DOT_SEPARATOR = ".";

        /// <summary>
        /// build child path of "/shenyu/register/metadata/{rpcType}/".
        /// </summary>
        /// <param name="rpcType"> rpc type </param>
        /// <returns> path string </returns>
        public static string BuildMetaDataContextPathParent(string rpcType)
        {
            return String.Join(SEPARATOR, ROOT_PATH, "metadata", rpcType);
        }

        /// <summary>
        /// build child path of "/shenyu/register/metadata/{rpcType}/{contextPath}/".
        /// </summary>
        /// <param name="rpcType"> rpc type </param>
        /// <param name="contextPath"> context path </param>
        /// <returns> path string </returns>
        public static string BuildMetaDataParentPath(string rpcType,string contextPath)
        {
            return String.Join(SEPARATOR, ROOT_PATH, "metadata", rpcType, contextPath);
        }

        /// <summary>
        /// Build uri path string.
        /// build child path of "/shenyu/register/uri/{rpcType}/".
        /// </summary>
        /// <param name="rpcType"> the rpc type </param>
        /// <returns> the string </returns>
        public static string BuildURIContextPathParent(string rpcType)
        {
            return String.Join(SEPARATOR, ROOT_PATH, "uri", rpcType);
        }

        /// <summary>
        /// Build uri path string.
        /// build child path of "/shenyu/register/uri/{rpcType}/{contextPath}/".
        /// </summary>
        /// <param name="rpcType"> the rpc type </param>
        /// <param name="contextPath"> the context path </param>
        /// <returns> the string </returns>
        public static string BuildURIParentPath(string rpcType,string contextPath)
        {
            return String.Join(SEPARATOR, ROOT_PATH, "uri", rpcType, contextPath);
        }

        /// <summary>
        /// Build instance parent path string.
        /// build child path of "/shenyu/register/instance/
        /// </summary>
        /// <returns> the string </returns>
        public static string BuildInstanceParentPath()
        {
            return String.Join(SEPARATOR, ROOT_PATH, "instance");
        }

        /// <summary>
        /// Build real node string.
        /// </summary>
        /// <param name="nodePath"> the node path </param>
        /// <param name="nodeName"> the node name </param>
        /// <returns> the string </returns>
        public static string BuildRealNode(string nodePath,string nodeName)
        {
            return String.Join(SEPARATOR, nodePath, nodeName);
        }

        /// <summary>
        /// Build nacos instance service path string.
        /// build child path of "shenyu.register.service.{rpcType}".
        /// </summary>
        /// <param name="rpcType"> the rpc type </param>
        /// <returns> the string </returns>
        public static string BuildServiceInstancePath(string rpcType)
        {
            return String.Join(SEPARATOR, ROOT_PATH, "service", rpcType).Replace("/", DOT_SEPARATOR).Substring(1);
        }

        /// <summary>
        /// Build nacos config service path string.
        /// build child path of "shenyu.register.service.{rpcType}.{contextPath}".
        /// </summary>
        /// <param name="rpcType"> the rpc type </param>
        /// <param name="contextPath"> the context path </param>
        /// <returns> the string </returns>
        public static string BuildServiceConfigPath(string rpcType,string contextPath)
        {
            string serviceConfigPathOrigin = String.Join(SEPARATOR, ROOT_PATH, "service", rpcType, contextPath)
                .Replace("/", DOT_SEPARATOR)
                .Replace("*", "");

            string serviceConfigPathAfterSubstring = serviceConfigPathOrigin.Substring(1);
            if (serviceConfigPathAfterSubstring.EndsWith(".", StringComparison.Ordinal))
            {
                return serviceConfigPathAfterSubstring.Substring(0, serviceConfigPathAfterSubstring.Length - 1);
            }
            return serviceConfigPathAfterSubstring;
        }

        /// <summary>
        /// Build node name by DOT_SEPARATOR.
        /// </summary>
        /// <param name="serviceName"> the service name </param>
        /// <param name="methodName"> the method name </param>
        /// <returns> the string </returns>
        public static string buildNodeName(string serviceName,string methodName)
        {
            return String.Join(DOT_SEPARATOR, serviceName, methodName);
        }
    }
}
