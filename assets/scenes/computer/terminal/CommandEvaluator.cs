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
        await terminal.AddLine("  'units'  - list known residential units");
        await terminal.AddLine("  'unlock' - unlock a unit's security door");
    }

    private async Task BlocksCommand(Terminal terminal)
    {
        await terminal.AddLine("BLOCK  EXTENSION  RESIDENTS  STATUS");
        await terminal.AddLine("----------------------------------------");
        await terminal.AddLine("NOVA   512        7          COMPROMISED");
        await terminal.AddLine("ECHO   506        12         SECURE");
        await terminal.AddLine("HOTEL  543        9          SECURE");
        await terminal.AddLine("TSAR   533        0          CONDEMNED");
        await terminal.AddLine("INCH   591        0          CONDEMNED");
    }

    private async Task UnitsCommand(Terminal terminal)
    {
        var prompt = terminal.AddPrompt("INPUT BLOCK NAME: ");

        prompt.OnLineSubmitted += async (block) =>
        {
            prompt.IsActive = false;

            var blockName = block.ToLower();

            if (blockName == "nova") 
            {
                await terminal.AddLine("              + NOVA BLOCK +              ");
                await terminal.AddLine("UNIT   PHONE    RESIDENTS  SURNAME  STATUS");
                await terminal.AddLine("------------------------------------------");
                await terminal.AddLine("101    512-101  0          ???      GOOD");
                await terminal.AddLine("102    512-102  0          ???      GOOD");
                await terminal.AddLine("103    512-103  1          ???      GOOD");
                await terminal.AddLine("104    512-101  2          ???      GOOD");
                await terminal.AddLine("201    512-201  1          ???      GOOD");
                await terminal.AddLine("202    512-202  0          ???      GOOD");
                await terminal.AddLine("203    512-203  1          ???      GOOD");
                await terminal.AddLine("204    512-204  1          ???      GOOD");
                await terminal.AddLine("301    512-301  0          ???      GOOD");
                await terminal.AddLine("302    512-302  1          ???      GOOD");
                await terminal.AddLine("303    512-303  0          ???      GOOD");
                await terminal.AddLine("304    512-304  1          ???      GOOD");
            } else if (blockName == "echo" || blockName == "hotel") {
                await terminal.AddLine("ACCESS DENIED");
            } else if (blockName == "tsar" || blockName == "inch") {
                await terminal.AddLine("BLOCK IS CONDEMNED");
            } else {
                await terminal.AddLine("BLOCK DOES NOT EXIST");
            }
        };

        await prompt.ToSignal(prompt, InputPrompt.SignalName.OnLineSubmitted);

    }
}
