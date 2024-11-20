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
                await terminal.AddLine("no one can help you");
                break;
            default:
                await terminal.AddLine("command not found: " + tokens[0]);
                break;
        }

    }
}
