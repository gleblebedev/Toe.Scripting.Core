using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Toe.Scripting
{
    public class NamingConvention : INamingConvention
    {
        public static INamingConvention Default = new NamingConvention();

        private readonly Dictionary<Type, string> _keys = new Dictionary<Type, string>
        {
            {typeof(string), "string"},
            {typeof(object), "object"},
            {typeof(char), "char"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(decimal), "decimal"},
            {typeof(float), "float"},
            {typeof(double), "double"}
        };

        private readonly Dictionary<Type, string> _names = new Dictionary<Type, string>();

        public string GetConstructorKey(ConstructorInfo methodInfo)
        {
            var sb = new StringBuilder();
            GetTypeKey(methodInfo.DeclaringType, sb);
            sb.Append(".");
            sb.Append(methodInfo.DeclaringType.Name);
            sb.Append("(");
            var separtor = "";
            foreach (var param in methodInfo.GetParameters())
            {
                sb.Append(separtor);
                separtor = ",";
                GetTypeKey(param.ParameterType, sb);
            }

            sb.Append(")");
            return sb.ToString();
        }

        public string GetMethodKey(MethodInfo methodInfo)
        {
            var sb = new StringBuilder();
            GetMemberKey(methodInfo, sb);
            sb.Append("(");
            var separtor = "";
            foreach (var param in methodInfo.GetParameters())
            {
                sb.Append(separtor);
                separtor = ",";
                GetTypeKey(param.ParameterType, sb);
            }

            sb.Append(")");
            return sb.ToString();
        }

        public string GetConstructorName(ConstructorInfo methodInfo)
        {
            var displayName =
                Attribute.GetCustomAttribute(methodInfo, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null) return displayName.DisplayName;
            var sb = new StringBuilder();
            GetTypeName(methodInfo.DeclaringType, sb);
            sb.Append(".");
            sb.Append(methodInfo.DeclaringType.Name);
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0)
                sb.Append("()");
            else if (parameters.Length == 1)
                sb.Append("(" + parameters[0].Name + ")");
            else
                sb.Append("(...)");
            return sb.ToString();
        }

        public string GetMethodName(MethodInfo methodInfo)
        {
            var displayName =
                Attribute.GetCustomAttribute(methodInfo, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null) return displayName.DisplayName;
            var sb = new StringBuilder();
            GetMemberName(methodInfo, sb);
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0)
                sb.Append("()");
            else if (parameters.Length == 1)
                sb.Append("(" + parameters[0].Name + ")");
            else
                sb.Append("(...)");
            return sb.ToString();
        }

        public string GetMemberKey(MemberInfo methodInfo)
        {
            var sb = new StringBuilder();
            GetMemberKey(methodInfo, sb);
            return sb.ToString();
        }

        public string GetMemberName(MemberInfo methodInfo)
        {
            var displayName =
                Attribute.GetCustomAttribute(methodInfo, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null) return displayName.DisplayName;
            var sb = new StringBuilder();
            GetMemberName(methodInfo, sb);
            return sb.ToString();
        }

        public void GetMemberKey(MemberInfo methodInfo, StringBuilder sb)
        {
            GetTypeKey(methodInfo.DeclaringType, sb);
            sb.Append(".");
            sb.Append(methodInfo.Name);
        }

        public void GetMemberName(MemberInfo methodInfo, StringBuilder sb)
        {
            var displayName =
                Attribute.GetCustomAttribute(methodInfo, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null)
            {
                sb.Append(displayName.DisplayName);
                return;
            }

            GetTypeName(methodInfo.DeclaringType, sb);
            sb.Append(".");
            sb.Append(methodInfo.Name);
        }

        public string GetGetterName(PropertyInfo methodInfo)
        {
            var getter = methodInfo.GetGetMethod();

            var displayName =
                Attribute.GetCustomAttribute(getter, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null) return displayName.DisplayName;
            var sb = new StringBuilder();
            sb.Append("Get ");
            GetMemberName(methodInfo, sb);
            return sb.ToString();
        }

        public string GetSetterName(PropertyInfo methodInfo)
        {
            var setter = methodInfo.GetSetMethod();

            var displayName =
                Attribute.GetCustomAttribute(setter, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null) return displayName.DisplayName;
            var sb = new StringBuilder();
            sb.Append("Set ");
            GetMemberName(methodInfo, sb);
            return sb.ToString();
        }

        public string GetTypeKey<T>()
        {
            string key;
            if (_keys.TryGetValue(typeof(T), out key))
                return key;
            var sb = new StringBuilder();
            GetTypeKey(typeof(T), sb);
            key = sb.ToString();
            _keys.Add(typeof(T), key);
            return key;
        }

        public string GetTypeKey(Type t)
        {
            string key;
            if (_keys.TryGetValue(t, out key))
                return key;
            var sb = new StringBuilder();
            GetTypeKey(t, sb);
            key = sb.ToString();
            _keys.Add(t, key);
            return sb.ToString();
        }

        public void GetTypeKey(Type t, StringBuilder sb)
        {
            if (t.IsArray)
            {
                GetTypeKey(t.GetElementType(), sb);
                sb.Append("[");
                for (var rank = 1; rank < t.GetArrayRank(); ++rank) sb.Append(",");

                sb.Append("]");
                return;
            }

            string key;
            if (_keys.TryGetValue(t, out key))
            {
                sb.Append(key);
                return;
            }

            if (!string.IsNullOrEmpty(t.Namespace))
            {
                sb.Append(t.Namespace);
                sb.Append(".");
            }

            if (t.GenericTypeArguments.Length > 0)
            {
                sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
                sb.Append("<");
                var separator = "";
                foreach (var argument in t.GenericTypeArguments)
                {
                    sb.Append(separator);
                    separator = ",";
                    GetTypeKey(argument, sb);
                }

                sb.Append(">");
            }
            else
            {
                sb.Append(t.Name);
            }
        }

        public string GetTypeName<T>()
        {
            string name;
            if (_names.TryGetValue(typeof(T), out name)) return name;
            var sb = new StringBuilder();
            GetTypeName(typeof(T), sb);
            name = sb.ToString();
            _names.Add(typeof(T), name);
            return name;
        }

        public string GetTypeName(Type t)
        {
            string name;
            if (_names.TryGetValue(t, out name)) return name;

            var sb = new StringBuilder();
            GetTypeName(t, sb);
            name = sb.ToString();
            _names.Add(t, name);
            return name;
        }

        public void GetTypeName(Type t, StringBuilder sb)
        {
            string name;
            if (_names.TryGetValue(t, out name))
            {
                sb.Append(name);
                return;
            }

            var displayName = Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayName != null)
            {
                sb.Append(displayName.DisplayName);
                return;
            }

            if (t.GenericTypeArguments.Length > 0)
            {
                sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
                sb.Append("<");
                var separator = "";
                foreach (var argument in t.GenericTypeArguments)
                {
                    sb.Append(separator);
                    separator = ",";
                    GetTypeName(argument, sb);
                }

                sb.Append(">");
            }
            else
            {
                sb.Append(t.Name);
            }
        }
    }
}