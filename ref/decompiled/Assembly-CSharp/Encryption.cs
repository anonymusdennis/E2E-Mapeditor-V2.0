using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Encryption
{
	public const string DEFAULT_PASSWORD = "default";

	private static char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".ToCharArray();

	private static string GeneratePassword()
	{
		return "Ilovedanbecauseheislovely" + Platform.GetInstance().GetUniqueID();
	}

	public static string Encrypt(object value, string Salt = "of all the flavours you choose to be salty", string HashAlgorithm = "SHA1", int PasswordIterations = 2, int KeySize = 256, string password = "")
	{
		if (value == null)
		{
			return string.Empty;
		}
		int max = letters.Length;
		char[] array = new char[16];
		for (int i = 0; i < 16; i++)
		{
			array[i] = letters[UnityEngine.Random.Range(0, max)];
		}
		byte[] bytes = Encoding.ASCII.GetBytes(array);
		byte[] bytes2 = Encoding.ASCII.GetBytes(Salt);
		byte[] array2 = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			binaryFormatter.Serialize(memoryStream, value);
			array2 = memoryStream.ToArray();
		}
		if (string.IsNullOrEmpty(password))
		{
			password = GeneratePassword();
		}
		PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, bytes2, HashAlgorithm, PasswordIterations);
		byte[] bytes3 = passwordDeriveBytes.GetBytes(KeySize / 8);
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.CBC;
		byte[] inArray = null;
		using (ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes3, bytes))
		{
			using MemoryStream memoryStream2 = new MemoryStream();
			using CryptoStream cryptoStream = new CryptoStream(memoryStream2, transform, CryptoStreamMode.Write);
			cryptoStream.Write(array2, 0, array2.Length);
			cryptoStream.FlushFinalBlock();
			inArray = memoryStream2.ToArray();
			memoryStream2.Close();
			cryptoStream.Close();
		}
		rijndaelManaged.Clear();
		return "Ver:1\n" + new string(array) + Convert.ToBase64String(inArray);
	}

	public static string Decrypt(string CipherText, string password = "", string Salt = "of all the flavours you choose to be salty", string HashAlgorithm = "SHA1", int PasswordIterations = 2, int KeySize = 256)
	{
		if (string.IsNullOrEmpty(CipherText))
		{
			return string.Empty;
		}
		char[] array = new char[16];
		string value = "Ver:";
		if (string.IsNullOrEmpty(password))
		{
			password = GeneratePassword();
		}
		int result = 0;
		int num = CipherText.IndexOf(':') + 1;
		int num2 = CipherText.IndexOf('\n');
		if (CipherText.StartsWith(value))
		{
			int.TryParse(CipherText.Substring(num, num2 - num), out result);
		}
		switch (result)
		{
		case 0:
			array = "i()l0veDaNL()adz".ToCharArray();
			break;
		case 1:
			array = CipherText.Substring(num2 + 1, 16).ToCharArray();
			CipherText = CipherText.Substring(num2 + 1 + 16);
			break;
		default:
			throw new Exception("unknown version for encrypted file.");
		}
		try
		{
			byte[] bytes = Encoding.ASCII.GetBytes(array);
			byte[] bytes2 = Encoding.ASCII.GetBytes(Salt);
			byte[] array2 = Convert.FromBase64String(CipherText);
			PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, bytes2, HashAlgorithm, PasswordIterations);
			byte[] bytes3 = passwordDeriveBytes.GetBytes(KeySize / 8);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Mode = CipherMode.CBC;
			byte[] array3 = new byte[array2.Length];
			using (ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes3, bytes))
			{
				using MemoryStream memoryStream = new MemoryStream(array2);
				using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
				cryptoStream.Read(array3, 0, array3.Length);
				memoryStream.Close();
				cryptoStream.Close();
			}
			rijndaelManaged.Clear();
			if (array3 != null)
			{
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					memoryStream2.Write(array3, 0, array3.Length);
					memoryStream2.Seek(0L, SeekOrigin.Begin);
					return binaryFormatter.Deserialize(memoryStream2) as string;
				}
			}
		}
		catch
		{
			return null;
		}
		return null;
	}
}
