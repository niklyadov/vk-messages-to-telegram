using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VkToTg.Commands.Core;

namespace VkToTg.Services.Telegram
{
    public class CommandsManager
    {
        IServiceScopeFactory _serviceScopeFactory;
        ILogger<CommandsManager> _logger;

        public CommandsManager(IServiceScopeFactory serviceScopeFactory, ILogger<CommandsManager> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public BaseCommand GetCommandByName(string commandName)
        {
            BaseCommand command = null;

            try
            {
                var commandType = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsDefined(typeof(Attributes.CommandAttribute)))
                .First(t => t.GetCustomAttributes<Attributes.CommandAttribute>()
                                .Count(a => a.Name == commandName) > 0);

                command = Activator.CreateInstance(commandType, _serviceScopeFactory) as BaseCommand;
            }
            catch (Exception ex) when (command == null)
            {
                command = new Commands.DefaultCommand(_serviceScopeFactory);

                if (ex != null)
                {
                    _logger.LogError($"Failed to find command {ex.Message}.");
                }
            }

            return command;
        }

        public ICollection<Attributes.CommandAttribute> GetAllCommandsAttribute()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsDefined(typeof(Attributes.CommandAttribute)))
                .Select(t => t.GetCustomAttributes<Attributes.CommandAttribute>().FirstOrDefault())
                .ToList();
        }
    }
}