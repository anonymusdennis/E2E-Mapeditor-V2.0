using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices;

public static class Helper
{
	private class AllocationInfo
	{
		public Type Type { get; private set; }

		public object Memory { get; private set; }

		public int? ArrayLength { get; private set; }

		public AllocationInfo(Type type, object memory, int? arrayLength = null)
		{
			Type = type;
			Memory = memory;
			ArrayLength = arrayLength;
		}
	}

	private class DelegateHolder
	{
		public Delegate Public { get; private set; }

		public Delegate Private { get; private set; }

		public Delegate[] Additional { get; private set; }

		public DelegateHolder(Delegate publicDelegate, Delegate privateDelegate, params Delegate[] additionalDelegates)
		{
			Public = publicDelegate;
			Private = privateDelegate;
			Additional = additionalDelegates;
		}
	}

	private static Dictionary<IntPtr, AllocationInfo> s_AllocationRegistry = new Dictionary<IntPtr, AllocationInfo>();

	private static Dictionary<IntPtr, DelegateHolder> s_CallDelegates = new Dictionary<IntPtr, DelegateHolder>();

	private static List<Type> s_PointerTypes = new List<Type> { typeof(string) };

	public static int GetAllocationCount()
	{
		return s_AllocationRegistry.Count;
	}

	internal static void RegisterAllocation<T>(ref IntPtr address, T memory)
	{
		ReleaseAllocation(ref address);
		if (address != IntPtr.Zero)
		{
			throw new Exception("Attempting to allocate over memory that is already externally allocated");
		}
		if (typeof(T).IsArray)
		{
			Type elementType = typeof(T).GetElementType();
			Array array = memory as Array;
			int num = 0;
			num = ((!s_PointerTypes.Contains(elementType)) ? Marshal.SizeOf(elementType) : Marshal.SizeOf(typeof(IntPtr)));
			address = Marshal.AllocHGlobal(array.Length * num);
			s_AllocationRegistry.Add(address, new AllocationInfo(typeof(T), memory, array.Length));
			for (int i = 0; i < array.Length; i++)
			{
				object value = array.GetValue(i);
				if (s_PointerTypes.Contains(elementType))
				{
					IntPtr address2 = IntPtr.Zero;
					RegisterAllocation(ref address2, value);
					Marshal.WriteIntPtr(address, i * num, address2);
				}
				else
				{
					Marshal.StructureToPtr(value, new IntPtr(address.ToInt64() + i * num), fDeleteOld: false);
				}
			}
		}
		else
		{
			if (memory is string)
			{
				string s = (string)(object)memory;
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				address = Marshal.AllocHGlobal(bytes.Length + 1);
				Marshal.Copy(bytes, 0, address, bytes.Length);
				Marshal.WriteByte(address, bytes.Length, 0);
			}
			else if (memory != null)
			{
				address = Marshal.AllocHGlobal(Marshal.SizeOf(memory.GetType()));
				Marshal.StructureToPtr(memory, address, fDeleteOld: false);
			}
			if (memory != null && !s_AllocationRegistry.ContainsKey(address))
			{
				s_AllocationRegistry.Add(address, new AllocationInfo(memory.GetType(), memory));
			}
		}
	}

	internal static void ReleaseAllocation(ref IntPtr address)
	{
		if (address == IntPtr.Zero)
		{
			return;
		}
		AllocationInfo value = null;
		if (!s_AllocationRegistry.TryGetValue(address, out value))
		{
			return;
		}
		if (value.Type.IsArray)
		{
			Type elementType = value.Type.GetElementType();
			int num = 0;
			num = ((!s_PointerTypes.Contains(elementType)) ? Marshal.SizeOf(elementType) : Marshal.SizeOf(typeof(IntPtr)));
			int num2 = 0;
			while (true)
			{
				int? arrayLength = value.ArrayLength;
				if (!arrayLength.HasValue || num2 >= arrayLength.GetValueOrDefault())
				{
					break;
				}
				IntPtr address2 = new IntPtr(address.ToInt64() + num2 * num);
				if (s_PointerTypes.Contains(elementType))
				{
					address2 = Marshal.ReadIntPtr(address2);
				}
				if (GetAllocation(address2, elementType) is IDisposable disposable)
				{
					disposable.Dispose();
				}
				if (s_PointerTypes.Contains(elementType))
				{
					ReleaseAllocation(ref address2);
				}
				num2++;
			}
		}
		if (value.Type.IsValueType && GetAllocation(address, value.Type) is IDisposable disposable2)
		{
			disposable2.Dispose();
		}
		Marshal.FreeHGlobal(address);
		s_AllocationRegistry.Remove(address);
		address = IntPtr.Zero;
	}

