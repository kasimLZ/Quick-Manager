using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Common.Linq
{
    internal class ClassFactory
    {
        // Fields
        private int classCount;
        private Dictionary<Signature, Type> classes;
        public static readonly ClassFactory Instance = new ClassFactory();
        private ModuleBuilder module;
        private ReaderWriterLock rwLock;

        // Methods
        private ClassFactory()
        {
            AssemblyName name = new AssemblyName("DynamicClasses");
            module = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("Module");
            classes = new Dictionary<Signature, Type>();
            rwLock = new ReaderWriterLock();
        }

        private Type CreateDynamicClass(DynamicProperty[] properties)
        {
            Type type;
            LockCookie lockCookie = this.rwLock.UpgradeToWriterLock(-1);
            try
            {
                string name = "DynamicClass" + (this.classCount + 1);
                TypeBuilder tb = this.module.DefineType(name, TypeAttributes.Public, typeof(DynamicClass));
                FieldInfo[] fields = this.GenerateProperties(tb, properties);
                GenerateEquals(tb, fields);
                GenerateGetHashCode(tb, fields);
                classCount++;
                type = tb.CreateType();
            }
            finally
            {
                this.rwLock.DowngradeFromWriterLock(ref lockCookie);
            }
            return type;
        }

        private void GenerateEquals(TypeBuilder tb, FieldInfo[] fields)
        {
            Type[] parameterTypes = new Type[] { typeof(object) };
            ILGenerator iLGenerator = tb.DefineMethod("Equals", MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, typeof(bool), parameterTypes).GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(tb);
            Label label = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Isinst, tb);
            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Brtrue_S, label);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Ret);
            iLGenerator.MarkLabel(label);
            foreach (FieldInfo info in fields)
            {
                Type fieldType = info.FieldType;
                Type[] typeArguments = new Type[] { fieldType };
                Type type2 = typeof(EqualityComparer<>).MakeGenericType(typeArguments);
                label = iLGenerator.DefineLabel();
                iLGenerator.EmitCall(OpCodes.Call, type2.GetMethod("get_Default"), null);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, info);
                iLGenerator.Emit(OpCodes.Ldloc, local);
                iLGenerator.Emit(OpCodes.Ldfld, info);
                Type[] types = new Type[] { fieldType, fieldType };
                iLGenerator.EmitCall(OpCodes.Callvirt, type2.GetMethod("Equals", types), null);
                iLGenerator.Emit(OpCodes.Brtrue_S, label);
                iLGenerator.Emit(OpCodes.Ldc_I4_0);
                iLGenerator.Emit(OpCodes.Ret);
                iLGenerator.MarkLabel(label);
            }
            iLGenerator.Emit(OpCodes.Ldc_I4_1);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields)
        {
            ILGenerator iLGenerator = tb.DefineMethod("GetHashCode", MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, typeof(int), Type.EmptyTypes).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            foreach (FieldInfo info in fields)
            {
                Type fieldType = info.FieldType;
                Type[] typeArguments = new Type[] { fieldType };
                Type type2 = typeof(EqualityComparer<>).MakeGenericType(typeArguments);
                iLGenerator.EmitCall(OpCodes.Call, type2.GetMethod("get_Default"), null);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, info);
                Type[] types = new Type[] { fieldType };
                iLGenerator.EmitCall(OpCodes.Callvirt, type2.GetMethod("GetHashCode", types), null);
                iLGenerator.Emit(OpCodes.Xor);
            }
            iLGenerator.Emit(OpCodes.Ret);
        }

        private FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
        {
            FieldInfo[] infoArray = new FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                DynamicProperty property = properties[i];
                FieldBuilder field = tb.DefineField("_" + property.Name, property.Type, FieldAttributes.Private);
                PropertyBuilder builder2 = tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.Type, null);
                MethodBuilder mdBuilder = tb.DefineMethod("get_" + property.Name, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, property.Type, Type.EmptyTypes);
                ILGenerator iLGenerator = mdBuilder.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, field);
                iLGenerator.Emit(OpCodes.Ret);
                Type[] parameterTypes = new Type[] { property.Type };
                MethodBuilder builder4 = tb.DefineMethod("set_" + property.Name, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, null, parameterTypes);
                ILGenerator generator2 = builder4.GetILGenerator();
                generator2.Emit(OpCodes.Ldarg_0);
                generator2.Emit(OpCodes.Ldarg_1);
                generator2.Emit(OpCodes.Stfld, field);
                generator2.Emit(OpCodes.Ret);
                builder2.SetGetMethod(mdBuilder);
                builder2.SetSetMethod(builder4);
                infoArray[i] = field;
            }
            return infoArray;
        }

        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
        {
            Type type2;
            rwLock.AcquireReaderLock(-1);
            try
            {
                Type type;
                Signature key = new Signature(properties);
                if (!classes.TryGetValue(key, out type))
                {
                    type = CreateDynamicClass(key.properties);
                    classes.Add(key, type);
                }
                type2 = type;
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
            return type2;
        }
    }


}
