using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }

        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new Exception();

            Amount = amount;
            Currency = currency;
        }
    }
}
