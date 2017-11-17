using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Core.Commands
{
    public class ErrorMessage
    {
        public string Error { get; set; }

        public ErrorMessage() { }
        public ErrorMessage(string error)
        {
            Error = error;
        }
        public ErrorMessage(Exception exception)
        {
            Error = GetMessages(exception);
        }

        private string GetMessages(Exception exception)
        {
            var errors = new List<Exception>();
            while (exception != null) {
                errors.Add(exception);
                exception = exception.InnerException;
            }
            return string.Join(Environment.NewLine, errors.Select(x=>x.Message));
        }
    }
}