using System;

namespace Arbor.Processing
{
    public struct ExitCode(int code) : IEquatable<ExitCode>
    {
        public bool Equals(ExitCode other) => Code == other.Code;

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is ExitCode other && Equals(other);
        }

        public override int GetHashCode() => Code;

        public static bool operator ==(ExitCode left, ExitCode right) => left.Equals(right);

        public static bool operator !=(ExitCode left, ExitCode right) => !left.Equals(right);

        public int Code { get; } = code;

        public static implicit operator int(ExitCode exitCode) => exitCode.Code;

        public override string ToString()
        {
            string message = IsSuccess ? "Success" : "Failure";

            return $"EXIT CODE [{Code}] {message}";
        }

        private static readonly Lazy<ExitCode> LazySuccess = new(() => new ExitCode(0));

        private static readonly Lazy<ExitCode> LazyFailure = new(() => new ExitCode(1));

        public static ExitCode Success => LazySuccess.Value;

        public static ExitCode Failure => LazyFailure.Value;

        public static ExitCode Failed(int exitCode)
        {
            if (exitCode == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exitCode), "Exit code cannot be 0 when failed");
            }

            return new ExitCode(exitCode);
        }

        public bool IsSuccess => Code == 0;

        public int ToInt32() => Code;
    }
}