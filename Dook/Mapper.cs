using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dook;
using Dook.Attributes;

public static class Mapper
{
    public static Dictionary<string,string> GetTableMapping<T>()
    {
        //getting properties in a specific order
        Dictionary<string,string> TableMapping = new Dictionary<string,string>();
        TypeInfo typeInfo = typeof(T).GetTypeInfo();
        List<PropertyInfo> properties = new List<PropertyInfo> { typeInfo.GetProperty("Id") }; //TODO: this is because Join reader always assume Id comes first
        properties.AddRange(typeof(T).GetTypeInfo().GetProperties().Where(p => p.Name != "Id").OrderBy(p => p.Name).ToList());
        foreach (PropertyInfo p in properties)
        {
            NotMappedAttribute nm = p.GetCustomAttribute<NotMappedAttribute>();
            if (nm == null)
            {
                ColumnNameAttribute cma = p.GetCustomAttribute<ColumnNameAttribute>();
                TableMapping.Add(p.Name, cma != null ? cma.ColumnName : p.Name);
            }
        }
        return TableMapping;
    }
}