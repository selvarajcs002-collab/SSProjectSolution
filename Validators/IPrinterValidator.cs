namespace SSProjectSolution.Validators
{
    public interface IPrinterValidator
    {
        (bool IsValid, string ErrorMessage) ValidatePrinter(string printerName);
    }
}
