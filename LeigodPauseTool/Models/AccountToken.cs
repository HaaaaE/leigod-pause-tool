using System;

namespace LeigodPauseTool.Models;

public class AccountToken
{
    public required string Token { get; set; }
    public required DateTime ExpiryTime { get; set; }
    
    public override string ToString()
    {
        return $"Token: {Token}, ExpiryTime: {ExpiryTime}";
    }
}