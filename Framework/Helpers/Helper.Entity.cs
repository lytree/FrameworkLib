﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework;

/// <summary>
/// 实体帮助类
/// </summary>
public static partial class Helper
{
	public static List<string> GetEntityPropertyNamesByAttribute<T, A>(bool inherit = false) where T : class where A : Attribute
	{
		Type classType = typeof(T);

		if (!classType.IsClass && !classType.IsAbstract)
		{
			throw new ArgumentException($"{classType.FullName} is not an class type.");
		}

		var propertyNames = classType.GetProperties()
			.Where(p => p.GetCustomAttribute<A>(inherit) != null)
			.Select(p => p.Name)
			.ToList();

		return propertyNames;
	}

	public static bool IsImplementInterface(Type type, Type interfaceType)
	{
		if (type == null || interfaceType == null || !interfaceType.IsInterface)
		{
			return false;
		}

		return interfaceType.IsAssignableFrom(type);
	}
}