using UnityEngine;

[CreateAssetMenu(fileName = "Constraint", menuName = "Team17/Customisation/Create Customisation Constraint")]
public class CustomisationConstraint : ScriptableObject
{
	[Header("Allowed")]
	public CustomisationSet allowed = new CustomisationSet();

	[Header("Fallback (if appearance has none of the above)")]
	public Customisation fallback = new Customisation();
}
