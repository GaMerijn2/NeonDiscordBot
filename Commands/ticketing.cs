using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

internal class ticketing : BaseCommandModule
{
    [Command("createticket")]
    public async Task CreateTicket(CommandContext ctx)
    {
        await ctx.Channel.SendMessageAsync("Yoing");
    }
}

