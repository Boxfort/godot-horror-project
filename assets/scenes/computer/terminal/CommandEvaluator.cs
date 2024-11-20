using System;
using Godot;

public class CommandEvaluator
{
    public CommandEvaluator() { }

    public void EvaluateCommand(string command, Terminal terminal)
    {
        terminal.AddLine(terminal.getPromptPrefix() + " " + command, true);

        String[] tokens = command.Split(' ');

        if (tokens.Length < 1)
            return;

        switch (command) {
            case "help":
                terminal.AddLine("no one can help you");
                break;
            default:
                terminal.AddLine("command not found: " + tokens[0]);
                break;
        }

    }
}