	internal static T GetAllocation<T>(IntPtr startAddress, int? arrayLength = null)
	{
		if (startAddress == IntPtr.Zero)
		{
			return default(T);
		}
		if (s_AllocationRegistry.ContainsKey(startAddress) && s_AllocationRegistry[startAddress].Type == typeof(T) && s_AllocationRegistry[startAddress].ArrayLength == arrayLength)
		{
			return (T)s_AllocationRegistry[startAddress].Memory;
		}
		if (typeof(T).IsArray)
		{
			Type elementType = typeof(T).GetElementType();
			Array array = Array.CreateInstance(elementType, arrayLength.Value);
			int num = 0;
			num = ((!s_PointerTypes.Contains(elementType)) ? Marshal.SizeOf(elementType) : Marshal.SizeOf(typeof(IntPtr)));
			for (int i = 0; i < arrayLength.Value; i++)
			{
				IntPtr intPtr = new IntPtr(startAddress.ToInt64() + i * num);
				if (s_PointerTypes.Contains(elementType))
				{
					intPtr = Marshal.ReadIntPtr(intPtr);
				}
				object allocation = GetAllocation(intPtr, elementType);
				array.SetValue(allocation, i);
			}
			return (T)(object)array;
		}
		return (T)GetAllocation(startAddress, typeof(T));
	}

	private static object GetAllocation(IntPtr address, Type type)
	{
		if (type == typeof(string))
		{
			return GetAllocatedString(address);
		}
		return Marshal.PtrToStructure(address, type);
	}

	private static string GetAllocatedString(IntPtr address)
	{
		if (address == IntPtr.Zero)
		{
			return null;
		}
		int i;
		for (i = 0; Marshal.ReadByte(address, i) != 0; i++)
		{
		}
		byte[] array = new byte[i];
		Marshal.Copy(address, array, 0, i);
		return Encoding.UTF8.GetString(array);
	}

	internal static void RegisterCall(ref IntPtr clientDataAddress, object clientData, Delegate publicDelegate, Delegate privateDelegate, params Delegate[] additionalDelegates)
	{
		RegisterAllocation(ref clientDataAddress, new BoxedData(clientData));
		s_CallDelegates.Add(clientDataAddress, new DelegateHolder(publicDelegate, privateDelegate, additionalDelegates));
	}

