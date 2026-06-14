using UnityEngine;

public class Knockout : MonoBehaviour
{
	public Animator m_Animator;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
	}
}
