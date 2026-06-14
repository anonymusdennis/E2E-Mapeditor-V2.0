using UnityEngine;

public class BlockTagger : MonoBehaviour
{
	public enum Classification
	{
		Nothing_Yet,
		JobOffice_InmateChair,
		JobOffice_OfficerChair,
		JobOffice_Officer
	}

	public Classification m_Classification;
}
