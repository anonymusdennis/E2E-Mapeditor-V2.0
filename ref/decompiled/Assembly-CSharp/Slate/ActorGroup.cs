using UnityEngine;
using UnityEngine.Serialization;

namespace Slate;

[Description("All tracks of an Actor Group affect a specific actor GameObject or one of it's Components. Specifying a name manually comes in handy if you want to set the target actor of this group via scripting. The ReferenceMode along with InitialCoordinates are essential when you are working with prefab actors.")]
public class ActorGroup : CutsceneGroup
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private GameObject _actor;

	[HideInInspector]
	[SerializeField]
	private ActorReferenceMode _referenceMode;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("_initialTransformation")]
	private ActorInitialTransformation _initialCoordinates;

	[HideInInspector]
	[SerializeField]
	private Vector3 _initialLocalPosition;

	[HideInInspector]
	[SerializeField]
	private Vector3 _initialLocalRotation;

	public override string name => (!string.IsNullOrEmpty(_name)) ? _name : ((!(actor != null)) ? null : actor.name);

	public override GameObject actor
	{
		get
		{
			return _actor;
		}
		set
		{
			if (_actor != value)
			{
				_actor = value;
			}
		}
	}

	public override ActorReferenceMode referenceMode
	{
		get
		{
			return _referenceMode;
		}
		set
		{
			_referenceMode = value;
		}
	}

	public override ActorInitialTransformation initialTransformation
	{
		get
		{
			return _initialCoordinates;
		}
		set
		{
			_initialCoordinates = value;
		}
	}

	public override Vector3 initialLocalPosition
	{
		get
		{
			return _initialLocalPosition;
		}
		set
		{
			_initialLocalPosition = value;
		}
	}

	public override Vector3 initialLocalRotation
	{
		get
		{
			return _initialLocalRotation;
		}
		set
		{
			_initialLocalRotation = value;
		}
	}
}
