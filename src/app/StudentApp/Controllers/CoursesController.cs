using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DataAccess;
using Models.Models;

namespace StudentApp.Controllers;

public class CoursesController : Controller
{
    private readonly StudentAppContext _context;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(StudentAppContext context, ILogger<CoursesController> logger)
    {
        _logger = logger;
        _context = context;
    }

    // GET: Courses
    public async Task<IActionResult> Index()
    {
        var random = new Random().Next();
        return View(await _context.Courses.ToListAsync());
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Courses == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.CourseId == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // GET: Courses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseId,Title,Credits")] Course course)
    {
        if (ModelState.IsValid)
        {
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    private bool IsPrime()
    {
        var currentSecond = DateTime.Now.Second;
        var counter = 0;
        for (var i = 1; i <= currentSecond; i++)
        {
            if (currentSecond % i == 0)
            {
                counter++;
            }
        }

        return counter == 2;
    }
    // GET: Courses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {

        try
        {
            //Adding Custom Random Errors
            if (IsPrime())
            {
                throw new MyCustomException($"Time is not Prime");
            }

            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Saving User");
            throw;
        }
    }

    // POST: Courses/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Credits")] Course course)
    {
        if (id != course.CourseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.CourseId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Courses == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.CourseId == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Courses == null)
        {
            return Problem("Entity set 'StudentAppContext.Course'  is null.");
        }
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }
}

class MyCustomException : Exception
{
    public MyCustomException(string message) : base(message) { }
}
