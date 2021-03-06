//
// InternalExtensions.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2014 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	internal static class TypeConversionExtensions
	{
		internal static object ConvertTo(this object value, Type toType, Func<ParameterInfo> pinfoRetriever,
			IServiceProvider serviceProvider)
		{
			Func<TypeConverter> getConverter = () =>
			{
				ParameterInfo pInfo;
				if (pinfoRetriever == null || (pInfo = pinfoRetriever()) == null)
					return null;

				var converterTypeName = pInfo.CustomAttributes.GetTypeConverterTypeName();
				if (converterTypeName == null)
					return null;
				var convertertype = Type.GetType(converterTypeName);
				return (TypeConverter)Activator.CreateInstance(convertertype);
			};

			return ConvertTo(value, toType, getConverter, serviceProvider);
		}

		internal static object ConvertTo(this object value, Type toType, Func<MemberInfo> minfoRetriever,
			IServiceProvider serviceProvider)
		{
			Func<object> getConverter = () =>
			{
				MemberInfo memberInfo;

				var converterTypeName = toType.GetTypeInfo().CustomAttributes.GetTypeConverterTypeName();
				if (minfoRetriever != null && (memberInfo = minfoRetriever()) != null)
					converterTypeName = memberInfo.CustomAttributes.GetTypeConverterTypeName() ?? converterTypeName;
				if (converterTypeName == null)
					return null;

				var convertertype = Type.GetType(converterTypeName);
				return Activator.CreateInstance(convertertype);
			};

			return ConvertTo(value, toType, getConverter, serviceProvider);
		}

		static string GetTypeConverterTypeName(this IEnumerable<CustomAttributeData> attributes)
		{
			var converterAttribute =
				attributes.FirstOrDefault(cad => TypeConverterAttribute.TypeConvertersType.Contains(cad.AttributeType.FullName));
			if (converterAttribute == null)
				return null;
			if (converterAttribute.ConstructorArguments[0].ArgumentType == typeof (string))
				return (string)converterAttribute.ConstructorArguments[0].Value;
			if (converterAttribute.ConstructorArguments[0].ArgumentType == typeof (Type))
				return ((Type)converterAttribute.ConstructorArguments[0].Value).AssemblyQualifiedName;
			return null;
		}

		//Don't change the name or the signature of this, it's used by XamlC
		public static object ConvertTo(this object value, Type toType, Type convertertype, IServiceProvider serviceProvider)
		{
			if (convertertype == null)
				return value.ConvertTo(toType, (Func<object>)null, serviceProvider);
			Func<object> getConverter = () => Activator.CreateInstance(convertertype);
			;
			return value.ConvertTo(toType, getConverter, serviceProvider);
		}

		internal static object ConvertTo(this object value, Type toType, Func<object> getConverter,
			IServiceProvider serviceProvider)
		{
			if (value == null)
				return null;

			var str = value as string;
			if (str != null)
			{
				//If there's a [TypeConverter], use it
				object converter = getConverter?.Invoke();
				var xfTypeConverter = converter as TypeConverter;
				var xfExtendedTypeConverter = xfTypeConverter as IExtendedTypeConverter;
				if (xfExtendedTypeConverter != null)
					return value = xfExtendedTypeConverter.ConvertFromInvariantString(str, serviceProvider);
				if (xfTypeConverter != null)
					return value = xfTypeConverter.ConvertFromInvariantString(str);
				var converterType = converter?.GetType();
				if (converterType != null)
				{
					var convertFromStringInvariant = converterType.GetRuntimeMethod("ConvertFromInvariantString",
						new[] { typeof (string) });
					if (convertFromStringInvariant != null)
						return value = convertFromStringInvariant.Invoke(converter, new object[] { str });
				}

				//If the type is nullable, as the value is not null, it's safe to assume we want the built-in conversion
				if (toType.GetTypeInfo().IsGenericType && toType.GetGenericTypeDefinition() == typeof (Nullable<>))
					toType = Nullable.GetUnderlyingType(toType);

				//Obvious Built-in conversions
				if (toType.GetTypeInfo().IsEnum)
					return Enum.Parse(toType, str);
				//TODO supports Int16, 64, Byte, Char, ...
				if (toType == typeof (Int32))
					return Int32.Parse(str, CultureInfo.InvariantCulture);
				if (toType == typeof (float))
					return Single.Parse(str, CultureInfo.InvariantCulture);
				if (toType == typeof (double))
					return Double.Parse(str, CultureInfo.InvariantCulture);
				if (toType == typeof (bool))
					return Boolean.Parse(str);
				if (toType == typeof (TimeSpan))
					return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
				if (toType == typeof (DateTime))
					return DateTime.Parse(str, CultureInfo.InvariantCulture);
				if (toType == typeof (string) && str.StartsWith("{}", StringComparison.Ordinal))
					return str.Substring(2);
				if (toType == typeof (string))
					return value;
			}

			//if there's an implicit conversion, convert
			if (value != null)
			{
				var cast = value.GetType().GetRuntimeMethod("op_Implicit", new[] { value.GetType() });
				if (cast != null && cast.ReturnType == toType)
					value = cast.Invoke(null, new[] { value });
			}
			return value;
		}
	}
}