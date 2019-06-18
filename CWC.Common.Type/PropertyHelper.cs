using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CWC.Common.Type
{
    internal class PropertyHelper
    {
        private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

        [CompilerGenerated]
        [Serializable]
        private sealed class <>c
		{
			public static readonly PropertyHelper.<>c<>9 = new PropertyHelper.<>c();

        public static Func<PropertyInfo, bool> <>9__18_0;

			internal bool <GetProperties>b__18_0(PropertyInfo prop)
        {
            bool flag = prop.GetIndexParameters().Length == 0;
            return flag && prop.GetMethod != null;
        }
    }

    private static ConcurrentDictionary<Type, PropertyHelper[]> _reflectionCache = new ConcurrentDictionary<Type, PropertyHelper[]>();

    private static readonly MethodInfo _callPropertyGetterOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetter", BindingFlags.Static | BindingFlags.NonPublic);

    private static readonly MethodInfo _callPropertyGetterByReferenceOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetterByReference", BindingFlags.Static | BindingFlags.NonPublic);

    private static readonly MethodInfo _callPropertySetterOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertySetter", BindingFlags.Static | BindingFlags.NonPublic);

    private Func<object, object> _valueGetter;

    public virtual string Name
    {
        get;
        protected set;
    }

    public PropertyHelper(PropertyInfo property)
    {
        this.Name = property.Name;
        this._valueGetter = PropertyHelper.MakeFastPropertyGetter(property);
    }


}
