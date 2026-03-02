using System;
using System.Collections.Generic;

namespace QuestFlag.Passport.Application.Common.DTOs;

public record AgentDto(
    string ClientId, 
    string DisplayName, 
    string Type, 
    HashSet<string> Permissions, 
    HashSet<Uri> RedirectUris, 
    HashSet<Uri> PostLogoutRedirectUris);
