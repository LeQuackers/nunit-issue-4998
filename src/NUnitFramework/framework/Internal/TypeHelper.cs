// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework.Interfaces;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// TypeHelper provides static methods that operate on Types.
    /// </summary>
    public static class TypeHelper
    {
        private const int STRING_MAX = 40;
        private const int STRING_LIMIT = STRING_MAX - 3;
        private const string THREE_DOTS = "...";

        /// <summary>
        /// Gets the display name for a Type as used by NUnit.
        /// </summary>
        /// <param name="type">The Type for which a display name is needed.</param>
        /// <returns>The display name for the Type</returns>
        public static string GetDisplayName(Type type)
        {
            if (type.IsGenericParameter)
                return type.Name;

            if (type.IsGenericType)
            {
                string name = type.FullName();
                int index = name.IndexOf('[');
                if (index >= 0)
                    name = name.Substring(0, index);

                index = name.LastIndexOf('.');
                if (index >= 0)
                    name = name.Substring(index + 1);

                var genericArguments = type.GetGenericArguments();
                var currentArgument = 0;

                StringBuilder sb = new StringBuilder();

                bool firstClassSeen = false;
                foreach (string nestedClass in name.Tokenize('+'))
                {
                    if (firstClassSeen)
                        sb.Append('+');

                    firstClassSeen = true;

                    index = nestedClass.IndexOf('`');
                    if (index >= 0)
                    {
                        var nestedClassName = nestedClass.Substring(0, index);
                        sb.Append(nestedClassName);
                        sb.Append('<');

                        var argumentCount = Int32.Parse(nestedClass.Substring(index + 1));
                        for (int i = 0; i < argumentCount; i++)
                        {
                            if (i > 0)
                                sb.Append(',');

                            sb.Append(GetDisplayName(genericArguments[currentArgument++]));
                        }
                        sb.Append('>');
                    }
                    else
                    {
                        sb.Append(nestedClass);
                    }
                }

                return sb.ToString();
            }

            string typeFullName = type.FullName();
            int lastdot = typeFullName.LastIndexOf('.');
            return lastdot >= 0
                ? typeFullName.Substring(lastdot + 1)
                : typeFullName;
        }

        /// <summary>
        /// Gets the display name for a Type as used by NUnit.
        /// </summary>
        /// <param name="type">The Type for which a display name is needed.</param>
        /// <param name="arglist">The arglist provided.</param>
        /// <returns>The display name for the Type</returns>
        public static string GetDisplayName(Type type, object?[]? arglist)
        {
            string baseName = GetDisplayName(type);
            if (arglist is null || arglist.Length == 0)
                return baseName;

            StringBuilder sb = new StringBuilder(baseName);

            sb.Append('(');
            sb.Append(DisplayName.GetValueString(arglist[0], STRING_MAX));

            for (int i = 1; i < arglist.Length; i++)
            {
                sb.Append(',');
                sb.Append(DisplayName.GetValueString(arglist[i], STRING_MAX));
            }

            sb.Append(')');

            return sb.ToString();
        }

        /// <summary>
        /// Returns the best fit for a common type to be used in
        /// matching actual arguments to a methods Type parameters.
        /// </summary>
        public static bool TryGetBestCommonType(Type? type1, Type? type2, [NotNullIfNotNull("type1"), NotNullIfNotNull("type2")] out Type? bestCommonType)
        {
#pragma warning disable SA1107 // Code should not contain multiple statements on one line
#pragma warning disable SA1501 // Statement should not be on a single line
            if (type1 == type2) { bestCommonType = type1; return true; }
            if (type1 is null) { bestCommonType = type2; return true; }
            if (type2 is null) { bestCommonType = type1; return true; }

            if (TypeHelper.IsNumeric(type1) && TypeHelper.IsNumeric(type2))
            {
                if (type1 == typeof(double)) { bestCommonType = type1; return true; }
                if (type2 == typeof(double)) { bestCommonType = type2; return true; }

                if (type1 == typeof(float)) { bestCommonType = type1; return true; }
                if (type2 == typeof(float)) { bestCommonType = type2; return true; }

                if (type1 == typeof(decimal)) { bestCommonType = type1; return true; }
                if (type2 == typeof(decimal)) { bestCommonType = type2; return true; }

                if (type1 == typeof(UInt64)) { bestCommonType = type1; return true; }
                if (type2 == typeof(UInt64)) { bestCommonType = type2; return true; }

                if (type1 == typeof(Int64)) { bestCommonType = type1; return true; }
                if (type2 == typeof(Int64)) { bestCommonType = type2; return true; }

                if (type1 == typeof(UInt32)) { bestCommonType = type1; return true; }
                if (type2 == typeof(UInt32)) { bestCommonType = type2; return true; }

                if (type1 == typeof(Int32)) { bestCommonType = type1; return true; }
                if (type2 == typeof(Int32)) { bestCommonType = type2; return true; }

                if (type1 == typeof(UInt16)) { bestCommonType = type1; return true; }
                if (type2 == typeof(UInt16)) { bestCommonType = type2; return true; }

                if (type1 == typeof(Int16)) { bestCommonType = type1; return true; }
                if (type2 == typeof(Int16)) { bestCommonType = type2; return true; }

                if (type1 == typeof(byte)) { bestCommonType = type1; return true; }
                if (type2 == typeof(byte)) { bestCommonType = type2; return true; }

                if (type1 == typeof(sbyte)) { bestCommonType = type1; return true; }
                if (type2 == typeof(sbyte)) { bestCommonType = type2; return true; }
            }

            if (type1.IsAssignableFrom(type2)) { bestCommonType = type1; return true; }
            if (type2.IsAssignableFrom(type1)) { bestCommonType = type2; return true; }

            bestCommonType = typeof(object);
            return false;
#pragma warning restore SA1501 // Statement should not be on a single line
#pragma warning restore SA1107 // Code should not contain multiple statements on one line
        }

        /// <summary>
        /// Determines whether the specified type is numeric.
        /// </summary>
        /// <param name="type">The type to be examined.</param>
        /// <returns>
        /// <see langword="true"/> if the specified type is numeric; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNumeric(Type type)
        {
            return type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(decimal) ||
                   type == typeof(Int64) ||
                   type == typeof(Int32) ||
                   type == typeof(Int16) ||
                   type == typeof(UInt64) ||
                   type == typeof(UInt32) ||
                   type == typeof(UInt16) ||
                   type == typeof(byte) ||
                   type == typeof(sbyte);
        }

        /// <summary>
        /// Convert an argument list to the required parameter types.
        /// Currently, only widening numeric conversions are performed.
        /// </summary>
        /// <param name="arglist">An array of args to be converted</param>
        /// <param name="parameters">A ParameterInfo[] whose types will be used as targets</param>
        public static void ConvertArgumentList(object?[] arglist, IParameterInfo[] parameters)
        {
            System.Diagnostics.Debug.Assert(arglist.Length <= parameters.Length);

            for (int i = 0; i < arglist.Length; i++)
            {
                object? arg = arglist[i];

                if (arg is IConvertible)
                {
                    Type argType = arg.GetType();
                    Type targetType = parameters[i].ParameterType;
                    bool convert = false;

                    if (argType != targetType && IsNumeric(argType) && IsNumeric(targetType))
                    {
                        if (targetType == typeof(double) || targetType == typeof(float))
                            convert = arg is int || arg is long || arg is short || arg is byte || arg is sbyte;
                        else
                            if (targetType == typeof(long))
                            convert = arg is int || arg is short || arg is byte || arg is sbyte;
                        else
                                if (targetType == typeof(short))
                            convert = arg is byte || arg is sbyte;
                    }

                    if (convert)
                    {
                        arglist[i] = Convert.ChangeType(arg, targetType,
                            System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can deduce type args for a generic type from the supplied arguments.
        /// </summary>
        /// <param name="type">The type to be examined.</param>
        /// <param name="arglist">The arglist.</param>
        /// <param name="typeArgsOut">The type args to be used.</param>
        /// <returns>
        /// <see langword="true"/> if this the provided args give sufficient information to determine the type args to be used; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool CanDeduceTypeArgsFromArgs(Type type, object?[] arglist, [NotNullWhen(true)] ref Type[]? typeArgsOut)
        {
            Type[] typeParameters = type.GetGenericArguments();

            foreach (ConstructorInfo ctor in type.GetConstructors())
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length != arglist.Length)
                    continue;

                Type?[]? typeArgs = new Type?[typeParameters.Length];
                for (int i = 0; i < typeArgs.Length; i++)
                {
                    for (int j = 0; j < arglist.Length; j++)
                    {
                        if (parameters[j].ParameterType.Equals(typeParameters[i]))
                        {
                            if (!TypeHelper.TryGetBestCommonType(
                                typeArgs[i],
                                arglist[j]?.GetType(),
                                out typeArgs[i]))
                            {
                                typeArgs[i] = null;
                                break;
                            }
                        }
                    }

                    if (typeArgs[i] is null)
                    {
                        typeArgs = null;
                        break;
                    }
                }

                if (typeArgs is not null)
                {
                    typeArgsOut = typeArgs!;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return the interfaces implemented by a Type.
        /// </summary>
        /// <param name="type">The Type to be examined.</param>
        /// <returns>An array of Types for the interfaces.</returns>
        public static Type[] GetDeclaredInterfaces(Type type)
        {
            List<Type> interfaces = new(type.GetInterfaces());

            if (type.BaseType is null || type.BaseType == typeof(object))
                return interfaces.ToArray();

            List<Type> baseInterfaces = new(type.BaseType.GetInterfaces());
            List<Type> declaredInterfaces = new();

            foreach (Type interfaceType in interfaces)
            {
                if (!baseInterfaces.Contains(interfaceType))
                    declaredInterfaces.Add(interfaceType);
            }

            return declaredInterfaces.ToArray();
        }

        /// <summary>
        /// Return whether or not the given type is a ValueTuple.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Whether or not the given type is a ValueTuple.</returns>
        public static bool IsValueTuple(Type type)
        {
            return IsTupleInternal(type, "System.ValueTuple");
        }

        /// <summary>
        /// Return whether or not the given type is a Tuple.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Whether or not the given type is a Tuple.</returns>
        public static bool IsTuple(Type type)
        {
            return IsTupleInternal(type, "System.Tuple");
        }

        /// <summary>
        /// For the given <paramref name="type"/>, find the primary <see cref="IEnumerable{T}"/> interface declared
        /// on it and return its generic type argument. If multiple <see cref="IEnumerable{T}"/> interfaces are declared,
        /// return <see langword="null"/>.
        /// </summary>
        /// <param name="type">The given <see cref="Type"/>.</param>
        /// <returns>
        /// The generic type argument of the primary <see cref="IEnumerable{T}"/> interface declared on the given
        /// <paramref name="type"/>, or <see langword="null"/> if zero interfaces are found or more than one interface is found.
        /// </returns>
        public static Type? FindPrimaryEnumerableInterfaceGenericTypeArgument(Type type)
        {
            static bool predicate(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

            Type? primaryEnumerableType = null;
            var found = false;
            var queue = new Queue<Type>();
            queue.Enqueue(type);

            // We need to scan interfaces declared on interfaces recursively here because the type
            // might only implement `IList<T>`, which technically isn't an `IEnumerable<T>` and would
            // cause a basic interface scan to fail.
            while (queue.Count > 0)
            {
                var @interface = queue.Dequeue();

                // if we run into multiple `IEnumerable<T>` that have the same T generic argument then that's fine,
                // i.e. if the primary enumerable type is equal to the interface type we can skip this check.
                if (primaryEnumerableType != @interface && predicate(@interface))
                {
                    if (found)
                    {
                        return null;
                    }
                    primaryEnumerableType = @interface;
                    found = true;
                }

                foreach (var subInterface in @interface.GetInterfaces())
                {
                    queue.Enqueue(subInterface);
                }
            }

            return primaryEnumerableType?.GenericTypeArguments[0];
        }

        private static bool IsTupleInternal(Type type, string tupleName)
        {
            string typeName = type.FullName();

            if (typeName.EndsWith("[]", StringComparison.Ordinal))
                return false;

            string typeNameWithoutGenerics = GetTypeNameWithoutGenerics(typeName);
            return typeNameWithoutGenerics == tupleName;
        }

        private static string GetTypeNameWithoutGenerics(string fullTypeName)
        {
            int index = fullTypeName.IndexOf('`');
            return index == -1 ? fullTypeName : fullTypeName.Substring(0, index);
        }

        /// <summary>
        /// Determines whether the cast to the given type would succeed.
        /// If <paramref name="obj"/> is <see langword="null"/> and <typeparamref name="T"/>
        /// can be <see langword="null"/>, the cast succeeds just like the C# language feature.
        /// </summary>
        /// <param name="obj">The object to cast.</param>
        internal static bool CanCast<T>(object? obj)
        {
            return obj is T || (obj is null && default(T) is null);
        }

        /// <summary>
        /// Casts to a value of the given type if possible.
        /// If <paramref name="obj"/> is <see langword="null"/> and <typeparamref name="T"/>
        /// can be <see langword="null"/>, the cast succeeds just like the C# language feature.
        /// </summary>
        /// <param name="obj">The object to cast.</param>
        /// <param name="value">The value of the object, if the cast succeeded.</param>
        internal static bool TryCast<T>(object? obj, [NotNullWhen(true)] out T? value)
        {
            if (obj is T tObj)
            {
                value = tObj;
                return true;
            }

            value = default(T);
            return obj is null && default(T) is null;
        }

        /// <summary>
        /// Gets the <see cref="Type.FullName"/> if available.
        /// </summary>
        /// <param name="type">The type to get the <see cref="Type.FullName"/> for.</param>
        /// <returns><see cref="Type.FullName"/> if available, throws otherwise.</returns>
        /// <exception cref="InvalidOperationException">If <see cref="Type.FullName"/> returns <see langword="null"/>.</exception>
        internal static string FullName(this Type type)
        {
            return type.FullName ?? throw new InvalidOperationException("No name for type: " + type);
        }

        internal static bool HasCompilerGeneratedEquals(this Type type)
        {
            var equalsMethod = type.GetMethod(nameof(type.Equals), BindingFlags.Instance | BindingFlags.Public,
                null, [type], null);

            return equalsMethod?.GetCustomAttribute<CompilerGeneratedAttribute>() is not null;
        }

        private static readonly Type[] EqualsObjectParameterTypes = { typeof(object) };

        internal static bool OverridesEqualsObject(Type type)
        {
            // Check for Equals(object) override
            var equalsObject = type.GetMethod(nameof(type.Equals), BindingFlags.Instance | BindingFlags.Public,
                null, EqualsObjectParameterTypes, null);
            return equalsObject is not null && equalsObject.DeclaringType != (type.IsValueType ? typeof(ValueType) : typeof(object));
        }
    }
}
