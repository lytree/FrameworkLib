﻿using System;

namespace Infrastructure.DynamicApi;

/// <summary>
/// 不格式化结果数据
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class NonFormatResultAttribute : Attribute
{
}