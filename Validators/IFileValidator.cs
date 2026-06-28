namespace SSProjectSolution.Validators
{
    public interface IFileValidator
    {
        (bool IsValid, string ErrorMessage) ValidateFolder(string folderPath);
        (bool IsValid, string ErrorMessage) ValidateFileName(string fileName);
    }
}
