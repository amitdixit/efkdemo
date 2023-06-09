﻿using StudenApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace StudenApp.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Privacy()
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);

            _logger.LogInformation(responseBody);
        }

        return View();
    }

    public IActionResult Calculate()
    {
        var number = new Random().Next(10, 30);
        long fact = 1;
        for (var i = 1; i <= number; i++)
        {
            // System.Console.WriteLine(i);
            fact = fact * i;
            if (fact < 0)
                break;
            System.Console.WriteLine(fact);
        }

        return Ok($"Factorial of {number} is:{fact}");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
