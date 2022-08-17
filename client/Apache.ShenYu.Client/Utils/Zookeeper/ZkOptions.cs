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
using System.Text;

namespace Apache.ShenYu.Client.Utils
{
    public class ZkOptions
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ZkOptions()
        {
            ConnectionSpanTimeout = TimeSpan.FromMilliseconds(60000);
            SessionSpanTimeout = TimeSpan.FromMilliseconds(60000);
            OperatingSpanTimeout = TimeSpan.FromMilliseconds(60000);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionTimeout"></param>
        /// <param name="operatingTimeout"></param>
        /// <param name="sessionTimeout"></param>
        protected ZkOptions(int connectionTimeout, int operatingTimeout, int sessionTimeout)
        {
            ConnectionSpanTimeout = TimeSpan.FromMilliseconds(connectionTimeout);
            SessionSpanTimeout = TimeSpan.FromMilliseconds(sessionTimeout);
            OperatingSpanTimeout = TimeSpan.FromMilliseconds(operatingTimeout);
        }

        /// <summary>
        /// create ZooKeeper client
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ZkOptions(string connectionString) : this()
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        /// <summary>
        /// create ZooKeeper client
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="connectionTimeout"></param>
        /// <param name="operatingTimeout"></param>
        /// <param name="sessionTimeout"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ZkOptions(string connectionString
            , int connectionTimeout
            , int operatingTimeout
            , int sessionTimeout) : this(connectionTimeout, operatingTimeout, sessionTimeout)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        public ZkOptions SetConnectionTimeout(int connectionTimeout)
        {
            this.ConnectionSpanTimeout = TimeSpan.FromMilliseconds(connectionTimeout);
            return this;
        }

        public ZkOptions SetSessionTimeout(int sessionTimeout)
        {
            this.SessionSpanTimeout = TimeSpan.FromMilliseconds(sessionTimeout);
            return this;
        }

        public ZkOptions SetOperatingTimeout(int operatingTimeout)
        {
            this.OperatingSpanTimeout = TimeSpan.FromMilliseconds(operatingTimeout);
            return this;
        }

        public ZkOptions SetMaxRetry(int maxRetry)
        {
            this.RetryCount = maxRetry;
            return this;
        }

        public ZkOptions SetSessionPassword(string sessionPassword)
        {
            this.SessionPasswd = sessionPassword;
            return this;
        }

        /// <summary>
        /// connect string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// retry count
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// readonly
        /// </summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// session Id。
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// session password
        /// </summary>
        public string SessionPasswd { get; set; }

        public byte[] SessionPasswdBytes
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SessionPasswd))
                {
                    return Encoding.UTF8.GetBytes(SessionPasswd);
                }
                return null;
            }
        }

        /// <summary>
        /// log to file options
        /// </summary>
        public bool LogToFile { get; set; } = false;

        /// <summary>
        /// base root path
        /// </summary>
        public string BaseRoutePath { get; set; }

        #region Internal

        /// <summary>
        /// wait zooKeeper connect span time
        /// </summary>
        internal TimeSpan ConnectionSpanTimeout { get; set; }

        /// <summary>
        /// execute zooKeeper handler retry span waittime
        /// </summary>
        internal TimeSpan OperatingSpanTimeout { get; set; }

        /// <summary>
        /// zookeeper session timeout
        /// </summary>
        internal TimeSpan SessionSpanTimeout { get; set; }

        #endregion Internal
    }
}
