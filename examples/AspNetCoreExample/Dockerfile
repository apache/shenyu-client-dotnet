# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed with
# this work for additional information regarding copyright ownership.
# The ASF licenses this file to You under the Apache License, Version 2.0
# (the "License"); you may not use this file except in compliance with
# the License.  You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# FROM mcr.microsoft.com/dotnet/aspnet:3.1-focal AS base
FROM mcr.microsoft.com/dotnet/sdk:3.1-focal AS build
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# FROM base AS final
WORKDIR /app
# COPY --from=publish /app/publish .
ADD ./bin/Release/netcoreapp3.1/ /app
ENTRYPOINT ["dotnet", "AspNetCoreExample.dll"]
