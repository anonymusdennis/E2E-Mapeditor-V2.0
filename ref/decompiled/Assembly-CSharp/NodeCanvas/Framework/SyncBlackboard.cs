using System;
using System.Collections.Generic;
using ParadoxNotion;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace NodeCanvas.Framework;

[RequireComponent(typeof(Blackboard))]
public class SyncBlackboard : NetworkBehaviour
{
	private class SyncVarMessage : MessageBase
	{
		public string name;

		public object value;

		public SyncVarMessage()
		{
		}

		public SyncVarMessage(string name, object value)
		{
			this.name = name;
			this.value = value;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(name);
			bool flag = value == null;
			writer.Write(flag);
			if (!flag)
			{
				string assemblyQualifiedName = value.GetType().AssemblyQualifiedName;
				Type type = Type.GetType(assemblyQualifiedName);
				writer.Write(assemblyQualifiedName);
				if (type == typeof(string))
				{
					writer.Write((string)value);
				}
				else if (type == typeof(bool))
				{
					writer.Write((bool)value);
				}
				else if (type == typeof(int))
				{
					writer.Write((int)value);
				}
				else if (type == typeof(float))
				{
					writer.Write((float)value);
				}
				else if (type == typeof(Color))
				{
					writer.Write((Color)value);
				}
				else if (type == typeof(Vector2))
				{
					writer.Write((Vector2)value);
				}
				else if (type == typeof(Vector3))
				{
					writer.Write((Vector3)value);
				}
				else if (type == typeof(Vector4))
				{
					writer.Write((Vector4)value);
				}
				else if (type == typeof(Quaternion))
				{
					writer.Write((Quaternion)value);
				}
				else if (type == typeof(GameObject))
				{
					writer.Write((GameObject)value);
				}
				else if (typeof(Component).RTIsAssignableFrom(type))
				{
					writer.Write((GameObject)value);
				}
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			name = reader.ReadString();
			if (reader.ReadBoolean())
			{
				value = null;
				return;
			}
			string typeName = reader.ReadString();
			Type type = Type.GetType(typeName);
			if (type == typeof(string))
			{
				value = reader.ReadString();
			}
			else if (type == typeof(bool))
			{
				value = reader.ReadBoolean();
			}
			else if (type == typeof(int))
			{
				value = reader.ReadInt32();
			}
			else if (type == typeof(float))
			{
				value = reader.ReadSingle();
			}
			else if (type == typeof(Color))
			{
				value = reader.ReadColor();
			}
			else if (type == typeof(Vector2))
			{
				value = reader.ReadVector2();
			}
			else if (type == typeof(Vector3))
			{
				value = reader.ReadVector3();
			}
			else if (type == typeof(Vector4))
			{
				value = reader.ReadVector4();
			}
			else if (type == typeof(Quaternion))
			{
				value = reader.ReadQuaternion();
			}
			else if (type == typeof(GameObject))
			{
				value = reader.ReadGameObject();
			}
			else if (typeof(Component).RTIsAssignableFrom(type))
			{
				GameObject gameObject = reader.ReadGameObject();
				if (gameObject != null)
				{
					value = gameObject.GetComponent(type);
				}
			}
		}
	}

	private const short VALUE_CHANGE = 1000;

	private const short REFRESH_VALUES = 1001;

	private List<string> waitResponseNames = new List<string>();

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler(1000, SyncValue);
		NetworkServer.RegisterHandler(1001, RefreshValues);
	}

	public override void OnStartClient()
	{
		if (!base.isServer)
		{
			NetworkManager.singleton.client.RegisterHandler(1000, SyncValue);
			int connectionId = NetworkManager.singleton.client.connection.connectionId;
			NetworkManager.singleton.client.Send(1001, new IntegerMessage(connectionId));
		}
		Blackboard component = GetComponent<Blackboard>();
		foreach (Variable value in component.variables.Values)
		{
			if (!value.isProtected)
			{
				value.onValueChanged += OnValueChange;
			}
		}
	}

	private void OnValueChange(string name, object value)
	{
		if (!waitResponseNames.Contains(name))
		{
			waitResponseNames.Add(name);
			if (base.isServer)
			{
				Debug.Log("Value Changed on Server: " + name);
				NetworkServer.SendToReady(base.gameObject, 1000, new SyncVarMessage(name, value));
			}
			else
			{
				Debug.Log("Value Changed on Client: " + name);
				NetworkManager.singleton.client.Send(1000, new SyncVarMessage(name, value));
			}
		}
	}

	private void RefreshValues(NetworkMessage msg)
	{
		int value = msg.ReadMessage<IntegerMessage>().value;
		Blackboard component = GetComponent<Blackboard>();
		foreach (Variable value2 in component.variables.Values)
		{
			NetworkServer.SendToClient(value, 1000, new SyncVarMessage(value2.name, value2.value));
		}
	}

	private void SyncValue(NetworkMessage msg)
	{
		SyncVarMessage syncVarMessage = msg.ReadMessage<SyncVarMessage>();
		string item = syncVarMessage.name;
		object value = syncVarMessage.value;
		Blackboard component = GetComponent<Blackboard>();
		component.SetValue(item, value);
		waitResponseNames.Remove(item);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
