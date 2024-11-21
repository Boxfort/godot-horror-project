using System;
using System.Threading.Tasks;
using Godot;

public class CommandEvaluator
{
    public CommandEvaluator() { }

    public async Task EvaluateCommand(string command, Terminal terminal)
    {
        _ = terminal.AddLine(terminal.getPromptPrefix() + command, true);

        String[] tokens = command.Split(' ');

        if (tokens.Length < 1)
            return;

        switch (command) {
            case "help":
                await HelpCommand(terminal);
                break;
            case "blocks":
                await BlocksCommand(terminal);
                break;
            case "units":
                await UnitsCommand(terminal);
                break;
            default:
                await terminal.AddLine("command not found: " + tokens[0]);
                break;
        }
    }

    private async Task HelpCommand(Terminal terminal)
    {
        await terminal.AddLine("available commands:");
        await terminal.AddLine("  'help'   - show this message");
        await terminal.AddLine("  'blocks' - list known residential blocks");
        await terminal.AddLine("  'units'  - list known units in residential block");
    }

    private async Task BlocksCommand(Terminal terminal)
    {
        await terminal.AddLine("BLOCK  EXTENSION  RESIDENTS  STATUS");
        await terminal.AddLine("----------------------------------------");
        await terminal.AddLine("ECHO   512        7          SECURE");
        await terminal.AddLine("HOTEL  543        6          SECURE");
        await terminal.AddLine("NOVA   502        1          COMPROMISED");
    }

    private async Task UnitsCommand(Terminal terminal)
    {
        var prompt = terminal.AddPrompt("INPUT BLOCK NAME: ");

        prompt.OnLineSubmitted += async (block) =>
        {
            prompt.IsActive = false;

            if (block.ToLower() == "echo") 
            {
                await terminal.AddLine("            ECHO BLOCK               ");
                await terminal.AddLine("UNIT   PHONE    RESIDENTS  STATUS    ");
                await terminal.AddLine("---------------------------------");
                await terminal.AddLine("102    512-102  1          GOOD");
                await terminal.AddLine("103    512-103  2          GOOD");
                await terminal.AddLine("201    512-201  1          GOOD");
            } else {
                await terminal.AddLine("BLOCK DOES NOT EXIST");
            }
        };

        await prompt.ToSignal(prompt, InputPrompt.SignalName.OnLineSubmitted);

    }
}
