using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Kogase.Core
{
    public static class FormEncoding
    {
        public static string ToForm<T>(this T obj)
        {
            if (obj == null) throw new ArgumentException("Can't form-encode null");

            var properties = typeof(T).GetProperties();

            if (properties.Length == 0) throw new ArgumentException("Can't form-encode type without public properties");

            var formParams = new Dictionary<string, string>();

            foreach (var property in properties)
            {
                var name = GetMemberName(property);

                if (IsPropertyIgnored(property)) continue;

                var value = property.GetValue(obj);

                if (TryEncodeProperty(property, value, out var encodedValue))
                    formParams.Add(Uri.EscapeDataString(name), Uri.EscapeDataString(encodedValue));
            }

            return string.Join("&", formParams.Select(p => $"{p.Key}={p.Value}").ToArray());
        }

        static string GetMemberName(MemberInfo member)
        {
            var name = member.Name;
            var nameAttribute = member.GetCustomAttribute<DataMemberAttribute>();

            if (nameAttribute != null && !string.IsNullOrEmpty(nameAttribute.Name)) name = nameAttribute.Name;

            return name;
        }

        static bool TryEncodeProperty(PropertyInfo property, object value, out string encodedValue)
        {
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (value == null)
            {
                encodedValue = null;

                return false;
            }

            if (type == typeof(DateTime))
            {
                encodedValue = (value is DateTime d ? d : default).ToString("O");

                return true;
            }

            if (type == typeof(string) || type.IsPrimitive)
            {
                encodedValue = value.ToString();

                return true;
            }

            if (type.IsEnum)
            {
                encodedValue = GetMemberName(type.GetMember(Enum.GetName(type, value) ?? "")[0]);

                return true;
            }

            throw new ArgumentException("Can't form-encode unsupported type: " + property.PropertyType.Name);
        }

        static bool IsPropertyIgnored(MemberInfo property)
        {
            return property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null;
        }
    }
}