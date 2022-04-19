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

namespace Apache.ShenYu.Client.Attibutes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ShenyuClientAttribute : Attribute
    {
        public string Path { get; set; }
        public string RuleName { get; set; }
        public string Desc { get; set; }
        public bool Enabled { get; set; }
        public bool RegisterMetaData { get; set; }

        public ShenyuClientAttribute(string path)
        {
            this.Path = path;
        }
    }
}

