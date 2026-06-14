public static class NodeHelp
{
	public const int LEFT = 0;

	public const int RIGHT = 1;

	public const int TOP = 2;

	public const int BOTTOM = 3;

	public const int LEFT_NODE_LINK = 1;

	public const int RIGHT_NODE_LINK = 2;

	public const int TOP_NODE_LINK = 4;

	public const int BOTTOM_NODE_LINK = 8;

	public const int ALLOW_TOP_INPUT = 1;

	public const int ALLOW_TOP_OUTPUT = 16;

	public const int ALLOW_LEFT_INPUT = 256;

	public const int ALLOW_LEFT_OUTPUT = 4096;

	public const int ALLOW_RIGHT_INPUT = 65536;

	public const int ALLOW_RIGHT_OUTPUT = 1048576;

	public const int ALLOW_BOTTOM_INPUT = 16777216;

	public const int ALLOW_BOTTOM_OUTPUT = 268435456;

	public static int EncodeNode(int nodeID, int ourDirection, int otherDirection)
	{
		int num = 0;
		num = nodeID & 0xFFFF;
		num |= ourDirection << 16;
		return num | (otherDirection << 20);
	}

	public static void DecodeNode(int encoded, out int nodeID, out int ourDirection, out int otherDirection)
	{
		nodeID = encoded & 0xFFFF;
		ourDirection = (encoded & 0xF0000) >> 16;
		otherDirection = (encoded & 0xF00000) >> 20;
	}

	public static int GetDirectionAsFlag(int direction)
	{
		return direction switch
		{
			0 => 1, 
			1 => 2, 
			2 => 4, 
			_ => 8, 
		};
	}

	public static int GetDirectionFromFlag(int flag)
	{
		return flag switch
		{
			1 => 0, 
			2 => 1, 
			4 => 2, 
			_ => 3, 
		};
	}
}
