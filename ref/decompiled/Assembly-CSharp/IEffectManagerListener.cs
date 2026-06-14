using UnityEngine;

public interface IEffectManagerListener
{
	void OnEffectSpawned(GameObject newEffect, EffectManager.effectType type);
}
