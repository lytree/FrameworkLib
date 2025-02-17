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
using Framework.OSS.SDK.HuaweiCloud;
using OBS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.OSS.SDK.HuaweiCloud.Internal
{
    internal class HttpContext
    {

        private readonly IList<HttpResponseHandler> _handlers = new List<HttpResponseHandler>();
        public HttpContext(SecurityProvider sp, ObsConfig obsConfig)
        {
            SecurityProvider = sp;
            ObsConfig = obsConfig;
        }

        public string RedirectLocation
        {
            get;
            set;
        }

        public ObsConfig ObsConfig
        {
            get;
            set;
        }

        public SecurityProvider SecurityProvider
        {
            get;
            set;
        }

        public bool SkipAuth
        {
            get;
            set;
        }

        public AuthTypeEnum? AuthType
        {
            get;
            set;
        }

        public IList<HttpResponseHandler> Handlers
        {
            get { return _handlers; }
        }

        public AuthTypeEnum ChooseAuthType
        {
            get { return AuthType.HasValue ? AuthType.Value : ObsConfig.AuthType; }
        }

    }
}
