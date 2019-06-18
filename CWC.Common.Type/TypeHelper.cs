using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CWC.Common.Type
{
    public class TypeHelper
    {
        public static Dictionary<string, object> ObjectToDictionary(object value)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            bool flag = value != null;
            if (flag)
            {
                PropertyHelper[] properties = PropertyHelper.GetProperties(value);
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyHelper propertyHelper = properties[i];
                    dictionary.Add(propertyHelper.Name, propertyHelper.GetValue(value));
                }
            }
            return dictionary;
        }

        public static Dictionary<string, object> ObjectToDictionaryUncached(object value)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            bool flag = value != null;
            if (flag)
            {
                PropertyHelper[] properties = PropertyHelper.GetProperties(value);
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyHelper propertyHelper = properties[i];
                    dictionary.Add(propertyHelper.Name, propertyHelper.GetValue(value));
                }
            }
            return dictionary;
        }

        public static void AddAnonymousObjectToDictionary(IDictionary<string, object> dictionary, object value)
        {
            foreach (KeyValuePair<string, object> current in TypeHelper.ObjectToDictionary(value))
            {
                dictionary.Add(current);
            }
        }

        public static bool IsAnonymousType(Type type)
        {
            bool flag = type == null;
            if (flag)
            {
                throw new ArgumentNullException("type");
            }
            bool flag2 = !Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false) || !type.IsGenericType || !type.Name.Contains("AnonymousType") || (!type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) && !type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase));
            bool result;
            if (flag2)
            {
                result = false;
            }
            else
            {
                int attributes = (int)type.Attributes;
                result = true;
            }
            return result;
        }
    }
}