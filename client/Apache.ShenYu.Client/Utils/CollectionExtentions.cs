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
using System.Threading.Tasks;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

namespace Apache.ShenYu.Client.Utils
{
    public static class CollectionExtentions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static async Task CreateWithParentAsync(
            this ZooKeeper zooKeeper,
            string path,
            byte[] data,
            List<ACL> acl,
            CreateMode createMode
            )
        {
            var paths = path.Split('/');
            var cur = "";
            foreach (var item in paths)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                cur += $"/{item}";
                var existStat = await zooKeeper.existsAsync(cur, null);
                if (existStat != null)
                {
                    continue;
                }

                if (cur.Equals(path))
                {
                    await zooKeeper.createAsync(cur, data, acl, createMode);
                }
                else
                {
                    await zooKeeper.createAsync(cur, null, acl, createMode);
                }
            }
        }
    }
}