/*using System.CommandLine;
using Microsoft.DotNet.Interactive;

namespace Dax.Interactive
{
    public class ChooseDaxKernelDirective : ChooseKernelDirective
    {
        public ChooseDaxKernelDirective(Kernel kernel) : base(kernel, $"Run a DAX query using the \"{kernel.Name}\" connection.")
        {
            Add(NameOption);
        }

        public Option<string> NameOption { get; } = new(
            "--name",
            description: "Specify the value name to store the results.",
            getDefaultValue: () => "lastResults");

    }
}*/