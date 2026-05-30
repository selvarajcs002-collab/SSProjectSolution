namespace SSProjectSolution.Data
{
    public static class SPConstants
    {
        public const string ManageUser = "sp_ManageUser";
        public const string ManageCompany = "sp_ManageCompany";
        public const string LoginUser = "sp_LoginUser";
        public const string GetCompanyList = "sp_GetCompanyList";
        public const string GetCompanyById = "sp_GetCompanyById";
        public const string InsertInward = "sp_InsertInward";
        public const string InsertInwardSizeCounts = "sp_InsertInwardSizeCounts";
        public const string SaveInwardMeter = "SP_SAVE_INWARD_METER";
        public const string GetSizesByColourStyle = "sp_GetSizes_ByColour_Style";
        public const string GetInwardByCompanyAndDc = "sp_GetInward_ByCompany_And_DCNo";
        public const string UpdateInward = "sp_UpdateInward";
        public const string GetDesignStyleColourByCompany = "sp_GetDesignStyleColour_ByCompany";
        
        // Employee
        public const string ManageEmployee = "sp_ManageEmployee";
        public const string GetAllEmployees = "sp_GetAllEmployees";
        public const string GetEmployeeById = "sp_GetEmployeeById";
        public const string DeleteEmployee = "sp_DeleteEmployee";
        public const string SaveAttendance = "sp_SaveAttendance";
        public const string GetAttendanceByDate = "sp_GetAttendanceByDate";
        public const string GetAttendanceByMonth = "sp_GetAttendanceByMonth";
        public const string GeneratePayroll = "sp_GeneratePayroll";
        public const string GetPayrollByMonth = "sp_GetPayrollByMonth";
        public const string GetPayrollSummary = "sp_GetPayrollSummary";
        
        public const string SaveOutward = "usp_SaveOutward";
        public const string GetInwardOutwardDetailsFilter = "usp_GetInwardOutwardDetails_Filter";
        public const string GetInwardOutwardDetails = "usp_GetInwardOutwardDetails";
        public const string GetDetailsByIdMode = "usp_GetDetails_ById_Mode";

        // ── Meter-Based Outward (new — isolated) ───────────────────────────────
        public const string SaveMeterOutward = "SP_SAVE_OUTWARD_METER";
        public const string GetMetersByColourStyle = "sp_GetMeters_ByColour_Style";
    }
}
