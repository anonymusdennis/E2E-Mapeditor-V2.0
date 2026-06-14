using UnityEngine;

public class Snore : MonoBehaviour
{
	public Animator m_Animator;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
	}
}
