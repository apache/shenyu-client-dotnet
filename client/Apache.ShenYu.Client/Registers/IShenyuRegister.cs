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
    public interface IShenyuRegister
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="shenyuOptions"></param>
        Task Init(ShenyuOptions shenyuOptions);

        /// <summary>
        ///
        /// </summary>
        /// <param name="metadata"></param>
        Task PersistInterface(MetaDataRegisterDTO metadata);

        /// <summary>
        /// persist URI.
        /// </summary>
        /// <param name="registerDTO"></param>
        Task PersistURI(URIRegisterDTO registerDTO);

        /// <summary>
        /// Close.
        /// </summary>
        Task Close();
    }
}
