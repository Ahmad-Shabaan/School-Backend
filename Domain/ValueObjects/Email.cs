using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("Email is required.");

            if (!value.Contains("@"))
                throw new Exception("Invalid email.");

            Value = value;
        }
    }
}
