namespace Authentication.Exceptions
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException() : base("Email already exists.") { }
    }

    public class IncorrectPasswordException : Exception
    {
        public IncorrectPasswordException() : base("Incorrect password.") { }
    }

    public class AccessDeniedException : UnauthorizedAccessException
    {
        public AccessDeniedException() : base("Access denied.") { }
    }
}
