using System;
using UnityEngine;

public abstract class BuildingBlock_Single : BaseBuildingBlock
{
	public Vector2 m_Position = Vector2.zero;

	public UnityEngine.Object m_Prefab;

	public UnityEngine.Object m_VisualPrefab;

	public override void MakeVisualRepresentation(int iIndex)
	{
		UnityEngine.Object prefab = GetPrefab(iIndex, bVisual: true);
		if (prefab == null)
		{
			base.MakeVisualRepresentation(iIndex);
			return;
		}
		GameObject gameObject = null;
		gameObject = UnityEngine.Object.Instantiate(prefab, m_Representation.transform) as GameObject;
		gameObject.isStatic = false;
		gameObject.name = "Vis Rep " + iIndex;
		gameObject.SetActive(IsVariantDefault(iIndex));
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		m_Representations[iIndex] = gameObject;
		Component[] componentsInChildren = gameObject.GetComponentsInChildren(typeof(Component), includeInactive: true);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		int num = componentsInChildren.Length;
		Type[] array = new Type[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = componentsInChildren[i].GetType();
		}
		int num2 = 20;
		while (flag3 && num2 > 0)
		{
			flag3 = false;
			for (int j = 0; j < num; j++)
			{
				if (!(componentsInChildren[j] != null))
				{
					continue;
				}
				flag = false;
				flag2 = false;
				Type type = array[j];
				ProcessComponent(gameObject, componentsInChildren[j], type, ref flag, ref flag2, iIndex);
				if (!flag)
				{
					for (int k = 0; k < num; k++)
					{
						if (flag)
						{
							break;
						}
						if (!(componentsInChildren[k] != null))
						{
							continue;
						}
						RequireComponent[] array2 = Attribute.GetCustomAttributes(array[k], typeof(RequireComponent)) as RequireComponent[];
						if (array2.Length <= 0)
						{
							continue;
						}
						for (int num3 = array2.Length - 1; num3 >= 0; num3--)
						{
							if (array2[num3].m_Type0 == type || (array2[num3].m_Type0 != null && type.IsSubclassOf(array2[num3].m_Type0)) || array2[num3].m_Type1 == type || (array2[num3].m_Type1 != null && type.IsSubclassOf(array2[num3].m_Type1)) || array2[num3].m_Type2 == type || (array2[num3].m_Type2 != null && type.IsSubclassOf(array2[num3].m_Type2)))
							{
								flag = true;
								flag3 = true;
								break;
							}
						}
					}
				}
				if (!flag)
				{
					try
					{
						UnityEngine.Object.DestroyImmediate(componentsInChildren[j]);
						flag2 = true;
						flag3 = true;
					}
					catch (Exception)
					{
					}
				}
				if (flag2)
				{
					if (flag)
					{
					}
					componentsInChildren[j] = null;
				}
			}
			num2--;
		}
		if (num2 == 0)
		{
		}
		bool flag4 = true;
		while (flag4)
		{
			flag4 = false;
			Transform[] componentsInChildren2 = gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren2)
			{
				Component[] components = transform.GetComponents<Component>();
				string value = string.Empty;
				if (!transform.gameObject.activeSelf)
				{
					value = "Disabled";
				}
				else if (transform.childCount == 0 && components.Length <= 1)
				{
					value = "Only one Component";
				}
				if (!string.IsNullOrEmpty(value) && transform.gameObject != gameObject)
				{
					UnityEngine.Object.DestroyImmediate(transform.gameObject);
					flag4 = true;
					break;
				}
			}
		}
	}

	public override void MakeActualObject(int iIndex)
	{
		UnityEngine.Object prefab = GetPrefab(iIndex);
		if (prefab != null)
		{
			m_RealObjects[iIndex] = UnityEngine.Object.Instantiate(prefab, m_RealObject.transform) as GameObject;
			m_RealObjects[iIndex].name = prefab.name + " - " + iIndex;
			BuildingBlockHelper.AddLayerShift(this, m_RealObjects[iIndex]);
		}
		else
		{
			m_RealObjects[iIndex] = new GameObject("Empty Object " + iIndex);
			m_RealObjects[iIndex].transform.parent = m_RealObject.transform;
		}
		m_RealObjects[iIndex].SetActive(value: false);
		m_RealObjects[iIndex].transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	public virtual UnityEngine.Object GetPrefab(int iIndex, bool bVisual = false)
	{
		return m_Prefab;
	}

	public virtual bool IsVariantDefault(int iVariant)
	{
		return iVariant == 0;
	}

	public override CompletionState GetBlockCompletionState(ref string strProblems, bool bCreateErrorString = false)
	{
		CompletionState result = base.GetBlockCompletionState(ref strProblems, bCreateErrorString);
		if (m_Prefab == null)
		{
			if (bCreateErrorString)
			{
				strProblems += "There is no default prefab set up\n";
			}
			result = CompletionState.Unfinished;
		}
		return result;
	}
}
