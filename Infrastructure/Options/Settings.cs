using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Options
{
    public class Settings : ISettings
    {
        public const string SectionName = "Settings";
        public string DefaultPassword { get; init; } = default!;
    }
}
