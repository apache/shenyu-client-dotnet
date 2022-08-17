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

namespace Apache.ShenYu.Client.Utils{

/// <summary>
	/// The type Context path utils.
	/// </summary>
	public class ContextPathUtils
	{

		/// <summary>
		/// Build context path string.
		/// </summary>
		/// <param name="contextPath"> the context path </param>
		/// <param name="appName"> the app name </param>
		/// <returns> the string </returns>
		public static string BuildContextPath(string contextPath, string appName)
		{
			return UriUtils.RepairData(string.IsNullOrEmpty(contextPath) ? appName : contextPath);
		}

		/// <summary>
		/// Build real node string.
		/// </summary>
		/// <param name="contextPath"> the context path </param>
		/// <param name="appName"> the app name </param>
		/// <returns> the string </returns>
		public static string BuildRealNode(string contextPath, string appName)
		{
			return UriUtils.RemovePrefix(string.IsNullOrEmpty(contextPath) ? appName : contextPath);
		}
	}

}
