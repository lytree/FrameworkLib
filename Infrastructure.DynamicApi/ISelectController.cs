﻿using System;
using System.Reflection;
using Infrastructure.DynamicApi.Helpers;
using Infrastructure.DynamicApi.Attributes;

namespace Infrastructure.DynamicApi;

public interface ISelectController
{
	bool IsController(Type type);
}

internal class DefaultSelectController : ISelectController
{
	public bool IsController(Type type)
	{
		var typeInfo = type.GetTypeInfo();

		if (!typeof(IDynamicApi).IsAssignableFrom(type) ||
			!typeInfo.IsPublic || typeInfo.IsAbstract || typeInfo.IsGenericType)
		{
			return false;
		}


		var attr = ReflectionHelper.GetSingleAttributeOrDefaultByFullSearch<DynamicApiAttribute>(typeInfo);

		if (attr == null)
		{
			return false;
		}

		if (ReflectionHelper.GetSingleAttributeOrDefaultByFullSearch<NonDynamicApiAttribute>(typeInfo) != null)
		{
			return false;
		}

		return true;
	}
}