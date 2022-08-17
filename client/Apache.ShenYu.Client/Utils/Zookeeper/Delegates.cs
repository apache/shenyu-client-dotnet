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
using org.apache.zookeeper;

namespace Apache.ShenYu.Client.Utils
{
    /// <summary>
    /// connect state changehander params
    /// </summary>
    public class ConnectionStateChangeArgs
    {
        /// <summary>
        /// connect state
        /// </summary>
        public Watcher.Event.KeeperState State { get; set; }
    }

    /// <summary>
    /// connectstat change delegate
    /// </summary>
    /// <param name="client"></param>
    /// <param name="args"></param>
    public delegate Task ConnectionStateChangeHandler(IZookeeperClient client, ConnectionStateChangeArgs args);
}
