using UnityEngine;

public class ReleaseBuildVersion : MonoBehaviour
{
	public T17Text m_TextItem;

	private string m_ReleaseString = BuildVersion.m_ReleaseVersionString + "." + BuildVersion.m_ChangeList;

	private void Start()
	{
		if (m_TextItem == null)
		{
			m_TextItem = GetComponent<T17Text>();
		}
		if (m_TextItem != null)
		{
			m_TextItem.SetNonLocalizedText(m_ReleaseString);
		}
	}
}
