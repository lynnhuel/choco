﻿// Copyright © 2011 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.app.commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using attributes;
    using commandline;
    using configuration;
    using domain;
    using infrastructure.commands;
    using logging;
    using services;

    [CommandFor(CommandNameType.source)]
    public sealed class ChocolateySourceCommand : ICommand
    {
        private readonly IChocolateyConfigSettingsService _configSettingsService;

        public ChocolateySourceCommand(IChocolateyConfigSettingsService configSettingsService)
        {
            _configSettingsService = configSettingsService;
        }

        public void configure_argument_parser(OptionSet optionSet, ChocolateyConfiguration configuration)
        {
            configuration.Sources = string.Empty;

            optionSet
                .Add("n=|name=",
                     "Name - the name of the source. Required with some actions. Defaults to empty.",
                     option => configuration.SourceCommand.Name = option)
                .Add("s=|source=",
                     "Source - The source. Defaults to empty.",
                     option => configuration.Sources = option)
                .Add("u=|user=",
                     "User - used with authenticated feeds. Defaults to empty.",
                     option => configuration.SourceCommand.Username = option)
                .Add("p=|password=",
                     "Password - the user's password to the source. Encrypted in file.",
                     option => configuration.SourceCommand.Password = option)
                ;
        }

        public void handle_additional_argument_parsing(IList<string> unparsedArguments, ChocolateyConfiguration configuration)
        {
            configuration.Input = string.Join(" ", unparsedArguments);

            if (unparsedArguments.Count > 1)
            {
                throw new ApplicationException("A single sources command must be listed. Please see the help menu for those commands");
            }

            var command = SourceCommandType.unknown;
            Enum.TryParse(unparsedArguments.DefaultIfEmpty(string.Empty).FirstOrDefault(), true, out command);

            if (command == SourceCommandType.unknown) command = SourceCommandType.list;

            configuration.SourceCommand.Command = command;
        }

        public void handle_validation(ChocolateyConfiguration configuration)
        {
            if (configuration.SourceCommand.Command != SourceCommandType.list && string.IsNullOrWhiteSpace(configuration.SourceCommand.Name))
            {
                throw new ApplicationException("When specifying the subcommand '{0}', you must also specify --name.".format_with(configuration.SourceCommand.Command.to_string()));
            }
        }

        public void help_message(ChocolateyConfiguration configuration)
        {
            this.Log().Info(ChocolateyLoggers.Important, "Sources Command");
            this.Log().Info(@"
Chocolatey will allow you to interact with sources.

Usage: choco source [list]|add|remove|disable|enable [options/switches]

Examples:

 choco source   
 choco source list  
 choco source add -n=bob -s""https://somewhere/out/there/api/v2/""
 choco source add -n=bob -s""https://somewhere/out/there/api/v2/"" -u=bob -p=12345
 choco source disable -n=bob
 choco source enable -n=bob
 choco source remove -n=bob 

");
        }

        public void noop(ChocolateyConfiguration configuration)
        {
            _configSettingsService.noop(configuration);
        }

        public void run(ChocolateyConfiguration configuration)
        {
            switch (configuration.SourceCommand.Command)
            {
                case SourceCommandType.list:
                    _configSettingsService.source_list(configuration);
                    break;
                case SourceCommandType.add:
                    _configSettingsService.source_add(configuration);
                    break;
                case SourceCommandType.remove:
                    _configSettingsService.source_remove(configuration);
                    break;
                case SourceCommandType.disable:
                    _configSettingsService.source_disable(configuration);
                    break;
                case SourceCommandType.enable:
                    _configSettingsService.source_enable(configuration);
                    break;
            }
        }
    }
}