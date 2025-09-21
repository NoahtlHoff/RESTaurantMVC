<<<<<<< HEAD
﻿namespace RESTaurantMVC.Models
{
    public class AuthTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        //public DateTimeOffset? ExpiresAt { get; set; }
    }
=======
using System;

namespace RESTaurantMVC.Models;

public class AuthTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; set; }
>>>>>>> cb1d0dd99a364873478e5922fa6853247b2cc6df
}
