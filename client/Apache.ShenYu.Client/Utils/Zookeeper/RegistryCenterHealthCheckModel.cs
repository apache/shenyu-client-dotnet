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

namespace Apache.ShenYu.Client.Utils
{
    public class RegistryCenterHealthCheckModel
    {
        public RegistryCenterHealthCheckModel()
        {
            UnHealthTimes = 0;
        }

        public RegistryCenterHealthCheckModel(bool isHealth)
        {
            IsHealth = isHealth;
        }

        public RegistryCenterHealthCheckModel(bool isHealth, int unHealthTimes)
        {
            IsHealth = isHealth;
            UnHealthTimes = unHealthTimes;
        }

        public bool IsHealth { get; set; }

        public int UnHealthTimes { get; set; }

        public HealthTypeEnum? HealthType { get; set; }

        public string UnHealthReason { get; set; }

        public void SetHealth()
        {
            IsHealth = true;
            UnHealthTimes = 0;
            UnHealthReason = null;
            HealthType = HealthTypeEnum.Connected;
        }

        public void SetUnHealth(HealthTypeEnum healthType, string unHealthReason)
        {
            IsHealth = false;
            UnHealthTimes += 1;
            UnHealthReason = unHealthReason;
            HealthType = healthType;
        }
    }
}
