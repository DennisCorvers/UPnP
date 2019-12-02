using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace UPnP.Utils
{
    internal static class FieldAccess
    {
        public static Func<TType, TMember> CreateGetter<TType, TMember>(this FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(TMember), new Type[] { typeof(TType) }, true);
            ILGenerator gen = getterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType != typeof(TType))
                    gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Func<TType, TMember>)getterMethod.CreateDelegate(typeof(Func<TType, TMember>));
        }
    }
}
