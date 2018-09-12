using System;
using System.Reflection;
using System.Text;

namespace Toe.Scripting
{
    public interface INamingConvention
    {
        string GetConstructorKey(ConstructorInfo methodInfo);
        string GetConstructorName(ConstructorInfo methodInfo);
        string GetGetterName(PropertyInfo methodInfo);
        string GetMemberKey(MemberInfo methodInfo);
        void GetMemberKey(MemberInfo methodInfo, StringBuilder sb);
        string GetMemberName(MemberInfo methodInfo);
        void GetMemberName(MemberInfo methodInfo, StringBuilder sb);
        string GetMethodKey(MethodInfo methodInfo);
        string GetMethodName(MethodInfo methodInfo);
        string GetSetterName(PropertyInfo methodInfo);
        string GetTypeKey(Type t);
        void GetTypeKey(Type t, StringBuilder sb);
        string GetTypeKey<T>();
        void GetTypeName(Type t, StringBuilder sb);
        string GetTypeName(Type t);
        string GetTypeName<T>();
    }
}