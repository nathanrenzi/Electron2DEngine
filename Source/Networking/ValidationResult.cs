namespace Electron2D.Networking
{
    public readonly struct ValidationResult
    {
        public bool Success { get; init; }
        public string FailureReason { get; init; }

        public static ValidationResult Allowed() =>
            new ValidationResult { Success = true };
        public static ValidationResult Denied(string reason) =>
            new ValidationResult { Success = false, FailureReason = reason };
    }
}
