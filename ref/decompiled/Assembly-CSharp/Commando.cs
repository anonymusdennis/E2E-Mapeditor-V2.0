using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class Commando
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class RequiredAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NameAttribute : Attribute
	{
		public string Name { get; private set; }

		public NameAttribute(string name)
		{
			Name = name;
		}
	}

	private object optionsObject;

	private Queue<FieldInfo> requiredOptions = new Queue<FieldInfo>();

	private Dictionary<string, FieldInfo> optionalOptions = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);

	private List<string> requiredUsageHelp = new List<string>();

	private List<string> optionalUsageHelp = new List<string>();

	private string argsPrefix;

	private string filePrefix;

	public Commando(object optionsObject, string argsPrefix, string filePrefix)
	{
		this.optionsObject = optionsObject;
		this.argsPrefix = argsPrefix;
		this.filePrefix = filePrefix;
		FieldInfo[] fields = optionsObject.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			string optionName = GetOptionName(fieldInfo);
			if (GetAttribute<RequiredAttribute>(fieldInfo) != null)
			{
				requiredOptions.Enqueue(fieldInfo);
				requiredUsageHelp.Add($"<{optionName}>");
			}
			else
			{
				optionalOptions.Add(optionName.ToLowerInvariant(), fieldInfo);
				optionalUsageHelp.Add(GetOptionHelp(fieldInfo));
			}
		}
	}

	private string GetOptionHelp(FieldInfo field)
	{
		string empty = string.Empty;
		string optionName = GetOptionName(field);
		if (field.FieldType == typeof(bool))
		{
			return $"/{optionName}";
		}
		if (IsList(field))
		{
			return string.Format("/{0}:{1}", optionName, ListElementType(field).ToString().Replace("System.", string.Empty));
		}
		return string.Format("/{0}:{1}", optionName, field.FieldType.ToString().Replace("System.", string.Empty));
	}

	public bool ParseCommandLine(string[] args)
	{
		foreach (string text in args)
		{
			if (!ParseArgument(text.Trim()))
			{
				return false;
			}
		}
		FieldInfo fieldInfo = requiredOptions.FirstOrDefault((FieldInfo field) => !IsList(field) || GetList(field).Count == 0);
		if (fieldInfo != null)
		{
			LogErrorFormat("Missing argument '{0}'", GetOptionName(fieldInfo));
			return false;
		}
		return true;
	}

	private bool ParseArgument(string arg)
	{
		UnityEngine.Debug.Log("   *******  ParseArgument    " + arg);
		if (!string.IsNullOrEmpty(arg))
		{
			if (arg.StartsWith("/"))
			{
				char[] separator = new char[1] { ':' };
				string[] array = arg.Substring(1).Split(separator, 2, StringSplitOptions.None);
				string text = array[0];
				string value = ((array.Length <= 1) ? "true" : array[1]);
				if (!optionalOptions.TryGetValue(text.ToLowerInvariant(), out var value2))
				{
					LogErrorFormat("Unknown option '{0}'", text);
					return false;
				}
				return SetOption(value2, value);
			}
			if (requiredOptions.Count == 0)
			{
				LogErrorFormat("Too many arguments");
				return false;
			}
			FieldInfo field = requiredOptions.Peek();
			if (!IsList(field))
			{
				requiredOptions.Dequeue();
			}
			return SetOption(field, arg);
		}
		return false;
	}

	private bool SetOption(FieldInfo field, string value)
	{
		try
		{
			if (IsList(field))
			{
				GetList(field).Add(ChangeType(value, ListElementType(field)));
			}
			else
			{
				field.SetValue(optionsObject, ChangeType(value, field.FieldType));
			}
			return true;
		}
		catch
		{
			LogErrorFormat("Invalid value '{0}' for option '{1}'", value, GetOptionName(field));
			return false;
		}
	}

	private static object ChangeType(string value, Type type)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(type);
		return converter.ConvertFromInvariantString(value);
	}

	private static bool IsList(FieldInfo field)
	{
		return typeof(IList).IsAssignableFrom(field.FieldType);
	}

	private IList GetList(FieldInfo field)
	{
		return (IList)field.GetValue(optionsObject);
	}

	private static Type ListElementType(FieldInfo field)
	{
		IEnumerable<Type> source = from i in field.FieldType.GetInterfaces()
			where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
			select i;
		return source.First().GetGenericArguments()[0];
	}

	private static string GetOptionName(FieldInfo field)
	{
		NameAttribute attribute = GetAttribute<NameAttribute>(field);
		if (attribute != null)
		{
			return attribute.Name;
		}
		return field.Name;
	}

	private void LogErrorFormat(string format, params object[] args)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format(format, args));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Usage:");
		if (!string.IsNullOrEmpty(argsPrefix))
		{
			if (!string.IsNullOrEmpty(filePrefix))
			{
				stringBuilder.AppendLine("1) Command line mode:");
			}
			if (optionalUsageHelp.Count == 0)
			{
				stringBuilder.AppendLine(string.Format("{0} <Unity options> {1}{2}", fileNameWithoutExtension, argsPrefix, string.Join(" ", requiredUsageHelp.ToArray())));
			}
			else
			{
				stringBuilder.AppendLine(string.Format("{0} <Unity options> {1}{2}[;<Commando options>]", fileNameWithoutExtension, argsPrefix, string.Join(" ", requiredUsageHelp.ToArray())));
				stringBuilder.AppendLine("<Commando options> is a list of the following optional parameters each separated by a semi-colon:");
				foreach (string item in optionalUsageHelp)
				{
					stringBuilder.AppendFormat($"  {item}");
				}
				stringBuilder.AppendLine();
			}
		}
		if (!string.IsNullOrEmpty(filePrefix))
		{
			stringBuilder.AppendLine();
			if (!string.IsNullOrEmpty(argsPrefix))
			{
				stringBuilder.AppendLine("2) Response file mode:");
			}
			stringBuilder.AppendLine($"{fileNameWithoutExtension} <Unity options> {filePrefix}<filename>");
			if (requiredUsageHelp.Count > 0)
			{
				stringBuilder.AppendLine($"File MUST first contain a list of the following required parameters in the specified order, each on a separate line:");
				stringBuilder.AppendLine(string.Format("{0}", string.Join(" ", requiredUsageHelp.ToArray())));
			}
			if (optionalUsageHelp.Count > 0)
			{
				stringBuilder.AppendLine($"(Optional): File can contain a list of the following parameters, each on a separate line:");
				foreach (string item2 in optionalUsageHelp)
				{
					stringBuilder.AppendFormat($"  {item2}");
				}
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("A line beginning with a hash (#) is treated as a comment line.");
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine("Nb. Required options are specified without the /ParameterName: prefix");
		stringBuilder.AppendLine($"For a list of available <Unity options> see 'http://docs.unity3d.com/Manual/CommandLineArguments.html'");
		UnityEngine.Debug.LogError(stringBuilder.ToString());
	}

	private static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
	{
		return provider.GetCustomAttributes(typeof(T), inherit: false).OfType<T>().FirstOrDefault();
	}
}
