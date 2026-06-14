using System;
using UnityEngine;

namespace Slate;

public static class Easing
{
	public static Func<float, float, float, float>[] EaseFunctions = new Func<float, float, float, float>[31]
	{
		Linear, QuadraticIn, QuadraticOut, QuadraticInOut, QuarticIn, QuarticOut, QuarticInOut, QuinticIn, QuinticOut, QuinticInOut,
		CubicIn, CubicOut, CubicInOut, ExponentialIn, ExponentialOut, ExponentialInOut, CircularIn, CircularOut, CircularInOut, SinusoidalIn,
		SinusoidalOut, SinusoidalInOut, ElasticIn, ElasticOut, ElasticInOut, BounceIn, BounceOut, BounceInOut, BackIn, BackOut,
		BackInOut
	};

	public static float Ease(EaseType type, float from, float to, float t)
	{
		if (t <= 0f)
		{
			return from;
		}
		if (t >= 1f)
		{
			return to;
		}
		return Function(type)(from, to, t);
	}

	public static Vector3 Ease(EaseType type, Vector3 from, Vector3 to, float t)
	{
		if (t <= 0f)
		{
			return from;
		}
		if (t >= 1f)
		{
			return to;
		}
		return Vector3.LerpUnclamped(from, to, Function(type)(0f, 1f, t));
	}

	public static Quaternion Ease(EaseType type, Quaternion from, Quaternion to, float t)
	{
		if (t <= 0f)
		{
			return from;
		}
		if (t >= 1f)
		{
			return to;
		}
		return Quaternion.LerpUnclamped(from, to, Function(type)(0f, 1f, t));
	}

	public static Color Ease(EaseType type, Color from, Color to, float t)
	{
		if (t <= 0f)
		{
			return from;
		}
		if (t >= 1f)
		{
			return to;
		}
		return Color.LerpUnclamped(from, to, Function(type)(0f, 1f, t));
	}

	public static float Difference(this float f, float a, float b)
	{
		if (a > b)
		{
			return 0f - Mathf.Abs(a - b);
		}
		return Mathf.Abs(a - b);
	}

	public static float Linear(float from, float to, float t)
	{
		return Mathf.Lerp(from, to, t);
	}

