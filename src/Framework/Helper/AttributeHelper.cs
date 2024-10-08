﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework;

public static partial class Helper
{
    public static string GetAttributeDisplayName<T>(string propertyDisplayName)
    {
        return (TypeDescriptor.GetProperties(typeof(T))[propertyDisplayName].Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute).DisplayName;
    }

    public static string GetAttributeDisplay<T>(string propertyDisplayName)
    {
        return (TypeDescriptor.GetProperties(typeof(T))[propertyDisplayName].Attributes[typeof(DisplayAttribute)] as DisplayAttribute).Name;
    }
}
