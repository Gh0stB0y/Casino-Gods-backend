﻿using CasinoGodsAPI.Databases;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CasinoGodsAPI.Models
{
    public class GlobalFunctions
    {
        public static async Task<string> RefreshTokenGlobal(string jwt, IDatabase redisDbLogin, IDatabase redisDbJwt, IConfiguration _configuration)
        {
            var ActivePlayerCheck = await redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return "Session expired, log in again";
            else
            {
                string username = ActivePlayerCheck.ToString();

                await redisDbJwt.KeyDeleteAsync(jwt);
                await redisDbLogin.KeyDeleteAsync(username);
                var ActivePlayer = new RedisActivePlayer(username, jwt, _configuration);
                await redisDbLogin.StringSetAsync(ActivePlayer.Name, ActivePlayer.Jwt, new TimeSpan(0,0,5,0));
                await redisDbJwt.StringSetAsync(ActivePlayer.Jwt, ActivePlayer.Name, new TimeSpan(0,0,5,0));
                //Console.WriteLine("login ktorego szukam: "+ActivePlayer.Name);

                return ActivePlayer.Jwt;
            }
        }
        public static async Task<string> RefreshTokenGlobal(string jwt,string UName, IDatabase redisDbLogin, IDatabase redisDbJwt, IConfiguration _configuration)
        {
            var ActivePlayerCheck = await redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return "Session expired, log in again";
            else
            {
                string username = ActivePlayerCheck.ToString();
                if (UName == username)
                {
                    await redisDbJwt.KeyDeleteAsync(jwt);
                    await redisDbLogin.KeyDeleteAsync(username);
                    var ActivePlayer = new RedisActivePlayer(username, jwt, _configuration);
                    Console.WriteLine("JWT ZAPISANE DO REDISA" + ActivePlayer.Jwt);
                    await redisDbLogin.StringSetAsync(ActivePlayer.Name, ActivePlayer.Jwt, new TimeSpan(0,0,5,0));
                    await redisDbJwt.StringSetAsync(ActivePlayer.Jwt, ActivePlayer.Name, new TimeSpan(0,0,5,0));
                    return ActivePlayer.Jwt;
                }
                else return "Redis data error, log in again";
            }
        }
        public static async Task<bool>LookForTokenGlobal(string UName,IDatabase redisLogin, IDatabase redisDbJwt, IConfiguration _configuration)
        {
            var ActivePlayerCheck = await redisLogin.StringGetAsync(UName);
            return !ActivePlayerCheck.IsNull;
        }
        public static async Task<bool>LookForJWTGlobal(string jwt, IDatabase redisLogin, IDatabase redisDbJwt, IConfiguration _configuration)
        {
            var ActivePlayerCheck = await redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return false;
            else return true;
        }
    }
}
