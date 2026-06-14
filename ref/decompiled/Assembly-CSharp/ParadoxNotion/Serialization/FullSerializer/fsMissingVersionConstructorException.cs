using System;

namespace ParadoxNotion.Serialization.FullSerializer;

public sealed class fsMissingVersionConstructorException : Exception
{
	public fsMissingVersionConstructorException(Type versionedType, Type constructorType)
		: base(string.Concat(versionedType, " is missing a constructor for previous model type ", constructorType))
	{
	}
}
