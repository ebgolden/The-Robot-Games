using System.Collections.Generic;
using System.Reflection;
using System;

public static class TypeTools
{
    public static List<Type> getTypesOfBaseType(Type baseType)
    {
        if (baseType.IsGenericType)
            baseType = baseType.GetGenericTypeDefinition();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> types = new List<Type>();
        foreach (Assembly assembly in assemblies)
            foreach (Type type in assembly.GetTypes())
                if (type.BaseType != null && type.BaseType.ToString() == baseType.ToString())
                    types.Add(type);
        return types;
    }
}