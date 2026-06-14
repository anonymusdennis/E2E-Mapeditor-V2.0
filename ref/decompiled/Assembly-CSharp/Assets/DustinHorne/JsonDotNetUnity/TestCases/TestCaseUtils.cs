using System;
using System.Collections.Generic;
using Assets.DustinHorne.JsonDotNetUnity.TestCases.TestModels;
using UnityEngine;

namespace Assets.DustinHorne.JsonDotNetUnity.TestCases;

public static class TestCaseUtils
{
	private static System.Random _rnd = new System.Random();

	public static SampleBase GetSampleBase()
	{
		SampleBase sampleBase = new SampleBase();
		sampleBase.TextValue = Guid.NewGuid().ToString();
		sampleBase.NumberValue = _rnd.Next();
		int num = _rnd.Next();
		int num2 = _rnd.Next();
		int num3 = _rnd.Next();
		sampleBase.VectorValue = new Vector3(num, num2, num3);
		return sampleBase;
	}

	public static SampleChild GetSampleChid()
	{
		SampleChild sampleChild = new SampleChild();
		sampleChild.TextValue = Guid.NewGuid().ToString();
		sampleChild.NumberValue = _rnd.Next();
		int num = _rnd.Next();
		int num2 = _rnd.Next();
		int num3 = _rnd.Next();
		sampleChild.VectorValue = new Vector3(num, num2, num3);
		sampleChild.ObjectDictionary = new Dictionary<int, SimpleClassObject>();
		for (int i = 0; i < 4; i++)
		{
			SimpleClassObject simpleClassObject = GetSimpleClassObject();
			sampleChild.ObjectDictionary.Add(i, simpleClassObject);
		}
		sampleChild.ObjectList = new List<SimpleClassObject>();
		for (int j = 0; j < 4; j++)
		{
			SimpleClassObject simpleClassObject2 = GetSimpleClassObject();
			sampleChild.ObjectList.Add(simpleClassObject2);
		}
		return sampleChild;
	}

	public static SimpleClassObject GetSimpleClassObject()
	{
		SimpleClassObject simpleClassObject = new SimpleClassObject();
		simpleClassObject.TextValue = Guid.NewGuid().ToString();
		simpleClassObject.NumberValue = _rnd.Next();
		int num = _rnd.Next();
		int num2 = _rnd.Next();
		int num3 = _rnd.Next();
		simpleClassObject.VectorValue = new Vector3(num, num2, num3);
		return simpleClassObject;
	}
}
