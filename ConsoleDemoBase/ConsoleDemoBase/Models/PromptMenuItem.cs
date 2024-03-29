namespace ConsoleDemoBase.Models;
public record PromptMenuItem(int Id, string Text)
{
    public override string ToString() => Text;
}