	private static bool CanRemoveCallDelegate(object delegateInfo)
	{
		PropertyInfo propertyInfo = (from property in delegateInfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
			where property.PropertyType == typeof(Result)
			select property).FirstOrDefault();
		if (propertyInfo != null)
		{
			Result result = (Result)propertyInfo.GetValue(delegateInfo, null);
			if (result == Result.OperationWillRetry || result == Result.AuthPinGrantCode)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	private static bool TryGetAndRemovePublicCallDelegate<TDelegate>(IntPtr clientDataAddress, object delegateInfo, out TDelegate callDelegate) where TDelegate : class
	{
		callDelegate = (TDelegate)null;
		if (clientDataAddress != IntPtr.Zero && s_CallDelegates.ContainsKey(clientDataAddress))
		{
			callDelegate = s_CallDelegates[clientDataAddress].Public as TDelegate;
			if (CanRemoveCallDelegate(delegateInfo))
			{
				s_CallDelegates.Remove(clientDataAddress);
				ReleaseAllocation(ref clientDataAddress);
			}
			return true;
		}
		return false;
	}

	private static bool TryGetAdditionalCallDelegate<TDelegate>(IntPtr clientDataAddress, out TDelegate additionalCallDelegate) where TDelegate : class
	{
		additionalCallDelegate = (TDelegate)null;
		if (clientDataAddress != IntPtr.Zero && s_CallDelegates.ContainsKey(clientDataAddress))
		{
			additionalCallDelegate = s_CallDelegates[clientDataAddress].Additional.FirstOrDefault((Delegate delegat) => delegat.GetType() == typeof(TDelegate)) as TDelegate;
			if (additionalCallDelegate != null)
			{
				return true;
			}
		}
		return false;
	}

	internal static string StringFromByteArray(byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}

	internal static byte[] StringToByteArray(string str, int sizeConst)
	{
		if (str.Length >= sizeConst - 1)
		{
			return Encoding.UTF8.GetBytes(new string(str.Take(sizeConst - 1).ToArray()));
		}
		return Encoding.UTF8.GetBytes(str.PadRight(sizeConst, '\0'));
	}

	internal static T GetDefault<T>()
	{
		return default(T);
	}

	internal static void CopyProperties(object source, object target)
	{
		if (source == null || target == null)
		{
			return;
		}
		PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
		PropertyInfo[] properties2 = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
		PropertyInfo[] array = properties;
		foreach (PropertyInfo sourceProperty in array)
		{
			PropertyInfo propertyInfo = properties2.SingleOrDefault((PropertyInfo property) => property.Name == sourceProperty.Name);
			if (propertyInfo == null || propertyInfo.GetSetMethod(nonPublic: false) == null)
			{
				continue;
			}
			if (sourceProperty.PropertyType == propertyInfo.PropertyType)
			{
				propertyInfo.SetValue(target, sourceProperty.GetValue(source, null), null);
			}
			else if (propertyInfo.PropertyType.IsArray)
			{
				if (sourceProperty.GetValue(source, null) is Array array2)
				{
					Array array3 = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), array2.Length);
					for (int j = 0; j < array2.Length; j++)
					{
						object value = array2.GetValue(j);
						object obj = Activator.CreateInstance(propertyInfo.PropertyType.GetElementType());
						CopyProperties(value, obj);
						array3.SetValue(obj, j);
					}
					propertyInfo.SetValue(target, array3, null);
				}
				else
				{
					propertyInfo.SetValue(target, null, null);
				}
			}
			else
			{
				object obj2 = Activator.CreateInstance(propertyInfo.PropertyType);
				CopyProperties(sourceProperty.GetValue(source, null), obj2);
				propertyInfo.SetValue(target, obj2, null);
			}
		}
	}

	internal static THandle GetHandle<THandle>(IntPtr innerHandle) where THandle : Handle, new()
	{
		if (innerHandle != IntPtr.Zero)
		{
			return new THandle
			{
				InnerHandle = innerHandle
			};
		}
		return (THandle)null;
	}

	internal static IntPtr GetInnerHandle<THandle>(THandle handle) where THandle : Handle
	{
		if (handle != null)
		{
			return handle.InnerHandle;
		}
		return IntPtr.Zero;
	}

	internal static bool GetBoolFromInt(int value)
	{
		return value != 0;
	}

	internal static int GetIntFromBool(bool value)
	{
		return value ? 1 : 0;
	}

	internal static T CopyPropertiesToNew<T>(object value) where T : new()
	{
		object obj = new T();
		CopyProperties(value, obj);
		return (T)obj;
	}

	internal static object GetClientData(IntPtr clientDataAddress)
	{
		return GetAllocation<BoxedData>(clientDataAddress)?.Data;
	}

