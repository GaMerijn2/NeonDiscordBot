using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading.Tasks;

internal class Ticketing : BaseCommandModule
{
    [Command("ticketsetup")]
    public async Task TicketSetup(CommandContext ctx)
    {
        var createTicketButton = new DiscordButtonComponent(ButtonStyle.Primary, "create_ticket", "Create a Ticket");

        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithContent("Click the button below to create a new ticket.")
            .AddComponents(createTicketButton));
    }

    public async Task CreateTicket(ComponentInteractionCreateEventArgs e)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (member == null)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Error: Could not find your member object."));
            return;
        }

        var everyoneRole = e.Guild.EveryoneRole;
        var staffRole = e.Guild.Roles.Values.FirstOrDefault(role => role.Name == "Staff");
        var memberRole = e.Guild.Roles.Values.FirstOrDefault(role => role.Name == "【💻】Nerd");
        var gitMasterRole = e.Guild.Roles.Values.FirstOrDefault(role => role.Name == "【🛠️】Git Master");

        if (staffRole == null || memberRole == null || gitMasterRole == null)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Error: Required roles not found."));
            return;
        }

        var permissionsStaff = new DiscordOverwriteBuilder(staffRole)
            .Allow(Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory | Permissions.ManageChannels);

        var permissionsMember = new DiscordOverwriteBuilder(memberRole)
            .Deny(Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory);

        var permissionsUser = new DiscordOverwriteBuilder(member)
            .Allow(Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory);

        var permissionsEveryone = new DiscordOverwriteBuilder(everyoneRole)
            .Deny(Permissions.AccessChannels);

        string channelName = $"ticket-{member.Username.ToLower()}";
        var category = e.Guild.Channels.Values.FirstOrDefault(c => c.Name == "Tickets" && c.Type == ChannelType.Category);

        if (category == null)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Error: No 'Tickets' category found."));
            return;
        }

        var ticketChannel = await e.Guild.CreateTextChannelAsync(channelName, parent: category, overwrites: new[]
        {
            permissionsStaff,
            permissionsMember,
            permissionsEveryone,
            permissionsUser
        });


        var deleteTicketButton = new DiscordButtonComponent(ButtonStyle.Danger, "delete_ticket", "Delete Ticket");
        await ticketChannel.SendMessageAsync($"Hello {member.Mention}, please provide all relevant information or questions you have here. ||{gitMasterRole.Mention}||");
        await ticketChannel.SendMessageAsync(new DiscordMessageBuilder()
            .WithContent("Click the button below to delete this ticket.")
            .AddComponents(deleteTicketButton));
    }

    private async Task DeleteTicket(ComponentInteractionCreateEventArgs e)
    {
        var channel = e.Channel;

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Are you sure you want to delete this ticket channel? This action cannot be undone."));


        try
        {
            await channel.DeleteAsync("Channel deleted by user request.");
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("The ticket channel has been deleted."));
        }
        catch (Exception ex)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Failed to delete the channel: {ex.Message}"));
        }
    }

    public async Task HandleInteraction(ComponentInteractionCreateEventArgs e)
    {
        switch (e.Id)
        {
            case "create_ticket":
                await CreateTicket(e);
                break;

            case "delete_ticket":
                await DeleteTicket(e);
                break;
        }
    }
}
