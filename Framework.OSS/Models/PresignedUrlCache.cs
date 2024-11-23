﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.OSS.Models
{
    class PresignedUrlCache
    {
        public string Name { get; set; }

        public string BucketName { get; set; }

        public long CreateTime { get; set; }

        public string Url { get; set; }

        public PresignedObjectType Type { get; set; }
    }
}
