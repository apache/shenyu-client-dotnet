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
	/// uri util.
	/// </summary>
	public class UriUtils
	{

		private const string PRE_FIX = "/";

        /// <summary>
        /// create URI <seealso cref="URI"/>.
        /// </summary>
        /// <param name="uri"> uri string eg:/fallback </param>
        /// <returns> created <seealso cref="URI"/> from uri </returns>
        public static Uri CreateUri(string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                return new Uri(uri);
            }
            return null;
        }

        /// <summary>
        /// Repair data string.
        /// </summary>
        /// <param name="name"> the name </param>
        /// <returns> the string </returns>
        public static string RepairData(string name)
		{
			return name.StartsWith(PRE_FIX, StringComparison.Ordinal) ? name : PRE_FIX + name;
		}

		/// <summary>
		/// Remove prefix string.
		/// </summary>
		/// <param name="name"> the name </param>
		/// <returns> the string </returns>
		public static string RemovePrefix(string name)
		{
			return name.StartsWith(PRE_FIX, StringComparison.Ordinal) ? name.Substring(1) : name;
		}

        /// <summary>
        /// Remove ends string.
        /// </summary>
        /// <param name="name"> the name </param>
        /// <returns> the string </returns>
        public static string RemoveSuffix(string name)
        {
            return name.EndsWith(PRE_FIX, StringComparison.Ordinal) ? name.Substring(0,name.Length-1) : name;
        }

        /// <summary>
        /// Get the path of uri with parameters.
        /// </summary>
        /// <param name="uri"> the uri. </param>
        /// <returns> absolute uri string with parameters. </returns>
        public static string GetPathWithParams(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }
            // string @params = string.IsNullOrEmpty(uri.PathAndQuery) ? "" : "?" + uri.PathAndQuery;
            // return uri.AbsoluteUri + @params;
            return uri.AbsoluteUri;
        }
    }

}
