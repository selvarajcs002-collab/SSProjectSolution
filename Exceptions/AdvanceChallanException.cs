using System;

namespace SSProjectSolution.Exceptions
{
    public class AdvanceAmountNotFoundException : Exception
    {
        public AdvanceAmountNotFoundException()
            : base("Advance amount record not found.")
        {
        }
    }

    public class AdvanceChallanValidationException : Exception
    {
        public AdvanceChallanValidationException(string message)
            : base(message)
        {
        }
    }
}