	internal static THandle[] GetHandleArrayAllocation<THandle>(IntPtr address, int arrayLength) where THandle : Handle, new()
	{
		if (address != IntPtr.Zero)
		{
			IntPtr[] allocation = GetAllocation<IntPtr[]>(address, arrayLength);
			return allocation.Select((IntPtr handleAddress) => new THandle
			{
				InnerHandle = handleAddress
			}).ToArray();
		}
		return null;
	}

	internal static void RegisterHandleArrayAllocation<THandle>(ref IntPtr address, THandle[] array, out uint arrayLength) where THandle : Handle
	{
		arrayLength = (uint)array.Length;
		IntPtr[] memory = array.Select((THandle handle) => handle.InnerHandle).ToArray();
		RegisterAllocation(ref address, memory);
	}

	internal static void RegisterArrayAllocation<T>(ref IntPtr address, T[] array, out uint arrayLength)
	{
		arrayLength = (uint)array.Length;
		RegisterAllocation(ref address, array);
	}

	internal static void RegisterArrayAllocation<T>(ref IntPtr address, T[] array, out int arrayLength)
	{
		arrayLength = array.Length;
		RegisterAllocation(ref address, array);
	}

	internal static bool TryMarshal<TMarshalFrom, TMarshalTo>(IntPtr address, out TMarshalTo to) where TMarshalFrom : struct, IDisposable where TMarshalTo : class, new()
	{
		to = (TMarshalTo)null;
		if (address != IntPtr.Zero)
		{
			TMarshalFrom val = (TMarshalFrom)Marshal.PtrToStructure(address, typeof(TMarshalFrom));
			to = CopyPropertiesToNew<TMarshalTo>(val);
			val.Dispose();
			return true;
		}
		return false;
	}

	private static bool TryMarshalCallbackInfo<TCallbackInfoInternal, TCallbackInfo>(IntPtr address, out TCallbackInfo callbackInfo, out IntPtr clientDataAddress) where TCallbackInfoInternal : struct, ICallbackInfo where TCallbackInfo : class, new()
	{
		callbackInfo = (TCallbackInfo)null;
		clientDataAddress = IntPtr.Zero;
		if (address != IntPtr.Zero)
		{
			TCallbackInfoInternal val = (TCallbackInfoInternal)Marshal.PtrToStructure(address, typeof(TCallbackInfoInternal));
			callbackInfo = CopyPropertiesToNew<TCallbackInfo>(val);
			clientDataAddress = val.ClientDataAddress;
			val.Dispose();
			return true;
		}
		return false;
	}

	internal static bool TryGetAndRemovePublicCallDelegate<TDelegate, TCallbackInfoInternal, TCallbackInfo>(IntPtr address, out TDelegate callDelegate, out TCallbackInfo callbackInfo) where TDelegate : class where TCallbackInfoInternal : struct, ICallbackInfo where TCallbackInfo : class, new()
	{
		callDelegate = (TDelegate)null;
		callbackInfo = (TCallbackInfo)null;
		IntPtr clientDataAddress = IntPtr.Zero;
		if (TryMarshalCallbackInfo<TCallbackInfoInternal, TCallbackInfo>(address, out callbackInfo, out clientDataAddress) && TryGetAndRemovePublicCallDelegate<TDelegate>(clientDataAddress, callbackInfo, out callDelegate))
		{
			return true;
		}
		return false;
	}

	internal static bool TryGetAdditionalCallDelegate<TDelegate, TCallbackInfoInternal, TCallbackInfo>(IntPtr address, out TDelegate callDelegate, out TCallbackInfo callbackInfo) where TDelegate : class where TCallbackInfoInternal : struct, ICallbackInfo where TCallbackInfo : class, new()
	{
		callDelegate = (TDelegate)null;
		callbackInfo = (TCallbackInfo)null;
		IntPtr clientDataAddress = IntPtr.Zero;
		if (TryMarshalCallbackInfo<TCallbackInfoInternal, TCallbackInfo>(address, out callbackInfo, out clientDataAddress) && TryGetAdditionalCallDelegate<TDelegate>(clientDataAddress, out callDelegate))
		{
			return true;
		}
		return false;
	}
}
