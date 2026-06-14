public enum AkGlobalCallbackLocation
{
	AkGlobalCallbackLocation_Register = 1,
	AkGlobalCallbackLocation_Begin = 2,
	AkGlobalCallbackLocation_PreProcessMessageQueueForRender = 4,
	AkGlobalCallbackLocation_BeginRender = 8,
	AkGlobalCallbackLocation_EndRender = 16,
	AkGlobalCallbackLocation_End = 32,
	AkGlobalCallbackLocation_Term = 64,
	AkGlobalCallbackLocation_Num = 7
}
