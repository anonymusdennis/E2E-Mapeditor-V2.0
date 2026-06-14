using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ;

public class TextureWriter
{
	public enum tTextureFormat
	{
		Unknown,
		ARGB32,
		ARGB2101010,
		RHalf,
		RGHalf,
		ARGBHalf,
		RFloat,
		RGFloat,
		ARGBFloat,
		RInt,
		RGInt,
		ARGBInt
	}

	public enum tDataFormat
	{
		Unknown,
		Half,
		Half2,
		Half3,
		Half4,
		Float,
		Float2,
		Float3,
		Float4,
		Int,
		Int2,
		Int3,
		Int4,
		LInt
	}

	public static tTextureFormat GetTextureFormat(RenderTexture v)
	{
		return v.format switch
		{
			RenderTextureFormat.ARGB32 => tTextureFormat.ARGB32, 
			RenderTextureFormat.RHalf => tTextureFormat.RHalf, 
			RenderTextureFormat.RGHalf => tTextureFormat.RGHalf, 
			RenderTextureFormat.ARGBHalf => tTextureFormat.ARGBHalf, 
			RenderTextureFormat.RFloat => tTextureFormat.RFloat, 
			RenderTextureFormat.RGFloat => tTextureFormat.RGFloat, 
			RenderTextureFormat.ARGBFloat => tTextureFormat.ARGBFloat, 
			RenderTextureFormat.RInt => tTextureFormat.RInt, 
			RenderTextureFormat.RGInt => tTextureFormat.RGInt, 
			RenderTextureFormat.ARGBInt => tTextureFormat.ARGBInt, 
			_ => tTextureFormat.Unknown, 
		};
	}

	public static tTextureFormat GetTextureFormat(Texture2D v)
	{
		return v.format switch
		{
			TextureFormat.ARGB32 => tTextureFormat.ARGB32, 
			TextureFormat.RHalf => tTextureFormat.RHalf, 
			TextureFormat.RGHalf => tTextureFormat.RGHalf, 
			TextureFormat.RGBAHalf => tTextureFormat.ARGBHalf, 
			TextureFormat.RFloat => tTextureFormat.RFloat, 
			TextureFormat.RGFloat => tTextureFormat.RGFloat, 
			TextureFormat.RGBAFloat => tTextureFormat.ARGBFloat, 
			_ => tTextureFormat.Unknown, 
		};
	}

	[DllImport("TextureWriter")]
	private static extern int tWriteTexture(IntPtr dst_tex, int dst_width, int dst_height, tTextureFormat dst_fmt, IntPtr src, int src_num, tDataFormat src_fmt);

	public static bool Write(RenderTexture dst_tex, IntPtr src, int src_num, tDataFormat src_fmt)
	{
		return tWriteTexture(dst_tex.GetNativeTexturePtr(), dst_tex.width, dst_tex.height, GetTextureFormat(dst_tex), src, src_num, src_fmt) != 0;
	}

	public static bool Write(Texture2D dst_tex, IntPtr src, int src_num, tDataFormat src_fmt)
	{
		return tWriteTexture(dst_tex.GetNativeTexturePtr(), dst_tex.width, dst_tex.height, GetTextureFormat(dst_tex), src, src_num, src_fmt) != 0;
	}
}
