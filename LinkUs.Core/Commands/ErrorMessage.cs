using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Core.Commands
{
    public class ErrorMessage
    {
        private static readonly string Separator = "----- End of exception ----- " + Environment.NewLine;

        public string Message { get; set; }
        public string FullError { get; set; }

        public ErrorMessage() { }
        public ErrorMessage(Exception exception)
        {
            Message = exception.Message;
            FullError = BuildInlineError(exception);
        }

        private static string BuildInlineError(Exception exception)
        {
            var errors = new List<Exception>();
            while (exception != null) {
                errors.Add(exception);
                exception = exception.InnerException;
            }
            return string.Join(Separator, errors.Select(x=>x.ToString()));
        }
    }
}