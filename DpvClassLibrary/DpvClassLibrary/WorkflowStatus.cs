namespace DpvClassLibrary
{
	public enum WorkflowStatus
	{
		Empty,
		StartedAndReadyToSignIn,
		SignedInButNotCollecting,
		CollectingAndSendingData,
		SignOutSent,
		StoppedAndPreparingToExit
	}
}