	public static float QuadraticIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		return from + num * t * t;
	}

	public static float QuadraticOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		return from + num * t * (2f - t);
	}

	public static float QuadraticInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = ((!((t *= 2f) < 1f)) ? (-0.5f * ((t -= 1f) * (t - 2f) - 1f)) : (0.5f * t * t));
		return from + num * num2;
	}

	public static float QuarticIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		return from + num * t * t * t * t;
	}

	public static float QuarticOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 1f - (t -= 1f) * t * t * t;
		return from + num * num2;
	}

	public static float QuarticInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		if ((t *= 2f) < 1f)
		{
			return from + num * 0.5f * t * t * t * t;
		}
		return from + num * -0.5f * ((t -= 2f) * t * t * t - 2f);
	}

	public static float QuinticIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		return from + num * t * t * t * t * t;
	}

	public static float QuinticOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = (t -= 1f) * t * t * t * t + 1f;
		return from + num * num2;
	}

	public static float QuinticInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		if ((t *= 2f) < 1f)
		{
			return from + num * 0.5f * t * t * t * t * t;
		}
		return from + num * 0.5f * ((t -= 2f) * t * t * t * t + 2f);
	}

	public static float CubicIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		return from + num * t * t * t;
	}

	public static float CubicOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = (t -= 1f) * t * t + 1f;
		return from + num * num2;
	}

	public static float CubicInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to) * 0.5f;
		float num2 = ((!((t *= 2f) < 1f)) ? ((t -= 2f) * t * t + 2f) : (t * t * t));
		return from + num * num2;
	}

	public static float SinusoidalIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 1f - Mathf.Cos(t * (float)Math.PI / 2f);
		return from + num * num2;
	}

	public static float SinusoidalOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = Mathf.Sin(t * (float)Math.PI / 2f);
		return from + num * num2;
	}

	public static float SinusoidalInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 0.5f * (1f - Mathf.Cos((float)Math.PI * t));
		return from + num * num2;
	}

	public static float ExponentialIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = ((!Mathf.Approximately(0f, t)) ? Mathf.Pow(1024f, t - 1f) : 0f);
		return from + num * num2;
	}

	public static float ExponentialOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = ((!Mathf.Approximately(1f, t)) ? (1f - Mathf.Pow(2f, -10f * t)) : 1f);
		return from + num * num2;
	}

	public static float ExponentialInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		if (Mathf.Approximately(0f, t))
		{
			return from;
		}
		if (Mathf.Approximately(1f, t))
		{
			return from + num;
		}
		if ((t *= 2f) < 1f)
		{
			return from + num * 0.5f * Mathf.Pow(1024f, t - 1f);
		}
		return from + num * 0.5f * (0f - Mathf.Pow(2f, -10f * (t - 1f)) + 2f);
	}

	public static float CircularIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 1f - Mathf.Sqrt(1f - t * t);
		return from + num * num2;
	}

	public static float CircularOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		return from + num * Mathf.Sqrt(1f - (t -= 1f) * t);
	}

	public static float CircularInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		if ((t *= 2f) < 1f)
		{
			return from + num * -0.5f * (Mathf.Sqrt(1f - t * t) - 1f);
		}
		return from + num * 0.5f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f);
	}

	public static float ElasticIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 0.1f;
		float num3 = 0.4f;
		if (Mathf.Approximately(0f, t))
		{
			return from;
		}
		if (Mathf.Approximately(1f, t))
		{
			return from + num;
		}
		float num4;
		if (num2 < 1f)
		{
			num2 = 1f;
			num4 = num3 / 4f;
		}
		else
		{
			num4 = num3 * Mathf.Asin(1f / num2) / ((float)Math.PI * 2f);
		}
		return from + num * (0f - num2 * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - num4) * ((float)Math.PI * 2f) / num3));
	}

	public static float ElasticOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 0.1f;
		float num3 = 0.4f;
		if (Mathf.Approximately(0f, t))
		{
			return from;
		}
		if (Mathf.Approximately(1f, t))
		{
			return from + num;
		}
		float num4;
		if (num2 < 1f)
		{
			num2 = 1f;
			num4 = num3 / 4f;
		}
		else
		{
			num4 = num3 * Mathf.Asin(1f / num2) / ((float)Math.PI * 2f);
		}
		return from + num * (num2 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num4) * ((float)Math.PI * 2f) / num3) + 1f);
	}

	public static float ElasticInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 0.1f;
		float num3 = 0.4f;
		if (Mathf.Approximately(0f, t))
		{
			return from;
		}
		if (Mathf.Approximately(1f, t))
		{
			return from + num;
		}
		float num4;
		if (num2 < 1f)
		{
			num2 = 1f;
			num4 = num3 / 4f;
		}
		else
		{
			num4 = num3 * Mathf.Asin(1f / num2) / ((float)Math.PI * 2f);
		}
		float num5 = ((!((t *= 2f) < 1f)) ? (num2 * Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t - num4) * ((float)Math.PI * 2f) / num3) * 0.5f + 1f) : (-0.5f * (num2 * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - num4) * ((float)Math.PI * 2f) / num3))));
		return from + num * num5;
	}

	public static float BounceIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 1f - BounceOut(0f, 1f, 1f - t);
		return from + num * num2;
	}

	public static float BounceOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = ((t < 0.36363637f) ? (7.5625f * t * t) : ((t < 0.72727275f) ? (7.5625f * (t -= 0.54545456f) * t + 0.75f) : ((!(t < 0.90909094f)) ? (7.5625f * (t -= 21f / 22f) * t + 63f / 64f) : (7.5625f * (t -= 0.8181818f) * t + 0.9375f))));
		return from + num * num2;
	}

	public static float BounceInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = ((!(t < 0.5f)) ? (BounceOut(0f, 1f, t * 2f - 1f) * 0.5f + 0.5f) : (BounceIn(0f, 1f, t * 2f) * 0.5f));
		return from + num * num2;
	}

	public static float BackIn(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 1.70158f;
		return from + num * t * t * ((num2 + 1f) * t - num2);
	}

	public static float BackOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 1.70158f;
		float num3 = (t -= 1f) * t * ((num2 + 1f) * t + num2) + 1f;
		return from + num * num3;
	}

	public static float BackInOut(float from, float to, float t)
	{
		t = Mathf.Clamp01(t);
		float num = from.Difference(from, to);
		float num2 = 2.5949094f;
		if ((t *= 2f) < 1f)
		{
			return from + num * 0.5f * (t * t * ((num2 + 1f) * t - num2));
		}
		return from + num * 0.5f * ((t -= 2f) * t * ((num2 + 1f) * t + num2) + 2f);
	}

	public static Func<float, float, float, float> Function(EaseType type)
	{
		return type switch
		{
			EaseType.Linear => EaseFunctions[0], 
			EaseType.QuadraticIn => EaseFunctions[1], 
			EaseType.QuadraticOut => EaseFunctions[2], 
			EaseType.QuadraticInOut => EaseFunctions[3], 
			EaseType.QuarticIn => EaseFunctions[4], 
			EaseType.QuarticOut => EaseFunctions[5], 
			EaseType.QuarticInOut => EaseFunctions[6], 
			EaseType.QuinticIn => EaseFunctions[7], 
			EaseType.QuinticOut => EaseFunctions[8], 
			EaseType.QuinticInOut => EaseFunctions[9], 
			EaseType.CubicIn => EaseFunctions[10], 
			EaseType.CubicOut => EaseFunctions[11], 
			EaseType.CubicInOut => EaseFunctions[12], 
			EaseType.ExponentialIn => EaseFunctions[13], 
			EaseType.ExponentialOut => EaseFunctions[14], 
			EaseType.ExponentialInOut => EaseFunctions[15], 
			EaseType.CircularIn => EaseFunctions[16], 
			EaseType.CircularOut => EaseFunctions[17], 
			EaseType.CircularInOut => EaseFunctions[18], 
			EaseType.SinusoidalIn => EaseFunctions[19], 
			EaseType.SinusoidalOut => EaseFunctions[20], 
			EaseType.SinusoidalInOut => EaseFunctions[21], 
			EaseType.ElasticIn => EaseFunctions[22], 
			EaseType.ElasticOut => EaseFunctions[23], 
			EaseType.ElasticInOut => EaseFunctions[24], 
			EaseType.BounceIn => EaseFunctions[25], 
			EaseType.BounceOut => EaseFunctions[26], 
			EaseType.BounceInOut => EaseFunctions[27], 
			EaseType.BackIn => EaseFunctions[28], 
			EaseType.BackOut => EaseFunctions[29], 
			EaseType.BackInOut => EaseFunctions[30], 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
