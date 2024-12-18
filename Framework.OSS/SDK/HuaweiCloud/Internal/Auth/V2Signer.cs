﻿/*----------------------------------------------------------------------------------
// Copyright 2019 Huawei Technologies Co.,Ltd.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License.  You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations under the License.
//----------------------------------------------------------------------------------*/
using Microsoft.Extensions.Logging;

namespace Framework.OSS.SDK.HuaweiCloud.Internal.Auth
{
    internal class V2Signer : AbstractSigner
    {
        private static readonly ILogger _logger = OSSServiceFactory.CreateLogger<V2Signer>();
        private static V2Signer instance = new V2Signer();

        private V2Signer()
        {

        }

        public static Signer GetInstance()
        {
            return instance;
        }

        protected override string GetAuthPrefix()
        {
            return "AWS";
        }

    }
}
