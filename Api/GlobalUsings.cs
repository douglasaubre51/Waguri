global using System.Diagnostics;
global using System.Text;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.ComponentModel.DataAnnotations;
global using System.Collections.Generic;

global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.AspNetCore.SignalR;

global using Api.Data;
global using Api.Dtos;
global using Api.Models;
global using Api.Repositories;
global using Api.Services;
global using Api.Services.HubServices;
global using Api.ViewModels;
global using Api.Wrappers;
global using Api.Hubs;

global using MailKit.Net.Smtp;
global using MimeKit;
