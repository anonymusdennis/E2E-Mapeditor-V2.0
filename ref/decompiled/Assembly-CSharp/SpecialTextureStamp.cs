using UnityEngine;

public class SpecialTextureStamp : MonoBehaviour
{
	public Texture2D m_Stamp;

	public virtual Texture2D GetStampTexture()
	{
		return m_Stamp;
	}
}
