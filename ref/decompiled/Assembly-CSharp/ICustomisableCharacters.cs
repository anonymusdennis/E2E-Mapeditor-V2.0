public interface ICustomisableCharacters
{
	Customisation GetCustomisationToModify();

	int GetCurrentCustomsiationIndex();

	CustomisationConstraint GetCustomisationConstraint();

	void OnCustomisationModified();
}
